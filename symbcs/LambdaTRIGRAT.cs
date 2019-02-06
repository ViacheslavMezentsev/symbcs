using System;
using System.Collections;

internal class LambdaTRIGRAT : Lambda
{
	public virtual int lambda(Stack st)
	{
		int narg = getNarg(st);
		Algebraic f = getAlgebraic(st);
		f = f.rat();
		debug("Rational: " + f);
		f = (new ExpandUser()).f_exakt((Algebraic) f);
		debug("User Function expand: " + f);
		f = (new TrigExpand()).f_exakt((Algebraic) f);
		debug("Trigexpand: " + f);
		f = (new NormExp()).f_exakt((Algebraic) f);
		debug("Norm: " + f);
		f = (new TrigInverseExpand()).f_exakt((Algebraic) f);
		debug("Triginverse: " + f);
		f = (new SqrtExpand()).f_exakt((Algebraic) f);
		debug("Sqrtexpand: " + f);
		st.Push(f);
		return 0;
	}
}
internal class LambdaTRIGEXP : Lambda
{
	public virtual int lambda(Stack st)
	{
		int narg = getNarg(st);
		Algebraic f = getAlgebraic(st);
		f = f.rat();
		debug("Rational: " + f);
		f = (new ExpandUser()).f_exakt((Algebraic) f);
		debug("User Function expand: " + f);
		f = (new TrigExpand()).f_exakt((Algebraic) f);
		debug("Trigexpand: " + f);
		f = (new NormExp()).f_exakt((Algebraic) f);
		f = (new SqrtExpand()).f_exakt((Algebraic) f);
		st.Push(f);
		return 0;
	}
}
internal class TrigExpand : LambdaAlgebraic
{
	internal override Algebraic f_exakt(Algebraic x)
	{
		if (x is Polynomial && ((Polynomial)x).v is FunctionVariable)
		{
			Polynomial xp = (Polynomial)x;
			FunctionVariable f = (FunctionVariable)xp.v;
			object la = pc.env.getValue(f.fname);
			if (la != null && la is LambdaAlgebraic && ((LambdaAlgebraic)la).trigrule != null)
			{
				try
				{
					string trigrule = ((LambdaAlgebraic)la).trigrule;
					Algebraic fexp = evalx(trigrule, f.arg);
					Algebraic r = Zahl.ZERO;
					for (int i = xp.a.Length - 1; i > 0; i--)
					{
						r = r.add(f_exakt(xp.a[i])).mult(fexp);
					}
					if (xp.a.Length > 0)
					{
						r = r.add(f_exakt(xp.a[0]));
					}
					return r;
				}
				catch (Exception e)
				{
					throw new JasymcaException(e.ToString());
				}
			}
		}
		return x.map(this);
	}
}
internal class SqrtExpand : LambdaAlgebraic
{
	internal override Algebraic f_exakt(Algebraic x)
	{
		if (!(x is Polynomial))
		{
			return x.map(this);
		}
		Polynomial xp = (Polynomial)x;
		Variable @var = xp.v;
		if (@var is Root)
		{
			Vektor cr = ((Root)@var).poly;
			if (cr.length() == xp.degree() + 1)
			{
				Algebraic[] xr = new Algebraic[xp.degree() + 1];
				Algebraic ratio = null;
				for (int i = xr.Length - 1; i >= 0; i--)
				{
					xr[i] = xp.a[i].map(this);
					if (i == xr.Length - 1)
					{
						ratio = xr[i];
					}
					else if (i > 0 && ratio != null)
					{
						if (!cr.get(i).mult(ratio).Equals(xr[i]))
						{
							ratio = null;
						}
					}
				}
				if (ratio != null)
				{
					return xr[0].sub(ratio.mult(cr.get(0)));
				}
				else
				{
					return new Polynomial(@var, xr);
				}
			}
		}
		Algebraic xf = null;
		if (@var is FunctionVariable && ((FunctionVariable)@var).fname.Equals("sqrt") && ((FunctionVariable)@var).arg is Polynomial)
		{
			Polynomial arg = (Polynomial)((FunctionVariable)@var).arg;
			Algebraic[] sqfr = arg.square_free_dec(arg.v);
			bool issquare = true;
			if (sqfr.Length > 0 && !sqfr[0].Equals(arg.a[arg.a.Length - 1]))
			{
				issquare = false;
			}
			for (int i = 2; i < sqfr.Length && issquare; i++)
			{
				if ((i + 1) % 2 == 1 && !sqfr[i].Equals(Zahl.ONE))
				{
					issquare = false;
				}
			}
			if (issquare)
			{
				xf = Zahl.ONE;
				for (int i = 1; i < sqfr.Length; i += 2)
				{
					if (!sqfr[i].Equals(Zahl.ZERO))
					{
						xf = xf.mult(sqfr[i].pow_n((i + 1) / 2));
					}
				}
				Algebraic r = Zahl.ZERO;
				for (int i = xp.a.Length - 1; i > 0; i--)
				{
					r = r.add(f_exakt(xp.a[i])).mult(xf);
				}
				if (xp.a.Length > 0)
				{
					r = r.add(f_exakt(xp.a[0]));
				}
				return r;
			}
		}
		if (@var is FunctionVariable && ((FunctionVariable)@var).fname.Equals("sqrt") && xp.degree() > 1)
		{
			xf = ((FunctionVariable)@var).arg;
			Polynomial sq = new Polynomial(@var);
			Algebraic r = f_exakt(xp.a[0]);
			Algebraic xv = Zahl.ONE;
			for (int i = 1; i < xp.a.Length; i++)
			{
				if (i % 2 == 1)
				{
					r = r.add(f_exakt(xp.a[i]).mult(xv).mult(sq));
				}
				else
				{
					xv = xv.mult(xf);
					r = r.add(f_exakt(xp.a[i]).mult(xv));
				}
			}
			return r;
		}
		return x.map(this);
	}
}
internal class TrigInverseExpand : LambdaAlgebraic
{
	public virtual Algebraic divExponential(Algebraic x, FunctionVariable fv, int n)
	{
		Algebraic[] a = new Algebraic[2];
		a[1] = x;
		Algebraic xk = Zahl.ZERO;
		for (int i = n; i >= 0; i--)
		{
			Algebraic kf = FunctionVariable.create("exp",fv.arg).pow_n(i);
			a[0] = a[1];
			a[1] = kf;
			Poly.polydiv(a, fv);
			if (!a[0].Equals(Zahl.ZERO))
			{
				Algebraic kfi = FunctionVariable.create("exp", fv.arg.mult(Zahl.MINUS)).pow_n(n - i);
				xk = xk.add(a[0].mult(kfi));
			}
			if (a[1].Equals(Zahl.ZERO))
			{
				break;
			}
		}
		return f_exakt(xk);
	}
	internal override Algebraic f_exakt(Algebraic x)
	{
		if (x is Rational)
		{
			Rational xr = (Rational)x;
			if (xr.den.v is FunctionVariable && ((FunctionVariable)xr.den.v).fname.Equals("exp") && ((FunctionVariable)xr.den.v).arg.komplexq())
			{
				FunctionVariable fv = (FunctionVariable)xr.den.v;
				int maxdeg = Math.Max(Poly.degree(xr.nom,fv), Poly.degree(xr.den,fv));
				if (maxdeg % 2 == 0)
				{
					return divExponential(xr.nom, fv, maxdeg / 2).div(divExponential(xr.den, fv, maxdeg / 2));
				}
				else
				{
					FunctionVariable fv2 = new FunctionVariable("exp", ((FunctionVariable)xr.den.v).arg.div(Zahl.TWO), ((FunctionVariable)xr.den.v).la);
					Algebraic ex = new Polynomial(fv2, new Algebraic[] {Zahl.ZERO, Zahl.ZERO,Zahl.ONE});
					Algebraic xr1 = xr.nom.value(xr.den.v, ex).div(xr.den.value(xr.den.v, ex));
					return f_exakt(xr1);
				}
			}
		}
		if (x is Polynomial && ((Polynomial)x).v is FunctionVariable)
		{
			Polynomial xp = (Polynomial)x;
			Algebraic xf = null;
			FunctionVariable @var = (FunctionVariable)xp.v;
			if (@var.fname.Equals("exp"))
			{
				Algebraic re = @var.arg.realpart();
				Algebraic im = @var.arg.imagpart();
				if (!im.Equals(Zahl.ZERO))
				{
					bool _minus = minus(im);
					if (_minus)
					{
						im = im.mult(Zahl.MINUS);
					}
					Algebraic a = FunctionVariable.create("exp",re);
					Algebraic b = FunctionVariable.create("cos", im);
					Algebraic c = FunctionVariable.create("sin", im).mult(Zahl.IONE);
					xf = a.mult(_minus?(b.sub(c)):b.add(c));
				}
			}
			if (@var.fname.Equals("log"))
			{
				Algebraic arg = @var.arg;
				Algebraic factor = Zahl.ONE, sum = Zahl.ZERO;
				if (arg is Polynomial && ((Polynomial)arg).degree() == 1 && ((Polynomial)arg).v is FunctionVariable && ((Polynomial)arg).a[0].Equals(Zahl.ZERO) && ((FunctionVariable)((Polynomial)arg).v).fname.Equals("sqrt"))
				{
					sum = FunctionVariable.create("log",((Polynomial)arg).a[1]);
					factor = new Unexakt(0.5);
					arg = ((FunctionVariable)((Polynomial)arg).v).arg;
					xf = FunctionVariable.create("log", arg);
				}
				try
				{
					Algebraic re = arg.realpart();
					Algebraic im = arg.imagpart();
					if (!im.Equals(Zahl.ZERO))
					{
						bool min_im = minus(im);
						if (min_im)
						{
							im = im.mult(Zahl.MINUS);
						}
						Algebraic a1 = (new SqrtExpand()).f_exakt(arg.mult(arg.cc()));
						Algebraic a = FunctionVariable.create("log",a1).div(Zahl.TWO);
						Algebraic b1 = f_exakt(re.div(im));
						Algebraic b = FunctionVariable.create("atan", b1).mult(Zahl.IONE);
						xf = min_im? a.add(b) :a.sub(b);
						Algebraic pi2 = Zahl.PI.mult(Zahl.IONE).div(Zahl.TWO);
						xf = min_im? xf.sub(pi2):xf.add(pi2);
					}
				}
				catch (JasymcaException)
				{
				}
				if (xf != null)
				{
					xf = xf.mult(factor).add(sum);
				}
			}
			if (xf == null)
			{
				return x.map(this);
			}
			Algebraic r = Zahl.ZERO;
			for (int i = xp.a.Length - 1; i > 0; i--)
			{
				r = r.add(f_exakt(xp.a[i])).mult(xf);
			}
			if (xp.a.Length > 0)
			{
				r = r.add(f_exakt(xp.a[0]));
			}
			return r;
		}
		return x.map(this);
	}
	internal static bool minus(Algebraic x)
	{
		if (x is Zahl)
		{
			return ((Zahl)x).smaller(Zahl.ZERO);
		}
		if (x is Polynomial)
		{
			return minus(((Polynomial)x).a[((Polynomial)x).degree()]);
		}
		if (x is Rational)
		{
			bool a = minus(((Rational)x).nom);
			bool b = minus(((Rational)x).den);
			return (a && !b) || (!a && b);
		}
		throw new JasymcaException("minus not implemented for " + x);
	}
}
internal class LambdaSIN : LambdaAlgebraic
{
	public LambdaSIN()
	{
		diffrule = "cos(x)";
		intrule = "-cos(x)";
		trigrule = "1/(2*i)*(exp(i*x)-exp(-i*x))";
	}
	internal override Zahl f(Zahl x)
	{
		Unexakt z = x.unexakt();
		if (z.imag == 0.0)
		{
			return new Unexakt(Math.Sin(z.real));
		}
		return (Zahl)evalx(trigrule, z);
	}
	internal override Algebraic f_exakt(Algebraic x)
	{
		if (x.Equals(Zahl.ZERO))
		{
			return Zahl.ZERO;
		}
		return null;
	}
}
internal class LambdaCOS : LambdaAlgebraic
{
	public LambdaCOS()
	{
		diffrule = "-sin(x)";
		intrule = "sin(x)";
		trigrule = "1/2 *(exp(i*x)+exp(-i*x))";
	}
	internal override Zahl f(Zahl x)
	{
		Unexakt z = x.unexakt();
		if (z.imag == 0.0)
		{
			return new Unexakt(Math.Cos(z.real));
		}
		return (Zahl)evalx(trigrule, z);
	}
	internal override Algebraic f_exakt(Algebraic x)
	{
		if (x.Equals(Zahl.ZERO))
		{
			return Zahl.ONE;
		}
		return null;
	}
}
internal class LambdaTAN : LambdaAlgebraic
{
	public LambdaTAN()
	{
		diffrule = "1/(cos(x))^2";
		intrule = "-log(cos(x))";
		trigrule = "-i*(exp(i*x)-exp(-i*x))/(exp(i*x)+exp(-i*x))";
	}
	internal override Zahl f(Zahl x)
	{
		Unexakt z = x.unexakt();
		if (z.imag == 0.0)
		{
			return new Unexakt(Math.Tan(z.real));
		}
		return (Zahl)evalx(trigrule, z);
	}
	internal override Algebraic f_exakt(Algebraic x)
	{
		if (x.Equals(Zahl.ZERO))
		{
			return Zahl.ZERO;
		}
		return null;
	}
}
internal class LambdaATAN : LambdaAlgebraic
{
	public LambdaATAN()
	{
		diffrule = "1/(1+x^2)";
		intrule = "x*atan(x)-1/2*log(1+x^2)";
		trigrule = "-i/2*log((1+i*x)/(1-i*x))";
	}
	internal override Zahl f(Zahl x)
	{
		Unexakt z = x.unexakt();
		if (z.imag == 0.0)
		{
			return new Unexakt(JMath.atan(z.real));
		}
		return (Zahl)evalx(trigrule, z);
	}
	internal override Algebraic f_exakt(Algebraic x)
	{
		if (x.Equals(Zahl.ZERO))
		{
			return Zahl.ZERO;
		}
		return null;
	}
}
internal class LambdaASIN : LambdaAlgebraic
{
	public LambdaASIN()
	{
		diffrule = "1/sqrt(1-x^2)";
		intrule = "x*asin(x)+sqrt(1-x^2)";
		trigrule = "-i*log(i*x+i*sqrt(1-x^2))";
	}
	internal override Zahl f(Zahl x)
	{
		Unexakt z = x.unexakt();
		if (z.imag == 0.0)
		{
			return new Unexakt(JMath.asin(z.real));
		}
		return (Zahl)evalx(trigrule, z);
	}
}
internal class LambdaACOS : LambdaAlgebraic
{
	public LambdaACOS()
	{
		diffrule = "-1/sqrt(1-x^2)";
		intrule = "x*acos(x)-sqrt(1-x^2)";
		trigrule = "-i*log(x+i*sqrt(1-x^2))";
	}
	internal override Zahl f(Zahl x)
	{
		Unexakt z = x.unexakt();
		if (z.imag == 0.0)
		{
			return new Unexakt(JMath.acos(z.real));
		}
		return (Zahl)evalx(trigrule, z);
	}
}
internal class LambdaATAN2 : LambdaAlgebraic
{
	internal override Zahl f(Zahl x)
	{
		return null;
	}
	internal override Algebraic f_exakt(Algebraic x)
	{
		throw new JasymcaException("Usage: ATAN2(y,x).");
	}
	internal override Algebraic f_exakt(Algebraic[] x)
	{
		throw new JasymcaException("Usage: ATAN2(y,x).");
	}
	internal override Algebraic f_exakt(Algebraic x, Algebraic y)
	{
		if (y is Unexakt && !y.komplexq() && x is Unexakt && !x.komplexq())
		{
			return new Unexakt(JMath.atan2(((Unexakt)y).real, ((Unexakt)x).real));
		}
		if (!Zahl.ZERO.Equals(x))
		{
			return FunctionVariable.create("atan", y.div(x)).add((FunctionVariable.create("sign",y).mult(Zahl.ONE.sub((FunctionVariable.create("sign",x)))).mult(Zahl.PI).div(Zahl.TWO)));
		}
		else
		{
			return (FunctionVariable.create("sign",y).mult(Zahl.PI).div(Zahl.TWO));
		}
	}
}
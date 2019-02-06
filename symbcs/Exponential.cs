using System;
using System.Collections;

public class Exponential : Polynomial
{
	public Variable expvar;
	public Algebraic exp_b;

	public Exponential(Algebraic a, Algebraic c, Variable x, Algebraic b)
	{
		this.a = new Algebraic[2];
		this.a[0] = c;
		this.a[1] = a;
		Algebraic[] z = new Algebraic[2];
		z[0] = Zahl.ZERO;
		z[1] = b;
		object la = Lambda.pc.env.getValue("exp");
		if (!(la is LambdaEXP))
		{
			la = new LambdaEXP();
		}
		this.v = new FunctionVariable("exp", new Polynomial(x, z),(LambdaAlgebraic)la);
		this.expvar = x;
		this.exp_b = b;
	}

	public Exponential(Polynomial x) : base(x.v, x.a)
	{
		this.expvar = ((Polynomial)((FunctionVariable)this.v).arg).v;
		this.exp_b = ((Polynomial)((FunctionVariable)this.v).arg).a[1];
	}

	public static Algebraic poly2exp(Algebraic x)
	{
		if (x is Exponential)
		{
			return x;
		}
		if (x is Polynomial && ((Polynomial)x).degree() == 1 && ((Polynomial)x).v is FunctionVariable && ((FunctionVariable)(((Polynomial)x).v)).fname.Equals("exp"))
		{
			Algebraic arg = ((FunctionVariable)(((Polynomial)x).v)).arg;
			if (arg is Polynomial && ((Polynomial)arg).degree() == 1 && ((Polynomial)arg).a[0].Equals(Zahl.ZERO))
			{
				return new Exponential((Polynomial)x);
			}
		}
		return x;
	}

	public override Algebraic cc()
	{
		return new Exponential(a[1].cc(), a[0].cc(), expvar, exp_b.cc());
	}

	internal static bool containsexp(Algebraic x)
	{
		if (x is Zahl)
		{
			return false;
		}
		if (x is Exponential)
		{
			return true;
		}
		if (x is Polynomial)
		{
			for (int i = 0; i < ((Polynomial)x).a.Length; i++)
			{
				if (containsexp(((Polynomial)x).a[i]))
				{
					return true;
				}
			}
				if (((Polynomial)x).v is FunctionVariable)
				{
					return containsexp(((FunctionVariable)((Polynomial)x).v).arg);
				}
				return false;
		}
		if (x is Rational)
		{
			return containsexp(((Rational)x).nom) || containsexp(((Rational)x).den);
		}
		if (x is Vektor)
		{
			for (int i = 0; i < ((Vektor)x).length(); i++)
			{
				if (containsexp(((Vektor)x).get(i)))
				{
					return true;
				}
			}
				return false;
		}
		throw new JasymcaException("containsexp not suitable for x");
	}

	public override Algebraic add(Algebraic x)
	{
		if (x is Zahl)
		{
			return new Exponential(a[1], x.add(a[0]), expvar, exp_b);
		}
		if (x is Exponential)
		{
			if (v.Equals(((Exponential)x).v))
			{
				return poly2exp(base.add(x));
			}
			if (v.smaller(((Exponential)x).v))
			{
				return x.add(this);
			}
			return new Exponential(a[1], x.add(a[0]), expvar, exp_b);
		}
		return poly2exp(base.add(x));
	}
	public override Algebraic mult(Algebraic x)
	{
		if (x.Equals(Zahl.ZERO))
		{
			return x;
		}
		if (x is Zahl)
		{
			return new Exponential(a[1].mult(x), a[0].mult(x), expvar, exp_b);
		}
		if (x is Exponential && expvar.Equals(((Exponential)x).expvar))
		{
			Exponential xp = (Exponential)x;
			Algebraic r = Zahl.ZERO;
			Algebraic nex = exp_b.add(xp.exp_b);
			if (nex.Equals(Zahl.ZERO))
			{
				r = a[1].mult(xp.a[1]);
			}
			else
			{
				r = new Exponential(a[1].mult(xp.a[1]),Zahl.ZERO,expvar, nex);
			}
			r = r.add(a[0].mult(xp));
			r = r.add(mult(xp.a[0]));
			r = r.reduce();
			return r;
		}
		return poly2exp(base.mult(x));
	}

	public override Algebraic reduce()
	{
		if (a[1].reduce().Equals(Zahl.ZERO))
		{
			return a[0].reduce();
		}
		if (exp_b.Equals(Zahl.ZERO))
		{
			return a[0].add(a[1]).reduce();
		}
		return this;
	}

	public override Algebraic div(Algebraic x)
	{
		if (x is Zahl)
		{
			return new Exponential((Polynomial)base.div(x));
		}
		return base.div(x);
	}

	public override Algebraic map(LambdaAlgebraic f)
	{
		return poly2exp(base.map(f));
	}

	public static Zahl exp_gcd(ArrayList v, Variable x)
	{
		Zahl gcd = Zahl.ZERO;
		int k = 0;
		for (int i = 0; i < v.Count; i++)
		{
			Algebraic a = (Algebraic)v[i];
			Algebraic c;
			if (Poly.degree(a,x) == 1 && (c = Poly.coefficient(a,x,1)) is Zahl)
			{
				k++;
				gcd = gcd.gcd((Zahl)c);
			}
		}
		return (k > 0 ? gcd : Zahl.ONE);
	}

	public static Algebraic reduce_exp(Algebraic p)
	{
		Algebraic[] a = new Algebraic[] {p};
		a = reduce_exp(a);
		return a[0];
	}

	public static Algebraic[] reduce_exp(Algebraic[] p)
	{
		ArrayList v = new ArrayList();
		ArrayList vars = new ArrayList();
		GetExpVars2 g = new GetExpVars2(v);
		for (int i = 0; i < p.Length; i++)
		{
			g.f_exakt(p[i]);
		}
		for (int i = 0; i < v.Count; i++)
		{
			Algebraic a = (Algebraic)v[i];
			Variable x = null;
			if (a is Polynomial)
			{
				x = ((Polynomial)a).v;
			}
			else
			{
				continue;
			}
			if (vars.Contains(x))
			{
				continue;
			}
			else
			{
				vars.Add(x);
			}
			Zahl gcd = exp_gcd(v, x);
			if (!gcd.Equals(Zahl.ZERO) && !gcd.Equals(Zahl.ONE))
			{
				SubstExp sb = new SubstExp(gcd, x);
				for (int k = 0; k < p.Length; k++)
				{
					p[k] = sb.f_exakt(p[k]);
				}
			}
		}
		return p;
	}
}

internal class SubstExp : LambdaAlgebraic
{
	internal Zahl gcd;
	internal Variable @var;
	internal Variable t = new SimpleVariable("t_exponential");

	public SubstExp(Zahl gcd, Variable @var)
	{
		this.gcd = gcd;
		this.@var = @var;
	}

	public SubstExp(Variable @var, Algebraic expr)
	{
		this.@var = @var;
		ArrayList v = new ArrayList();
		(new GetExpVars2(v)).f_exakt(expr);
		this.gcd = Exponential.exp_gcd(v, @var);
		if (gcd.Equals(Zahl.ZERO))
		{
			t = @var;
		}
	}

	public virtual Algebraic ratsubst(Algebraic expr)
	{
		if (gcd.Equals(Zahl.ZERO))
		{
			return expr;
		}
		if (!expr.depends(@var))
		{
			return expr;
		}
		if (expr is Rational)
		{
			return ratsubst(((Rational)expr).nom).div(ratsubst(((Rational)expr).den));
		}

		if (expr is Polynomial && ((Polynomial)expr).v is FunctionVariable 
            && ((FunctionVariable)((Polynomial)expr).v).fname.Equals("exp") 
            && ((FunctionVariable)((Polynomial)expr).v).arg is Polynomial 
            && ((Polynomial)((FunctionVariable)((Polynomial)expr).v).arg).v.Equals(@var) 
            && ((Polynomial)((FunctionVariable)((Polynomial)expr).v).arg).degree() == 1 
            && ((Polynomial)((FunctionVariable)((Polynomial)expr).v).arg).a[0].Equals(Zahl.ZERO))
		{
			Polynomial pexpr = (Polynomial)expr;
			int degree = pexpr.degree();
			Algebraic[] a = new Algebraic[degree+1];
			for (int i = 0; i <= degree; i++)
			{
				Algebraic cf = pexpr.a[i];
				if (cf.depends(@var))
				{
					throw new JasymcaException("Rationalize failed: 2");
				}
				a[i] = cf;
			}
			return new Polynomial(t, a);
		}
		throw new JasymcaException("Could not rationalize " + expr);
	}

	public virtual Algebraic rational(Algebraic expr)
	{
		return ratsubst(expr).div(gcd).div(new Polynomial(t)).reduce();
	}

	public virtual Algebraic rat_reverse(Algebraic expr)
	{
		if (gcd.Equals(Zahl.ZERO))
		{
			return expr;
		}
		Zahl gc = gcd;
		Algebraic s = new Exponential(Zahl.ONE, Zahl.ZERO, @var, Zahl.ONE.mult(gc));
		return expr.value(t, s);
	}

	internal override Algebraic f_exakt(Algebraic f)
	{
		if (gcd.Equals(Zahl.ZERO))
		{
			return f;
		}
		if (f is Polynomial)
		{
			Polynomial p = (Polynomial)f;
			if (p.v is FunctionVariable && ((FunctionVariable)p.v).fname.Equals("exp") && Poly.degree(((FunctionVariable)p.v).arg,@var) == 1)
			{
				Algebraic arg = ((FunctionVariable)p.v).arg;
				Algebraic[] new_coef = new Algebraic[2];
				new_coef[1] = gcd.unexakt();
				new_coef[0] = Zahl.ZERO;
				Algebraic new_arg = new Polynomial(@var, new_coef);
				Algebraic subst = FunctionVariable.create("exp", new_arg);
				Algebraic exp = Poly.coefficient(arg,@var,1).div(gcd);
				if (!(exp is Zahl) && !((Zahl)exp).integerq())
				{
					throw new JasymcaException("Not integer exponent in exponential simplification.");
				}
				subst = subst.pow_n(((Zahl)exp).intval());
				subst = subst.mult(FunctionVariable.create("exp", Poly.coefficient(arg,@var,0)));
				int n = p.a.Length;
				Algebraic r = f_exakt(p.a[n - 1]);
				for (int i = n - 2; i >= 0; i--)
				{
					r = r.mult(subst).add(f_exakt(p.a[i]));
				}
				return r;
			}
		}

		return f.map(this);
	}
}

internal class NormExp : LambdaAlgebraic
{
	internal override Algebraic f_exakt(Algebraic f)
	{
		if (f is Rational)
		{
			Algebraic nom = f_exakt(((Rational)f).nom);
			Algebraic den = f_exakt(((Rational)f).den);
			if (den is Zahl)
			{
				return f_exakt(nom.div(den));
			}
			if (den is Exponential && ((Polynomial)den).a[0].Equals(Zahl.ZERO) && ((Polynomial)den).a[1] is Zahl)
			{
				if (nom is Zahl || nom is Polynomial)
				{
					Exponential denx = (Exponential)den;
					Exponential den_inv = new Exponential(Zahl.ONE.div(denx.a[1]),Zahl.ZERO, denx.expvar, denx.exp_b.mult(Zahl.MINUS));
					return nom.mult(den_inv);
				}
			}
			f = nom.div(den);
			return f;
		}
		if (f is Exponential)
		{
			return f.map(this);
		}
		if (!(f is Polynomial))
		{
			return f.map(this);
		}
		Polynomial fp = (Polynomial)f;
		if (!(fp.v is FunctionVariable) || !((FunctionVariable)fp.v).fname.Equals("exp"))
		{
			return f.map(this);
		}
		Algebraic arg = ((FunctionVariable)fp.v).arg.reduce();
		if (arg is Zahl)
		{
			return fp.value(FunctionVariable.create("exp",arg)).map(this);
		}
		if (!(arg is Polynomial) || !(((Polynomial)arg).degree() == 1))
		{
			return f.map(this);
		}
		Algebraic r = Zahl.ZERO;
		Algebraic a = ((Polynomial)arg).a[1];
		for (int i = 1; i < fp.a.Length; i++)
		{
			Algebraic b = ((Polynomial)arg).a[0];
			Zahl I = new Unexakt((double)i);
			Algebraic ebi = Zahl.ONE;
			while (b is Polynomial && ((Polynomial)b).degree() == 1)
			{
				Algebraic f1 = FunctionVariable.create("exp", (new Polynomial(((Polynomial)b).v)).mult(((Polynomial)b).a[1].mult(I)));
				f1 = Exponential.poly2exp(f1);
				ebi = ebi.mult(f1);
				b = ((Polynomial)b).a[0];
			}
			ebi = ebi.mult(FunctionVariable.create("exp", b.mult(I)));
			Algebraic cf = f_exakt(fp.a[i].mult(ebi));
			Algebraic f2 = FunctionVariable.create("exp", (new Polynomial(((Polynomial)arg).v)).mult(a.mult(I)));
			f2 = Exponential.poly2exp(f2);
			r = r.add(cf.mult(f2));
		}
		if (fp.a.Length > 0)
		{
			r = r.add(f_exakt(fp.a[0]));
		}
		return Exponential.poly2exp(r);
	}
}

internal class CollectExp : LambdaAlgebraic
{
	internal ArrayList v;

	public CollectExp(Algebraic f)
	{
		v = new ArrayList();

		(new GetExpVars(v)).f_exakt(f);
	}

	internal override Algebraic f_exakt(Algebraic x1)
	{
		if (v.Count == 0)
		{
			return x1;
		}

		if (!(x1 is Exponential))
		{
			return x1.map(this);
		}

		Exponential e = (Exponential)x1;

		int exp = 1;

		Algebraic exp_b = e.exp_b;

		if (exp_b is Zahl && ((Zahl)exp_b).smaller(Zahl.ZERO))
		{
			exp *= -1;
			exp_b = exp_b.mult(Zahl.MINUS);
		}

		Variable x = e.expvar;

		for (int i = 0; i < v.Count; i++)
		{
			Polynomial y = (Polynomial)v[i];

			if (y.v.Equals(x))
			{
				Algebraic rat = exp_b.div(y.a[1]);

				if (rat is Zahl && !((Zahl)rat).komplexq())
				{
					int _cfs = cfs( ( ( Zahl ) rat ).unexakt().real );

					if (_cfs != 0 && _cfs != 1)
					{
						exp *= _cfs;
						exp_b = exp_b.div(new Unexakt((double)_cfs));
					}
				}
			}
		}

        var p = ( new Polynomial( x ) ).mult( exp_b );

        p = FunctionVariable.create( "exp", p ).pow_n( exp );

        return p.mult( f_exakt( e.a[1] ) ).add( f_exakt( e.a[0] ) );
    }

	internal virtual int cfs(double x)
	{
		if (x < 0)
		{
			return cfs(-x);
		}

		int a0 = ( int ) Math.Floor(x);

		if ( x == ( double ) a0 )
		{
			return a0;
		}

		int a1 = ( int ) Math.Floor( 1.0 / ( x - a0 ) );

		int z = a0 * a1 + 1;

	    if ( Math.Abs( ( double ) z / ( double ) a1 - x ) < 1.0e-6 )
		{
			return z;
		}

		return 0;
	}
}

internal class GetExpVars : LambdaAlgebraic
{
	internal ArrayList v;

	public GetExpVars(ArrayList v)
	{
		this.v = v;
	}

	internal override Algebraic f_exakt(Algebraic f)
	{
		if (f is Exponential)
		{
			Algebraic x = new Polynomial(((Exponential)f).expvar);
			x = x.mult(((Exponential)f).exp_b);
			v.Add(x);
			f_exakt(((Exponential)f).a[1]);
			f_exakt(((Exponential)f).a[0]);
			return Zahl.ONE;
		}
		return f.map(this);
	}
}

internal class GetExpVars2 : LambdaAlgebraic
{
	internal ArrayList v;

	public GetExpVars2(ArrayList v)
	{
		this.v = v;
	}

	internal override Algebraic f_exakt(Algebraic f)
	{
		if (f is Polynomial)
		{
			Polynomial p = (Polynomial)f;
			if (p.v is FunctionVariable && ((FunctionVariable)p.v).fname.Equals("exp"))
			{
				v.Add(((FunctionVariable)p.v).arg);
			}
			for (int i = 0; i < p.a.Length; i++)
			{
				f_exakt(p.a[i]);
			}
			return Zahl.ONE;
		}
		return f.map(this);
	}
}

internal class DeExp : LambdaAlgebraic
{
	internal override Algebraic f_exakt(Algebraic f)
	{
		if (f is Exponential)
		{
			Exponential x = (Exponential)f;
			Algebraic[] cn = new Algebraic[2];
			cn[0] = f_exakt(x.a[0]);
			cn[1] = f_exakt(x.a[1]);
			return new Polynomial(x.v, cn);
		}

		return f.map(this);
	}
}

internal class LambdaEXP : LambdaAlgebraic
{
	public LambdaEXP()
	{
		diffrule = "exp(x)";
		intrule = "exp(x)";
	}

	internal override Zahl f(Zahl x)
	{
		Unexakt z = x.unexakt();

		double r = JMath.exp(z.real);

		if (z.imag != 0.0)
		{
			return new Unexakt(r * Math.Cos(z.imag), r * Math.Sin(z.imag));
		}

		return new Unexakt(r);
	}

	internal override Algebraic f_exakt(Algebraic x)
	{
		if (x.Equals(Zahl.ZERO))
		{
			return Zahl.ONE;
		}

		if (x is Polynomial 
            && ((Polynomial)x).degree() == 1 
            && ((Polynomial)x).a[0].Equals(Zahl.ZERO))
		{
			Polynomial xp = (Polynomial)x;

			if (xp.v is SimpleVariable && ((SimpleVariable)xp.v).name.Equals("pi"))
			{
				Algebraic q = xp.a[1].div(Zahl.IONE);
				if (q is Zahl)
				{
					return fzexakt((Zahl)q);
				}
			}
			if (xp.a[1] is Zahl 
                && xp.v is FunctionVariable 
                && ((FunctionVariable)xp.v).fname.Equals("log"))
			{
				if (((Zahl)xp.a[1]).integerq())
				{
					int n = ((Zahl)xp.a[1]).intval();

					return ((FunctionVariable)xp.v).arg.pow_n(n);
				}
			}
		}
		return null;
	}

	internal virtual Algebraic fzexakt(Zahl x)
	{
		if (x.smaller(Zahl.ZERO))
		{
			Algebraic r = fzexakt((Zahl)x.mult(Zahl.MINUS));
			if (r != null)
			{
				return r.cc();
			}
			return r;
		}

		if (x.integerq())
		{
			if (x.intval() % 2 == 0)
			{
				return Zahl.ONE;
			}
			else
			{
				return Zahl.MINUS;
			}
		}

		Algebraic qs = x.add(new Unexakt(.5));

		if (((Zahl)qs).integerq())
		{
			if (((Zahl)qs).intval() % 2 == 0)
			{
				return Zahl.IMINUS;
			}
			else
			{
				return Zahl.IONE;
			}
		}

		qs = x.mult(new Unexakt(4));

		if (((Zahl)qs).integerq())
		{
			Algebraic sq2 = FunctionVariable.create("sqrt",new Unexakt(0.5));
			switch (((Zahl)qs).intval() % 8)
			{
				case 1:
					return Zahl.ONE.add(Zahl.IONE).div(Zahl.SQRT2);
				case 3:
					return Zahl.MINUS.add(Zahl.IONE).div(Zahl.SQRT2);
				case 5:
					return Zahl.MINUS.add(Zahl.IMINUS).div(Zahl.SQRT2);
				case 7:
					return Zahl.ONE.add(Zahl.IMINUS).div(Zahl.SQRT2);
			}
		}
		qs = x.mult(new Unexakt(6));
		if (((Zahl)qs).integerq())
		{
			switch (((Zahl)qs).intval() % 12)
			{
				case 1:
					return Zahl.SQRT3.add(Zahl.IONE).div(Zahl.TWO);
				case 2:
					return Zahl.ONE.add(Zahl.SQRT3.mult(Zahl.IONE)).div(Zahl.TWO);
				case 4:
					return Zahl.SQRT3.mult(Zahl.IONE).add(Zahl.MINUS).div(Zahl.TWO);
				case 5:
					return Zahl.IONE.sub(Zahl.SQRT3).div(Zahl.TWO);
				case 7:
					return Zahl.IMINUS.sub(Zahl.SQRT3).div(Zahl.TWO);
				case 8:
					return Zahl.SQRT3.mult(Zahl.IMINUS).sub(Zahl.ONE).div(Zahl.TWO);
				case 10:
					return Zahl.SQRT3.mult(Zahl.IMINUS).add(Zahl.ONE).div(Zahl.TWO);
				case 11:
					return Zahl.IMINUS.add(Zahl.SQRT3).div(Zahl.TWO);
			}
		}
		return null;
	}
}

internal class LambdaLOG : LambdaAlgebraic
{
	public LambdaLOG()
	{
		diffrule = "1/x";
		intrule = "x*log(x)-x";
	}

	internal override Zahl f(Zahl x)
	{
		Unexakt z = x.unexakt();
		if (z.real < 0 || z.imag != 0.0)
		{
			return new Unexakt(JMath.log(z.real * z.real + z.imag * z.imag) / 2, JMath.atan2(z.imag,z.real));
		}
		return new Unexakt(JMath.log(z.real));
	}

	internal override Algebraic f_exakt(Algebraic x)
	{
		if (x.Equals(Zahl.ONE))
		{
			return Zahl.ZERO;
		}
		if (x.Equals(Zahl.MINUS))
		{
			return Zahl.PI.mult(Zahl.IONE);
		}
		if (x is Polynomial 
            && ((Polynomial)x).degree() == 1 
            && ((Polynomial)x).a[0].Equals(Zahl.ZERO) 
            && ((Polynomial)x).v is FunctionVariable 
            && ((FunctionVariable)((Polynomial)x).v).fname.Equals("exp"))
		{
			return ((FunctionVariable)((Polynomial)x).v).arg.add(FunctionVariable.create("log",((Polynomial)x).a[1]));
		}
		return null;
	}
}
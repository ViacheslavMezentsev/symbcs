using System;
using System.Collections;

public class Polynomial : Algebraic
{
	public Algebraic[] a = null;
	public Variable @var = null;
	public Polynomial()
	{
	}
	public Polynomial(Variable @var, Algebraic[] a)
	{
		this.@var = @var;
		this.a = Poly.reduce(a);
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Polynomial(Variable var, Vektor v) throws JasymcaException
	public Polynomial(Variable @var, Vektor v)
	{
		this.@var = @var;
		this.a = new Algebraic[v.length()];
		for (int i = 0; i < a.Length; i++)
		{
			a[i] = v.get(a.Length - 1 - i);
		}
		this.a = Poly.reduce(a);
	}
	public Polynomial(Variable @var)
	{
		a = new Zahl[] {Zahl.ZERO, Zahl.ONE};
		this.@var = @var;
	}
	public virtual Variable Var
	{
		get
		{
			return @var;
		}
	}
	public virtual Vektor coeff()
	{
		Algebraic[] c = Poly.clone(a);
		return new Vektor(c);
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic coefficient(Variable var, int n)throws JasymcaException
	public virtual Algebraic coefficient(Variable @var, int n)
	{
		if (@var.Equals(this.@var))
		{
			return coefficient(n);
		}
		Algebraic c = Zahl.ZERO;
		for (int i = 0; i < a.Length; i++)
		{
			Algebraic ci = a[i];
			if (ci is Polynomial)
			{
				c = c.add(((Polynomial)ci).coefficient(@var,n).mult((new Polynomial(this.@var)).pow_n(i)));
			}
			else if (n == 0)
			{
				c = c.add(ci.mult((new Polynomial(this.@var)).pow_n(i)));
			}
		}
		return c;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic coefficient(int i) throws JasymcaException
	public virtual Algebraic coefficient(int i)
	{
		if (i >= 0 && i < a.Length)
		{
			return a[i];
		}
		return Zahl.ZERO;
	}
	public override bool ratfunc(Variable v)
	{
		if (@var is FunctionVariable && ((FunctionVariable)this.@var).arg.depends(v))
		{
			return false;
		}
		for (int i = 0; i < a.Length; i++)
		{
			if (!a[i].ratfunc(v))
			{
				return false;
			}
		}
			return true;
	}
	public virtual int degree()
	{
		return a.Length - 1;
	}
	public virtual int degree(Variable v)
	{
		if (v.Equals(@var))
		{
			return a.Length - 1;
		}
		int degree = 0;
		for (int i = 0; i < a.Length; i++)
		{
			int d = Poly.degree(a[i], v);
			if (d > degree)
			{
				degree = d;
			}
		}
		return degree;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic add(Algebraic p) throws JasymcaException
	public override Algebraic add(Algebraic p)
	{
		if (p is Rational)
		{
			return p.add(this);
		}
		if (p is Polynomial)
		{
			if (@var.Equals(((Polynomial)p).@var))
			{
				int len = Math.Max(a.Length, ((Polynomial)p).a.Length);
				Algebraic[] csum = new Algebraic[len];
				for (int i = 0; i < len; i++)
				{
					csum[i] = coefficient(i).add(((Polynomial)p).coefficient(i));
				}
				return (new Polynomial(@var, csum)).reduce();
			}
			else if (@var.smaller(((Polynomial)p).@var))
			{
				return p.add(this);
			}
		}
		Algebraic[] csum = Poly.clone(a);
		csum[0] = a[0].add(p);
		return (new Polynomial(@var, csum)).reduce();
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic mult(Algebraic p) throws JasymcaException
	public override Algebraic mult(Algebraic p)
	{
		if (p is Rational)
		{
			return p.mult(this);
		}
		if (p is Polynomial)
		{
			if (@var.Equals(((Polynomial)p).@var))
			{
				int len = a.Length + ((Polynomial)p).a.Length - 1;
				Algebraic[] cprod = new Algebraic[len];
				for (int i = 0; i < len; i++)
				{
					cprod[i] = Zahl.ZERO;
				}
				for (int i = 0; i < a.Length; i++)
				{
					for (int k = 0; k < ((Polynomial)p).a.Length; k++)
					{
						cprod[i + k] = cprod[i + k].add(a[i].mult(((Polynomial)p).a[k]));
					}
				}
					return (new Polynomial(@var, cprod)).reduce();
			}
			else if (@var.smaller(((Polynomial)p).@var))
			{
				return p.mult(this);
			}
		}
		Algebraic[] cprod = new Algebraic[a.Length];
		for (int i = 0; i < a.Length; i++)
		{
			cprod[i] = a[i].mult(p);
		}
		return (new Polynomial(@var, cprod)).reduce();
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic div(Algebraic q) throws JasymcaException
	public override Algebraic div(Algebraic q)
	{
		if (q is Zahl)
		{
			Algebraic[] c = new Algebraic[a.Length];
			for (int i = 0; i < a.Length; i++)
			{
				c[i] = a[i].div(q);
			}
			return new Polynomial(@var, c);
		}
		return base.div(q);
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic reduce() throws JasymcaException
	public override Algebraic reduce()
	{
		if (a.Length == 0)
		{
			return Zahl.ZERO;
		}
		if (a.Length == 1)
		{
			return a[0].reduce();
		}
		return this;
	}
	public override string ToString()
	{
		ArrayList x = new ArrayList();
		for (int i = a.Length - 1; i > 0;i--)
		{
			if (a[i].Equals(Zahl.ZERO))
			{
				continue;
			}
			string s = "";
			if (a[i].Equals(Zahl.MINUS))
			{
				s += "-";
			}
			else if (!a[i].Equals(Zahl.ONE))
			{
				s += a[i].ToString() + "*";
			}
			s += @var.ToString();
			if (i > 1)
			{
				s += "^" + i;
			}
			x.Add(s);
		}
		if (!a[0].Equals(Zahl.ZERO))
		{
			x.Add(a[0].ToString());
		}
		string s = "";
		if (x.Count > 1)
		{
			s += "(";
		}
		for (int i = 0; i < x.Count; i++)
		{
			s += (string)x[i];
			if (i < x.Count - 1 && !(((string)x[i + 1])[0] == '-'))
			{
				s += "+";
			}
		}
		if (x.Count > 1)
		{
			s += ")";
		}
		return s;
	}
	public override bool Equals(object x)
	{
		if (!(x is Polynomial))
		{
			return false;
		}
		if (!(@var.Equals(((Polynomial)x).@var)) || a.Length != ((Polynomial)x).a.Length)
		{
			return false;
		}
		for (int i = 0; i < a.Length; i++)
		{
			if (!a[i].Equals(((Polynomial)x).a[i]))
			{
				return false;
			}
		}
			return true;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic deriv(Variable var) throws JasymcaException
	public override Algebraic deriv(Variable @var)
	{
		Algebraic r1 = Zahl.ZERO, r2 = Zahl.ZERO;
		Polynomial x = new Polynomial(this.@var);
		for (int i = a.Length - 1; i > 1; i--)
		{
			r1 = r1.add(a[i].mult(new Unexakt(i))).mult(x);
		}
		if (a.Length > 1)
		{
			r1 = r1.add(a[1]);
		}
		for (int i = a.Length - 1; i > 0; i--)
		{
			r2 = r2.add(a[i].deriv(@var)).mult(x);
		}
		if (a.Length > 0)
		{
			r2 = r2.add(a[0].deriv(@var));
		}
		return r1.mult(this.@var.deriv(@var)).add(r2).reduce();
	}
	public override bool depends(Variable @var)
	{
		if (a.Length == 0)
		{
			return false;
		}
		if (this.@var.Equals(@var))
		{
			return true;
		}
		if (this.@var is FunctionVariable && ((FunctionVariable)this.@var).arg.depends(@var))
		{
			return true;
		}
		for (int i = 0; i < a.Length; i++)
		{
			if (a[i].depends(@var))
			{
				return true;
			}
		}
			return false;
	}
	internal static bool loopPartial = false;
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic integrate(Variable var) throws JasymcaException
	public override Algebraic integrate(Variable @var)
	{
		Algebraic @in = Zahl.ZERO;
		for (int i = 1; i < a.Length; i++)
		{
			if (!a[i].depends(@var))
			{
				if (@var.Equals(this.@var))
				{
					@in = @in.add(a[i].mult((new Polynomial(@var)).pow_n(i + 1).div(new Unexakt(i + 1))));
				}
				else if (this.@var is FunctionVariable && ((FunctionVariable)this.@var).arg.depends(@var))
				{
					if (i == 1)
					{
						@in = @in.add(((FunctionVariable)this.@var).integrate(@var).mult(a[1]));
					}
					else
					{
						throw new JasymcaException("Integral not supported.");
					}
				}
					else
					{
						@in = @in.add(a[i].mult((new Polynomial(@var)).mult((new Polynomial(this.@var)).pow_n(i))));
					}
			}
					else if (@var.Equals(this.@var))
					{
						throw new JasymcaException("Integral not supported.");
					}
					else if (this.@var is FunctionVariable && ((FunctionVariable)this.@var).arg.depends(@var))
					{
						if (i == 1 && a[i] is Polynomial && ((Polynomial)a[i]).@var.Equals(@var))
						{
							p("Trying to isolate inner derivative " + this);
							try
							{
								FunctionVariable f = (FunctionVariable)this.@var;
								Algebraic w = f.arg;
								Algebraic q = a[i].div(w.deriv(@var));
								if (q.deriv(@var).Equals(Zahl.ZERO))
								{
									SimpleVariable v = new SimpleVariable("v");
									Algebraic p = FunctionVariable.create(f.fname, new Polynomial(v));
									Algebraic r = p.integrate(v).value(v,w).mult(q);
									@in = @in.add(r);
									continue;
								}
							}
							catch (JasymcaException)
							{
							}
							p("Failed.");
							for (int k = 0;k < ((Polynomial)a[i]).a.Length;k++)
							{
								if (((Polynomial)a[i]).a[k].depends(@var))
								{
									throw new JasymcaException("Function not supported by this method");
								}
							}
								if (loopPartial)
								{
									loopPartial = false;
									p("Partial Integration Loop detected.");
									throw new JasymcaException("Partial Integration Loop: " + this);
								}
								p("Trying partial integration: x^n*f(x) , n-times diff " + this);
								try
								{
									loopPartial = true;
									Algebraic p = a[i];
									Algebraic f = ((FunctionVariable)this.@var).integrate(@var);
									Algebraic r = f.mult(p);
									while (!(p = p.deriv(@var)).Equals(Zahl.ZERO))
									{
										f = f.integrate(@var).mult(Zahl.MINUS);
										r = r.add(f.mult(p));
									}
									loopPartial = false;
									@in = @in.add(r);
									continue;
								}
								catch (JasymcaException)
								{
									loopPartial = false;
								}
								p("Failed.");
								p("Trying partial integration: x^n*f(x) , 1-times int " + this);
								try
								{
									loopPartial = true;
									Algebraic p = a[i].integrate(@var);
									Algebraic f = new Polynomial((FunctionVariable)this.@var);
									Algebraic r = p.mult(f).sub(p.mult(f.deriv(@var)).integrate(@var));
									loopPartial = false;
									@in = @in.add(r);
									continue;
								}
								catch (JasymcaException)
								{
									loopPartial = false;
								}
								p("Failed");
								throw new JasymcaException("Function not supported by this method");
						}
						else
						{
							throw new JasymcaException("Integral not supported.");
						}
					}
					else
					{
						@in = @in.add(a[i].integrate(@var).mult((new Polynomial(this.@var)).pow_n(i)));
					}
		}
		if (a.Length > 0)
		{
			@in = @in.add(a[0].integrate(@var));
		}
		return @in;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic cc() throws JasymcaException
	public override Algebraic cc()
	{
		Polynomial xn = new Polynomial(@var.cc());
		Algebraic r = Zahl.ZERO;
		for (int i = a.Length - 1; i > 0; i--)
		{
			r = r.add(a[i].cc()).mult(xn);
		}
		if (a.Length > 0)
		{
			r = r.add(a[0].cc());
		}
		return r;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic value(Variable var, Algebraic x) throws JasymcaException
	public override Algebraic value(Variable @var, Algebraic x)
	{
		Algebraic r = Zahl.ZERO;
		Algebraic v = this.@var.value(@var,x);
		for (int i = a.Length - 1; i > 0; i--)
		{
			r = r.add(a[i].value(@var,x)).mult(v);
		}
		if (a.Length > 0)
		{
			r = r.add(a[0].value(@var,x));
		}
		return r;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic value(Algebraic x) throws JasymcaException
	public virtual Algebraic value(Algebraic x)
	{
		return value(this.@var, x);
	}
	public override bool exaktq()
	{
		bool exakt = a[0].exaktq();
		for (int i = 1; i < a.Length; i++)
		{
			exakt = exakt && a[i].exaktq();
		}
		return exakt;
	}
	public override double norm()
	{
		double norm = 0.0;
		for (int i = 0; i < a.Length; i++)
		{
			norm += a[i].norm();
		}
		return norm;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic map(LambdaAlgebraic f) throws JasymcaException
	public override Algebraic map(LambdaAlgebraic f)
	{
		Algebraic x = @var is SimpleVariable ? new Polynomial(@var): FunctionVariable.create(((FunctionVariable)@var).fname, f.f_exakt(((FunctionVariable)@var).arg));
		Algebraic r = Zahl.ZERO;
		for (int i = a.Length - 1; i > 0; i--)
		{
			r = r.add(f.f_exakt(a[i])).mult(x);
		}
		if (a.Length > 0)
		{
			r = r.add(f.f_exakt(a[0]));
		}
		return r;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Polynomial monic() throws JasymcaException
	public virtual Polynomial monic()
	{
		Algebraic cm = a[a.Length - 1];
		if (cm.Equals(Zahl.ONE))
		{
			return this;
		}
		if (cm.Equals(Zahl.ZERO) || (cm.depends(@var)))
		{
			throw new JasymcaException("Ill conditioned polynomial: main coefficient Zero or not number");
		}
		Algebraic[] b = new Algebraic[a.Length];
		b[a.Length - 1] = Zahl.ONE;
		for (int i = 0; i < a.Length - 1; i++)
		{
			b[i] = a[i].div(cm);
		}
		return new Polynomial(@var, b);
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic[] square_free_dec(Variable var) throws JasymcaException
	public virtual Algebraic[] square_free_dec(Variable @var)
	{
		if (!ratfunc(@var))
		{
			return null;
		}
		Algebraic dp = deriv(@var);
		Algebraic gcd_pdp = Poly.poly_gcd(this,dp);
		Algebraic q = Poly.polydiv(this,gcd_pdp);
		Algebraic p1 = Poly.polydiv(q, Poly.poly_gcd(q,gcd_pdp));
		if (gcd_pdp is Polynomial && gcd_pdp.depends(@var) && ((Polynomial)gcd_pdp).ratfunc(@var))
		{
			Algebraic[] sq = ((Polynomial)gcd_pdp).square_free_dec(@var);
			Algebraic[] result = new Algebraic[sq.Length + 1];
			result[0] = p1;
			for (int i = 0; i < sq.Length;i++)
			{
				result[i + 1] = sq[i];
			}
			return result;
		}
		else
		{
			Algebraic[] result = new Algebraic[] {p1};
			return result;
		}
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Zahl gcd_coeff() throws JasymcaException
	public virtual Zahl gcd_coeff()
	{
		Zahl gcd;
		if (a[0] is Zahl)
		{
			gcd = (Zahl)a[0];
		}
		else if (a[0] is Polynomial)
		{
			gcd = ((Polynomial)a[0]).gcd_coeff();
		}
		else
		{
			throw new JasymcaException("Cannot calculate gcd from " + this);
		}
		for (int i = 1; i < a.Length; i++)
		{
			if (a[i] is Zahl)
			{
				gcd = gcd.gcd((Zahl)a[i]);
			}
			else if (a[i] is Polynomial)
			{
				gcd = gcd.gcd(((Polynomial)a[i]).gcd_coeff());
			}
			else
			{
				throw new JasymcaException("Cannot calculate gcd from " + this);
			}
		}
		return gcd;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Vektor solve(Variable var) throws JasymcaException
	public virtual Vektor solve(Variable @var)
	{
		if (!@var.Equals(this.@var))
		{
			return ((Polynomial)value(@var, Poly.top)).solve(SimpleVariable.top);
		}
		Algebraic[] factors = square_free_dec(@var);
		ArrayList s = new ArrayList();
		int n = factors == null?0:factors.Length;
		for (int i = 0; i < n; i++)
		{
			if (factors[i] is Polynomial)
			{
				Vektor sol = null;
				Algebraic equ = factors[i];
				try
				{
					sol = ((Polynomial)equ).solvepoly();
				}
				catch (JasymcaException)
				{
					sol = ((Polynomial)equ).monic().roots();
				}
				for (int k = 0; k < sol.length(); k++)
				{
					s.Add(sol.get(k));
				}
			}
		}
		Algebraic[] cn = new Algebraic[s.Count];
		for (int i = 0; i < cn.Length; i++)
		{
			cn[i] = (Algebraic)s[i];
		}
		return new Vektor(cn);
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Vektor solvepoly() throws JasymcaException
	public virtual Vektor solvepoly()
	{
		ArrayList s = new ArrayList();
		switch (degree())
		{
			case 0:
				break;
			case 1:
				s.Add(Zahl.MINUS.mult(a[0].div(a[1])));
				break;
			case 2:
				Algebraic p = a[1].div(a[2]);
				Algebraic q = a[0].div(a[2]);
				p = Zahl.MINUS.mult(p).div(Zahl.TWO);
				q = p.mult(p).sub(q);
				if (q.Equals(Zahl.ZERO))
				{
					s.Add(p);
					break;
				}
				q = FunctionVariable.create("sqrt", q);
				s.Add(p.add(q));
				s.Add(p.sub(q));
				break;
			default:
				int gcd = -1;
				for (int i = 1; i < a.Length; i++)
				{
					if (!a[i].Equals(Zahl.ZERO))
					{
						if (gcd < 0)
						{
							gcd = i;
						}
						else
						{
							gcd = Poly.gcd(i,gcd);
						}
					}
				}
				int deg = degree() / gcd;
				if (deg < 3)
				{
					Algebraic[] cn = new Algebraic[deg + 1];
					for (int i = 0; i < cn.Length; i++)
					{
						cn[i] = a[i * gcd];
					}
					Polynomial pr = new Polynomial(@var, cn);
					Vektor sn = pr.solvepoly();
					if (gcd == 2)
					{
						cn = new Algebraic[sn.length() * 2];
						for (int i = 0; i < sn.length(); i++)
						{
							cn[2 * i] = FunctionVariable.create("sqrt", sn.get(i));
							cn[2 * i + 1] = cn[2 * i].mult(Zahl.MINUS);
						}
					}
					else
					{
						cn = new Algebraic[sn.length()];
						Zahl wx = new Unexakt(1.0 / gcd);
						for (int i = 0; i < sn.length(); i++)
						{
							Algebraic exp = FunctionVariable.create("log",sn.get(i));
							cn[i] = FunctionVariable.create("exp", exp.mult(wx));
						}
					}
					return new Vektor(cn);
				}
				throw new JasymcaException("Can't solve expression " + this);
		}
		return Vektor.create(s);
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Vektor roots() throws JasymcaException
	public virtual Vektor roots()
	{
		if (a.Length == 2)
		{
			Algebraic[] result = new Algebraic[] {a[0].mult(Zahl.MINUS).div(a[1])};
			return new Vektor(result);
		}
		else if (a.Length == 3)
		{
			return new Vektor(Poly.pqsolve(a[1].div(a[2]), a[0].div(a[2])));
		}
		double[] ar = new double[a.Length];
		double[] ai = new double[a.Length];
		bool[] err = new bool[a.Length];
		bool komplex = false;
		for (int i = 0; i < a.Length; i++)
		{
			Algebraic cf = a[i];
			if (!(cf is Zahl))
			{
				throw new JasymcaException("Roots requires constant coefficients.");
			}
			ar[i] = ((Zahl)cf).unexakt().real;
			ai[i] = ((Zahl)cf).unexakt().imag;
			if (ai[i] != 0.0)
			{
				komplex = true;
			}
		}
		if (komplex)
		{
			Pzeros.aberth(ar, ai, err);
		}
		else
		{
			Pzeros.bairstow(ar, ai, err);
			bool ok = true;
			for (int i = 0; i < err.Length - 1; i++)
			{
				if (err[i])
				{
					ok = false;
				}
			}
			if (!ok)
			{
				for (int i = 0; i < a.Length; i++)
				{
					Algebraic cf = a[i];
					ar[i] = ((Zahl)cf).unexakt().real;
					ai[i] = ((Zahl)cf).unexakt().imag;
				}
				Pzeros.aberth(ar, ai, err);
			}
		}
		Algebraic[] r = new Algebraic[a.Length - 1];
		for (int i = 0; i < r.Length; i++)
		{
			if (!err[i])
			{
				Unexakt x0 = new Unexakt(ar[i],ai[i]);
				r[i] = x0;
			}
			else
			{
				throw new JasymcaException("Could not calculate root " + i);
			}
		}
		return new Vektor(r);
	}
}
using System;
using System.Collections;

public class Polynomial : Algebraic
{
	public Algebraic[] a = null;
	public Variable v = null;
	public Polynomial()
	{
	}
	public Polynomial(Variable v, Algebraic[] a)
	{
		this.v = v;
		this.a = Poly.reduce(a);
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Polynomial(Variable var, Vektor v) throws JasymcaException
	public Polynomial(Variable @var, Vektor v)
	{
		this.v = @var;
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
		this.v = @var;
	}
	public virtual Variable Var
	{
		get
		{
			return v;
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
		if (@var.Equals(this.v))
		{
			return coefficient(n);
		}
		Algebraic c = Zahl.ZERO;
		for (int i = 0; i < a.Length; i++)
		{
			Algebraic ci = a[i];
			if (ci is Polynomial)
			{
				c = c.add(((Polynomial)ci).coefficient(@var,n).mult((new Polynomial(this.v)).pow_n(i)));
			}
			else if (n == 0)
			{
				c = c.add(ci.mult((new Polynomial(this.v)).pow_n(i)));
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
		if (v is FunctionVariable && ((FunctionVariable)this.v).arg.depends(v))
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
		if (v.Equals(v))
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
			if (v.Equals(((Polynomial)p).v))
			{
				int len = Math.Max(a.Length, ((Polynomial)p).a.Length);
				Algebraic[] csum = new Algebraic[len];
				for (int i = 0; i < len; i++)
				{
					csum[i] = coefficient(i).add(((Polynomial)p).coefficient(i));
				}
				return (new Polynomial(v, csum)).reduce();
			}
			else if (v.smaller(((Polynomial)p).v))
			{
				return p.add(this);
			}
		}
		Algebraic[] _csum = Poly.clone(a);
		_csum[0] = a[0].add(p);
		return (new Polynomial(v, _csum)).reduce();
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
			if (v.Equals(((Polynomial)p).v))
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
					return (new Polynomial(v, cprod)).reduce();
			}
			else if (v.smaller(((Polynomial)p).v))
			{
				return p.mult(this);
			}
		}
		Algebraic[] _cprod = new Algebraic[a.Length];
		for (int i = 0; i < a.Length; i++)
		{
			_cprod[i] = a[i].mult(p);
		}
		return (new Polynomial(v, _cprod)).reduce();
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
			return new Polynomial(v, c);
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
			s += v.ToString();
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
		string _s = "";
		if (x.Count > 1)
		{
			_s += "(";
		}
		for (int i = 0; i < x.Count; i++)
		{
			_s += (string)x[i];
			if (i < x.Count - 1 && !(((string)x[i + 1])[0] == '-'))
			{
				_s += "+";
			}
		}
		if (x.Count > 1)
		{
			_s += ")";
		}
		return _s;
	}
	public override bool Equals(object x)
	{
		if (!(x is Polynomial))
		{
			return false;
		}
		if (!(v.Equals(((Polynomial)x).v)) || a.Length != ((Polynomial)x).a.Length)
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
		Polynomial x = new Polynomial(this.v);
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
		return r1.mult(this.v.deriv(@var)).add(r2).reduce();
	}
	public override bool depends(Variable @var)
	{
		if (a.Length == 0)
		{
			return false;
		}
		if (this.v.Equals(@var))
		{
			return true;
		}
		if (this.v is FunctionVariable && ((FunctionVariable)this.v).arg.depends(@var))
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
	public override Algebraic integrate(Variable item)
	{
		Algebraic tmp = Zahl.ZERO;
		for (int i = 1; i < a.Length; i++)
		{
			if (!a[i].depends(item))
			{
				if (item.Equals(this.v))
				{
					tmp = tmp.add(a[i].mult((new Polynomial(item)).pow_n(i + 1).div(new Unexakt(i + 1))));
				}
				else if (this.v is FunctionVariable && ((FunctionVariable)this.v).arg.depends(item))
				{
					if (i == 1)
					{
						tmp = tmp.add(((FunctionVariable)this.v).integrate(item).mult(a[1]));
					}
					else
					{
						throw new JasymcaException("Integral not supported.");
					}
				}
					else
					{
						tmp = tmp.add(a[i].mult((new Polynomial(item)).mult((new Polynomial(this.v)).pow_n(i))));
					}
			}
					else if (item.Equals(this.v))
					{
						throw new JasymcaException("Integral not supported.");
					}
					else if (this.v is FunctionVariable && ((FunctionVariable)this.v).arg.depends(item))
					{
						if (i == 1 && a[i] is Polynomial && ((Polynomial)a[i]).v.Equals(item))
						{
							debug("Trying to isolate inner derivative " + this);
							try
							{
								FunctionVariable f = (FunctionVariable)this.v;
								Algebraic w = f.arg;
								Algebraic q = a[i].div(w.deriv(item));
								if (q.deriv(item).Equals(Zahl.ZERO))
								{
									SimpleVariable v = new SimpleVariable("v");
									Algebraic p = FunctionVariable.create(f.fname, new Polynomial(v));
									Algebraic r = p.integrate(v).value(v,w).mult(q);
									tmp = tmp.add(r);
									continue;
								}
							}
							catch (JasymcaException)
							{
							}
							debug("Failed.");
							for (int k = 0;k < ((Polynomial)a[i]).a.Length;k++)
							{
								if (((Polynomial)a[i]).a[k].depends(item))
								{
									throw new JasymcaException("Function not supported by this method");
								}
							}
								if (loopPartial)
								{
									loopPartial = false;
									debug("Partial Integration Loop detected.");
									throw new JasymcaException("Partial Integration Loop: " + this);
								}
								debug("Trying partial integration: x^n*f(x) , n-times diff " + this);
								try
								{
									loopPartial = true;
									Algebraic _p = a[i];
									Algebraic f = ((FunctionVariable)this.v).integrate(item);
									Algebraic r = f.mult(_p);
									while (!(_p = _p.deriv(item)).Equals(Zahl.ZERO))
									{
										f = f.integrate(item).mult(Zahl.MINUS);
										r = r.add(f.mult(_p));
									}
									loopPartial = false;
									tmp = tmp.add(r);
									continue;
								}
								catch (JasymcaException)
								{
									loopPartial = false;
								}
								debug("Failed.");
								debug("Trying partial integration: x^n*f(x) , 1-times int " + this);
								try
								{
									loopPartial = true;
									Algebraic p1 = a[i].integrate(item);
									Algebraic f = new Polynomial((FunctionVariable)this.v);
									Algebraic r = p1.mult(f).sub(p1.mult(f.deriv(item)).integrate(item));
									loopPartial = false;
									tmp = tmp.add(r);
									continue;
								}
								catch (JasymcaException)
								{
									loopPartial = false;
								}
								debug("Failed");
								throw new JasymcaException("Function not supported by this method");
						}
						else
						{
							throw new JasymcaException("Integral not supported.");
						}
					}
					else
					{
						tmp = tmp.add(a[i].integrate(item).mult((new Polynomial(this.v)).pow_n(i)));
					}
		}
		if (a.Length > 0)
		{
			tmp = tmp.add(a[0].integrate(item));
		}
		return tmp;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic cc() throws JasymcaException
	public override Algebraic cc()
	{
		Polynomial xn = new Polynomial(v.cc());
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
		Algebraic v = this.v.value(@var,x);
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
		return value(this.v, x);
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
		Algebraic x = v is SimpleVariable ? new Polynomial(v): FunctionVariable.create(((FunctionVariable)v).fname, f.f_exakt(((FunctionVariable)v).arg));
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
		if (cm.Equals(Zahl.ZERO) || (cm.depends(v)))
		{
			throw new JasymcaException("Ill conditioned polynomial: main coefficient Zero or not number");
		}
		Algebraic[] b = new Algebraic[a.Length];
		b[a.Length - 1] = Zahl.ONE;
		for (int i = 0; i < a.Length - 1; i++)
		{
			b[i] = a[i].div(cm);
		}
		return new Polynomial(v, b);
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
		if (!@var.Equals(this.v))
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
					Polynomial pr = new Polynomial(v, cn);
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
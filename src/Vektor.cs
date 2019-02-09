using System.Collections;

public class Vektor : Algebraic
{
	private Algebraic[] a;
	public Vektor(Algebraic[] a)
	{
		this.a = a;
	}
	public Vektor(Algebraic x, int n)
	{
		this.a = new Zahl[n];
		for (int i = 0; i < n; i++)
		{
			this.a[i] = x;
		}
	}
	public Vektor(int n) : this(Zahl.ZERO, n)
	{
	}
	public Vektor(Algebraic c)
	{
		if (c is Vektor)
		{
			a = Poly.clone(((Vektor)c).a);
		}
		else
		{
			a = new Algebraic[] {c};
		}
	}
	public Vektor(double[] x)
	{
		a = new Algebraic[x.Length];
		for (int i = 0; i < x.Length; i++)
		{
			a[i] = new Unexakt(x[i]);
		}
	}
	public Vektor(double[] r, double[] i)
	{
		a = new Algebraic[r.Length];
		for (int k = 0; k < r.Length; k++)
		{
			a[k] = new Unexakt(r[k], i[k]);
		}
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static Vektor create(java.util.Vector v) throws JasymcaException
	public static Vektor create(ArrayList v)
	{
		Algebraic[] a = new Algebraic[v.Count];
		for (int i = 0; i < a.Length; i++)
		{
			object x = v[i];
			if (!(x is Algebraic))
			{
				throw new JasymcaException("Error creating Vektor.");
			}
			a[i] = (Algebraic)x;
		}
		return new Vektor(a);
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic get(int i) throws JasymcaException
	public virtual Algebraic get(int i)
	{
		if (i < 0 || i >= a.Length)
		{
			throw new JasymcaException("Index out of bounds.");
		}
		return a[i];
	}
	public virtual Algebraic[] get()
	{
		return a;
	}
	public virtual ArrayList vector()
	{
		ArrayList r = new ArrayList(a.Length);
		for (int i = 0; i < a.Length; i++)
		{
			r.Add(a[i]);
		}
		return r;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public double[] getDouble() throws JasymcaException
	public virtual double[] Double
	{
		get
		{
			double[] x = new double[a.Length];
			for (int i = 0; i < a.Length; i++)
			{
				Algebraic c = a[i];
				if (!(c is Zahl))
				{
					throw new JasymcaException("Vector element not constant:" + c);
				}
				x[i] = ((Zahl)c).unexakt().real;
			}
			return x;
		}
	}
	public virtual Vektor reverse()
	{
		Algebraic[] b = new Algebraic[a.Length];
		for (int i = 0; i < a.Length; i++)
		{
			b[i] = a[a.Length - i - 1];
		}
		return new Vektor(b);
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void set(int i, Algebraic x) throws JasymcaException
	public virtual void set(int i, Algebraic x)
	{
		if (i < 0 || i >= a.Length)
		{
			throw new JasymcaException("Index out of bounds.");
		}
		a[i] = x;
	}
	public virtual int length()
	{
		return a.Length;
	}
	public override bool scalarq()
	{
		return false;
	}
	public override bool constantq()
	{
		for (int i = 0; i < a.Length; i++)
		{
			if (!a[i].constantq())
			{
				return false;
			}
		}
			return true;
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
	public override Algebraic reduce()
	{
		if (a.Length == 1)
		{
			return a[0];
		}
		return this;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic cc() throws JasymcaException
	public override Algebraic cc()
	{
		Algebraic[] b = new Algebraic[a.Length];
		for (int i = 0; i < a.Length; i++)
		{
			b[i] = a[i].cc();
		}
		return new Vektor(b);
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic map(LambdaAlgebraic f) throws JasymcaException
	public override Algebraic map(LambdaAlgebraic f)
	{
		Algebraic[] cn = new Algebraic[a.Length];
		for (int i = 0; i < a.Length; i++)
		{
			cn[i] = f.f_exakt(a[i]);
		}
		return new Vektor(cn);
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic map_lambda(LambdaAlgebraic f, Algebraic arg2) throws ParseException,JasymcaException
	public override Algebraic map(LambdaAlgebraic f, Algebraic arg2)
	{
		Algebraic[] b = new Algebraic[a.Length];
		if (arg2 is Vektor && ((Vektor)arg2).length() == a.Length)
		{
			for (int i = 0; i < b.Length; i++)
			{
				Algebraic c = ((Vektor)arg2).get(i);
				object r = a[i].map(f, c);
				if (r is Algebraic)
				{
					b[i] = (Algebraic)r;
				}
				else
				{
					throw new JasymcaException("Cannot evaluate function to algebraic.");
				}
			}
		}
		else
		{
			for (int i = 0; i < b.Length; i++)
			{
				object r = a[i].map(f, arg2);
				if (r is Algebraic)
				{
					b[i] = (Algebraic)r;
				}
				else
				{
					throw new JasymcaException("Cannot evaluate function to algebraic.");
				}
			}
		}
		return new Vektor(b);
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic value(Variable var, Algebraic x) throws JasymcaException
	public override Algebraic value(Variable @var, Algebraic x)
	{
		Algebraic[] b = new Algebraic[a.Length];
		for (int i = 0; i < b.Length; i++)
		{
			b[i] = a[i].value(@var,x);
		}
		return new Vektor(b);
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic add(Algebraic x) throws JasymcaException
	public override Algebraic add(Algebraic x)
	{
		if (x.scalarq())
		{
			x = x.promote(this);
		}
		if (x is Vektor && ((Vektor)x).length() == a.Length)
		{
			Algebraic[] b = new Algebraic[a.Length];
			for (int i = 0; i < a.Length; i++)
			{
				b[i] = a[i].add(((Vektor)x).a[i]);
			}
			return new Vektor(b);
		}
		throw new JasymcaException("Wrong Vektor dimension.");
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic mult(Algebraic x) throws JasymcaException
	public override Algebraic mult(Algebraic x)
	{
		if (x.scalarq())
		{
			Algebraic[] b = new Algebraic[a.Length];
			for (int i = 0; i < a.Length; i++)
			{
				b[i] = x.mult(a[i]);
			}
			return new Vektor(b);
		}
		if (x is Vektor && ((Vektor)x).length() == a.Length)
		{
			Algebraic r = Zahl.ZERO;
			for (int i = 0; i < a.Length; i++)
			{
				r = r.add(a[i].mult(((Vektor)x).a[i]));
			}
			return r;
		}
		throw new JasymcaException("Wrong Vektor dimension.");
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic div(Algebraic x) throws JasymcaException
	public override Algebraic div(Algebraic x)
	{
		if (x.scalarq())
		{
			Algebraic[] b = new Algebraic[a.Length];
			for (int i = 0; i < a.Length; i++)
			{
				b[i] = a[i].div(x);
			}
			return new Vektor(b);
		}
		throw new JasymcaException("Divide not implemented for vektors");
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic deriv(Variable var) throws JasymcaException
	public override Algebraic deriv(Variable @var)
	{
		Algebraic[] nc = new Algebraic[a.Length];
		for (int i = 0; i < a.Length; i++)
		{
			nc[i] = a[i].deriv(@var);
		}
		return new Vektor(nc);
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic integrate(Variable var) throws JasymcaException
	public override Algebraic integrate(Variable @var)
	{
		Algebraic[] nc = new Algebraic[a.Length];
		for (int i = 0; i < a.Length; i++)
		{
			nc[i] = a[i].integrate(@var);
		}
		return new Vektor(nc);
	}
	public override double norm()
	{
		double r = 0.0;
		for (int i = 0; i < a.Length; i++)
		{
			r += a[i].norm();
		}
		return r;
	}
	public override bool Equals(object x)
	{
		if (!(x is Vektor) || ((Vektor)x).a.Length != a.Length)
		{
			return false;
		}
		for (int i = 0; i < a.Length; i++)
		{
			if (!a[i].Equals(((Vektor)x).a[i]))
			{
				return false;
			}
		}
			return true;
	}
	public override string ToString()
	{
		string r = "[ ";
		for (int i = 0; i < a.Length; i++)
		{
			r += StringFmt.compact(a[i].ToString());
			if (i < a.Length - 1)
			{
				r += "  ";
			}
		}
		return r + " ]";
	}
	public override void print(PrintStream p)
	{
		p.print("[ ");
		for (int i = 0; i < a.Length; i++)
		{
			string r = StringFmt.compact(a[i].ToString());
			if (i < a.Length - 1)
			{
				r += "  ";
			}
			p.print(r);
		}
		p.print(" ]");
	}
	public override bool depends(Variable @var)
	{
		for (int i = 0; i < a.Length; i++)
		{
			if (a[i].depends(@var))
			{
				return true;
			}
		}
			return false;
	}
}
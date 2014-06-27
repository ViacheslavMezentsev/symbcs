using System;

public abstract class Algebraic
{
	internal string name = null;
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract Algebraic add(Algebraic x) throws JasymcaException;
	public abstract Algebraic add(Algebraic x);
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic sub(Algebraic x) throws JasymcaException
	public virtual Algebraic sub(Algebraic x)
	{
		return add(x.mult(Zahl.MINUS));
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract Algebraic mult(Algebraic x) throws JasymcaException;
	public abstract Algebraic mult(Algebraic x);
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic div(Algebraic x) throws JasymcaException
	public virtual Algebraic div(Algebraic x)
	{
		if (x is Polynomial)
		{
			return (new Rational(this, (Polynomial)x)).reduce();
		}
		if (x is Rational)
		{
			return ((Rational)x).den.mult(this).div(((Rational)x).nom);
		}
		if (!x.scalarq())
		{
			return (new Matrix(this)).div(x);
		}
		throw new JasymcaException("Can not divide " + this + " through " + x);
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic pow_n(int n) throws JasymcaException
	public virtual Algebraic pow_n(int n)
	{
		Algebraic pow , x = this;
		if (n <= 0)
		{
			if (n == 0 || Equals(Zahl.ONE))
			{
				return Zahl.ONE;
			}
			if (Equals(Zahl.ZERO))
			{
				throw new JasymcaException("Division by Zero.");
			}
			x = Zahl.ONE.div(x);
			n = -n;
		}
		for (pow = Zahl.ONE; ;)
		{
			if ((n & 1) != 0)
			{
				pow = pow.mult(x);
			}
			if ((n >>= 1) != 0)
			{
				x = x.mult(x);
			}
			else
			{
				break;
			}
		}
		return pow;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract Algebraic cc() throws JasymcaException;
	public abstract Algebraic cc();
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic realpart() throws JasymcaException
	public virtual Algebraic realpart()
	{
		return add(cc()).div(Zahl.TWO);
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic imagpart() throws JasymcaException
	public virtual Algebraic imagpart()
	{
		return sub(cc()).div(Zahl.TWO).div(Zahl.IONE);
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract Algebraic deriv(Variable var) throws JasymcaException;
	public abstract Algebraic deriv(Variable @var);
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract Algebraic integrate(Variable var) throws JasymcaException;
	public abstract Algebraic integrate(Variable @var);
	public abstract double norm();
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract Algebraic map(LambdaAlgebraic f) throws JasymcaException;
	public abstract Algebraic map(LambdaAlgebraic f);
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic rat() throws JasymcaException
	public virtual Algebraic rat()
	{
		return map(new LambdaRAT());
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic reduce() throws JasymcaException
	public virtual Algebraic reduce()
	{
		return this;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic value(Variable var, Algebraic x) throws JasymcaException
	public virtual Algebraic value(Variable @var, Algebraic x)
	{
		return this;
	}
	public virtual bool depends(Variable @var)
	{
		return false;
	}
	public virtual bool ratfunc(Variable v)
	{
		return true;
	}
	public virtual bool depdir(Variable @var)
	{
		return depends(@var) && ratfunc(@var);
	}
	public virtual bool constantq()
	{
		return false;
	}
	public override abstract bool Equals(object x);
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public boolean komplexq() throws JasymcaException
	public virtual bool komplexq()
	{
		return !imagpart().Equals(Zahl.ZERO);
	}
	public virtual bool scalarq()
	{
		return true;
	}
	public virtual bool exaktq()
	{
		return false;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic promote(Algebraic b) throws JasymcaException
	public virtual Algebraic promote(Algebraic b)
	{
		if (b.scalarq())
		{
			return this;
		}
		if (b is Vektor)
		{
			Vektor bv = (Vektor)b;
			if (this is Vektor && ((Vektor)this).length() == bv.length())
			{
				return this;
			}
			if (scalarq())
			{
				return new Vektor(this, bv.length());
			}
		}
		if (b is Matrix)
		{
			Matrix bm = (Matrix)b;
			if (this is Matrix && bm.equalsized((Matrix)this))
			{
				return this;
			}
			if (scalarq())
			{
				return new Matrix(this,bm.nrow(), bm.ncol());
			}
		}
		throw new JasymcaException("Wrong argument type.");
	}
	public virtual void print(PrintStream p)
	{
		p.print(StringFmt.compact(ToString()));
	}
	internal static void p(string s)
	{
		Lambda.p(s);
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic map_lambda(LambdaAlgebraic lambda, Algebraic arg2) throws ParseException,JasymcaException
	public virtual Algebraic map_lambda(LambdaAlgebraic lambda, Algebraic arg2)
	{
		if (arg2 == null)
		{
			Algebraic r = lambda.f_exakt(this);
			if (r != null)
			{
				return r;
			}
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			string fname = lambda.GetType().FullName;
			if (fname.StartsWith("Lambda", StringComparison.Ordinal))
			{
				fname = fname.Substring("Lambda".Length);
				fname = fname.ToLower();
				return FunctionVariable.create(fname, this);
			}
			throw new JasymcaException("Wrong type of arguments.");
		}
		else
		{
			return lambda.f_exakt(this, arg2);
		}
	}
}
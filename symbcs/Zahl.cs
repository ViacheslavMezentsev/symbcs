using System;

public abstract class Zahl : Algebraic
{
	public static Zahl ZERO = new Unexakt(0.0);
	public static Zahl HALF = new Unexakt(0.5);
	public static Zahl ONE = new Unexakt(1.0);
	public static Zahl TWO = new Unexakt(2.0);
	public static Zahl THREE = new Unexakt(3.0);
	public static Zahl MINUS = new Unexakt(-1.0);
	public static Zahl IONE = new Unexakt(0.0,1.0);
	public static Zahl IMINUS = new Unexakt(0.0,-1.0);
	public static Polynomial PI = new Polynomial(new Constant("pi", Math.PI));
	public static Algebraic SQRT2 = new Polynomial(new Root(new Vektor(new Algebraic[]{new Unexakt(-2.0), ZERO, ONE}),0));
	public static Algebraic SQRT3 = new Polynomial(new Root(new Vektor(new Algebraic[]{new Unexakt(-3.0), ZERO, ONE}),0));
	public override bool constantq()
	{
		return true;
	}
	public override Algebraic deriv(Variable @var)
	{
		return ZERO;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic integrate(Variable var) throws JasymcaException
	public override Algebraic integrate(Variable @var)
	{
		if (this.Equals(Zahl.ZERO))
		{
			return this;
		}
		return (new Polynomial(@var)).mult(this);
	}
	public abstract int intval();
	public abstract bool imagq();
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic value(Variable var, Algebraic x) throws JasymcaException
	public override Algebraic value(Variable @var, Algebraic x)
	{
		return this;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic cc() throws JasymcaException
	public override Algebraic cc()
	{
		return realpart().add(imagpart().mult(Zahl.IMINUS));
	}
	public abstract Zahl abs();
	public virtual Exakt exakt()
	{
		return this is Exakt? (Exakt)this : new Exakt(((Unexakt)this).real, ((Unexakt)this).imag);
	}
	public virtual Unexakt unexakt()
	{
		return this is Unexakt? (Unexakt)this : ((Exakt)this).tofloat();
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic map(LambdaAlgebraic f) throws JasymcaException
	public override Algebraic map(LambdaAlgebraic f)
	{
		return f.f(this);
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Zahl gcd(Zahl x) throws JasymcaException
	public virtual Zahl gcd(Zahl x)
	{
		return exakt().gcd(x.exakt());
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract boolean smaller(Zahl x) throws JasymcaException;
	public abstract bool smaller(Zahl x);
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic[] div(Algebraic q1, Algebraic[] result) throws JasymcaException
	public virtual Algebraic[] div(Algebraic q1, Algebraic[] result)
	{
		return exakt().div(q1,result);
	}
	public abstract bool integerq();
}
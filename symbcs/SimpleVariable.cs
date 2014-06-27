public class SimpleVariable : Variable
{
	internal string name;
	internal static SimpleVariable top = new SimpleVariable("top");
	public SimpleVariable(string name)
	{
		this.name = name.intern();
	}
	public override Algebraic deriv(Variable x)
	{
		if (Equals(x))
		{
			return Zahl.ONE;
		}
		else
		{
			return Zahl.ZERO;
		}
	}
	public override bool Equals(object x)
	{
		return x is SimpleVariable && ((SimpleVariable)x).name.Equals(name);
	}
	public override string ToString()
	{
		return name;
	}
	public virtual object toPrefix()
	{
		return name;
	};
	public override bool smaller(Variable v)
	{
		if (v == top)
		{
			return true;
		}
		if (this == top)
		{
			return false;
		}
		if (v is Constant)
		{
			return false;
		}
		if (!(v is SimpleVariable))
		{
			return true;
		}
		return name.CompareTo(((SimpleVariable)v).name) < 0;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic value(Variable var, Algebraic x) throws JasymcaException
	public override Algebraic value(Variable @var, Algebraic x)
	{
		if (@var.Equals(this))
		{
			return x;
		}
		else
		{
			return new Polynomial(this);
		}
	}
}
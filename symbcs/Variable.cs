public abstract class Variable
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract Algebraic deriv(Variable x) throws JasymcaException;
	public abstract Algebraic deriv(Variable x);
	public abstract override bool Equals(object x);
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract boolean smaller(Variable v) throws JasymcaException;
	public abstract bool smaller(Variable v);
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract Algebraic value(Variable var, Algebraic x) throws JasymcaException;
	public abstract Algebraic value(Variable @var, Algebraic x);
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Variable cc() throws JasymcaException
	public virtual Variable cc()
	{
		return this;
	}
}
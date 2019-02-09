using System;
using System.Collections;

internal class LambdaLENGTH : Lambda
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		Algebraic x = getAlgebraic(st);
		if (x.scalarq() && !x.constantq())
		{
			throw new JasymcaException("Unknown variable dimension: " + x);
		}
		Matrix m = new Matrix(x);
		st.Push(new Unexakt((double)Math.Max(m.ncol(), m.nrow())));
		return 0;
	}
}
internal class LambdaPROD : Lambda
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		Algebraic x = getAlgebraic(st);
		if (x.scalarq() && !x.constantq())
		{
			throw new JasymcaException("Unknown variable dimension: " + x);
		}
		Matrix mx = new Matrix(x);
		Algebraic s = mx.col(1);
		for (int i = 2; i <= mx.ncol(); i++)
		{
			s = s.mult(mx.col(i));
		}
		st.Push(s);
		return 0;
	}
}
internal class LambdaSIZE : Lambda
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		Algebraic x = getAlgebraic(st);
		if (x.scalarq() && !x.constantq())
		{
			throw new JasymcaException("Unknown variable dimension: " + x);
		}
		Matrix mx = new Matrix(x);
		Unexakt nr = new Unexakt((double)mx.nrow()), nc = new Unexakt((double)mx.ncol());
		if (length == 2)
		{
			st.Push(nr);
			st.Push(nc);
			length = 1;
		}
		else
		{
			st.Push(new Vektor(new Algebraic[]{nr,nc}));
		}
		return 0;
	}
}
internal class LambdaMIN : Lambda
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		Algebraic x = getAlgebraic(st);
		Matrix mx;
		if (x is Vektor)
		{
			mx = Matrix.column((Vektor) x);
		}
		else
		{
			mx = new Matrix(x);
		}
		st.Push(mx.min());
		return 0;
	}
}
internal class LambdaMAX : Lambda
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		Algebraic x = getAlgebraic(st);
		Matrix mx;
		if (x is Vektor)
		{
			mx = Matrix.column((Vektor) x);
		}
		else
		{
			mx = new Matrix(x);
		}
		st.Push(mx.max());
		return 0;
	}
}
internal class LambdaFIND : Lambda
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		Algebraic x = getAlgebraic(st);
		Matrix mx = new Matrix(x);
		st.Push(mx.find());
		return 0;
	}
}
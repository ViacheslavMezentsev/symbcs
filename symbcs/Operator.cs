using System;
using System.Collections;

public class Operator : Constants
{
	internal string mnemonic;
	internal string symbol;
	internal int precedence;
	internal int associativity;
	internal int type;
	internal Lambda func = null;
	internal static Operator[] OPS = new Operator[0];
	public virtual bool unary()
	{
		return (type & Constants_Fields.UNARY) != 0;
	}
	public virtual bool binary()
	{
		return (type & BINARY) != 0;
	}
	public virtual bool ternary()
	{
		return (type & TERNARY) != 0;
	}
	public virtual bool lvalue()
	{
		return (type & LVALUE) != 0;
	}
	public virtual bool list()
	{
		return (type & LIST) != 0;
	}
	public virtual bool left_right()
	{
		return associativity == Constants_Fields.LEFT_RIGHT;
	}
	public Operator(string mnemonic, string symbol, int precedence, int associativity, int type)
	{
		this.mnemonic = mnemonic;
		this.symbol = symbol;
		this.precedence = precedence;
		this.associativity = associativity;
		this.type = type;
	}
	public override string ToString()
	{
		return symbol;
	}
	internal static Operator get(object text_in)
	{
		if (!(text_in is string))
		{
			return null;
		}
		string text = (string) text_in;
		for (int k = 0; k < OPS.Length; k++)
		{
			Operator op = OPS[k];
			if (text.StartsWith(op.symbol, StringComparison.Ordinal))
			{
				return op;
			}
		}
		return null;
	}
	internal static Operator get(object text_in, int pos)
	{
		if (!(text_in is string))
		{
			return null;
		}
		string text = (string) text_in;
		for (int k = 0; k < OPS.Length; k++)
		{
			Operator op = OPS[k];
			if (text.StartsWith(op.symbol, StringComparison.Ordinal))
			{
				switch (pos)
				{
					case Constants_Fields.START:
						if (op.unary() && op.left_right())
						{
							return op;
						}
						continue;
					case END:
						if (op.unary() && !op.left_right())
						{
							return op;
						}
						continue;
					case MID:
						if (op.binary() || op.ternary())
						{
							return op;
						}
						continue;
				}
			}
		}
		return null;
	}
	internal virtual Lambda Lambda
	{
		get
		{
			if (func == null)
			{
				try
				{
					Type c = Type.GetType(mnemonic);
					func = (Lambda)c.newInstance();
				}
				catch (Exception)
				{
				}
			}
			return func;
		}
	}
}
internal class ADJ : Lambda
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		Matrix m = new Matrix(getAlgebraic(st));
		st.Push(m.adjunkt().reduce());
		return 0;
	}
}
internal class TRN : Lambda
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		Matrix m = new Matrix(getAlgebraic(st));
		st.Push(m.transpose().reduce());
		return 0;
	}
}
internal class FCT : LambdaAlgebraic
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		Algebraic arg = getAlgebraic(st);
		if (arg is Zahl)
		{
			st.Push(f((Zahl)arg));
		}
		else
		{
			st.Push(FunctionVariable.create("factorial", arg));
		}
		return 0;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Algebraic f_exakt(Algebraic x) throws JasymcaException
	internal override Algebraic f_exakt(Algebraic x)
	{
		if (x is Zahl)
		{
			return f((Zahl)x);
		}
		;
		return null;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Zahl f(Zahl x) throws JasymcaException
	internal override Zahl f(Zahl x)
	{
		if (!x.integerq() || x.smaller(Zahl.ZERO))
		{
			throw new JasymcaException("Argument to factorial must be a positive integer, is " + x);
		}
		Algebraic r = Zahl.ONE;
		while (Zahl.ONE.smaller(x))
		{
			r = r.mult(x);
			x = (Zahl)x.sub(Zahl.ONE);
		}
		return (Zahl)r;
	}
}
internal class LambdaFACTORIAL : FCT
{
}
internal class FCN : Lambda
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		List code_in = getList(st);
		string fname = getSymbol(st).Substring(1);
		int nvar = getNarg(st);
		SimpleVariable[] vars = new SimpleVariable[nvar];
		for (int i = 0; i < nvar; i++)
		{
			vars[i] = new SimpleVariable(getSymbol(st));
		}
		Lambda func = null;
		Environment env = new Environment();
		Stack ups = new Stack();
		object y = null;
		if (nvar == 1)
		{
			int res = UserProgram.process_block(code_in, ups, env, false);
			if (res != Processor.ERROR)
			{
				y = ups.Pop();
			}
		}
		if (y is Algebraic)
		{
			func = new UserFunction(fname, vars, (Algebraic)y, null, null);
		}
		else
		{
			func = new UserProgram(fname, vars, code_in, null, env, ups);
		}
		pc.env.putValue(fname, func);
		st.Push(fname);
		return 0;
	}
}
internal class POW : LambdaAlgebraic
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Algebraic f_exakt(Algebraic x, Algebraic y) throws JasymcaException
	internal override Algebraic f_exakt(Algebraic x, Algebraic y)
	{
		if (x.Equals(Zahl.ZERO))
		{
			if (y.Equals(Zahl.ZERO))
			{
				return Zahl.ONE;
			}
			return Zahl.ZERO;
		}
		if (y is Zahl && ((Zahl)y).integerq())
		{
			return x.pow_n(((Zahl)y).intval());
		}
		return FunctionVariable.create("exp",FunctionVariable.create("log",x).mult(y));
	}
}
internal class PPR : Lambda
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
	public override int lambda(Stack st)
	{
		return ASS.lambdai(st, true, false);
	}
}
internal class MMR : Lambda
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
	public override int lambda(Stack st)
	{
		return ASS.lambdai(st, false, false);
	}
}
internal class PPL : Lambda
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
	public override int lambda(Stack st)
	{
		return ASS.lambdai(st, true, true);
	}
}
internal class MML : Lambda
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
	public override int lambda(Stack st)
	{
		return ASS.lambdai(st, false, true);
	}
}
internal class ADE : Lambda
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
	public override int lambda(Stack st)
	{
		return ASS.lambdap(st, Operator.get("+").Lambda);
	}
}
internal class SUE : Lambda
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
	public override int lambda(Stack st)
	{
		return ASS.lambdap(st, Operator.get("-").Lambda);
	}
}
internal class MUE : Lambda
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
	public override int lambda(Stack st)
	{
		return ASS.lambdap(st, Operator.get("*").Lambda);
	}
}
internal class DIE : Lambda
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
	public override int lambda(Stack st)
	{
		return ASS.lambdap(st, Operator.get("/").Lambda);
	}
}
internal class ADD : LambdaAlgebraic
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Algebraic f_exakt(Algebraic x) throws JasymcaException
	internal override Algebraic f_exakt(Algebraic x)
	{
		return x;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Algebraic f_exakt(Algebraic x, Algebraic y) throws JasymcaException
	internal override Algebraic f_exakt(Algebraic x, Algebraic y)
	{
		return x.add(y);
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Zahl f(Zahl x) throws JasymcaException
	internal override Zahl f(Zahl x)
	{
		return (Zahl)f_exakt(x);
	}
}
internal class SUB : LambdaAlgebraic
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Algebraic f_exakt(Algebraic x) throws JasymcaException
	internal override Algebraic f_exakt(Algebraic x)
	{
		return x.mult(Zahl.MINUS);
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Algebraic f_exakt(Algebraic x, Algebraic y) throws JasymcaException
	internal override Algebraic f_exakt(Algebraic x, Algebraic y)
	{
		return x.add(y.mult(Zahl.MINUS));
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Zahl f(Zahl x) throws JasymcaException
	internal override Zahl f(Zahl x)
	{
		return (Zahl)f_exakt(x);
	}
}
internal class MUL : LambdaAlgebraic
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Algebraic f_exakt(Algebraic x, Algebraic y) throws JasymcaException
	internal override Algebraic f_exakt(Algebraic x, Algebraic y)
	{
		return x.mult(y);
	}
}
internal class MMU : LambdaAlgebraic
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		if (narg != 2)
		{
			throw new ParseException("Wrong number of arguments for \"*\".");
		}
		Algebraic b = getAlgebraic(st);
		Algebraic a = getAlgebraic(st);
		if (b.scalarq())
		{
			st.Push(a.mult(b));
		}
		else if (a.scalarq())
		{
			st.Push(b.mult(a));
		}
		else if (a is Vektor && b is Vektor)
		{
			st.Push(a.mult(b));
		}
		else
		{
			st.Push((new Matrix(a)).mult(new Matrix(b)).reduce());
		}
		return 0;
	}
}
internal class MPW : LambdaAlgebraic
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		Algebraic a = getAlgebraic(st);
		Algebraic b = getAlgebraic(st);
		if (a.scalarq() && b.scalarq())
		{
			st.Push((new POW()).f_exakt(b, a));
			return 0;
		}
		if (!(a is Zahl) || !((Zahl)a).integerq())
		{
			throw new JasymcaException("Wrong arguments to function Matrixpow.");
		}
		st.Push((new Matrix(b)).mpow(((Zahl)a).intval()));
		return 0;
	}
}
internal class DIV : LambdaAlgebraic
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Algebraic f_exakt(Algebraic x, Algebraic y) throws JasymcaException
	internal override Algebraic f_exakt(Algebraic x, Algebraic y)
	{
		return x.div(y);
	}
}
internal class MDR : Lambda
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		if (narg != 2)
		{
			throw new ParseException("Wrong number of arguments for \"/\".");
		}
		Algebraic b = getAlgebraic(st);
		Matrix a = new Matrix(getAlgebraic(st));
		st.Push(a.div(b).reduce());
		return 0;
	}
}
internal class MDL : Lambda
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		if (narg != 2)
		{
			throw new ParseException("Wrong number of arguments for \"\\\".");
		}
		Matrix b = new Matrix(getAlgebraic(st));
		Matrix a = new Matrix(getAlgebraic(st));
		st.Push(((Matrix)b.transpose().div(a.transpose())).transpose().reduce());
		return 0;
	}
}
internal class EQU : LambdaAlgebraic
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Algebraic f_exakt(Algebraic x1, Algebraic y1) throws JasymcaException
	internal override Algebraic f_exakt(Algebraic x1, Algebraic y1)
	{
		Zahl x = ensure_Zahl(x1);
		Zahl y = ensure_Zahl(y1);
		return y.Equals(x) ? Zahl.ONE : Zahl.ZERO;
	}
}
internal class NEQ : LambdaAlgebraic
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Algebraic f_exakt(Algebraic x1, Algebraic y1) throws JasymcaException
	internal override Algebraic f_exakt(Algebraic x1, Algebraic y1)
	{
		Zahl x = ensure_Zahl(x1);
		Zahl y = ensure_Zahl(y1);
		return y.Equals(x) ? Zahl.ZERO : Zahl.ONE;
	}
}
internal class GEQ : LambdaAlgebraic
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Algebraic f_exakt(Algebraic x1, Algebraic y1) throws JasymcaException
	internal override Algebraic f_exakt(Algebraic x1, Algebraic y1)
	{
		Zahl x = ensure_Zahl(x1);
		Zahl y = ensure_Zahl(y1);
		return x.smaller(y) ? Zahl.ZERO : Zahl.ONE;
	}
}
internal class GRE : LambdaAlgebraic
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Algebraic f_exakt(Algebraic x1, Algebraic y1) throws JasymcaException
	internal override Algebraic f_exakt(Algebraic x1, Algebraic y1)
	{
		Zahl x = ensure_Zahl(x1);
		Zahl y = ensure_Zahl(y1);
		return y.smaller(x) ? Zahl.ONE : Zahl.ZERO;
	}
}
internal class LEQ : LambdaAlgebraic
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Algebraic f_exakt(Algebraic x1, Algebraic y1) throws JasymcaException
	internal override Algebraic f_exakt(Algebraic x1, Algebraic y1)
	{
		Zahl x = ensure_Zahl(x1);
		Zahl y = ensure_Zahl(y1);
		return y.smaller(x) ? Zahl.ZERO : Zahl.ONE;
	}
}
internal class LES : LambdaAlgebraic
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Algebraic f_exakt(Algebraic x1, Algebraic y1) throws JasymcaException
	internal override Algebraic f_exakt(Algebraic x1, Algebraic y1)
	{
		Zahl x = ensure_Zahl(x1);
		Zahl y = ensure_Zahl(y1);
		return x.smaller(y) ? Zahl.ONE : Zahl.ZERO;
	}
}
internal class NOT : LambdaAlgebraic
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Zahl f(Zahl x) throws JasymcaException
	internal override Zahl f(Zahl x)
	{
		return x.Equals(Zahl.ZERO) ? Zahl.ONE : Zahl.ZERO;
	}
}
internal class OR : LambdaAlgebraic
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Algebraic f_exakt(Algebraic x1, Algebraic y1) throws JasymcaException
	internal override Algebraic f_exakt(Algebraic x1, Algebraic y1)
	{
		Zahl x = ensure_Zahl(x1);
		Zahl y = ensure_Zahl(y1);
		return x.Equals(Zahl.ONE) || y.Equals(Zahl.ONE) ? Zahl.ONE : Zahl.ZERO;
	}
}
internal class AND : LambdaAlgebraic
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Algebraic f_exakt(Algebraic x1, Algebraic y1) throws JasymcaException
	internal override Algebraic f_exakt(Algebraic x1, Algebraic y1)
	{
		Zahl x = ensure_Zahl(x1);
		Zahl y = ensure_Zahl(y1);
		return x.Equals(Zahl.ONE) && y.Equals(Zahl.ONE)? Zahl.ONE : Zahl.ZERO;
	}
}
internal class LambdaGAMMA : LambdaAlgebraic
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Zahl f(Zahl x) throws JasymcaException
	internal override Zahl f(Zahl x)
	{
		return new Unexakt(Sfun.gamma(x.unexakt().real));
	}
}
internal class LambdaGAMMALN : LambdaAlgebraic
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Zahl f(Zahl x) throws JasymcaException
	internal override Zahl f(Zahl x)
	{
		return new Unexakt(Sfun.logGamma(x.unexakt().real));
	}
}
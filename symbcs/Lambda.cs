using System;
using System.Collections;
using System.IO;
using System.Text;

public abstract class Lambda : Constants
{
	internal static Processor pc;
	internal static Parser pr;
	internal const bool DEBUG = false;
	internal static void debug(string s)
	{
	    if ( DEBUG )
		{
			Console.WriteLine(s);
		}
	}
	internal static int length = 1;
	public virtual int lambda(Stack x)
	{
		return 0;
	}
	internal static Algebraic getAlgebraic(Stack st)
	{
		object arg_in = st.Pop();
		if (!(arg_in is Algebraic))
		{
			pc.process_instruction(arg_in, true);
			arg_in = st.Pop();
		}
		if (!(arg_in is Algebraic))
		{
			throw new JasymcaException("Expected algebraic, got: " + arg_in);
		}
		return (Algebraic)arg_in;
	}
	internal static Zahl getNumber(Stack st)
	{
		object arg = st.Pop();
		if (arg is Algebraic)
		{
			arg = (new ExpandConstants()).f_exakt((Algebraic)arg);
		}
		if (!(arg is Zahl))
		{
			throw new ParseException("Expected number, got " + arg);
		}
		return (Zahl)arg;
	}
	internal static int getNarg(Stack st)
	{
		object arg_in = st.Pop();
		if (!(arg_in is int?))
		{
			throw new JasymcaException("Expected Integer, got: " + arg_in);
		}
		return (int)((int?)arg_in);
	}
	internal static string getSymbol(Stack st)
	{
		object arg_in = st.Pop();
		if (!(arg_in is string) || ((string)arg_in).Length == 0 || ((string)arg_in)[0] == ' ')
		{
			throw new JasymcaException("Expected Symbol, got: " + arg_in);
		}
		return (string)arg_in;
	}
	internal static Polynomial getPolynomial(Stack st)
	{
		object arg = getAlgebraic(st);
		if (!(arg is Polynomial))
		{
			throw new ParseException("Expected polynomial, got " + arg);
		}
		return (Polynomial)arg;
	}
	internal static Vektor getVektor(Stack st)
	{
		object arg = st.Pop();
		if (!(arg is Vektor))
		{
			throw new ParseException("Expected vector, got " + arg);
		}
		return (Vektor)arg;
	}
	internal static Variable getVariable(Stack st)
	{
		Polynomial p = getPolynomial(st);
		return p.v;
	}
	internal static int getInteger(Stack st)
	{
		object arg = st.Pop();
		if (!(arg is Zahl) || !((Zahl)arg).integerq())
		{
			throw new ParseException("Expected integer, got " + arg);
		}
		return ((Zahl)arg).intval();
	}
	internal static int getInteger(Algebraic arg)
	{
		if (!(arg is Zahl) || !((Zahl)arg).integerq())
		{
			throw new ParseException("Expected integer, got " + arg);
		}
		return ((Zahl)arg).intval();
	}
	internal static List getList(Stack st)
	{
		object arg = st.Pop();
		if (!(arg is List))
		{
			throw new ParseException("Expected list, got " + arg);
		}
		return (List)arg;
	}
	internal static Zahl ensure_Zahl(object x)
	{
		if (!(x is Zahl))
		{
			throw new JasymcaException("Expected number, got " + x);
		}
		return (Zahl)x;
	}
	internal static Environment sandbox = null;
	internal static Algebraic evalx(string rule, Algebraic x)
	{
		try
		{
			List pgm = pr.compile(rule);
			Environment global = pc.Environment;
			if (sandbox == null)
			{
				sandbox = new Environment();
				sandbox.putValue("x", new Polynomial(new SimpleVariable("x")));
				sandbox.putValue("X", new Polynomial(new SimpleVariable("X")));
				sandbox.putValue("a", new Polynomial(new SimpleVariable("a")));
				sandbox.putValue("b", new Polynomial(new SimpleVariable("b")));
				sandbox.putValue("c", new Polynomial(new SimpleVariable("c")));
			}
			pc.Environment = sandbox;
			pc.process_list(pgm, true);
			pc.Environment = global;
			Algebraic y = getAlgebraic(pc.stack);
			y = y.value(new SimpleVariable("x"), x);
			return y;
		}
		catch (Exception e)
		{
			throw new JasymcaException("Could not evaluate expression " + rule + ": " + e.ToString());
		}
	}
}
/*internal*/ public abstract class LambdaAlgebraic : Lambda
{
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		switch (narg)
		{
			case 0:
				throw new JasymcaException("Lambda functions expect argument.");
			case 1:
				Algebraic arg = getAlgebraic(st);
				st.Push(arg.map_lambda(this, null));
				break;
			case 2:
				Algebraic arg2 = getAlgebraic(st);
				Algebraic arg1 = getAlgebraic(st);
				arg1 = arg1.promote(arg2);
				st.Push(arg1.map_lambda(this, arg2));
				break;
			default:
				Algebraic[] args = new Algebraic[narg];
				for (int i = narg - 1; i >= 0; i--)
				{
					args[i] = getAlgebraic(st);
				}
				st.Push(f_exakt(args));
				break;
		}
		return 0;
	}
	internal virtual Zahl f(Zahl x)
	{
		return x;
	}
	internal virtual Algebraic f_exakt(Algebraic x)
	{
		return null;
	}
	internal virtual Algebraic f_exakt(Algebraic x, Algebraic y)
	{
		return null;
	}
	internal virtual Algebraic f_exakt(Algebraic[] x)
	{
		return null;
	}
	internal string diffrule = null, intrule = null, trigrule = null;
	public virtual Algebraic integrate(Algebraic arg, Variable x)
	{
		if (!(arg.depends(x)))
		{
			throw new JasymcaException("Expression in function does not depend on Variable.");
		}
		if (!(arg is Polynomial) || ((Polynomial)arg).degree() != 1 || !((Polynomial)arg).ratfunc(x) || intrule == null)
		{
			throw new JasymcaException("Can not integrate function ");
		}
		try
		{
			Algebraic y = evalx(intrule, arg);
			return y.div(((Polynomial)arg).a[1]);
		}
		catch (Exception)
		{
			throw new JasymcaException("Error integrating function");
		}
	}
}
internal class LambdaFUNC : Lambda
{
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		if (narg != 3)
		{
			throw new ParseException("Wrong function definition.");
		}
		List ret = getList(st);
		List prot = getList(st);
		if (prot.Count < 1 || !(prot[prot.Count - 1] is string))
		{
			throw new ParseException("Wrong function definition.");
		}
		string fname = ((string)prot[prot.Count - 1]).Substring(1);
		List vars_in = prot.subList(0, prot.Count - 1);
		List code_in = getList(st);
		SimpleVariable[] vars = null;
		if (vars_in.Count != 0)
		{
			int fnarg = (int)((int?)vars_in[vars_in.Count - 1]);
			vars = new SimpleVariable[fnarg];
			for (int i = 0; i < vars.Length; i++)
			{
				vars[i] = new SimpleVariable((string)vars_in[vars.Length - i - 1]);
			}
		}
		SimpleVariable result = new SimpleVariable(((string)ret[0]).Substring(1));
		Lambda func = null;
		Environment env = new Environment();
		Stack ups = new Stack();
		for (int i = 0; i < vars.Length; i++)
		{
			env.putValue(vars[i].name, new Polynomial(vars[i]));
		}
		object y = null;
		if (vars.Length == 1)
		{
			int res = UserProgram.process_block(code_in, ups, env, true);
			if (res != Processor.ERROR)
			{
				y = env.getValue(result.name);
			}
		}
		if (y is Algebraic)
		{
			func = new UserFunction(fname, vars, (Algebraic)y, result, env);
		}
		else
		{
			func = new UserProgram(fname, vars, code_in, result, env, ups);
		}
		pc.env.putValue(fname, func);
		st.Push(func);
		return 0;
	}
}
internal class UserProgram : Lambda
{
	internal string fname;
	internal List body;
	internal SimpleVariable[] @var;
	internal SimpleVariable result;
	internal Environment env = null;
	internal Stack ups = null;
	public UserProgram()
	{
	}
	public UserProgram(string fname, SimpleVariable[] @var, List body, SimpleVariable result, Environment env, Stack ups)
	{
		this.fname = fname;
		this.@var = @var;
		this.body = body;
		this.result = result;
		this.env = env;
		this.ups = ups;
	}
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		if (@var.Length != narg)
		{
			throw new JasymcaException(fname + " requires " + @var.Length + " Arguments.");
		}
		for (int i = 0; i < @var.Length; i++)
		{
			object a = st.Pop();
			env.putValue(@var[i].name, a);
		}
		int ret = process_block(body, ups, env, result != null);
		if (ret != Processor.ERROR)
		{
			object y = (result != null?env.getValue(result.name):ups.Pop());
			if (y is Algebraic && result != null)
			{
				((Algebraic)y).name = result.name;
			}
			if (y != null)
			{
				st.Push(y);
			}
		}
		return 0;
	}
	internal static int process_block(List code, Stack st, Environment env, bool clear_stack)
	{
		Environment global = pc.Environment;
		Stack old_stack = pc.stack;
		pc.Environment = env;
		pc.stack = st;
		int ret;
		try
		{
			ret = pc.process_list(code, true);
		}
		catch (Exception)
		{
			ret = Processor.ERROR;
		}
		pc.stack = old_stack;
		pc.Environment = global;
		if (clear_stack)
		{
			while (st.Count > 0)
			{
				st.Pop();
			}
		}
		return ret;
	}
}
internal class UserFunction : LambdaAlgebraic
{
	internal string fname;
	internal Algebraic body;
	internal SimpleVariable[] @var;
	internal SimpleVariable result;
	internal Environment env = null;
	public UserFunction()
	{
	}
	public UserFunction(string fname, SimpleVariable[] @var, Algebraic body, SimpleVariable result, Environment env)
	{
		this.fname = fname;
		this.@var = @var;
		this.body = body;
		this.result = result;
		this.env = env;
	}
	internal override Zahl f(Zahl x)
	{
		Algebraic y = f_exakt(x);
		if (y is Zahl)
		{
			return (Zahl)y;
		}
		y = (new ExpandConstants()).f_exakt(y);
		if (y is Zahl)
		{
			return (Zahl)y;
		}
		throw new JasymcaException("Can not evaluate Function " + fname + " to number, got " + y + " for " + x);
	}
	internal override Algebraic f_exakt(Algebraic x)
	{
		if (@var.Length != 1)
		{
			throw new JasymcaException("Wrong number of arguments.");
		}
		Algebraic y = body.value(@var[0], x);
		return y;
	}
	internal override Algebraic f_exakt(Algebraic x, Algebraic y)
	{
		if (@var.Length != 2)
		{
			throw new JasymcaException("Wrong number of arguments.");
		}
		Algebraic z = body.value(@var[0], y);
		z = z.value(@var[1], x);
		return z;
	}
	internal override Algebraic f_exakt(Algebraic[] x)
	{
		if (@var.Length != x.Length)
		{
			throw new JasymcaException("Wrong number of arguments.");
		}
		Algebraic y = body;
		for (int i = 0; i < x.Length; i++)
		{
			y = y.value(@var[x.Length - i - 1], x[i]);
		}
		return y;
	}
	internal virtual Algebraic fv(Vektor x)
	{
		Environment global = pc.env;
		pc.env = env;
		Algebraic r = body;
		pc.env = global;
		for (int i = 0; i < @var.Length; i++)
		{
			r = r.value(@var[i], x.get(i));
		}
		return r;
	}
	public override Algebraic integrate(Algebraic arg, Variable x)
	{
		if (!(body is Algebraic))
		{
			throw new JasymcaException("Can not integrate function " + fname);
		}
		if (!(arg.depends(x)))
		{
			throw new JasymcaException("Expression in function does not depend on Variable.");
		}
		if (@var.Length == 1)
		{
			return ((Algebraic)body).value(@var[0], arg).integrate(x);
		}
		if (arg is Vektor && ((Vektor)arg).length() == @var.Length)
		{
			return fv((Vektor)arg).integrate(x);
		}
		throw new JasymcaException("Wrong argument to function " + fname);
	}
}
internal class LambdaFLOAT : LambdaAlgebraic
{
	internal double eps = 1.0e-8;
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		Algebraic exp = getAlgebraic(st);
		Zahl a = pc.env.getnum("algepsilon");
		if (a != null)
		{
			double epstry = a.unexakt().real;
			if (epstry > 0)
			{
				eps = epstry;
			}
		}
		exp = (new ExpandConstants()).f_exakt(exp);
		st.Push(exp.map(this));
		return 0;
	}
	internal override Zahl f(Zahl x)
	{
		Unexakt f = x.unexakt();
		if (f.Equals(Zahl.ZERO))
		{
			return f;
		}
		double abs = ((Unexakt)f.abs()).real;
		double r = f.real;
		if (Math.Abs(r / abs) < eps)
		{
			r = 0.0;
		}
		double i = f.imag;
		if (Math.Abs(i / abs) < eps)
		{
			i = 0.0;
		}
		return new Unexakt(r,i);
	}
	internal override Algebraic f_exakt(Algebraic x)
	{
		return x.map(this);
	}
}
internal class LambdaMATRIX : Lambda
{
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		Algebraic[][]a = new Algebraic[narg][];
		for (int i = 0; i < narg; i++)
		{
			Algebraic b = getAlgebraic(st);
			if (b is Vektor)
			{
				a[i] = ((Vektor)b).get();
			}
			else
			{
				a[i] = new Algebraic[1];
				a[i][0] = b;
			}
			if ( a[i].GetLength(0) != a[0].GetLength(0) )
			{
				throw new JasymcaException("Matrix rows must have equal length.");
			}
		}
		st.Push((new Matrix(a)).reduce());
		return 0;
	}
}
internal class LambdaFORMAT : Lambda
{
	public override int lambda(Stack st)
	{
		int nargs = getNarg(st);
		if (nargs == 1)
		{
			object arg = st.Pop();
			if ("$short".Equals(arg.ToString()))
			{
				Jasymca.fmt = new NumFmtVar(10, 5);
				return 0;
			}
			else if ("$long".Equals(arg.ToString()))
			{
				Jasymca.fmt = new NumFmtJava();
				return 0;
			}
			throw new JasymcaException("Usage: format long | short | base significant");
		}
		else if (nargs == 2)
		{
			int @base = getInteger(st);
			int nsign = getInteger(st);
			if (@base < 2 || nsign < 1)
			{
				throw new JasymcaException("Invalid variables.");
			}
			Jasymca.fmt = new NumFmtVar(@base, nsign);
			return 0;
		}
		throw new JasymcaException("Usage: format long | short | base significant");
	}
}
internal class LambdaSYMS : Lambda
{
	public override int lambda(Stack st)
	{
		int nargs = getNarg(st);
		while (nargs-- > 0)
		{
			object arg = st.Pop();
			if (arg is string)
			{
				string s = ((string)arg).Substring(1);
				pc.env.putValue(s, new Polynomial(new SimpleVariable(s)));
			}
		}
		return 0;
	}
}
internal class LambdaCLEAR : Lambda
{
	public override int lambda(Stack st)
	{
		int nargs = getNarg(st);
		while (nargs-- > 0)
		{
			object arg = st.Pop();
			if (arg is string)
			{
				string s = ((string)arg).Substring(1);
				pc.env.Remove(s);
			}
		}
		return 0;
	}
}
internal class CreateVector : Lambda
{
	public override int lambda(Stack st)
	{
		int nr = getNarg(st);
		int nrow = 1, ncol = 1;
		Matrix m = new Matrix(nr,1);
		while (nr-- > 0)
		{
			Algebraic row = crv(st);
			Index idx = new Index(nrow, ncol, row);
			m.insert(new Matrix(row), idx);
			nrow = idx.row_max + 1;
		}
		st.Push(m.reduce());
		return 0;
	}
	internal static Algebraic crv(Stack st)
	{
		int nc = getNarg(st);
		if (nc == 1)
		{
			return getAlgebraic(st);
		}
		Matrix m = new Matrix(1,nc);
		int nrow = 1, ncol = 1;
		while (nc-- > 0)
		{
			Algebraic x = getAlgebraic(st);
			Index idx = new Index(nrow, ncol, x);
			m.insert(new Matrix(x), idx);
			ncol = idx.col_max + 1;
		}
		return m.reduce();
	}
}
internal class CR1 : Lambda
{
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		Algebraic a , b , c = getAlgebraic(st);
		if (narg == 2)
		{
			b = Zahl.ONE;
			a = getAlgebraic(st);
		}
		else
		{
			b = getAlgebraic(st);
			a = getAlgebraic(st);
		}
		Algebraic na = c.sub(a).div(b);
		if (!(na is Zahl))
		{
			na = (new ExpandConstants()).f_exakt(na);
		}
		if (!(na is Zahl))
		{
			throw new ParseException("CreateVector requires numbers.");
		}
		int n = (int)(((Zahl)na).unexakt().real + 1.0);
		Algebraic[] coord = new Algebraic[n];
		for (int i = 0; i < n; i++)
		{
			coord[i] = a.add(b.mult(new Unexakt((double)i)));
		}
		st.Push(new Vektor(coord));
		return 0;
	}
}
internal class LambdaEYE : Lambda
{
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		if (narg < 1)
		{
			throw new JasymcaException("Usage: EYE( nrow, ncol ).");
		}
		int nrow = getInteger(st);
		int ncol = nrow;
		if (narg > 1)
		{
			ncol = getInteger(st);
		}
		st.Push(Matrix.eye(nrow, ncol).reduce());
		return 0;
	}
}
internal class LambdaZEROS : Lambda
{
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		if (narg < 1)
		{
			throw new JasymcaException("Usage: ZEROS( nrow, ncol ).");
		}
		int nrow = getInteger(st);
		int ncol = nrow;
		if (narg > 1)
		{
			ncol = getInteger(st);
		}
		st.Push((new Matrix(nrow, ncol)).reduce());
		return 0;
	}
}
internal class LambdaONES : Lambda
{
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		if (narg < 1)
		{
			throw new JasymcaException("Usage: ONES( nrow, ncol ).");
		}
		int nrow = getInteger(st);
		int ncol = nrow;
		if (narg > 1)
		{
			ncol = getInteger(st);
		}
		st.Push((new Matrix(Zahl.ONE, nrow, ncol)).reduce());
		return 0;
	}
}
internal class LambdaRAND : Lambda
{
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		if (narg < 1)
		{
			throw new JasymcaException("Usage: RAND( nrow, ncol ).");
		}
		int nrow = getInteger(st);
		int ncol = nrow;
		if (narg > 1)
		{
			ncol = getInteger(st);
		}
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: Algebraic[][] a = new Algebraic[nrow][ncol];
		Algebraic[][] a = RectangularArrays.ReturnRectangularAlgebraicArray(nrow, ncol);
		for (int i = 0; i < nrow; i++)
		{
			for (int k = 0; k < ncol; k++)
			{
				a[i][k] = new Unexakt(JMath.random());
			}
		}
			st.Push((new Matrix(a)).reduce());
			return 0;
	}
}
internal class LambdaDIAG : Lambda
{
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		if (narg < 1)
		{
			throw new JasymcaException("Usage: DIAG( matrix, k ).");
		}
		Algebraic x = getAlgebraic(st).reduce();
		int k = 0;
		if (narg > 1)
		{
			k = getInteger(st);
		}
		if (x.scalarq())
		{
			x = new Vektor(new Algebraic[] {x});
		}
		if (x is Vektor)
		{
			Vektor xv = (Vektor)x;
			if (k >= 0)
			{
				Matrix m = new Matrix(xv.length() + k, xv.length() + k);
				for (int i = 0; i < xv.length(); i++)
				{
					m.set(i, i + k, xv.get(i));
				}
				st.Push(m);
			}
			else
			{
				Matrix m = new Matrix(xv.length() - k, xv.length() - k);
				for (int i = 0; i < xv.length(); i++)
				{
					m.set(i - k, i, xv.get(i));
				}
				st.Push(m);
			}
		}
		else if (x is Matrix)
		{
			Matrix xm = (Matrix)x;
			if (k >= 0 && k < xm.ncol())
			{
				Algebraic[] a = new Algebraic[xm.ncol() - k];
				for (int i = 0; i < a.Length; i++)
				{
					a[i] = xm.get(i, i + k);
				}
				st.Push(new Vektor(a));
			}
			else if (k < 0 && (-k) < xm.nrow())
			{
				Algebraic[] a = new Algebraic[xm.nrow() + k];
				for (int i = 0; i < a.Length; i++)
				{
					a[i] = xm.get(i - k, i);
				}
				st.Push(new Vektor(a));
			}
			else
			{
				throw new JasymcaException("Argument k to DIAG out of range.");
			}
		}
		else
		{
			throw new JasymcaException("Argument to DIAG must be vector or matrix.");
		}
		return 0;
	}
}
internal class LambdaGCD : Lambda
{
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);

		if (narg < 2)
		{
			throw new ParseException("GCD requires at least 2 arguments.");
		}

		var _gcd = getAlgebraic(st);

		for (int i = 1; i < narg; i++)
		{
			_gcd = gcd( _gcd, getAlgebraic(st) );
		}

		st.Push(_gcd);

		return 0;
	}
	internal virtual Algebraic gcd(Algebraic x, Algebraic y)
	{
		if (!x.exaktq())
		{
			x = (new LambdaRAT()).f_exakt(x);
		}
		if (!y.exaktq())
		{
			y = (new LambdaRAT()).f_exakt(y);
		}
		if (x is Zahl && y is Zahl)
		{
			return ((Zahl)x).gcd((Zahl)y);
		}
		if (x is Polynomial)
		{
			Zahl gcd_x = ((Polynomial)x).gcd_coeff();
			if (y is Polynomial)
			{
				Zahl gcd_y = ((Polynomial)y).gcd_coeff();
				return Poly.poly_gcd(x,y).mult(gcd_x.gcd(gcd_y));
			}
			if (y is Zahl)
			{
				return gcd_x.gcd((Zahl)y);
			}
		}
		if (y is Polynomial && x is Zahl)
		{
			return gcd(y,x);
		}
		throw new JasymcaException("Not implemented.");
	}
}
internal class LambdaEXPAND : Lambda
{
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		object x = st.Pop();
		if (x is List)
		{
			pc.process_list((List)x, true);
			x = pc.stack.Pop();
		}
		if (x is Algebraic)
		{
			x = (new SqrtExpand()).f_exakt((Algebraic) x);
		}
		st.Push(x);
		return 0;
	}
}
internal class LambdaREALPART : Lambda
{
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		st.Push(getAlgebraic(st).realpart());
		return 0;
	}
}
internal class LambdaIMAGPART : Lambda
{
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		st.Push(getAlgebraic(st).imagpart());
		return 0;
	}
}
internal class LambdaCONJ : Lambda
{
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		st.Push(getAlgebraic(st).cc());
		return 0;
	}
}
internal class LambdaANGLE : Lambda
{
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		Algebraic x = getAlgebraic(st);
		object atan2 = pc.env.getValue("atan2");
		if (!(atan2 is LambdaAlgebraic))
		{
			throw new JasymcaException("Function ATAN2 not installed.");
		}
		st.Push(((LambdaAlgebraic)atan2).f_exakt(x.imagpart(), x.realpart()));
		return 0;
	}
}
internal class LambdaCFS : Lambda
{
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		Algebraic y = getAlgebraic(st).rat();
		if (!(y is Exakt))
		{
			throw new ParseException("Argument must be exact number");
		}
		double eps = 1.0e-5;
		if (narg > 1)
		{
			eps = getNumber(st).unexakt().real;
		}
		st.Push(((Exakt)y).cfs(eps));
		return 0;
	}
}
internal class LambdaDIFF : Lambda
{
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		if (narg == 0)
		{
			throw new ParseException("Argument to diff missing.");
		}
		Algebraic f = getAlgebraic(st);
		Variable v;
		if (narg > 1)
		{
			v = getVariable(st);
		}
		else
		{
			if (f is Polynomial)
			{
				v = ((Polynomial)f).v;
			}
			else if (f is Rational)
			{
				v = ((Rational)f).den.v;
			}
			else
			{
				throw new ParseException("Could not determine Variable.");
			}
		}
		Algebraic df = f.deriv(v);
		if (df is Rational && !df.exaktq())
		{
			df = (new LambdaRAT()).f_exakt(df);
		}
		st.Push(df);
		return 0;
	}
}
internal class LambdaSUBST : Lambda
{
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		if (narg != 3)
		{
			throw new ParseException("Usage: SUBST (a, b, c), substitutes a for b in c");
		}
		Algebraic a = getAlgebraic(st);
		Polynomial b = getPolynomial(st);
		Algebraic c = getAlgebraic(st);
		Variable bx = b.v;
		while (bx is FunctionVariable)
		{
			Algebraic arg = ((FunctionVariable)bx).arg;
			if (!(arg is Polynomial))
			{
				throw new JasymcaException("Can not solve " + b + " for a variable.");
			}
			bx = ((Polynomial)arg).v;
		}
		Vektor sol = LambdaSOLVE.solve(a.sub(b), bx);
		Algebraic[] res = new Algebraic[sol.length()];
		for (int i = 0; i < sol.length(); i++)
		{
			Algebraic y = sol.get(i);
			res[i] = c.value(bx, y);
		}
		st.Push((new Vektor(res)).reduce());
		return 0;
	}
}
internal class LambdaCOEFF : Lambda
{
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		if (narg != 3)
		{
			throw new ParseException("Usage: COEFF (a, b, c), find coeff of b^c in a");
		}
		Polynomial a = getPolynomial(st);
		Variable b = getVariable(st);
		Algebraic c_in = getAlgebraic(st);
		if (c_in.scalarq())
		{
			st.Push(a.coefficient(b, getInteger(c_in)));
		}
		else if (c_in is Vektor)
		{
			Vektor c = (Vektor)c_in;
			Algebraic[] v = new Algebraic[c.length()];
			for (int i = 0; i < v.Length; i++)
			{
				v[i] = a.coefficient(b, getInteger(c.get(i)));
			}
			st.Push(new Vektor(v));
		}
		else
		{
			throw new ParseException("Usage: COEFF (a, b, c), find coeff of b^c in a");
		}
		return 0;
	}
}
internal class LambdaSUM : Lambda
{
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		if (narg == 1)
		{
			Algebraic x = getAlgebraic(st);
			if (x.scalarq() && !x.constantq())
			{
				throw new JasymcaException("Unknown variable dimension: " + x);
			}
			Matrix m = new Matrix(x);
			bool addcols = (m.ncol() > 1);
			if (narg > 1)
			{
				if (getInteger(st) == 2)
				{
					addcols = false;
				}
			}
			if (addcols)
			{
				Algebraic s = m.col(1);
				for (int i = 2; i <= m.ncol(); i++)
				{
					s = s.add(m.col(i));
				}
				st.Push(s);
			}
			else
			{
				Algebraic s = m.row(1);
				for (int i = 2; i <= m.nrow(); i++)
				{
					s = s.add(m.row(i));
				}
				st.Push(s);
			}
			return 0;
		}
		if (narg != 4)
		{
			throw new ParseException("Usage: SUM (exp, ind, lo, hi)");
		}
		Algebraic exp = getAlgebraic(st);
		Variable v = getVariable(st);
		int lo = getInteger(st);
		int hi = getInteger(st);
		Algebraic sum = Zahl.ZERO;
		for (; lo <= hi; lo++)
		{
			sum = sum.add(exp.value(v,new Unexakt((double)lo)));
		}
		st.Push(sum);
		return 0;
	}
}
internal class LambdaLSUM : Lambda
{
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		if (narg != 3)
		{
			throw new ParseException("Usage: LSUM (exp, ind, list)");
		}
		Algebraic exp = getAlgebraic(st);
		Variable v = getVariable(st);
		Vektor list = getVektor(st);
		Algebraic sum = Zahl.ZERO;
		for (int i = 0; i < list.length(); i++)
		{
			sum = sum.add(exp.value(v,list.get(i)));
		}
		st.Push(sum);
		return 0;
	}
}
internal class LambdaDIVIDE : Lambda
{
	public override int lambda(Stack st)
	{
		int size = getNarg(st);
		if (size != 3 && size != 2)
		{
			throw new ParseException("Usage: DIVIDE (p1, p2, var)");
		}
		Algebraic p1 = getAlgebraic(st);
		if (!p1.exaktq())
		{
			p1 = (new LambdaRAT()).f_exakt(p1);
		}
		Algebraic p2 = getAlgebraic(st);
		if (!p2.exaktq())
		{
			p2 = (new LambdaRAT()).f_exakt(p2);
		}
		Algebraic[] a = new Algebraic[] {p1, p2};
		if (size == 3)
		{
			Variable v = getVariable(st);
			Poly.polydiv(a, v);
		}
		else
		{
			if (p1 is Zahl && p2 is Zahl)
			{
				a = ((Zahl)p1).div(p2, a);
			}
			else
			{
				a[0] = Poly.polydiv(p1, p2);
				a[1] = p1.sub(a[0].mult(p2));
			}
		}
		st.Push(new Vektor(a));
		return 0;
	}
}
internal class LambdaTAYLOR : Lambda
{
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		if (narg != 4)
		{
			throw new ParseException("Usage: TAYLOR (exp, var, pt, pow)");
		}
		Algebraic exp = getAlgebraic(st);
		Variable v = getVariable(st);
		Algebraic pt = getAlgebraic(st);
		int n = getInteger(st);
		Algebraic r = exp.value(v, pt);
		Algebraic t = (new Polynomial(v)).sub(pt);
		double nf = 1.0;
		for (int i = 1; i <= n; i++)
		{
			exp = exp.deriv(v);
			nf *= i;
			r = r.add(exp.value(v,pt).mult(t.pow_n(i)).div(new Unexakt(nf)));
		}
		st.Push(r);
		return 0;
	}
}
internal class LambdaSAVE : Lambda
{
	public override int lambda( Stack st )
	{
		int size = getNarg(st);

		if (size < 2)
		{
			throw new ParseException("Usage: SAVE (filename,arg1, arg2,...,argi)");
		}

		var filename = st.Pop();

		try
		{
			var f = Jasymca.getFileOutputStream( (string) filename, true );

			for ( var i = 1; i < size; i++ )
			{
				var name = ( string ) st.Pop();

				if ( "ALL".Equals( name, StringComparison.CurrentCultureIgnoreCase ) )
				{
					var en = pc.env.Keys.GetEnumerator();

					while ( en.MoveNext() )
					{
						var key = en.Current;

					    if ( "pi".Equals( ( string ) key, StringComparison.CurrentCultureIgnoreCase ) ) continue;

					    var val = pc.env.getValue( ( string ) key );

					    if ( val is Lambda ) continue;

					    var line = key + ":" + val + ";\n";

                        var bytes = Encoding.UTF8.GetBytes( line );

                        f.Write( bytes, 0, bytes.Length );
					}
				}
				else
				{
					var val = pc.env.getValue( name );

					var line = name + ":" + val + ";\n";

                    var bytes = Encoding.UTF8.GetBytes( line );

                    f.Write( bytes, 0, bytes.Length );
				}
			}

			f.Close();

			Console.WriteLine( "Wrote variables to " + filename );
		}
		catch ( Exception ex )
		{
			throw new JasymcaException( "Could not write to " + filename + " : " + ex.Message );
		}

		return 0;
	}
}

internal class LambdaLOADFILE : Lambda
{
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		if (narg != 1)
		{
			throw new ParseException("Usage: LOADFILE (filename)");
		}
		object filename = st.Pop();
		if (!(filename is string))
		{
			throw new JasymcaException(filename + " not a valid filename.");
		}
		try
		{
			readFile((string)filename);
			Console.WriteLine("Loaded Variables from " + filename);
		}
		catch (Exception e)
		{
			throw new JasymcaException("Could not read from " + filename + " :" + e.ToString());
		}
		return 0;
	}
	public static void readFile(string fname)
	{
	    if ( File.Exists( fname ) )
	    {
            Stream stream = new FileStream( Path.GetFullPath( fname ), FileMode.Open );
	        
            readFile( stream );
	    }
		//string sep = "/";
		//string s;
        //
		//var c = typeof( LambdaLOADFILE );
        //
		//for (int i = 0; i < pc.env.path.Count; i++)
		//{
		//	string dir = (string)pc.env.path[i];
        //
		//	s = fname.StartsWith(sep, StringComparison.Ordinal) ? dir + fname : dir + sep + fname;
        //
		//	Stream f = c.getResourceAsStream(s);
        //
		//	readFile(f);
        //
		//	return;
		//}

		throw new IOException("Could not open " + fname + ".");
	}
	public static void readFile( Stream f )
	{
		var old_stack = pc.stack;

		pc.stack = new Stack();

		try
		{
			while (true)
			{
				var code = pr.compile(f,null);

				if ( code == null )
				{
					break;
				}

				pc.process_list( code, true );
			}

			f.Close();

			pc.stack = old_stack;
		}
		catch ( Exception ex )
		{
			pc.stack = old_stack;

			throw new JasymcaException( ex.Message );
		}
	}
}

internal class LambdaRAT : LambdaAlgebraic
{
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		Algebraic arg = getAlgebraic(st).reduce();
		if (arg is Unexakt)
		{
			st.Push(f((Zahl)arg));
		}
		else if (arg is Exakt)
		{
			st.Push(arg);
		}
		else
		{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			st.Push(FunctionVariable.create(this.GetType().FullName.Substring("Lambda".Length).ToLower(), arg));
		}
		return 0;
	}
	internal override Algebraic f_exakt(Algebraic x)
	{
		if (x is Zahl)
		{
			return (Zahl)x.rat();
		}
		return x.map(this);
	}
	internal override Zahl f(Zahl x)
	{
		return (Zahl)x.rat();
	}
}
internal class LambdaSQFR : Lambda
{
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		Algebraic f = getAlgebraic(st);
		if (f is Zahl)
		{
			st.Push(f);
			return 0;
		}
		if (!(f is Polynomial))
		{
			throw new ParseException("Argument to sqfr() must be polynomial.");
		}
		f = ((Polynomial)f).rat();
		Algebraic[] fs = ((Polynomial)f).square_free_dec(((Polynomial)f).v);
		if (fs == null)
		{
			st.Push(f);
			return 0;
		}
		st.Push(new Vektor(fs));
		return 0;
	}
}
internal class LambdaALLROOTS : Lambda
{
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		Algebraic x = getAlgebraic(st);
		if (x is Vektor)
		{
			x = new Polynomial(new SimpleVariable("x"), (Vektor)x);
		}
		if (!(x is Polynomial))
		{
			throw new JasymcaException("Argument to allroots must be polynomial.");
		}
		Polynomial p = (Polynomial)((Polynomial)x).rat();
		Algebraic[] ps = p.square_free_dec(p.v);
		Vektor r;
		ArrayList v = new ArrayList();
		for (int i = 0; i < ps.Length; i++)
		{
			if (ps[i] is Polynomial)
			{
				r = ((Polynomial)ps[i]).monic().roots();
				for (int k = 0; r != null && k < r.length() ; k++)
				{
					for (int j = 0; j <= i; j++)
					{
						v.Add(r.get(k));
					}
				}
			}
		}
		st.Push(Vektor.create(v));
		return 0;
	}
}
internal class LambdaDET : Lambda
{
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		Matrix m = new Matrix(getAlgebraic(st));
		st.Push(m.det());
		return 0;
	}
}
internal class LambdaEIG : Lambda
{
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		Matrix m = new Matrix(getAlgebraic(st));
		st.Push(m.eigenvalues());
		return 0;
	}
}
internal class LambdaINV : Lambda
{
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		Matrix m = new Matrix(getAlgebraic(st));
		st.Push(m.invert());
		return 0;
	}
}
internal class LambdaPINV : Lambda
{
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		Matrix m = new Matrix(getAlgebraic(st));
		st.Push(m.pseudoinverse());
		return 0;
	}
}
internal class LambdaHILB : Lambda
{
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		int n = getInteger(st);
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: Algebraic[][] a = new Algebraic[n][n];
		Algebraic[][] a = RectangularArrays.ReturnRectangularAlgebraicArray(n, n);
		for (int i = 0; i < n; i++)
		{
			for (int k = 0; k < n; k++)
			{
				a[i][k] = new Exakt(1L, (long)(i + k + 1));
			}
		}
			st.Push(new Matrix(a));
			return 0;
	}
}
internal class LambdaLU : Lambda
{
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		Matrix m = (new Matrix(getAlgebraic(st))).copy();
		Matrix B = new Matrix(1,1);
		Matrix P = new Matrix(1,1);
		m.rank_decompose(B, P);
		if (length != 2 && length != 3)
		{
			throw new JasymcaException("Usage: [l,u,p] = LU( Matrix ).");
		}
		if (length >= 2)
		{
			st.Push(B);
			st.Push(m);
			if (length == 3)
			{
				st.Push(P);
			}
		}
		length = 1;
		return 0;
	}
}
internal class LambdaSQRT : LambdaAlgebraic
{
	public LambdaSQRT()
	{
		diffrule = "1/(2*sqrt(x))";
		intrule = "2/3*x*sqrt(x)";
	}
	internal static string intrule2 = "(2*a*x+b)*sqrt(X)/(4*a)+(4*a*c-b*b)/(8*a*sqrt(a))*log(2*sqrt(a*X)+2*a*x+b)";
	public override Algebraic integrate(Algebraic arg, Variable x)
	{
		try
		{
			return base.integrate(arg,x);
		}
		catch (JasymcaException)
		{
			if (!(arg.depends(x)))
			{
				throw new JasymcaException("Expression in function does not depend on Variable.");
			}
			if (!(arg is Polynomial) || ((Polynomial)arg).degree() != 2 || !((Polynomial)arg).ratfunc(x))
			{
				throw new JasymcaException("Can not integrate function ");
			}
			Algebraic xp = new Polynomial(x);
			Polynomial X = (Polynomial)arg;
			Algebraic y = evalx(intrule2, xp);
			y = y.value(new SimpleVariable("X"), X);
			y = y.value(new SimpleVariable("a"), X.a[2]);
			y = y.value(new SimpleVariable("b"), X.a[1]);
			y = y.value(new SimpleVariable("c"), X.a[0]);
			y = (new SqrtExpand()).f_exakt(y);
			return y;
		}
	}
	internal override Zahl f(Zahl x)
	{
		Unexakt z = x.unexakt();
		if (z.imag == 0.0)
		{
			if (z.real < 0.0)
			{
				return new Unexakt(0, Math.Sqrt(-z.real));
			}
			return new Unexakt(Math.Sqrt(z.real));
		}
		double sr = Math.Sqrt(Math.Sqrt(z.real * z.real + z.imag * z.imag));
		double phi = JMath.atan2(z.imag,z.real) / 2.0;
		return new Unexakt(sr * Math.Cos(phi), sr * Math.Sin(phi));
	}
	internal override Algebraic f_exakt(Algebraic x)
	{
		if (x.Equals(Zahl.ONE) || x.Equals(Zahl.ZERO))
		{
			return x;
		}
		if (x.Equals(Zahl.MINUS))
		{
			return Zahl.IONE;
		}
		if (x is Zahl)
		{
			return fzexakt((Zahl)x);
		}
		if (x is Polynomial && ((Polynomial)x).degree() == 1 && ((Polynomial)x).a[0].Equals(Zahl.ZERO) && ((Polynomial)x).a[1].Equals(Zahl.ONE) && ((Polynomial)x).v is FunctionVariable && ((FunctionVariable)((Polynomial)x).v).fname.Equals("exp"))
		{
			return FunctionVariable.create("exp", ((FunctionVariable)((Polynomial)x).v).arg.div(Zahl.TWO));
		}
		return null;
	}
	internal virtual Algebraic fzexakt(Zahl x)
	{
		if (x is Exakt && !x.komplexq())
		{
			if (x.smaller(Zahl.ZERO))
			{
				Algebraic r = fzexakt((Zahl)x.mult(Zahl.MINUS));
				if (r != null)
				{
					return Zahl.IONE.mult(r);
				}
				return r;
			}
			long nom = (long)((Exakt)x).real[0].longValue();
			long den = (long)((Exakt)x).real[1].longValue();
			long a0 = introot(nom), a1 = nom / (a0 * a0);
			long b0 = introot(den), b1 = den / (b0 * b0);
			BigInteger[] br = new BigInteger[] {BigInteger.valueOf(a0), BigInteger.valueOf(b0 * b1)};
			Exakt r1 = new Exakt(br);
			a0 = a1 * b1;
			if (a0 == 1L)
			{
				return r1;
			}
			return r1.mult(new Polynomial(new FunctionVariable("sqrt", new Exakt(BigInteger.valueOf(a0)), this)));
		}
		return null;
	}
	internal virtual long introot(long x)
	{
		long s = 1L; long f ; long g ; long[] t = new long[] {2L, 3L, 5L};
		for (int i = 0; i < t.Length; i++)
		{
			g = t[i];
			f = g * g;
			while (x % f == 0L && x != 1L)
			{
				s *= g;
				x /= f;
			}
		}
		for (long i = 6L; x != 1L ; i += 6L)
		{
			g = i + 1;
			f = g * g;
			while (x % f == 0L && x != 1L)
			{
				s *= g;
				x /= f;
			}
			g = i + 5;
			f = g * g;
			while (x % f == 0L && x != 1L)
			{
				s *= g;
				x /= f;
			}
			if (f > x)
			{
				break;
			}
		}
		return s;
	}
}
internal class LambdaSIGN : LambdaAlgebraic
{
	public LambdaSIGN()
	{
		diffrule = "x-x";
		intrule = "x*sign(x)";
	}
	internal override Algebraic f_exakt(Algebraic x)
	{
		if (x is Zahl)
		{
			return f((Zahl)x);
		}
		return null;
	}
	internal override Zahl f(Zahl x)
	{
		return x.smaller(Zahl.ZERO)?Zahl.MINUS:Zahl.ONE;
	}
}
internal class LambdaABS : LambdaAlgebraic
{
	public LambdaABS()
	{
		diffrule = "sign(x)";
		intrule = "sign(x)*x^2/2";
	}
	internal override Algebraic f_exakt(Algebraic x)
	{
		if (x is Zahl)
		{
			return f((Zahl)x);
		}
		return FunctionVariable.create("sqrt", x.mult(x.cc()));
	}
	internal override Zahl f(Zahl x)
	{
		return new Unexakt(x.norm());
	}
}
internal class ExpandUser : LambdaAlgebraic
{
	internal override Algebraic f_exakt(Algebraic x1)
	{
		if (!(x1 is Polynomial))
		{
			return x1.map(this);
		}
		Polynomial p = (Polynomial)x1;
		if (p.v is SimpleVariable)
		{
			return p.map(this);
		}
		FunctionVariable f = (FunctionVariable)p.v;
		object lx = pc.env.getValue(f.fname);
		if (!(lx is UserFunction))
		{
			return p.map(this);
		}
		UserFunction la = (UserFunction)lx;
		if (!(la.body is Algebraic))
		{
			return x1;
		}
		Algebraic body = (Algebraic)la.body;
		Algebraic x;
		if (la.@var.Length == 1)
		{
			x = body.value(la.@var[0], f.arg);
		}
		else if (f.arg is Vektor && ((Vektor)f.arg).length() == la.@var.Length)
		{
			x = la.fv((Vektor)f.arg);
		}
		else
		{
			throw new JasymcaException("Wrong argument to function " + la.fname);
		}
		Algebraic r = Zahl.ZERO;
		for (int i = p.a.Length - 1; i > 0; i--)
		{
			r = r.add(f_exakt(p.a[i])).mult(x);
		}
		if (p.a.Length > 0)
		{
			r = r.add(f_exakt(p.a[0]));
		}
		return r;
	}
}
internal class ASS : Lambda
{
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		object[] val = new object[narg];
		for (int i = 0; i < narg; i++)
		{
			val[i] = st.Pop();
		}
		for (int i = narg - 1; i >= 0; i--)
		{
			string name = getSymbol(st);
			if (!name.StartsWith("$", StringComparison.Ordinal))
			{
				throw new JasymcaException("Illegal lvalue: " + name);
			}
			name = name.Substring(1);
			bool idxq = st.Count > 0 && st.Peek() is int?;
			if (!idxq)
			{
				pc.env.putValue(name, val[i]);
				if (val[i] is Algebraic)
				{
					((Algebraic)val[i]).name = name;
				}
			}
			else
			{
				if (!(val[i] is Algebraic))
				{
					throw new JasymcaException("No index allowed here: " + val[i]);
				}
				Matrix rhs = new Matrix((Algebraic)val[i]);
				Matrix lhs = new Matrix((Algebraic)pc.env.getValue(name));
				Index idx = Index.createIndex(st, lhs);
				lhs.insert(rhs, idx);
				val[i] = lhs.reduce();
				pc.env.putValue(name, val[i]);
			}
		}
		for (int i = 0; i < narg; i++)
		{
			st.Push(val[i]);
		}
		return 0;
	}
	internal static int lambdap(Stack st, Lambda op)
	{
		int narg = getNarg(st);
		object y = st.Pop();
		string name = getSymbol(st);
		if (!name.StartsWith("$", StringComparison.Ordinal))
		{
			throw new JasymcaException("Illegal lvalue: " + name);
		}
		List t = Comp.vec2list(new ArrayList());
		t.Add(name);
		t.Add(name.Substring(1));
		t.Add(y);
		t.Add(new int?(2));
		t.Add(op);
		t.Add(new int?(1));
		t.Add(Operator.get("=").Lambda);
		pc.process_list(t, true);
		return 0;
	}
	internal static int lambdai(Stack st, bool sign, bool pre)
	{
		int narg = getNarg(st);
		string name = getSymbol(st);
		if (!name.StartsWith("$", StringComparison.Ordinal))
		{
			throw new JasymcaException("Illegal lvalue: " + name);
		}
		object p = null;
		if (!pre)
		{
			p = pc.env.getValue(name.Substring(1));
		}
		List t = Comp.vec2list(new ArrayList());
		t.Add(name);
		t.Add(name.Substring(1));
		t.Add(Zahl.ONE);
		t.Add(new int?(2));
		t.Add(sign ? Operator.get("+").Lambda : Operator.get("-").Lambda);
		t.Add(new int?(1));
		t.Add(Operator.get("=").Lambda);
		pc.process_list(t, true);
		if (!pre && p != null)
		{
			if (p is Algebraic)
			{
				((Algebraic)p).name = null;
			}
			st.Pop();
			st.Push(p);
		}
		return 0;
	}
}
internal class LambdaWHO : Lambda
{
	public override int lambda(Stack st)
	{
		if (pc.ps != null)
		{
			pc.ps.println(pc.env.ToString());
		}
		return 0;
	}
}
internal class LambdaADDPATH : Lambda
{
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		while (narg-- > 0)
		{
			object s = st.Pop();
			if (!(s is string))
			{
				throw new JasymcaException("Usage: ADDPATH( dir1, dir2, ... )");
			}
			pc.env.addPath(((string)s).Substring(1));
		}
		return 0;
	}
}
internal class LambdaPATH : Lambda
{
	public override int lambda(Stack st)
	{
        int n = pc.env.path.Count;

		var s = "";

		while ( n-- > 0 )
		{
			var p = pc.env.path[n];

			s = s + p;

			if ( n != 0 )
			{
				s = s + ":";
			}
		}

		if ( pc.ps != null )
		{
			pc.ps.println(s);
		}

		return 0;
	}
}

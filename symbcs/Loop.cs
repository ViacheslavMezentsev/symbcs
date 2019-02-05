using System;
using System.Collections;
using System.Text;
using System.Threading;

internal class LambdaERROR : Lambda
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
	public override int lambda(Stack st)
	{
		LambdaPRINTF.printf(st);
		return Processor.ERROR;
	}
}
internal class LambdaEVAL : Lambda
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		object s_in = st.Pop();
		if (!(s_in is string))
		{
			throw new JasymcaException("Argument to EVAL must be string.");
		}
		string s = (string)s_in;
		List pgm = pr.compile(s);
		return pc.process_list(pgm, true);
	}
}
internal class LambdaBLOCK : Lambda
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		Environment local = pc.env.copy();
		List code = getList(st);
		Stack ups = new Stack();
		int ret = UserProgram.process_block(code, ups, local, false);
		pc.env.update(local);
		if (ret != Processor.ERROR && ups.Count > 0)
		{
			object y = ups.Pop();
			st.Push(y);
		}
		else
		{
			throw new JasymcaException("Error processing block.");
		}
		return 0;
	}
}
internal class LambdaBRANCH : Lambda
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
	public override int lambda(Stack st)
	{
		int narg = getNarg(st), sel ;
		List cond, b_true, b_false;
		switch (narg)
		{
			case 2:
				cond = getList(st);
				b_true = getList(st);
				pc.process_list(cond, true);
				sel = getInteger(pc.stack);
				if (sel == 1)
				{
					return pc.process_list(b_true, true);
				}
				else if (sel != 0)
				{
					throw new JasymcaException("Branch requires boolean type.");
				}
				break;
			case 3:
				cond = getList(st);
				b_true = getList(st);
				b_false = getList(st);
				pc.process_list(cond, true);
				sel = getInteger(pc.stack);
				if (sel == 1)
				{
					return pc.process_list(b_true, true);
				}
				else if (sel == 0)
				{
					return pc.process_list(b_false, true);
				}
				else
				{
					throw new JasymcaException("Branch requires boolean type, got " + sel);
				}
				default:
					throw new JasymcaException("Wrong number of arguments to branch.");
		}
		return 0;
	}
}
internal class LambdaFOR : Lambda
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		List cond = getList(st);
		List body = getList(st);
		pc.process_list(cond, true);
		if (pc.stack.Count == 0 || !(pc.stack.Peek() is Vektor) || ((Algebraic)pc.stack.Peek()).name == null)
		{
			throw new ParseException("Wrong format in for-loop.");
		}
		Vektor vals = (Vektor)pc.stack.Pop();
		for (int i = 0; i < vals.length() ; i++)
		{
			pc.env.putValue(vals.name, vals.get(i));
			int ret = pc.process_list(body, true);
			switch (ret)
			{
				case Processor.BREAK:
					return 0;
				case Processor.RETURN:
			case Processor.EXIT:
		case Processor.ERROR:
			return ret;
		case Processor.CONTINUE: break;
			}
		}
		return 0;
	}
}
internal class LambdaXFOR : Lambda
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		List cond = getList(st);
		List step_in = getList(st);
		List thru_in = getList(st);
		List body = getList(st);
		pc.process_list(cond, true);
		if (pc.stack.Count == 0 || !(pc.stack.Peek() is Zahl) || ((Algebraic)pc.stack.Peek()).name == null)
		{
			throw new ParseException("Non-constant initializer in for loop.");
		}
		Zahl x = (Zahl)pc.stack.Pop();
		string xname = x.name;
		pc.process_list(step_in, true);
		if (pc.stack.Count == 0 || !(pc.stack.Peek() is Zahl))
		{
			throw new ParseException("Step size must be constant.");
		}
		Zahl step = (Zahl)pc.stack.Pop();
		pc.process_list(thru_in, true);
		if (pc.stack.Count == 0 || !(pc.stack.Peek() is Zahl))
		{
			throw new ParseException("Wrong format in for-loop.");
		}
		Zahl thru = (Zahl)pc.stack.Pop();
		bool pos = !step.smaller(Zahl.ZERO);
		while (true)
		{
			if ((pos ? thru.smaller(x) : x.smaller(thru)))
			{
				break;
			}
			pc.env.putValue(xname, x);
			int ret = pc.process_list(body, true);
			switch (ret)
			{
				case Processor.BREAK:
					return 0;
				case Processor.RETURN:
			case Processor.EXIT:
		case Processor.ERROR:
			return ret;
		case Processor.CONTINUE: break;
			}
			x = (Zahl)x.add(step);
		}
		return 0;
	}
}
internal class LambdaWHILE : Lambda
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		List cond = getList(st);
		List body = getList(st);
		while (true)
		{
			pc.process_list(cond, true);
			object c = pc.stack.Pop();
			if (c.Equals(Zahl.ZERO))
			{
				break;
			}
			else if (!c.Equals(Zahl.ONE))
			{
				throw new JasymcaException("Not boolean: " + c);
			}
			int ret = pc.process_list(body, true);
			switch (ret)
			{
				case Processor.BREAK:
					return 0;
				case Processor.RETURN:
			case Processor.EXIT:
		case Processor.ERROR:
			return ret;
		case Processor.CONTINUE: break;
			}
		}
		return 0;
	}
}
internal class LambdaPRINTF : Lambda
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
	public override int lambda(Stack st)
	{
		printf(st);
		return 0;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static void printf(Stack st)throws ParseException, JasymcaException
	internal static void printf(Stack st)
	{
		int narg = getNarg(st);
		object s_in = st.Pop();
		if (!(s_in is string))
		{
			throw new JasymcaException("Argument to PRINTF must be string.");
		}
		string fmt = (string)s_in;
		int idx , i = 1;
		string cs = "%f";
		while ((idx = fmt.IndexOf(cs, StringComparison.Ordinal)) != -1 && st.Count > 0 && narg-- > 1)
		{
			object n = st.Pop();
			if (n != null)
			{
				StringBuilder sb = new StringBuilder(fmt);
				sb.Remove(idx, idx + cs.Length - idx);
				sb.Insert(idx, n.ToString());
				fmt = sb.ToString();
			}
			else
			{
				break;
			}
		} while ((idx = fmt.IndexOf("\\n", StringComparison.Ordinal)) != -1)
		{
			StringBuilder sb = new StringBuilder(fmt);
			sb.Remove(idx, idx + "\\n".Length - idx);
			sb.Insert(idx, "\n");
			fmt = sb.ToString();
		}
		if (pc.ps != null)
		{
			pc.ps.print(fmt.ToString());
		}
	}
}
internal class LambdaPAUSE : Lambda
{
	private readonly LambdaPRINTF outerInstance;

	public LambdaPAUSE(LambdaPRINTF outerInstance)
	{
		this.outerInstance = outerInstance;
	}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		int millis = Math.Abs(getInteger(st));
		try
		{
			Thread.Sleep(millis);
		}
		catch (Exception)
		{
		}
		return 0;
	}
}
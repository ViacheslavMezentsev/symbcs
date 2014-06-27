using System.Collections;

internal class LambdaSOLVE : Lambda
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(java.util.Stack st) throws ParseException, JasymcaException
	public virtual int lambda(Stack st)
	{
		int narg = getNarg(st);
		if (narg != 2)
		{
			throw new ParseException("solve requires 2 arguments.");
		}
		Algebraic expr = getAlgebraic(st).rat();
		if (!(expr is Polynomial || expr is Rational))
		{
			throw new JasymcaException("Wrong format for Expression in solve.");
		}
		Variable @var = getVariable(st);
		Algebraic r = solve(expr, @var).reduce();
		st.Push(r);
		return 0;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static Algebraic linfaktor(Algebraic expr,Variable var) throws JasymcaException
	public static Algebraic linfaktor(Algebraic expr, Variable @var)
	{
		if (expr is Vektor)
		{
			Algebraic[] cn = new Algebraic[((Vektor)expr).length()];
			for (int i = 0; i < ((Vektor)expr).length(); i++)
			{
				cn[i] = linfaktor(((Vektor)expr).get(i),@var);
			}
			return new Vektor(cn);
		}
		return (new Polynomial(@var)).sub(expr);
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static Vektor solve(Algebraic expr, Variable var) throws JasymcaException
	public static Vektor solve(Algebraic expr, Variable @var)
	{
		p("Solve: " + expr + " = 0, Variable: " + @var);
		expr = (new ExpandUser()).f_exakt(expr);
		expr = (new TrigExpand()).f_exakt(expr);
		p("TrigExpand: " + expr);
		expr = (new NormExp()).f_exakt(expr);
		p("Norm: " + expr);
		expr = (new CollectExp(expr)).f_exakt(expr);
		p("Collect: " + expr);
		expr = (new SqrtExpand()).f_exakt(expr);
		p("SqrtExpand: " + expr);
		if (expr is Rational)
		{
			expr = (new LambdaRAT()).f_exakt(expr);
			if (expr is Rational)
			{
				expr = ((Rational)expr).nom;
			}
		}
		p("Canonic Expression: " + expr);
		if (!(expr is Polynomial) || !((Polynomial)expr).depends(@var))
		{
			throw new JasymcaException("Expression does not depend of variable.");
		}
		Polynomial p = (Polynomial)expr;
		Vektor sol = null;
		ArrayList dep = depvars(p,@var);
		if (dep.Count == 0)
		{
			throw new JasymcaException("Expression does not depend of variable.");
		}
		if (dep.Count == 1)
		{
			Variable dvar = (Variable)dep[0];
			p("Found one Variable: " + dvar);
			sol = p.solve(dvar);
			p("Solution: " + dvar + " = " + sol);
			if (!dvar.Equals(@var))
			{
				ArrayList s = new ArrayList();
				for (int i = 0; i < sol.length(); i++)
				{
					p("Invert: " + sol.get(i) + " = " + dvar);
					Algebraic sl = finvert((FunctionVariable)dvar, sol.get(i));
					p("Result: " + sl + " = 0");
					Vektor t = solve(sl, @var);
					p("Solution: " + @var + " = " + t);
					for (int k = 0; k < t.length(); k++)
					{
						Algebraic tn = t.get(k);
						if (!s.Contains(tn))
						{
							s.Add(tn);
						}
					}
				}
				sol = Vektor.create(s);
			}
		}
		else if (dep.Count == 2)
		{
			p("Found two Variables: " + dep[0] + ", " + dep[1]);
			if (dep.Contains(@var))
			{
				FunctionVariable f = (FunctionVariable)(dep[0].Equals(@var)? dep[1]:dep[0]);
				if (f.fname.Equals("sqrt"))
				{
					p("Solving " + p + " for " + f);
					sol = p.solve(f);
					p("Solution: " + f + " = " + sol);
					ArrayList s = new ArrayList();
					for (int i = 0; i < sol.length(); i++)
					{
						p("Invert: " + sol.get(i) + " = " + f);
						Algebraic sl = finvert((FunctionVariable)f, sol.get(i));
						p("Result: " + sl + " = 0");
						if (sl is Polynomial && depvars(((Polynomial)sl),@var).Count == 1)
						{
							p("Solving " + sl + " for " + @var);
							Vektor t = solve(sl, @var);
							p("Solution: " + @var + " = " + t);
							for (int k = 0; k < t.length(); k++)
							{
								Algebraic tn = t.get(k);
								if (!s.Contains(tn))
								{
									s.Add(tn);
								}
							}
						}
						else
						{
							throw new JasymcaException("Could not solve equation.");
						}
					}
					sol = Vektor.create(s);
				}
				else
				{
					throw new JasymcaException("Can not solve equation.");
				}
			}
			else
			{
				throw new JasymcaException("Can not solve equation.");
			}
		}
		else
		{
			throw new JasymcaException("Can not solve equation.");
		}
		return sol;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static java.util.Vector depvars(Polynomial p, Variable var) throws JasymcaException
	private static ArrayList depvars(Polynomial p, Variable @var)
	{
		ArrayList r = new ArrayList();
		if (!p.@var.deriv(@var).Equals(Zahl.ZERO))
		{
			r.Add(p.@var);
		}
		for (int i = 0; i < p.a.Length; i++)
		{
			if (p.a[i] is Polynomial)
			{
				ArrayList c = depvars((Polynomial)p.a[i],@var);
				if (c.Count > 0)
				{
					for (int k = 0; k < c.Count; k++)
					{
						object v = c[k];
						if (!r.Contains(v))
						{
							r.Add(v);
						}
					}
				}
			}
		}
		return r;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static Algebraic finvert(FunctionVariable f, Algebraic b) throws JasymcaException
	internal static Algebraic finvert(FunctionVariable f, Algebraic b)
	{
		if (f.fname.Equals("sqrt"))
		{
			return b.mult(b).sub(f.arg);
		}
		if (f.fname.Equals("exp"))
		{
			return FunctionVariable.create("log",b).sub(f.arg);
		}
		if (f.fname.Equals("log"))
		{
			return FunctionVariable.create("exp",b).sub(f.arg);
		}
		if (f.fname.Equals("tan"))
		{
			return FunctionVariable.create("atan",b).sub(f.arg);
		}
		if (f.fname.Equals("atan"))
		{
			return FunctionVariable.create("tan",b).sub(f.arg);
		}
		throw new JasymcaException("Could not invert " + f);
	}
}
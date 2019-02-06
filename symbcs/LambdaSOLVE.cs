using System.Collections;

internal class LambdaSOLVE : Lambda
{
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
	public static Vektor solve(Algebraic expr, Variable @var)
	{
		debug("Solve: " + expr + " = 0, Variable: " + @var);
		expr = (new ExpandUser()).f_exakt(expr);
		expr = (new TrigExpand()).f_exakt(expr);
		debug("TrigExpand: " + expr);
		expr = (new NormExp()).f_exakt(expr);
		debug("Norm: " + expr);
		expr = (new CollectExp(expr)).f_exakt(expr);
		debug("Collect: " + expr);
		expr = (new SqrtExpand()).f_exakt(expr);
		debug("SqrtExpand: " + expr);
		if (expr is Rational)
		{
			expr = (new LambdaRAT()).f_exakt(expr);
			if (expr is Rational)
			{
				expr = ((Rational)expr).nom;
			}
		}
		debug("Canonic Expression: " + expr);
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
			debug("Found one Variable: " + dvar);
			sol = p.solve(dvar);
			debug("Solution: " + dvar + " = " + sol);
			if (!dvar.Equals(@var))
			{
				ArrayList s = new ArrayList();
				for (int i = 0; i < sol.length(); i++)
				{
					debug("Invert: " + sol.get(i) + " = " + dvar);
					Algebraic sl = finvert((FunctionVariable)dvar, sol.get(i));
					debug("Result: " + sl + " = 0");
					Vektor t = solve(sl, @var);
					debug("Solution: " + @var + " = " + t);
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
			debug("Found two Variables: " + dep[0] + ", " + dep[1]);
			if (dep.Contains(@var))
			{
				FunctionVariable f = (FunctionVariable)(dep[0].Equals(@var)? dep[1]:dep[0]);
				if (f.fname.Equals("sqrt"))
				{
					debug("Solving " + p + " for " + f);
					sol = p.solve(f);
					debug("Solution: " + f + " = " + sol);
					ArrayList s = new ArrayList();
					for (int i = 0; i < sol.length(); i++)
					{
						debug("Invert: " + sol.get(i) + " = " + f);
						Algebraic sl = finvert((FunctionVariable)f, sol.get(i));
						debug("Result: " + sl + " = 0");
						if (sl is Polynomial && depvars(((Polynomial)sl),@var).Count == 1)
						{
							debug("Solving " + sl + " for " + @var);
							Vektor t = solve(sl, @var);
							debug("Solution: " + @var + " = " + t);
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
	private static ArrayList depvars(Polynomial p, Variable @var)
	{
		ArrayList r = new ArrayList();
		if (!p.v.deriv(@var).Equals(Zahl.ZERO))
		{
			r.Add(p.v);
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
using System.Collections;

internal class LambdaALGSYS : Lambda
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		if (narg != 2)
		{
			throw new ParseException("algsys requires 2 arguments.");
		}
		Algebraic expr = getAlgebraic(st).rat();
		Algebraic vars = getAlgebraic(st);
		if (!(expr is Vektor) || !(vars is Vektor) || ((Vektor)expr).length() != ((Vektor)vars).length())
		{
			throw new ParseException("Wrong type of arguments to algsys.");
		}
		expr = (new ExpandUser()).f_exakt(expr);
		expr = (new TrigExpand()).f_exakt(expr);
		expr = (new NormExp()).f_exakt(expr);
		expr = (new CollectExp(expr)).f_exakt(expr);
		expr = (new SqrtExpand()).f_exakt(expr);
		ArrayList v = new ArrayList();
		for (int i = 0; i < ((Vektor)vars).length(); i++)
		{
			Algebraic p = ((Vektor)vars).get(i);
			if (!(p is Polynomial))
			{
				throw new ParseException("Wrong type of arguments to algsys.");
			}
			v.Add(((Polynomial)p).@var);
		}
		st.Push(solvesys(((Vektor)expr).vector(), v));
		return 0;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Vektor solvesys(Vector expr, Vector x) throws JasymcaException
	internal virtual Vektor solvesys(ArrayList expr, ArrayList x)
	{
		int nvars = x.Count;
		ArrayList lsg = new ArrayList(), vars = new ArrayList();
		lsg.Add(expr);
		vars.Add(x);
		int n = nvars;
		while (n > 0)
		{
			for (int i = 0; i < lsg.Count; i++)
			{
				try
				{
					ArrayList equ = (ArrayList)lsg[i];
					ArrayList xv = (ArrayList)vars[i];
					Vektor sol = solve(equ, xv, n);
					lsg.RemoveAt(i);
					vars.RemoveAt(i);
					Variable v = (Variable)xv[n - 1];
					for (int k = 0; k < sol.length(); k++)
					{
						ArrayList eq = new ArrayList();
						for (int j = 0; j < n - 1; j++)
						{
							eq.Add(((Algebraic)equ[j]).value(v, sol.get(k)));
						}
						eq.Add(sol.get(k));
						for (int j = n; j < nvars; j++)
						{
							eq.Add(equ[j]);
						}
						lsg.Insert(i, eq);
						vars.Insert(i, clonev(xv));
						i++;
					}
				}
				catch (JasymcaException)
				{
					lsg.RemoveAt(i);
					vars.RemoveAt(i);
					i--;
				}
			}
			if (lsg.Count == 0)
			{
				throw new JasymcaException("Could not solve equations.");
			}
			n--;
		}
		for (int i = 0; i < lsg.Count; i++)
		{
			ArrayList equ = (ArrayList)lsg[i];
			ArrayList xv = (ArrayList)vars[i];
			for (n = 1; n < nvars; n++)
			{
				Algebraic y = (Algebraic)equ[n - 1];
				Variable v = (Variable)xv[n - 1];
				for (int k = n; k < nvars; k++)
				{
					Algebraic z = (Algebraic)equ[k];
					equ.RemoveAt(k);
					equ.Insert(k, z.value(v,y));
				}
			}
		}
		for (int i = 0; i < lsg.Count; i++)
		{
			ArrayList equ = (ArrayList)lsg[i];
			ArrayList xv = (ArrayList)vars[i];
			for (n = 0; n < nvars; n++)
			{
				Variable v = (Variable)xv[n];
				Algebraic y = (new Polynomial(v)).sub((Algebraic)equ[n]);
				equ.RemoveAt(n);
				equ.Insert(n, y);
			}
		}
		Vektor[] r = new Vektor[lsg.Count];
		for (int i = 0; i < lsg.Count; i++)
		{
			r[i] = Vektor.create((ArrayList)lsg[i]);
		}
		return new Vektor(r);
	}
	internal virtual ArrayList clonev(ArrayList v)
	{
		ArrayList r = new ArrayList(v.Count);
		for (int i = 0; i < v.Count; i++)
		{
			r.Add(v[i]);
		}
		return r;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Vektor solve(Vector expr, Vector x, int n) throws JasymcaException
	internal virtual Vektor solve(ArrayList expr, ArrayList x, int n)
	{
		Algebraic equ = null;
		Variable v = null;
		int i , k , iv = 0, ke = 0;
		for (i = 0; i < n && equ == null; i++)
		{
			v = (Variable)x[i];
			double norm = -1.0;
			for (k = 0; k < n; k++)
			{
				Algebraic exp = (Algebraic)expr[k];
				if (exp is Rational)
				{
					exp = ((Rational)exp).nom;
				}
				Algebraic slope = exp.deriv(v);
				if (!slope.Equals(Zahl.ZERO) && slope is Zahl)
				{
					double nm = slope.norm() / exp.norm();
					if (nm > norm)
					{
						norm = nm;
						equ = exp;
						ke = k;
						iv = i;
					}
				}
			}
		}
		if (equ == null)
		{
			for (i = 0; i < n && equ == null; i++)
			{
				v = (Variable)x[i];
				for (k = 0; k < n; k++)
				{
					Algebraic exp = (Algebraic)expr[k];
					if (exp is Rational)
					{
						exp = ((Rational)exp).nom;
					}
					if (exp.depends(v))
					{
						equ = exp;
						ke = k;
						iv = i;
						break;
					}
				}
			}
		}
		if (equ == null)
		{
			throw new JasymcaException("Expressions do not depend of Variables.");
		}
		Vektor sol = LambdaSOLVE.solve(equ, v);
		expr.RemoveAt(ke);
		expr.Insert(n - 1, equ);
		x.RemoveAt(iv);
		x.Insert(n - 1, v);
		return sol;
	}
}
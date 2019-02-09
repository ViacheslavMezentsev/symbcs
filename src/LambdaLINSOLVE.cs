using System.Collections;

public class LambdaLINSOLVE : Lambda
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		if (narg != 2)
		{
			throw new ParseException("linsolve requires 2 arguments.");
		}
		Algebraic M_in = getAlgebraic(st);
		Algebraic b_in = getAlgebraic(st);
		Matrix M = new Matrix(M_in);
		Matrix b = (b_in is Vektor ? Matrix.column((Vektor)b_in) : new Matrix(b_in));
		Algebraic r = ((Matrix)b.transpose().div(M.transpose())).transpose().reduce();
		st.Push(r);
		return 0;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda2(Stack st) throws ParseException, JasymcaException
	public virtual int lambda2(Stack st)
	{
		int narg = getNarg(st);
		if (narg != 2)
		{
			throw new ParseException("linsolve requires 2 arguments.");
		}
		Vektor expr = (Vektor)getVektor(st).rat();
		Vektor vars = getVektor(st);
		elim(expr, vars, 0);
		subst(expr, vars, expr.length() - 1);
		st.Push(expr);
		return 0;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static void subst(Vektor expr, Vektor vars, int n) throws JasymcaException
	private static void subst(Vektor expr, Vektor vars, int n)
	{
		if (n < 0)
		{
			return;
		}
		Algebraic pa = expr.get(n);
		if (pa is Polynomial)
		{
			Polynomial p = (Polynomial)pa;
			Variable v = null;
			Algebraic c1 = null, c0 ;
			for (int k = 0; k < vars.length(); k++)
			{
				Variable va = ((Polynomial)vars.get(k)).v;
				c1 = p.coefficient(va,1);
				if (!c1.Equals(Zahl.ZERO))
				{
					v = va;
					break;
				}
			}
			if (v != null)
			{
				expr.set(n, p.div(c1));
				Algebraic val = p.coefficient(v,0).mult(Zahl.MINUS).div(c1);
				for (int k = 0; k < n; k++)
				{
					Algebraic ps = expr.get(k);
					if (ps is Polynomial)
					{
						expr.set(k, ((Polynomial)ps).value(v,val));
					}
				}
			}
		}
		subst(expr,vars,n - 1);
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static void elim(Vektor expr, Vektor vars, int n) throws JasymcaException
	private static void elim(Vektor expr, Vektor vars, int n)
	{
		if (n >= expr.length())
		{
			return;
		}
		double maxc = 0.0;
		int iv = 0, ie = 0;
		Variable vp = null;
		Algebraic f = Zahl.ONE;
		Polynomial pm = null;
		for (int i = 0; i < vars.length(); i++)
		{
			Variable v = ((Polynomial)vars.get(i)).v;
			for (int k = n; k < expr.length(); k++)
			{
				Algebraic pa = expr.get(k);
				if (pa is Polynomial)
				{
					Polynomial p = (Polynomial)pa;
					Algebraic c = p.coefficient(v, 1);
					double nm = c.norm();
					if (nm > maxc)
					{
						maxc = nm;
						vp = v;
						ie = k;
						iv = i;
						f = c;
						pm = p;
					}
				}
			}
		}
		if (maxc == 0.0)
		{
			return;
		}
		expr.set(ie, expr.get(n));
		expr.set(n, pm);
		for (int i = n + 1; i < expr.length(); i++)
		{
			Algebraic p = expr.get(i);
			if (p is Polynomial)
			{
				Algebraic fc = ((Polynomial)p).coefficient(vp,1);
				if (!fc.Equals(Zahl.ZERO))
				{
					p = p.sub(pm.mult(fc.div(f)));
				}
			}
			expr.set(i, p);
		}
		elim(expr,vars,n + 1);
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static void eliminierung(Matrix a, Vektor c) throws JasymcaException
	private static void eliminierung(Matrix a, Vektor c)
	{
		int n = c.length();
		for (int k = 0; k < n - 1; k++)
		{
			pivot(a,c,k);
			for (int i = k + 1; i < n; i++)
			{
				Algebraic factor = a.get(i,k).div(a.get(k,k));
				for (int j = k; j < n; j++)
				{
					a.set(i,j, a.get(i,j).sub(factor.mult(a.get(k,j))));
				}
				c.set(i, c.get(i).sub(factor.mult(c.get(k))));
			}
		}
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static Vektor substitution(Matrix a, Vektor c) throws JasymcaException
	public static Vektor substitution(Matrix a, Vektor c)
	{
		int n = c.length();
		Algebraic[] x = new Algebraic[n];
		x[n - 1] = c.get(n - 1).div(a.get(n - 1,n - 1));
		for (int i = n - 2; i >= 0; i--)
		{
			Algebraic sum = Zahl.ZERO;
			for (int j = i + 1; j < n; j++)
			{
				sum = sum.add(a.get(i,j).mult(x[j]));
			}
			x[i] = c.get(i).sub(sum).div(a.get(i,i));
		}
		return new Vektor(x);
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static Vektor Gauss(Matrix a, Vektor c) throws JasymcaException
	public static Vektor Gauss(Matrix a, Vektor c)
	{
		int n = c.length();
		Algebraic[] x = new Algebraic[n];
		for (int k = 0; k < n - 1; k++)
		{
			pivot(a,c,k);
			if (!a.get(k,k).Equals(Zahl.ZERO))
			{
				for (int i = k + 1; i < n; i++)
				{
					Algebraic factor = a.get(i,k).div(a.get(k,k));
					for (int j = k + 1; j < n; j++)
					{
						a.set(i,j, a.get(i,j).sub(factor.mult(a.get(k,j))));
					}
					c.set(i, c.get(i).sub(factor.mult(c.get(k))));
				}
			}
		}
		x[n - 1] = c.get(n - 1).div(a.get(n - 1,n - 1));
		for (int i = n - 2; i >= 0; i--)
		{
			Algebraic sum = Zahl.ZERO;
			for (int j = i + 1; j < n; j++)
			{
				sum = sum.add(a.get(i,j).mult(x[j]));
			}
			x[i] = c.get(i).sub(sum).div(a.get(i,i));
		}
		return new Vektor(x);
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static int pivot(Matrix a, Vektor c, int k) throws JasymcaException
	private static int pivot(Matrix a, Vektor c, int k)
	{
		int pivot = k, n = c.length();
		double maxa = a.get(k,k).norm();
		for (int i = k + 1; i < n; i++)
		{
			double dummy = a.get(i,k).norm();
			if (dummy > maxa)
			{
				maxa = dummy;
				pivot = i;
			}
		}
		if (pivot != k)
		{
			for (int j = k;j < n;j++)
			{
				var dummy = a.get(pivot,j);
				a.set(pivot,j,a.get(k,j));
				a.set(k,j, dummy);
			}
		    {
		        var dummy = c.get(pivot);
		        c.set(pivot, c.get(k));
		        c.set(k, dummy);
		    }
		}
		return pivot;
	}
}
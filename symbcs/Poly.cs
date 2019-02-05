public sealed class Poly
{
	public static Polynomial top = new Polynomial(SimpleVariable.top);
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static Algebraic[] pqsolve(Algebraic p, Algebraic q) throws JasymcaException
	internal static Algebraic[] pqsolve(Algebraic p, Algebraic q)
	{
		Algebraic r = p.mult(Zahl.MINUS).div(Zahl.TWO);
		Algebraic s = FunctionVariable.create("sqrt", r.mult(r).sub(q));
		Algebraic[] result = new Algebraic[] {r.add(s), r.sub(s)};
		return result;
	}
	public static int degree(Algebraic p, Variable v)
	{
		if (p is Polynomial)
		{
			return ((Polynomial)p).degree(v);
		}
		if (p is Rational)
		{
			Rational r = (Rational)p;
			if (r.den.depends(v))
			{
				return 0;
			}
			return degree(r.nom,v);
		}
		return 0;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static Algebraic coefficient(Algebraic p, Variable v, int n) throws JasymcaException
	public static Algebraic coefficient(Algebraic p, Variable v, int n)
	{
		if (p is Polynomial)
		{
			return ((Polynomial)p).coefficient(v,n);
		}
		if (p is Rational)
		{
			Rational r = (Rational)p;
			if (r.den.depends(v))
			{
				throw new JasymcaException("Cannot determine coefficient of " + v + " in " + r);
			}
			return coefficient(r.nom,v,n).div(r.den);
		}
		return n == 0 ? p : Zahl.ZERO;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void polydiv(Algebraic[] a, Variable v) throws JasymcaException
	public static void polydiv(Algebraic[] a, Variable v)
	{
		int d0 = degree(a[0],v), d1 = degree(a[1],v), d = d0 - d1;
		if (d < 0)
		{
			a[1] = a[0];
			a[0] = Zahl.ZERO;
			return;
		}
		if (d1 == 0)
		{
			a[1] = Zahl.ZERO;
			return;
		}
		Algebraic[] cdiv = new Algebraic[d + 1];
		Algebraic[] nom = new Algebraic[d0 + 1];
		for (int i = 0; i < nom.Length; i++)
		{
			nom[i] = coefficient(a[0],v,i);
		}
		Algebraic den = coefficient(a[1], v, d1);
		for (int i = d, k = d0; i >= 0; i--,k--)
		{
			Algebraic cd = nom[k].div(den);
			cdiv[i] = cd;
			nom[k] = Zahl.ZERO;
			for (int j = k - 1,l = d1 - 1; j > k - (d1 + 1); j--,l--)
			{
				nom[j] = nom[j].sub(cd.mult(coefficient(a[1], v,l)));
			}
		}
		a[0] = horner(v,cdiv,d + 1);
		a[1] = horner(v,nom,d1);
		return;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static Algebraic horner(Variable x, Algebraic[] c, int n) throws JasymcaException
	public static Algebraic horner(Variable x, Algebraic[] c, int n)
	{
		if (n == 0)
		{
			return Zahl.ZERO;
		}
		if (n > c.Length)
		{
			throw new JasymcaException("Can not create horner polynomial.");
		}
		Polynomial X = new Polynomial(x);
		Algebraic p = c[n - 1];
		for (int i = n - 2; i >= 0; i--)
		{
			p = p.mult(X).add(c[i]);
		}
		return p;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static Algebraic horner(Variable x, Algebraic[] c) throws JasymcaException
	public static Algebraic horner(Variable x, Algebraic[] c)
	{
		return horner(x,c,c.Length);
	}
	public static Algebraic[] clone(Algebraic[] x)
	{
		Algebraic[] c = new Algebraic[x.Length];
		for (int i = 0; i < x.Length;i++)
		{
			c[i] = x[i];
		}
		return c;
	}
	public static Algebraic[] reduce(Algebraic[] x)
	{
		int len = x.Length;
		while (len > 0 && (x[len - 1] == null || x[len - 1].Equals(Zahl.ZERO)))
		{
			len--;
		}
		if (len == 0)
		{
			len = 1;
		}
		if (len != x.Length)
		{
			Algebraic[] na = new Algebraic[len];
			for (int i = 0; i < len; i++)
			{
				na[i] = x[i];
			}
			return na;
		}
		return x;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static Algebraic polydiv(Algebraic p1, Algebraic q1) throws JasymcaException
	public static Algebraic polydiv(Algebraic p1, Algebraic q1)
	{
		if (q1 is Zahl)
		{
			return p1.div(q1);
		}
		if (p1.Equals(Zahl.ZERO))
		{
			return Zahl.ZERO;
		}
		if (!(p1 is Polynomial) || !(q1 is Polynomial))
		{
			throw new JasymcaException("Polydiv is implemented for polynomials only.Got " + p1 + " / " + q1);
		}
		Polynomial p = (Polynomial)p1;
		Polynomial q = (Polynomial)q1;
		if (p.v.Equals(q.v))
		{
			int len = p.degree() - q.degree();
			if (len < 0)
			{
				throw new JasymcaException("Polydiv requires zero rest.");
			}
			Algebraic[] cdiv = new Algebraic[len + 1];
			Algebraic[] nom = clone(p.a);
			Algebraic den = q.a[q.a.Length - 1];
			for (int i = len, k = nom.Length - 1; i >= 0; i--,k--)
			{
				cdiv[i] = polydiv(nom[k], den);
				nom[k] = Zahl.ZERO;
				for (int j = k - 1,l = q.a.Length - 2; j > k - q.a.Length; j--,l--)
				{
					nom[j] = nom[j].sub(cdiv[i].mult(q.a[l]));
				}
			}
			return horner(p.v,cdiv);
		}
		else
		{
			Algebraic[] cn = new Algebraic[p.a.Length];
			for (int i = 0; i < p.a.Length; i++)
			{
				cn[i] = polydiv(p.a[i], q1);
			}
			return horner(p.v,cn);
		}
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static Algebraic mod(Algebraic p, Algebraic q, Variable r) throws JasymcaException
	public static Algebraic mod(Algebraic p, Algebraic q, Variable r)
	{
		int len = degree(p,r) - degree(q,r);
		if (len < 0)
		{
			return p;
		}
		Algebraic[] cdiv = new Algebraic[len + 1];
		Algebraic[] nom = new Algebraic[degree(p,r) + 1];
		for (int i = 0; i < nom.Length; i++)
		{
			nom[i] = coefficient(p,r,i);
		}
		Algebraic den = coefficient(q,r,degree(q,r));
		for (int i = len, k = nom.Length - 1; i >= 0; i--,k--)
		{
			cdiv[i] = polydiv(nom[k], den);
			nom[k] = Zahl.ZERO;
			for (int j = k - 1,l = (degree(q,r) + 1) - 2; j > k - (degree(q,r) + 1); j--,l--)
			{
				nom[j] = nom[j].sub(cdiv[i].mult(coefficient(q,r,l)));
			}
		}
		return horner(r,nom,nom.Length - 1 - len);
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static Algebraic euclid(Algebraic p, Algebraic q, Variable r) throws JasymcaException
	public static Algebraic euclid(Algebraic p, Algebraic q, Variable r)
	{
		int dp = degree(p,r);
		int dq = degree(q,r);
		Algebraic a = dp < dq ? p : p.mult(coefficient(q,r,dq).pow_n(dp - dq + 1));
		Algebraic b = q;
		Algebraic c = mod(a, b,r);
		Algebraic result = c.Equals(Zahl.ZERO) ? b : euclid(b,c,r);
		return result;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static Algebraic poly_gcd(Algebraic p, Algebraic q) throws JasymcaException
	public static Algebraic poly_gcd(Algebraic p, Algebraic q)
	{
		if (p.Equals(Zahl.ZERO))
		{
			return q;
		}
		if (q.Equals(Zahl.ZERO))
		{
			return p;
		}
		if (p is Zahl || q is Zahl)
		{
			return Zahl.ONE;
		}
		Variable r = ((Polynomial)q).v.smaller(((Polynomial)p).v) ? ((Polynomial)p).v : ((Polynomial)q).v;
		Algebraic pc = content(p,r), qc = content(q,r);
		Algebraic eu = euclid(polydiv(p,pc), polydiv(q,qc), r);
		Algebraic re = polydiv(eu, content(eu,r)).mult(poly_gcd(pc,qc));
		if (re is Zahl)
		{
			return Zahl.ONE;
		}
		Polynomial rp = (Polynomial)re;
		Algebraic res = rp;
		if (rp.a[rp.degree()] is Zahl)
		{
			res = rp.div(rp.a[rp.degree()]);
		}
		return res;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static Algebraic content(Algebraic p, Variable r) throws JasymcaException
	public static Algebraic content(Algebraic p, Variable r)
	{
		if (p is Zahl)
		{
			return p;
		}
		Algebraic result = coefficient(p,r,0);
		for (int i = 0; i <= degree(p,r) && !result.Equals(Zahl.ONE); i++)
		{
			result = poly_gcd(result, coefficient(p,r,i));
		}
		return result;
	}
	internal static int gcd(int a, int b)
	{
		int c = 1;
		while (c != 0)
		{
			c = a % b;
			a = b;
			b = c;
		}
		return a;
	}
}
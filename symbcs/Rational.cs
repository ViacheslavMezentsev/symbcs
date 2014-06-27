public class Rational : Algebraic
{
	internal Algebraic nom;
	internal Polynomial den;
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Rational(Algebraic nom, Polynomial den) throws JasymcaException
	public Rational(Algebraic nom, Polynomial den)
	{
		Algebraic norm = den.a[den.degree()];
		if (norm is Zahl)
		{
			this.nom = nom.div(norm);
			this.den = (Polynomial)den.div(norm);
		}
		else
		{
			this.nom = nom;
			this.den = den;
		}
	}
	public override bool ratfunc(Variable v)
	{
		return nom.ratfunc(v) && den.ratfunc(v);
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic reduce() throws JasymcaException
	public override Algebraic reduce()
	{
		if (nom is Zahl)
		{
			if (nom.Equals(Zahl.ZERO))
			{
				return Zahl.ZERO;
			}
			return this;
		}
		Algebraic[] pq = new Algebraic[] {nom, den};
		pq = Exponential.reduce_exp(pq);
		if (!nom.Equals(pq[0]) || !den.Equals(pq[1]))
		{
			return pq[0].div(pq[1]).reduce();
		}
		if (exaktq())
		{
			Algebraic gcd = Poly.poly_gcd(den,nom);
			if (!gcd.Equals(Zahl.ONE))
			{
				Algebraic n = Poly.polydiv(nom,gcd);
				Algebraic d = Poly.polydiv(den,gcd);
				if (d.Equals(Zahl.ONE))
				{
					return n;
				}
				else if (d is Zahl)
				{
					return n.div(d);
				}
				else
				{
					return new Rational(n,(Polynomial)d);
				}
			}
		}
		return this;
	}
	public override bool exaktq()
	{
		return nom.exaktq() && den.exaktq();
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic add(Algebraic x) throws JasymcaException
	public override Algebraic add(Algebraic x)
	{
		if (x is Rational)
		{
			return nom.mult(((Rational)x).den).add(((Rational)x).nom.mult(den)).div(den.mult(((Rational)x).den)).reduce();
		}
		else
		{
			return nom.add(x.mult(den)).div(den).reduce();
		}
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic mult(Algebraic x) throws JasymcaException
	public override Algebraic mult(Algebraic x)
	{
		if (x is Rational)
		{
			return nom.mult(((Rational)x).nom).div(den.mult(((Rational)x).den)).reduce();
		}
		else
		{
			return nom.mult(x).div(den).reduce();
		}
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic div(Algebraic x) throws JasymcaException
	public override Algebraic div(Algebraic x)
	{
		if (x is Rational)
		{
			return nom.mult(((Rational)x).den).div(den.mult(((Rational)x).nom)).reduce();
		}
		else
		{
			return nom.div(den.mult(x)).reduce();
		}
	}
	public override string ToString()
	{
		return "(" + nom + "/" + den + ")";
	}
	public override bool Equals(object x)
	{
		return x is Rational && ((Rational)x).nom.Equals(nom) && ((Rational)x).den.Equals(den);
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic deriv(Variable var) throws JasymcaException
	public override Algebraic deriv(Variable @var)
	{
		return nom.deriv(@var).mult(den).sub(den.deriv(@var).mult(nom)).div(den.mult(den)).reduce();
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic integrate(Variable var) throws JasymcaException
	public override Algebraic integrate(Variable @var)
	{
		if (!den.depends(@var))
		{
			return nom.integrate(@var).div(den);
		}
		Algebraic quot = den.deriv(@var).div(nom);
		if (quot.deriv(@var).Equals(Zahl.ZERO))
		{
			return FunctionVariable.create("log",den).div(quot);
		}
		Algebraic[] q = new Algebraic[] {nom, den};
		Poly.polydiv(q, @var);
		if (!q[0].Equals(Zahl.ZERO) && nom.ratfunc(@var) && den.ratfunc(@var))
		{
			return q[0].integrate(@var).add(q[1].div(den).integrate(@var));
		}
		if (ratfunc(@var))
		{
			Algebraic r = Zahl.ZERO;
			Vektor h = horowitz(nom,den,@var);
			if (h.get(0) is Rational)
			{
				r = r.add(h.get(0));
			}
			if (h.get(1) is Rational)
			{
				r = r.add((new TrigInverseExpand()).f_exakt(((Rational)h.get(1)).intrat(@var)));
			}
			return r;
		}
		throw new JasymcaException("Could not integrate Function " + this);
	}
	public override double norm()
	{
		return nom.norm() / den.norm();
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic cc() throws JasymcaException
	public override Algebraic cc()
	{
		return nom.cc().div(den.cc());
	}
	public override bool depends(Variable @var)
	{
		return nom.depends(@var) || den.depends(@var);
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic value(Variable var, Algebraic x) throws JasymcaException
	public override Algebraic value(Variable @var, Algebraic x)
	{
		return nom.value(@var,x).div(den.value(@var,x));
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Algebraic map(LambdaAlgebraic f) throws JasymcaException
	public override Algebraic map(LambdaAlgebraic f)
	{
		return f.f_exakt(nom).div(f.f_exakt(den));
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static Vektor horowitz(Algebraic p, Polynomial q, Variable x) throws JasymcaException
	public static Vektor horowitz(Algebraic p, Polynomial q, Variable x)
	{
		if (Poly.degree(p,x) >= Poly.degree(q,x))
		{
			throw new JasymcaException("Degree of p must be smaller than degree of q");
		}
		p = p.rat();
		q = (Polynomial)q.rat();
		Algebraic d = Poly.poly_gcd(q, q.deriv(x));
		Algebraic b = Poly.polydiv(q,d);
		int m = b is Polynomial? ((Polynomial)b).degree():0;
		int n = d is Polynomial? ((Polynomial)d).degree():0;
		SimpleVariable[] a = new SimpleVariable[m];
		Polynomial X = new Polynomial(x);
		Algebraic A = Zahl.ZERO;
		for (int i = a.Length - 1; i >= 0; i--)
		{
			a[i] = new SimpleVariable("a" + i);
			A = A.add(new Polynomial(a[i]));
			if (i > 0)
			{
				A = A.mult(X);
			}
		}
		SimpleVariable[] c = new SimpleVariable[n];
		Algebraic C = Zahl.ZERO;
		for (int i = c.Length - 1; i >= 0; i--)
		{
			c[i] = new SimpleVariable("c" + i);
			C = C.add(new Polynomial(c[i]));
			if (i > 0)
			{
				C = C.mult(X);
			}
		}
		Algebraic r = Poly.polydiv(C.mult(b).mult(d.deriv(x)),d);
		r = b.mult(C.deriv(x)).sub(r).add(d.mult(A));
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: Algebraic[][] aik = new Algebraic[m+n][m+n];
		Algebraic[][] aik = RectangularArrays.ReturnRectangularAlgebraicArray(m + n, m + n);
		Algebraic cf ; Algebraic[] co = new Algebraic[m + n];
		for (int i = 0; i < m + n; i++)
		{
			co[i] = Poly.coefficient(p,x,i);
			cf = Poly.coefficient(r,x,i);
			for (int k = 0; k < m; k++)
			{
				aik[i][k] = cf.deriv(a[k]);
			}
			for (int k = 0; k < n; k++)
			{
				aik[i][k + m] = cf.deriv(c[k]);
			}
		}
		Vektor s = LambdaLINSOLVE.Gauss(new Matrix(aik), new Vektor(co));
		A = Zahl.ZERO;
		for (int i = m - 1; i >= 0; i--)
		{
			A = A.add(s.get(i));
			if (i > 0)
			{
				A = A.mult(X);
			}
		}
		C = Zahl.ZERO;
		for (int i = n - 1; i >= 0; i--)
		{
			C = C.add(s.get(i + m));
			if (i > 0)
			{
				C = C.mult(X);
			}
		}
		co = new Algebraic[2];
		co[0] = C.div(d);
		co[1] = A.div(b);
		return new Vektor(co);
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Algebraic intrat(Variable x) throws JasymcaException
	internal virtual Algebraic intrat(Variable x)
	{
		Algebraic de = den.deriv(x);
		if (de is Zahl)
		{
			return makelog(nom.div(de), x, den.a[0].mult(Zahl.MINUS).div(de));
		}
		Algebraic r = nom.div(de);
		Vektor xi = den.monic().roots();
		Algebraic rs = Zahl.ZERO;
		for (int i = 0; i < xi.length(); i++)
		{
			Algebraic c = r.value(x,xi.get(i));
			rs = rs.add(makelog(c,x,xi.get(i)));
		}
		return rs;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Algebraic makelog(Algebraic c, Variable x, Algebraic a) throws JasymcaException
	internal virtual Algebraic makelog(Algebraic c, Variable x, Algebraic a)
	{
		Algebraic arg = (new Polynomial(x)).sub(a);
		return FunctionVariable.create("log", arg).mult(c);
	}
}
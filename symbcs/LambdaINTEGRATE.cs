using System;
using System.Collections;

internal class LambdaINTEGRATE : Lambda
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		if (narg == 0)
		{
			throw new ParseException("Argument to integrate missing.");
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
		Algebraic fi = integrate(f,v);
		if (fi is Rational && !fi.exaktq())
		{
			fi = (new LambdaRAT()).f_exakt(fi);
		}
		st.Push(fi);
		return 0;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static Algebraic integrate(Algebraic expr, Variable v)throws JasymcaException
	public static Algebraic integrate(Algebraic expr, Variable v)
	{
		Algebraic e = (new ExpandUser()).f_exakt(expr);
		try
		{
			e = e.integrate(v);
			e = (new TrigInverseExpand()).f_exakt(e);
			e = remove_constant(e, v);
			return e;
		}
		catch (JasymcaException)
		{
		}
		debug("Second Attempt: " + expr);
		expr = (new ExpandUser()).f_exakt(expr);
		debug("Expand User Functions: " + expr);
		expr = (new TrigExpand()).f_exakt(expr);
		e = expr;
		try
		{
			expr = (new NormExp()).f_exakt(expr);
			debug("Norm Functions: " + expr);
			expr = expr.integrate(v);
			debug("Integrated: " + expr);
			expr = remove_constant(expr, v);
			expr = (new TrigInverseExpand()).f_exakt(expr);
			return expr;
		}
		catch (JasymcaException)
		{
		}
		debug("Third Attempt: " + expr);
		expr = e;
		SubstExp se = new SubstExp(v, expr);
		expr = se.f_exakt(expr);
		expr = se.rational(expr);
		debug("Rationalized: " + expr);
		expr = expr.integrate(se.t);
		debug("Integrated: " + expr);
		expr = se.rat_reverse(expr);
		debug("Reverse subst.: " + expr);
		expr = remove_constant(expr, v);
		expr = (new TrigInverseExpand()).f_exakt(expr);
		expr = remove_constant(expr, v);
		return expr;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static Algebraic remove_constant(Algebraic expr, Variable x)throws JasymcaException
	internal static Algebraic remove_constant(Algebraic expr, Variable x)
	{
		if (!expr.depends(x))
		{
			return Zahl.ZERO;
		}
		if (expr is Polynomial)
		{
			((Polynomial)expr).a[0] = remove_constant(((Polynomial)expr).a[0], x);
			return expr;
		}
		if (expr is Rational)
		{
			Polynomial den = ((Rational)expr).den;
			Algebraic nom = ((Rational)expr).nom;
			if (!den.depends(x))
			{
				return remove_constant(nom,x).div(den);
			}
			if (nom is Polynomial)
			{
				Algebraic[] a = new Algebraic[]{nom, den};
				Poly.polydiv(a, den.v);
				if (!a[0].depends(x))
				{
					return a[1].div(den);
				}
			}
		}
		return expr;
	}
}
internal class LambdaROMBERG : Lambda
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		if (narg != 4)
		{
			throw new ParseException("Usage: ROMBERG (exp,var,ll,ul)");
		}
		Algebraic exp = getAlgebraic(st);
		Variable v = getVariable(st);
		Algebraic ll = getAlgebraic(st);
		Algebraic ul = getAlgebraic(st);
		LambdaAlgebraic xc = new ExpandConstants();
		exp = xc.f_exakt(exp);
		ll = xc.f_exakt(ll);
		ul = xc.f_exakt(ul);
		if (!(ll is Zahl) || !(ul is Zahl))
		{
			throw new ParseException("Usage: ROMBERG (exp,var,ll,ul)");
		}
		double rombergtol = 1.0e-4;
		int rombergit = 11;
		Zahl a1 = pc.env.getnum("rombergit");
		if (a1 != null)
		{
			rombergit = a1.intval();
		}
		a1 = pc.env.getnum("rombergtol");
		if (a1 != null)
		{
			rombergtol = a1.unexakt().real;
		}
		double a = ((Zahl)ll).unexakt().real;
		double b = ((Zahl)ul).unexakt().real;
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: double[][] I = new double[rombergit][rombergit];
		double[][] I = RectangularArrays.ReturnRectangularDoubleArray(rombergit, rombergit);
		int i = 0, n = 1;
		Algebraic t = trapez(exp, v, n, a, b);
		if (!(t is Zahl))
		{
			throw new ParseException("Expression must evaluate to number");
		}
		I[0][0] = ((Zahl)t).unexakt().real;
		double epsa = 1.1 * rombergtol;
		while (epsa > rombergtol && i < rombergit - 1)
		{
			i++;
			n *= 2;
			t = trapez(exp, v, n, a, b);
			I[0][i] = ((Zahl)t).unexakt().real;
			double f = 1.0;
			for (int k = 1; k <= i; k++)
			{
				f *= 4;
				I[k][i] = I[k - 1][i] + (I[k - 1][i] - I[k - 1][i - 1]) / (f - 1.0);
			}
			epsa = Math.Abs((I[i][i] - I[i - 1][i - 1]) / I[i][i]);
		}
		st.Push(new Unexakt(I[i][i]));
		return 0;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Algebraic trapez(Algebraic exp, Variable v, int n, double a, double b) throws JasymcaException
	internal virtual Algebraic trapez(Algebraic exp, Variable v, int n, double a, double b)
	{
		Algebraic sum = Zahl.ZERO;
		double step = (b - a) / n;
		for (int i = 1; i < n; i++)
		{
			Algebraic x = exp.value(v, new Unexakt(a + step * i));
			sum = sum.add(x);
		}
		sum = exp.value(v, new Unexakt(a)).add(sum.mult(Zahl.TWO)).add(exp.value(v, new Unexakt(b)));
		return (new Unexakt(b - a)).mult(sum).div(new Unexakt(2.0 * n));
	}
}
using System.Collections;

internal class LambdaODE : Lambda
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		Algebraic dgl = getAlgebraic(st);
		Variable y = getVariable(st);
		Variable x = getVariable(st);
		Algebraic p = Poly.coefficient(dgl, y,1);
		Algebraic q = Poly.coefficient(dgl, y,0);
		Algebraic pi = LambdaINTEGRATE.integrate(p,x);
		if (pi is Rational && !pi.exaktq())
		{
			pi = (new LambdaRAT()).f_exakt(pi);
		}
		Variable vexp = new FunctionVariable("exp", pi, new LambdaEXP());
		Algebraic dn = new Polynomial(vexp);
		Algebraic qi = LambdaINTEGRATE.integrate(q.div(dn),x);
		if (qi is Rational && !qi.exaktq())
		{
			qi = (new LambdaRAT()).f_exakt(qi);
		}
		Algebraic cn = new Polynomial(new SimpleVariable("C"));
		Algebraic res = qi.add(cn).mult(dn);
		res = (new ExpandUser()).f_exakt(res);
		debug("User Function expand: " + res);
		res = (new TrigExpand()).f_exakt(res);
		debug("Trigexpand: " + res);
		res = (new NormExp()).f_exakt(res);
		debug("Norm: " + res);
		if (res is Rational)
		{
			res = (new LambdaRAT()).f_exakt(res);
		}
		res = (new TrigInverseExpand()).f_exakt(res);
		debug("Triginverse: " + res);
		res = (new SqrtExpand()).f_exakt(res);
		st.Push(res);
		return 0;
	}
}
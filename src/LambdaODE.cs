using System.Collections;

internal class LambdaODE : Lambda
{
	public override int Eval(Stack st)
	{
		int narg = GetNarg(st);

		var dgl = GetAlgebraic(st);
        var y = GetVariable( st );
        var x = GetVariable( st );

        var p = Poly.Coefficient( dgl, y, 1 );
        var q = Poly.Coefficient( dgl, y, 0 );

		var pi = LambdaINTEGRATE.Integrate(p,x);

		if (pi is Rational && !pi.IsNumber())
		{
			pi = (new LambdaRAT()).SymEval(pi);
		}

		Variable vexp = new FunctionVariable("exp", pi, new LambdaEXP());

		Algebraic dn = new Polynomial(vexp);

	    var qi = LambdaINTEGRATE.Integrate( q / dn, x );

		if (qi is Rational && !qi.IsNumber())
		{
			qi = (new LambdaRAT()).SymEval(qi);
		}

		Algebraic cn = new Polynomial(new SimpleVariable("C"));

		var res = ( qi +cn ) * dn;

		res = (new ExpandUser()).SymEval(res);

		Debug("User Function expand: " + res);

		res = (new TrigExpand()).SymEval(res);

		Debug("Trigexpand: " + res);

		res = (new NormExp()).SymEval(res);

		Debug("Norm: " + res);

		if (res is Rational)
		{
			res = (new LambdaRAT()).SymEval(res);
		}

		res = (new TrigInverseExpand()).SymEval(res);

		Debug("Triginverse: " + res);

		res = (new SqrtExpand()).SymEval(res);

		st.Push(res);

		return 0;
	}
}

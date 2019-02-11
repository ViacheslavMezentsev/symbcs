using System;
using System.Collections;

internal class LambdaINTEGRATE : Lambda
{
	public override int Eval( Stack stack )
	{
		int narg = GetNarg( stack );

		if ( narg == 0 )
		{
		    throw new ParseException( "Argument to integrate missing." );
		}

	    var f = GetAlgebraic( stack );

		Variable v;

		if ( narg > 1 )
		{
			v = GetVariable( stack );
		}
		else
		{
            if ( f is Polynomial )
            {
                v = ( ( Polynomial ) f )._v;
            }
            else if ( f is Rational )
            {
                v = ( ( Rational ) f ).den._v;
            }
            else
            {
                throw new ParseException( "Could not determine Variable." );
            }
		}

	    var fi = Integrate( f, v );

        if ( fi is Rational && !fi.IsNumber() )
        {
            fi = new LambdaRAT().SymEval( fi );
        }

        stack.Push( fi );

        return 0;
    }

	public static Algebraic Integrate( Algebraic expr, Variable v )
	{
        var e = new ExpandUser().SymEval( expr );

        try
        {
            e = e.Integrate(v);

            e = new TrigInverseExpand().SymEval(e);

            e = remove_constant( e, v );

            return e;
        }
        catch ( JasymcaException )
        {
        }

        Debug( "Second Attempt: " + expr );

        expr = new ExpandUser().SymEval( expr );

        Debug( "Expand User Functions: " + expr );

        expr = new TrigExpand().SymEval( expr );

        e = expr;

        try
        {
            expr = new NormExp().SymEval( expr );

            Debug( "Norm Functions: " + expr );

            expr = expr.Integrate( v );

            Debug( "Integrated: " + expr );

            expr = remove_constant( expr, v );

            expr = new TrigInverseExpand().SymEval( expr );

            return expr;
        }
        catch ( JasymcaException )
        {
        }

        Debug( "Third Attempt: " + expr );

        expr = e;

        var se = new SubstExp( v, expr );

        expr = se.SymEval( expr );
        expr = se.rational( expr );

        Debug( "Rationalized: " + expr );

        expr = expr.Integrate( se.t );

        Debug( "Integrated: " + expr );

        expr = se.rat_reverse( expr );

        Debug( "Reverse subst.: " + expr );

        expr = remove_constant( expr, v );

        expr = new TrigInverseExpand().SymEval( expr );

        expr = remove_constant( expr, v );

        return expr;
	}

	internal static Algebraic remove_constant(Algebraic expr, Variable x)
	{
        if ( !expr.Depends(x) )
        {
            return Symbolic.ZERO;
        }

        if ( expr is Polynomial )
        {
            ( ( Polynomial ) expr )[0] = remove_constant( ( ( Polynomial ) expr )[0], x );

            return expr;
        }

        if ( expr is Rational )
        {
            var den = ( ( Rational ) expr ).den;
            var nom = ( ( Rational ) expr ).nom;

            if ( !den.Depends(x) )
            {
                return remove_constant( nom, x ) / den;
            }

            if ( nom is Polynomial )
            {
                var a = new[] { nom, den };

                Poly.polydiv( a, den._v );

                if ( !a[0].Depends(x) )
                {
                    return a[1] / den;
                }
            }
        }

        return expr;
	}
}

internal class LambdaROMBERG : Lambda
{
	public override int Eval(Stack st)
	{
	    int narg = GetNarg( st );

	    if ( narg != 4 )
		{
		    throw new ParseException( "Usage: ROMBERG (exp,var,ll,ul)" );
		}

        var exp = GetAlgebraic( st );
        var v = GetVariable( st );
        var ll = GetAlgebraic( st );
        var ul = GetAlgebraic( st );

		LambdaAlgebraic xc = new ExpandConstants();

        exp = xc.SymEval( exp );
        ll = xc.SymEval( ll );
        ul = xc.SymEval( ul );

	    if ( !( ll is Symbolic ) || !( ul is Symbolic ) )
		{
		    throw new ParseException( "Usage: ROMBERG (exp,var,ll,ul)" );
		}

		var rombergtol = 1.0e-4;
		int rombergit = 11;

	    var a1 = pc.env.getnum( "rombergit" );

        if ( a1 != null )
        {
			rombergit = a1.ToInt();
		}

	    a1 = pc.env.getnum( "rombergtol" );

        if ( a1 != null )
        {
			rombergtol = a1.ToComplex().Re;
		}

        var a = ( ( Symbolic ) ll ).ToComplex().Re;
        var b = ( ( Symbolic ) ul ).ToComplex().Re;

		var I = Matrix.CreateRectangularArray<double>(rombergit, rombergit);
		int i = 0, n = 1;

		var t = trapez(exp, v, n, a, b);

	    if ( !( t is Symbolic ) )
		{
            throw new ParseException( "Expression must evaluate to number" );
        }

		I[0][0] = ( ( Symbolic ) t ).ToComplex().Re;

		double epsa = 1.1 * rombergtol;

		while (epsa > rombergtol && i < rombergit - 1)
		{
			i++;
			n *= 2;
		    t = trapez( exp, v, n, a, b );

			I[0][i] = ( ( Symbolic ) t ).ToComplex().Re;

			double f = 1.0;

			for ( int k = 1; k <= i; k++ )
			{
				f *= 4;
				I[k][i] = I[k - 1][i] + ( I[k - 1][i] - I[k - 1][i - 1] ) / ( f - 1.0 );
			}

			epsa = Math.Abs( ( I[i][i] - I[i - 1][i - 1] ) / I[i][i] );
		}

		st.Push( new Complex( I[i][i] ) );

		return 0;
	}

    internal virtual Algebraic trapez( Algebraic exp, Variable v, int n, double a, double b )
	{
		Algebraic sum = Symbolic.ZERO;

	    var step = ( b - a ) / n;

		for ( int i = 1; i < n; i++ )
		{
			var x = exp.Value( v, new Complex( a + step * i ) );

			sum = sum + x;
		}

		sum = exp.Value( v, new Complex(a) ) + sum * Symbolic.TWO + exp.Value( v, new Complex(b) );

		return new Complex( b - a ) * sum / new Complex( 2.0 * n );
	}
}

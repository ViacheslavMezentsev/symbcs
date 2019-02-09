using System.Collections;

internal class LambdaSOLVE : Lambda
{
    public override int lambda( Stack st )
    {
        int narg = getNarg( st );

        if ( narg != 2 )
        {
            throw new ParseException( "solve requires 2 arguments." );
        }

        var expr = getAlgebraic( st ).rat();

        if ( !( expr is Polynomial || expr is Rational ) )
        {
            throw new JasymcaException( "Wrong format for Expression in solve." );
        }

        var item = getVariable( st );

        var r = solve( expr, item ).reduce();

        st.Push(r);

        return 0;
    }

    public static Algebraic linfaktor( Algebraic expr, Variable item )
    {
        if ( expr is Vektor )
        {
            var cn = new Algebraic[ ( ( Vektor ) expr ).length() ];

            for ( int i = 0; i < ( ( Vektor ) expr ).length(); i++ )
            {
                cn[i] = linfaktor( ( ( Vektor ) expr ).get(i), item );
            }

            return new Vektor( cn );
        }

        return ( new Polynomial( item ) ).sub( expr );
    }

    public static Vektor solve( Algebraic expr, Variable item )
    {
        debug( "Solve: " + expr + " = 0, Variable: " + item );

        expr = ( new ExpandUser() ).f_exakt( expr );
        expr = ( new TrigExpand() ).f_exakt( expr );

        debug( "TrigExpand: " + expr );

        expr = ( new NormExp() ).f_exakt( expr );

        debug( "Norm: " + expr );

        expr = ( new CollectExp( expr ) ).f_exakt( expr );

        debug( "Collect: " + expr );

        expr = ( new SqrtExpand() ).f_exakt( expr );

        debug( "SqrtExpand: " + expr );

        if ( expr is Rational )
        {
            expr = ( new LambdaRAT() ).f_exakt( expr );

            if ( expr is Rational )
            {
                expr = ( ( Rational ) expr ).nom;
            }
        }

        debug( "Canonic Expression: " + expr );

        if ( !( expr is Polynomial ) || !( ( Polynomial ) expr ).depends( item ) )
        {
            throw new JasymcaException( "Expression does not depend of variable." );
        }

        var p = ( Polynomial ) expr;

        Vektor sol = null;

        var dep = depvars( p, item );

        if ( dep.Count == 0 )
        {
            throw new JasymcaException( "Expression does not depend of variable." );
        }

        if ( dep.Count == 1 )
        {
            var dvar = ( Variable ) dep[0];

            debug( "Found one Variable: " + dvar );

            sol = p.solve( dvar );

            debug( "Solution: " + dvar + " = " + sol );

            if ( !dvar.Equals( item ) )
            {
                var s = new ArrayList();

                for ( int i = 0; i < sol.length(); i++ )
                {
                    debug( "Invert: " + sol.get(i) + " = " + dvar );

                    var sl = finvert( ( FunctionVariable ) dvar, sol.get(i) );

                    debug( "Result: " + sl + " = 0" );

                    var t = solve( sl, item );

                    debug( "Solution: " + item + " = " + t );

                    for ( int k = 0; k < t.length(); k++ )
                    {
                        var tn = t.get(k);

                        if ( !s.Contains( tn ) )
                        {
                            s.Add( tn );
                        }
                    }
                }

                sol = Vektor.create(s);
            }
        }
        else if ( dep.Count == 2 )
        {
            debug( "Found two Variables: " + dep[0] + ", " + dep[1] );

            if ( dep.Contains( item ) )
            {
                var f = ( FunctionVariable ) ( dep[0].Equals( item ) ? dep[1] : dep[0] );

                if ( f.fname.Equals( "sqrt" ) )
                {
                    debug( "Solving " + p + " for " + f );

                    sol = p.solve( f );

                    debug( "Solution: " + f + " = " + sol );

                    var s = new ArrayList();

                    for ( int i = 0; i < sol.length(); i++ )
                    {
                        debug( "Invert: " + sol.get( i ) + " = " + f );

                        var sl = finvert( f, sol.get(i) );

                        debug( "Result: " + sl + " = 0" );

                        if ( sl is Polynomial && depvars( ( ( Polynomial ) sl ), item ).Count == 1 )
                        {
                            debug( "Solving " + sl + " for " + item );

                            var t = solve( sl, item );

                            debug( "Solution: " + item + " = " + t );

                            for ( int k = 0; k < t.length(); k++ )
                            {
                                var tn = t.get(k);

                                if ( !s.Contains( tn ) )
                                {
                                    s.Add( tn );
                                }
                            }
                        }
                        else
                        {
                            throw new JasymcaException( "Could not solve equation." );
                        }
                    }

                    sol = Vektor.create(s);
                }
                else
                {
                    throw new JasymcaException( "Can not solve equation." );
                }
            }
            else
            {
                throw new JasymcaException( "Can not solve equation." );
            }
        }
        else
        {
            throw new JasymcaException( "Can not solve equation." );
        }

        return sol;
    }

    private static ArrayList depvars( Polynomial p, Variable item )
    {
        var r = new ArrayList();

        if ( !p.v.deriv( item ).Equals( Zahl.ZERO ) )
        {
            r.Add( p.v );
        }

        foreach ( var t in p.a )
        {
            if ( t is Polynomial )
            {
                var c = depvars( ( Polynomial ) t, item );

                if ( c.Count > 0 )
                {
                    foreach ( var v in c )
                    {
                        if ( r.Contains(v) ) continue;

                        r.Add(v);
                    }
                }
            }
        }

        return r;
    }

    internal static Algebraic finvert( FunctionVariable f, Algebraic b )
    {
        if ( f.fname.Equals( "sqrt" ) )
        {
            return b.mult( b ).sub( f.arg );
        }

        if ( f.fname.Equals( "exp" ) )
        {
            return FunctionVariable.create( "log", b ).sub( f.arg );
        }

        if ( f.fname.Equals( "log" ) )
        {
            return FunctionVariable.create( "exp", b ).sub( f.arg );
        }

        if ( f.fname.Equals( "tan" ) )
        {
            return FunctionVariable.create( "atan", b ).sub( f.arg );
        }

        if ( f.fname.Equals( "atan" ) )
        {
            return FunctionVariable.create( "tan", b ).sub( f.arg );
        }

        throw new JasymcaException( "Could not invert " + f );
    }
}

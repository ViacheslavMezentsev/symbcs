using System.Collections;

namespace Tiny.Symbolic
{
    internal class LambdaSOLVE : Lambda
    {
        public override int Eval( Stack st )
        {
            int narg = GetNarg( st );

            if ( narg != 2 )
            {
                throw new ParseException( "solve requires 2 arguments." );
            }

            var expr = GetAlgebraic( st ).Rat();

            if ( !( expr is Polynomial || expr is Rational ) )
            {
                throw new SymbolicException( "Wrong format for Expression in solve." );
            }

            var item = GetVariable( st );

            var r = solve( expr, item ).Reduce();

            st.Push( r );

            return 0;
        }

        public static Algebraic linfaktor( Algebraic expr, Variable item )
        {
            if ( expr is Vector )
            {
                var cn = new Algebraic[ ( ( Vector ) expr ).Length() ];

                for ( int i = 0; i < ( ( Vector ) expr ).Length(); i++ )
                {
                    cn[ i ] = linfaktor( ( ( Vector ) expr )[ i ], item );
                }

                return new Vector( cn );
            }

            return ( new Polynomial( item ) ) - expr;
        }

        public static Vector solve( Algebraic expr, Variable item )
        {
            Debug( "Solve: " + expr + " = 0, Variable: " + item );

            expr = ( new ExpandUser() ).SymEval( expr );
            expr = ( new TrigExpand() ).SymEval( expr );

            Debug( "TrigExpand: " + expr );

            expr = ( new NormExp() ).SymEval( expr );

            Debug( "Norm: " + expr );

            expr = ( new CollectExp( expr ) ).SymEval( expr );

            Debug( "Collect: " + expr );

            expr = ( new SqrtExpand() ).SymEval( expr );

            Debug( "SqrtExpand: " + expr );

            if ( expr is Rational )
            {
                expr = ( new LambdaRAT() ).SymEval( expr );

                if ( expr is Rational )
                {
                    expr = ( ( Rational ) expr ).nom;
                }
            }

            Debug( "Canonic Expression: " + expr );

            if ( !( expr is Polynomial ) || !( ( Polynomial ) expr ).Depends( item ) )
            {
                throw new SymbolicException( "Expression does not depend of variable." );
            }

            var p = ( Polynomial ) expr;

            Vector sol = null;

            var dep = depvars( p, item );

            if ( dep.Count == 0 )
            {
                throw new SymbolicException( "Expression does not depend of variable." );
            }

            if ( dep.Count == 1 )
            {
                var dvar = ( Variable ) dep[ 0 ];

                Debug( "Found one Variable: " + dvar );

                sol = p.solve( dvar );

                Debug( "Solution: " + dvar + " = " + sol );

                if ( !dvar.Equals( item ) )
                {
                    var s = new ArrayList();

                    for ( int i = 0; i < sol.Length(); i++ )
                    {
                        Debug( "Invert: " + sol[ i ] + " = " + dvar );

                        var sl = finvert( ( FunctionVariable ) dvar, sol[ i ] );

                        Debug( "Result: " + sl + " = 0" );

                        var t = solve( sl, item );

                        Debug( "Solution: " + item + " = " + t );

                        for ( int k = 0; k < t.Length(); k++ )
                        {
                            var tn = t[ k ];

                            if ( !s.Contains( tn ) )
                            {
                                s.Add( tn );
                            }
                        }
                    }

                    sol = Vector.Create( s );
                }
            }
            else if ( dep.Count == 2 )
            {
                Debug( "Found two Variables: " + dep[ 0 ] + ", " + dep[ 1 ] );

                if ( dep.Contains( item ) )
                {
                    var f = ( FunctionVariable ) ( dep[ 0 ].Equals( item ) ? dep[ 1 ] : dep[ 0 ] );

                    if ( f.Name.Equals( "sqrt" ) )
                    {
                        Debug( "Solving " + p + " for " + f );

                        sol = p.solve( f );

                        Debug( "Solution: " + f + " = " + sol );

                        var s = new ArrayList();

                        for ( int i = 0; i < sol.Length(); i++ )
                        {
                            Debug( "Invert: " + sol[ i ] + " = " + f );

                            var sl = finvert( f, sol[ i ] );

                            Debug( "Result: " + sl + " = 0" );

                            if ( sl is Polynomial && depvars( ( ( Polynomial ) sl ), item ).Count == 1 )
                            {
                                Debug( "Solving " + sl + " for " + item );

                                var t = solve( sl, item );

                                Debug( "Solution: " + item + " = " + t );

                                for ( int k = 0; k < t.Length(); k++ )
                                {
                                    var tn = t[ k ];

                                    if ( !s.Contains( tn ) )
                                    {
                                        s.Add( tn );
                                    }
                                }
                            }
                            else
                            {
                                throw new SymbolicException( "Could not solve equation." );
                            }
                        }

                        sol = Vector.Create( s );
                    }
                    else
                    {
                        throw new SymbolicException( "Can not solve equation." );
                    }
                }
                else
                {
                    throw new SymbolicException( "Can not solve equation." );
                }
            }
            else
            {
                throw new SymbolicException( "Can not solve equation." );
            }

            return sol;
        }

        private static ArrayList depvars( Polynomial p, Variable item )
        {
            var r = new ArrayList();

            if ( !p._v.Derive( item ).Equals( Symbol.ZERO ) )
            {
                r.Add( p._v );
            }

            foreach ( var t in p.Coeffs )
            {
                if ( t is Polynomial )
                {
                    var c = depvars( ( Polynomial ) t, item );

                    if ( c.Count > 0 )
                    {
                        foreach ( var v in c )
                        {
                            if ( r.Contains( v ) )
                                continue;

                            r.Add( v );
                        }
                    }
                }
            }

            return r;
        }

        internal static Algebraic finvert( FunctionVariable f, Algebraic b )
        {
            if ( f.Name.Equals( "sqrt" ) )
            {
                return b * b - f.Var;
            }

            if ( f.Name.Equals( "exp" ) )
            {
                return FunctionVariable.Create( "log", b ) - f.Var;
            }

            if ( f.Name.Equals( "log" ) )
            {
                return FunctionVariable.Create( "exp", b ) - f.Var;
            }

            if ( f.Name.Equals( "tan" ) )
            {
                return FunctionVariable.Create( "atan", b ) - f.Var;
            }

            if ( f.Name.Equals( "atan" ) )
            {
                return FunctionVariable.Create( "tan", b ) - f.Var;
            }

            throw new SymbolicException( "Could not invert " + f );
        }
    }
}

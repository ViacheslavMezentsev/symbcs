using System.Collections;

namespace Tiny.Science.Symbolic
{
    public class LambdaLINSOLVE : Lambda
    {
        public override int Eval( Stack stack )
        {
            int narg = GetNarg( stack );

            if ( narg != 2 )
            {
                throw new ParseException( "linsolve requires 2 arguments." );
            }

            var M_in = GetAlgebraic( stack );
            var b_in = GetAlgebraic( stack );

            var M = new Matrix( M_in );

            var b = ( b_in is Vector ? Matrix.Column( ( Vector ) b_in ) : new Matrix( b_in ) );
            var r = ( ( Matrix ) ( b.transpose() / M.transpose() ) ).transpose().Reduce();

            stack.Push( r );

            return 0;
        }

        public virtual int lambda2( Stack stack )
        {
            int narg = GetNarg( stack );

            if ( narg != 2 )
            {
                throw new ParseException( "linsolve requires 2 arguments." );
            }

            var expr = ( Vector ) GetVektor( stack ).Rat();
            var vars = GetVektor( stack );

            elim( expr, vars, 0 );
            subst( expr, vars, expr.Length() - 1 );
            stack.Push( expr );

            return 0;
        }

        private static void subst( Vector expr, Vector vars, int n )
        {
            if ( n < 0 )
            {
                return;
            }
            var pa = expr[ n ];
            if ( pa is Polynomial )
            {
                var p = ( Polynomial ) pa;
                Variable v = null;
                Algebraic c1 = null, c0;
                for ( int k = 0; k < vars.Length(); k++ )
                {
                    var va = ( ( Polynomial ) vars[ k ] )._v;
                    c1 = p.coefficient( va, 1 );
                    if ( !c1.Equals( Symbol.ZERO ) )
                    {
                        v = va;
                        break;
                    }
                }
                if ( v != null )
                {
                    expr[ n ] = p / c1;

                    var val = -p.coefficient( v, 0 ) / c1;

                    for ( int k = 0; k < n; k++ )
                    {
                        var ps = expr[ k ];
                        if ( ps is Polynomial )
                        {
                            expr.set( k, ( ( Polynomial ) ps ).Value( v, val ) );
                        }
                    }
                }
            }
            subst( expr, vars, n - 1 );
        }

        private static void elim( Vector expr, Vector vars, int n )
        {
            if ( n >= expr.Length() )
            {
                return;
            }
            double maxc = 0.0;
            int iv = 0, ie = 0;
            Variable vp = null;
            Algebraic f = Symbol.ONE;
            Polynomial pm = null;
            for ( int i = 0; i < vars.Length(); i++ )
            {
                var v = ( ( Polynomial ) vars[ i ] )._v;
                for ( int k = n; k < expr.Length(); k++ )
                {
                    var pa = expr[ k ];
                    if ( pa is Polynomial )
                    {
                        var p = ( Polynomial ) pa;
                        var c = p.coefficient( v, 1 );
                        double nm = c.Norm();
                        if ( nm > maxc )
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
            if ( maxc == 0.0 )
            {
                return;
            }

            expr.set( ie, expr[ n ] );
            expr.set( n, pm );

            for ( int i = n + 1; i < expr.Length(); i++ )
            {
                var p = expr[ i ];

                if ( p is Polynomial )
                {
                    var fc = ( ( Polynomial ) p ).coefficient( vp, 1 );

                    if ( !fc.Equals( Symbol.ZERO ) )
                    {
                        p = p - pm * fc / f;
                    }
                }

                expr.set( i, p );
            }

            elim( expr, vars, n + 1 );
        }

        private static void eliminierung( Matrix a, Vector c )
        {
            int n = c.Length();

            for ( int k = 0; k < n - 1; k++ )
            {
                pivot( a, c, k );

                for ( int i = k + 1; i < n; i++ )
                {
                    var factor = a[ i, k ] / a[ k, k ];

                    for ( int j = k; j < n; j++ )
                    {
                        a[ i, j ] = a[ i, j ] - factor * a[ k, j ];
                    }

                    c[ i ] = c[ i ] - factor * c[ k ];
                }
            }
        }

        public static Vector Substitution( Matrix a, Vector c )
        {
            int n = c.Length();
            var x = new Algebraic[ n ];

            x[ n - 1 ] = c[ n - 1 ] / a[ n - 1, n - 1 ];

            for ( int i = n - 2; i >= 0; i-- )
            {
                Algebraic sum = Symbol.ZERO;

                for ( int j = i + 1; j < n; j++ )
                {
                    sum = ( sum + a[ i, j ] ) * x[ j ];
                }

                x[ i ] = ( c[ i ] - sum ) / a[ i, i ];
            }

            return new Vector( x );
        }

        public static Vector Gauss( Matrix a, Vector c )
        {
            int n = c.Length();
            var x = new Algebraic[ n ];

            for ( int k = 0; k < n - 1; k++ )
            {
                pivot( a, c, k );

                if ( a[ k, k ] != Symbol.ZERO )
                {
                    for ( int i = k + 1; i < n; i++ )
                    {
                        var factor = a[ i, k ] / a[ k, k ];

                        for ( int j = k + 1; j < n; j++ )
                        {
                            a[ i, j ] = a[ i, j ] - factor * a[ k, j ];
                        }

                        c.set( i, c[ i ] - factor * c[ k ] );
                    }
                }
            }

            x[ n - 1 ] = c[ n - 1 ] / a[ n - 1, n - 1 ];

            for ( int i = n - 2; i >= 0; i-- )
            {
                Algebraic sum = Symbol.ZERO;

                for ( int j = i + 1; j < n; j++ )
                {
                    sum = sum + a[ i, j ] * x[ j ];
                }

                x[ i ] = ( c[ i ] - sum ) / a[ i, i ];
            }

            return new Vector( x );
        }

        private static int pivot( Matrix a, Vector c, int k )
        {
            int pivot = k, n = c.Length();

            var maxa = a[ k, k ].Norm();

            for ( int i = k + 1; i < n; i++ )
            {
                var dummy = a[ i, k ].Norm();

                if ( dummy > maxa )
                {
                    maxa = dummy;
                    pivot = i;
                }
            }

            if ( pivot != k )
            {
                for ( int j = k; j < n; j++ )
                {
                    var dummy = a[ pivot, j ];

                    a[ pivot, j ] = a[ k, j ];

                    a[ k, j ] = dummy;
                }
                {
                    var dummy = c[ pivot ];

                    c[ pivot ] = c[ k ];

                    c[ k ] = dummy;
                }
            }

            return pivot;
        }
    }
}

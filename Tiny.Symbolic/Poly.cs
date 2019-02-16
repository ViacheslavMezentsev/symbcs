namespace Tiny.Symbolic
{
    public sealed class Poly
    {
        public static Polynomial top = new Polynomial( SimpleVariable.top );

        internal static Algebraic[] pqsolve( Algebraic p, Algebraic q )
        {
            var r = p * Symbol.MINUS / Symbol.TWO;

            var s = FunctionVariable.Create( "sqrt", r * r - q );

            var result = new[] { r + s, r - s };

            return result;
        }

        public static int Degree( Algebraic a, Variable v )
        {
            if ( a is Polynomial )
            {
                var p = ( Polynomial ) a;

                return p.Degree(v);
            }

            if ( a is Rational )
            {
                var r = ( Rational ) a;

                if ( r.den.Depends( v ) )
                {
                    return 0;
                }

                return Degree( r.nom, v );
            }

            return 0;
        }

        public static Algebraic Coefficient( Algebraic p, Variable v, int n )
        {
            if ( p is Polynomial )
            {
                return ( ( Polynomial ) p ).coefficient( v, n );
            }

            if ( p is Rational )
            {
                var r = ( Rational ) p;

                if ( r.den.Depends( v ) )
                {
                    throw new SymbolicException( "Cannot determine coefficient of " + v + " in " + r );
                }

                return Coefficient( r.nom, v, n ) / r.den;
            }

            return n == 0 ? p : Symbol.ZERO;
        }

        public static void polydiv( Algebraic[] a, Variable v )
        {
            int d0 = Degree( a[ 0 ], v ), d1 = Degree( a[ 1 ], v ), d = d0 - d1;

            if ( d < 0 )
            {
                a[ 1 ] = a[ 0 ];
                a[ 0 ] = Symbol.ZERO;

                return;
            }

            if ( d1 == 0 )
            {
                a[ 1 ] = Symbol.ZERO;
                return;
            }

            var cdiv = new Algebraic[ d + 1 ];
            var nom = new Algebraic[ d0 + 1 ];

            for ( int i = 0; i < nom.Length; i++ )
            {
                nom[ i ] = Coefficient( a[ 0 ], v, i );
            }

            var den = Coefficient( a[ 1 ], v, d1 );

            for ( int i = d, k = d0; i >= 0; i--, k-- )
            {
                var cd = nom[ k ] / den;

                cdiv[ i ] = cd;
                nom[ k ] = Symbol.ZERO;

                for ( int j = k - 1, l = d1 - 1; j > k - ( d1 + 1 ); j--, l-- )
                {
                    nom[ j ] = nom[ j ] - cd * Coefficient( a[ 1 ], v, l );
                }
            }

            a[ 0 ] = horner( v, cdiv, d + 1 );
            a[ 1 ] = horner( v, nom, d1 );
        }

        public static Algebraic horner( Variable x, Algebraic[] c, int n )
        {
            if ( n == 0 )
            {
                return Symbol.ZERO;
            }

            if ( n > c.Length )
            {
                throw new SymbolicException( "Can not create horner polynomial." );
            }

            var X = new Polynomial( x );

            var p = c[ n - 1 ];

            for ( int i = n - 2; i >= 0; i-- )
            {
                p = p * X + c[ i ];
            }

            return p;
        }

        public static Algebraic horner( Variable x, Algebraic[] c )
        {
            return horner( x, c, c.Length );
        }

        public static Algebraic[] Clone( Vector vec )
        {
            var c = new Algebraic[ vec.Length() ];

            for ( int n = 0; n < vec.Length(); n++ )
            {
                c[ n ] = vec[ n ];
            }

            return c;
        }

        public static Algebraic[] Clone( Algebraic[] x )
        {
            var c = new Algebraic[ x.Length ];

            for ( int i = 0; i < x.Length; i++ )
            {
                c[ i ] = x[ i ];
            }

            return c;
        }

        public static Algebraic[] Reduce( Algebraic[] x )
        {
            int len = x.Length;

            while ( len > 0 && ( x[ len - 1 ] == null || x[ len - 1 ].Equals( Symbol.ZERO ) ) )
            {
                len--;
            }

            if ( len == 0 )
            {
                len = 1;
            }

            if ( len != x.Length )
            {
                var na = new Algebraic[ len ];

                for ( int i = 0; i < len; i++ )
                {
                    na[ i ] = x[ i ];
                }

                return na;
            }

            return x;
        }

        public static Algebraic polydiv( Algebraic p1, Algebraic q1 )
        {
            if ( q1 is Symbol )
            {
                return p1 / q1;
            }

            if ( p1.Equals( Symbol.ZERO ) )
            {
                return Symbol.ZERO;
            }

            if ( !( p1 is Polynomial ) || !( q1 is Polynomial ) )
            {
                throw new SymbolicException( "Polydiv is implemented for polynomials only.Got " + p1 + " / " + q1 );
            }

            var p = ( Polynomial ) p1;
            var q = ( Polynomial ) q1;

            if ( p._v.Equals( q._v ) )
            {
                int len = p.Degree() - q.Degree();

                if ( len < 0 )
                {
                    throw new SymbolicException( "Polydiv requires zero rest." );
                }

                var cdiv = new Algebraic[ len + 1 ];
                var nom = Clone( p.Coeffs );
                var den = q[ q.Coeffs.Length - 1 ];

                for ( int i = len, k = nom.Length - 1; i >= 0; i--, k-- )
                {
                    cdiv[ i ] = polydiv( nom[ k ], den );
                    nom[ k ] = Symbol.ZERO;

                    for ( int j = k - 1, l = q.Coeffs.Length - 2; j > k - q.Coeffs.Length; j--, l-- )
                    {
                        nom[ j ] = nom[ j ] - cdiv[ i ] * q[ l ];
                    }
                }

                return horner( p._v, cdiv );
            }
            else
            {
                var cn = new Algebraic[ p.Coeffs.Length ];

                for ( int i = 0; i < p.Coeffs.Length; i++ )
                {
                    cn[ i ] = polydiv( p[ i ], q1 );
                }

                return horner( p._v, cn );
            }
        }

        public static Algebraic Mod( Algebraic p, Algebraic q, Variable r )
        {
            int len = Degree( p, r ) - Degree( q, r );

            if ( len < 0 )
            {
                return p;
            }

            var cdiv = new Algebraic[ len + 1 ];
            var nom = new Algebraic[ Degree( p, r ) + 1 ];

            for ( int i = 0; i < nom.Length; i++ )
            {
                nom[ i ] = Coefficient( p, r, i );
            }

            var den = Coefficient( q, r, Degree( q, r ) );

            for ( int i = len, k = nom.Length - 1; i >= 0; i--, k-- )
            {
                cdiv[ i ] = polydiv( nom[ k ], den );
                nom[ k ] = Symbol.ZERO;

                for ( int j = k - 1, l = ( Degree( q, r ) + 1 ) - 2; j > k - ( Degree( q, r ) + 1 ); j--, l-- )
                {
                    nom[ j ] = nom[ j ] - cdiv[ i ] * Coefficient( q, r, l );
                }
            }

            return horner( r, nom, nom.Length - 1 - len );
        }

        public static Algebraic Euclid( Algebraic p, Algebraic q, Variable r )
        {
            int dp = Degree( p, r );
            int dq = Degree( q, r );

            var a = dp < dq ? p : p * Coefficient( q, r, dq ) ^ ( dp - dq + 1 );
            var b = q;
            var c = Mod( a, b, r );
            var result = c.Equals( Symbol.ZERO ) ? b : Euclid( b, c, r );

            return result;
        }

        public static Algebraic poly_gcd( Algebraic p, Algebraic q )
        {
            if ( p.Equals( Symbol.ZERO ) )
            {
                return q;
            }

            if ( q.Equals( Symbol.ZERO ) )
            {
                return p;
            }

            if ( p is Symbol || q is Symbol )
            {
                return Symbol.ONE;
            }

            var r = ( ( Polynomial ) q )._v.Smaller( ( ( Polynomial ) p )._v ) ? ( ( Polynomial ) p )._v : ( ( Polynomial ) q )._v;

            Algebraic pc = Content( p, r ), qc = Content( q, r );

            var eu = Euclid( polydiv( p, pc ), polydiv( q, qc ), r );

            var re = polydiv( eu, Content( eu, r ) ) * poly_gcd( pc, qc );

            if ( re is Symbol )
            {
                return Symbol.ONE;
            }

            var rp = ( Polynomial ) re;
            Algebraic res = rp;

            if ( rp[ rp.Degree() ] is Symbol )
            {
                res = rp / rp[ rp.Degree() ];
            }

            return res;
        }

        public static Algebraic Content( Algebraic p, Variable r )
        {
            if ( p is Symbol )
            {
                return p;
            }

            var result = Coefficient( p, r, 0 );

            for ( int i = 0; i <= Degree( p, r ) && !result.Equals( Symbol.ONE ); i++ )
            {
                result = poly_gcd( result, Coefficient( p, r, i ) );
            }

            return result;
        }

        internal static int gcd( int a, int b )
        {
            int c = 1;

            while ( c != 0 )
            {
                c = a % b;
                a = b;
                b = c;
            }

            return a;
        }
    }
}

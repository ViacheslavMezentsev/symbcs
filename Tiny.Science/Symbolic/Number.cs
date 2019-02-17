using System.Collections;

using Tiny.Science.Numeric;
using BigInteger = Tiny.Science.Numeric.BigInteger;

namespace Tiny.Science.Symbolic
{
    public class Number : Symbol
    {
        internal BigInteger[] real;
        internal BigInteger[] imag;

        public Number( BigInteger[] real ) : this( real, null )
        {
        }

        public Number( long nom, long den )
        {
            real = new BigInteger[ 2 ];

            real[ 0 ] = BigInteger.valueOf( nom );
            real[ 1 ] = BigInteger.valueOf( den );
        }

        public Number( BigInteger r )
        {
            real = new BigInteger[ 2 ];

            real[ 0 ] = r;
            real[ 1 ] = BigInteger.ONE;
        }

        public Number( BigInteger[] real, BigInteger[] imag )
        {
            this.real = reducev( real );

            if ( imag != null && !imag[ 0 ].Equals( BigInteger.ZERO ) )
            {
                this.imag = reducev( imag );
            }
        }

        public Number( double x ) : this( x, 0.0 )
        {
        }

        public Number( double x, double y )
        {
            real = reducev( double2rat( x ) );

            if ( y != 0.0 )
            {
                imag = reducev( double2rat( y ) );
            }
        }


        internal virtual BigInteger double2big( double x )
        {
            int exp = 0;

            while ( x > 1e15 )
            {
                x /= 10.0;
                exp++;
            }

            BigInteger y = BigInteger.valueOf( ( long ) ( Math.round( x ) ) );

            if ( exp > 0 )
            {
                BigInteger ten = BigInteger.valueOf( 10L );

                y = y.multiply( ten.pow( exp ) );
            }

            return y;
        }

        private BigInteger[] double2rat( double x )
        {
            BigInteger[] br;

            if ( x == 0 )
            {
                br = new BigInteger[ 2 ];

                br[ 0 ] = BigInteger.ZERO;
                br[ 1 ] = BigInteger.ONE;

                return br;
            }

            if ( x < 0.0 )
            {
                br = double2rat( -x );
                br[ 0 ] = br[ 0 ].negate();

                return br;
            }

            double eps = 1.0e-8;

            var a = Session.Proc.Store.GetNum( "ratepsilon" );

            if ( a != null )
            {
                double epstry = a.ToComplex().Re;

                if ( epstry > 0 )
                {
                    eps = epstry;
                }
            }

            if ( x < 1 / eps )
            {
                double[] y = cfs( x, eps );

                br = new BigInteger[ 2 ];

                br[ 0 ] = double2big( y[ 0 ] );
                br[ 1 ] = double2big( y[ 1 ] );

                return br;
            }

            br = new BigInteger[ 2 ];

            br[ 0 ] = double2big( x );
            br[ 1 ] = BigInteger.ONE;

            return br;
        }

        private double[] cfs( double x, double tol )
        {
            var a = new ArrayList();

            var y = new double[ 2 ];

            tol = Math.abs( x * tol );

            var aa = Math.floor( x );

            a.Add( aa );

            var ra = x;

            cfsd( a, y );

            while ( Math.abs( x - y[ 0 ] / y[ 1 ] ) > tol )
            {
                ra = 1.0 / ( ra - aa );
                aa = Math.floor( ra );
                a.Add( aa );
                cfsd( a, y );
            }

            return y;
        }

        private void cfsd( ArrayList a, double[] y )
        {
            int i = a.Count - 1;

            double N = ( double ) ( ( double? ) a[ i ] ), Z = 1.0, N1;

            i--;

            while ( i >= 0 )
            {
                N1 = ( double ) ( ( double? ) a[ i ] ) * N + Z;
                Z = N;
                N = N1;
                i--;
            }

            y[ 0 ] = N;
            y[ 1 ] = Z;
        }

        internal virtual Number cfs( double tol1 )
        {
            var list = new ArrayList();

            Number error, y, ra, tol;
            BigInteger aa;

            tol = ( Number ) ( this * new Number( tol1 ) );

            aa = real[ 0 ].divide( real[ 1 ] );

            list.Add( aa );

            y = new Number( cfs( list ) );

            error = ( Number ) ( ( Number ) ( this - y ) ).Abs();
            ra = this;

            while ( tol.Smaller( error ) )
            {
                ra = ( Number ) ( ONE / ( ra - new Number( aa ) ) );

                aa = ra.real[ 0 ].divide( ra.real[ 1 ] );
                list.Add( aa );
                y = new Number( cfs( list ) );

                error = ( Number ) ( ( Number ) ( this - y ) ).Abs();
            }

            return y;
        }

        private BigInteger[] cfs( ArrayList a )
        {
            int i = a.Count - 1;

            BigInteger N = ( BigInteger ) a[ i ], Z = BigInteger.ONE, N1;

            i--;

            while ( i >= 0 )
            {
                N1 = ( ( BigInteger ) a[ i ] ).multiply( N ).add( Z );
                Z = N;
                N = N1;
                i--;
            }

            BigInteger[] r = new BigInteger[] { N, Z };

            return r;
        }

        private BigInteger[] reducev( BigInteger[] y )
        {
            BigInteger[] x = new BigInteger[ 2 ];

            x[ 0 ] = y[ 0 ];
            x[ 1 ] = y[ 1 ];

            BigInteger gcd = x[ 0 ].gcd( x[ 1 ] );

            if ( !gcd.Equals( BigInteger.ONE ) )
            {
                x[ 0 ] = x[ 0 ].divide( gcd );
                x[ 1 ] = x[ 1 ].divide( gcd );
            }

            if ( x[ 1 ].compareTo( BigInteger.ZERO ) < 0 )
            {
                x[ 0 ] = x[ 0 ].negate();
                x[ 1 ] = x[ 1 ].negate();
            }

            return x;
        }

        public override Algebraic RealPart()
        {
            return new Number( real );
        }

        public override Algebraic ImagPart()
        {
            if ( imag != null )
            {
                return new Number( imag );
            }

            return new Number( BigInteger.ZERO );
        }

        public override bool IsNumber()
        {
            return true;
        }

        private double floatValue( BigInteger[] x )
        {
            var q = x[ 0 ].divideAndRemainder( x[ 1 ] );

            return q[ 0 ].ToDouble() + q[ 1 ].ToDouble() / x[ 1 ].ToDouble();
        }

        public virtual Complex ToFloat()
        {
            return imag == null ? new Complex( floatValue( real ) ) : new Complex( floatValue( real ), floatValue( imag ) );
        }

        private BigInteger[] add( BigInteger[] x, BigInteger[] y )
        {
            if ( x == null )
            {
                return y;
            }

            if ( y == null )
            {
                return x;
            }

            BigInteger[] r = new BigInteger[ 2 ];

            r[ 0 ] = x[ 0 ].multiply( y[ 1 ] ).add( y[ 0 ].multiply( x[ 1 ] ) );
            r[ 1 ] = x[ 1 ].multiply( y[ 1 ] );

            return r;
        }

        private BigInteger[] sub( BigInteger[] x, BigInteger[] y )
        {
            if ( y == null )
            {
                return x;
            }

            BigInteger[] r = new BigInteger[ 2 ];

            r[ 0 ] = y[ 0 ].negate();
            r[ 1 ] = y[ 1 ];

            return add( x, r );
        }

        private BigInteger[] mult( BigInteger[] x, BigInteger[] y )
        {
            if ( x == null || y == null )
            {
                return null;
            }

            BigInteger[] r = new BigInteger[ 2 ];

            r[ 0 ] = x[ 0 ].multiply( y[ 0 ] );
            r[ 1 ] = x[ 1 ].multiply( y[ 1 ] );

            return r;
        }

        private BigInteger[] div( BigInteger[] x, BigInteger[] y )
        {
            if ( x == null )
            {
                return null;
            }

            if ( y == null )
            {
                throw new SymbolicException( "Division by Zero." );
            }

            BigInteger[] r = new BigInteger[ 2 ];

            r[ 0 ] = x[ 0 ].multiply( y[ 1 ] );
            r[ 1 ] = x[ 1 ].multiply( y[ 0 ] );

            return r;
        }

        private bool Equals( BigInteger[] x, BigInteger[] y )
        {
            if ( x == null && y == null )
            {
                return true;
            }

            if ( x == null || y == null )
            {
                return false;
            }

            return x[ 0 ].Equals( y[ 0 ] ) && x[ 1 ].Equals( y[ 1 ] );
        }

        protected override Algebraic Add( Algebraic x )
        {
            if ( !( x is Symbol ) )
            {
                return x + this;
            }

            Number X = ( ( Symbol ) x ).ToNumber();

            return new Number( add( real, X.real ), add( imag, X.imag ) );
        }

        protected override Algebraic Mul( Algebraic x )
        {
            if ( !( x is Symbol ) )
            {
                return x * this;
            }

            Number X = ( ( Symbol ) x ).ToNumber();

            return new Number( sub( mult( real, X.real ), mult( imag, X.imag ) ), add( mult( imag, X.real ), mult( real, X.imag ) ) );
        }

        protected override Algebraic Div( Algebraic x )
        {
            if ( !( x is Symbol ) )
            {
                return base.Div( x );
            }

            Number X = ( ( Symbol ) x ).ToNumber();

            BigInteger[] N = add( mult( X.real, X.real ), mult( X.imag, X.imag ) );

            if ( N == null || N[ 0 ].Equals( BigInteger.ZERO ) )

            {
                throw new SymbolicException( "Division by Zero." );
            }

            return new Number( div( add( mult( real, X.real ), mult( imag, X.imag ) ), N ), div( sub( mult( imag, X.real ), mult( real, X.imag ) ), N ) );
        }

        private BigInteger lsm( BigInteger x, BigInteger y )
        {
            return x.multiply( y ).divide( x.gcd( y ) );
        }

        public override Algebraic[] Div( Algebraic q1, Algebraic[] result )
        {
            if ( result == null )
            {
                result = new Algebraic[ 2 ];
            }

            if ( !( q1 is Symbol ) )
            {
                result[ 0 ] = Symbol.ZERO;
                result[ 1 ] = this;

                return result;
            }

            var q = ( ( Symbol ) q1 ).ToNumber();

            if ( !IsComplex() && q.IsComplex() )
            {
                result[ 0 ] = Symbol.ZERO;
                result[ 1 ] = this;
                return result;
            }

            if ( IsComplex() && !q.IsComplex() )
            {
                result[ 0 ] = Div( q );
                result[ 1 ] = Symbol.ZERO;

                return result;
            }

            if ( IsComplex() && q.IsComplex() )
            {
                result[ 0 ] = ImagPart() / q.ImagPart();
                result[ 1 ] = this - result[ 0 ] * q;

                return result;
            }

            if ( IsInteger() && q.IsInteger() )
            {
                var d = real[ 0 ].divideAndRemainder( q.real[ 0 ] );

                result[ 0 ] = new Number( d[ 0 ] );
                result[ 1 ] = new Number( d[ 1 ] );

                return result;
            }

            result[ 0 ] = Div( q );
            result[ 1 ] = Symbol.ZERO;

            return result;
        }

        private string b2string( BigInteger[] x )
        {
            if ( x[ 1 ].Equals( BigInteger.ONE ) )
            {
                return x[ 0 ].ToString();
            }

            return x[ 0 ] + "/" + x[ 1 ];
        }

        public override string ToString()
        {
            if ( imag == null || imag[ 0 ].Equals( BigInteger.ZERO ) )
            {
                return "" + b2string( real );
            }

            if ( real[ 0 ].Equals( BigInteger.ZERO ) )
            {
                return b2string( imag ) + "*i";
            }

            return "(" + b2string( real ) + ( imag[ 0 ].compareTo( BigInteger.ZERO ) > 0 ? "+" : "" ) + b2string( imag ) + "*i)";
        }

        public override bool IsInteger()
        {
            return real[ 1 ].Equals( BigInteger.ONE ) && imag == null;
        }

        public override bool Smaller( Symbol x )
        {
            return ToComplex().Smaller( x );
        }

        public override bool IsComplex()
        {
            return imag != null && !imag[ 0 ].Equals( BigInteger.ZERO );
        }

        public override bool IsImaginary()
        {
            return imag != null && !imag[ 0 ].Equals( BigInteger.ZERO ) && real[ 0 ].Equals( BigInteger.ZERO );
        }

        public override bool Equals( object x )
        {
            if ( x is Number )
            {
                return Equals( real, ( ( Number ) x ).real ) && Equals( imag, ( ( Number ) x ).imag );
            }

            return ToFloat().Equals( x );
        }

        public override double Norm()
        {
            return ToFloat().Norm();
        }

        public override Algebraic Rat()
        {
            return this;
        }

        public override Symbol Abs()
        {
            if ( IsComplex() )
            {
                return ToFloat().Abs();
            }

            BigInteger[] r = new BigInteger[ 2 ];

            r[ 0 ] = real[ 0 ].compareTo( BigInteger.ZERO ) < 0 ? real[ 0 ].negate() : real[ 0 ];
            r[ 1 ] = real[ 1 ];

            return new Number( r );
        }

        public virtual Number gcd( Number x )
        {
            if ( Equals( Symbol.ZERO ) )
            {
                return x;
            }
            else if ( x.Equals( Symbol.ZERO ) )
            {
                return this;
            }
            if ( IsComplex() && x.IsComplex() )
            {
                var r = ( ( Number ) RealPart() ).gcd( ( Number ) x.RealPart() );
                var i = ( ( Number ) ImagPart() ).gcd( ( Number ) x.ImagPart() );

                if ( r.Equals( Symbol.ZERO ) )
                {
                    return ( Number ) i.Mul( Symbol.IONE );
                }

                if ( ( RealPart() / r ).Equals( ImagPart() / i ) )
                {
                    return ( Number ) r.Add( i.Mul( Symbol.IONE ) );
                }
                else
                {
                    return Symbol.ONE.ToNumber();
                }
            }
            else if ( IsComplex() || x.IsComplex() )
            {
                return Symbol.ONE.ToNumber();
            }
            else
            {
                return ( Number ) ( new Number( real[ 0 ].multiply( x.real[ 1 ] ).gcd( real[ 1 ].multiply( x.real[ 0 ] ) ) ) ).Div( new Number( real[ 1 ].multiply( x.real[ 1 ] ) ) );
            }
        }

        public override int ToInt()
        {
            return real[ 0 ].intValue();
        }
    }
}

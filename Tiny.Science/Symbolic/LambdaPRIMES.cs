using System;
using System.Collections;

using Math = Tiny.Science.Numeric.Math;
using BigInteger = Tiny.Science.Numeric.BigInteger;

namespace Tiny.Science.Symbolic
{
    public class LambdaPRIMES : Lambda
    {
        public override int Eval( Stack stack )
        {
            int narg = GetNarg( stack );

            var x = GetAlgebraic( stack );

            if ( !( x is Symbol ) && !( ( Symbol ) x ).IsInteger() )
            {
                throw new SymbolicException( "Expected integer argument." );
            }

            if ( ( Symbol ) x < Symbol.ZERO )
            {
                x = -x;
            }

            Algebraic res;

            if ( x is Number )
            {
                var xl = ( long ) ( ( Symbol ) x ).ToComplex().Re;

                res = teiler( xl );
            }
            else
            {
                var xb = ( ( Number ) x ).real[ 0 ];

                if ( xb.compareTo( BigInteger.valueOf( long.MaxValue ) ) <= 0 )
                {
                    var xl = xb.longValue();

                    res = teiler( xl );
                }
                else
                {
                    res = teiler( xb );
                }
            }

            if ( res != null )
            {
                stack.Push( res );
            }

            return 0;
        }

        internal static readonly int[] mod = { 1, 7, 11, 13, 17, 19, 23, 29 };
        internal static readonly int[] moddif = { 1, 6, 4, 2, 4, 2, 4, 6 };

        internal static long kleinsterTeiler( long X, long start )
        {
            long stop = ( long ) Math.ceil( Math.Sqrt( ( double ) X ) );

            if ( start > stop )
            {
                return X;
            }

            long b = start / 30L;

            b *= 30L;

            long m = start % 30L;

            int i = 0;

            while ( m > mod[ i ] )
            {
                i++;
            }

            while ( start <= stop )
            {
                if ( Session.Proc.CheckInterrupt() )
                {
                    return -1L;
                }

                if ( X % start == 0 )
                {
                    return start;
                }

                i++;

                if ( i >= mod.Length )
                {
                    i = 0;
                    b += 30L;
                    start = b;
                }

                start += moddif[ i ];
            }

            return X;
        }

        internal static Vector teiler( long X )
        {
            var teiler = new ArrayList();

            while ( X % 2L == 0 )
            {
                teiler.Add( Symbol.TWO );
                X /= 2L;
            }

            while ( X % 3L == 0 )
            {
                teiler.Add( Symbol.THREE );
                X /= 3L;
            }

            while ( X % 5L == 0 )
            {
                teiler.Add( new Number( 5.0 ) );
                X /= 5L;
            }

            long f = 7L;

            while ( X != 1L )
            {
                f = kleinsterTeiler( X, f );

                if ( f < 0 )
                {
                    return null;
                }

                teiler.Add( new Number( f, 1L ) );
                X /= f;
            }

            return Vector.Create( teiler );
        }

        static BigInteger kleinsterTeiler( BigInteger X, BigInteger start )
        {
            var stop_in = new sbyte[ X.bitLength() / 2 + 1 ];

            stop_in[ 0 ] = ( sbyte ) 1;

            for ( int n = 1; n < stop_in.Length; n++ )
            {
                stop_in[ n ] = ( sbyte ) 0;
            }

            var stop = new BigInteger( stop_in );

            if ( start.compareTo( stop ) > 0 )
            {
                return X;
            }

            var b30 = BigInteger.valueOf( 30L );
            var b = start.divide( b30 );

            b = b.multiply( b30 );

            int m = ( int ) start.mod( b30 ).intValue();
            int i = 0;

            while ( m > mod[ i ] )
            {
                i++;
            }

            while ( start.compareTo( stop ) <= 0 )
            {
                if ( Session.Proc.CheckInterrupt() )
                {
                    return null;
                }

                if ( X.mod( start ).Equals( BigInteger.ZERO ) )
                {
                    return start;
                }

                i++;

                if ( i >= mod.Length )
                {
                    i = 0;
                    b = b.add( b30 );
                    start = b;
                }

                start = start.add( BigInteger.valueOf( ( long ) moddif[ i ] ) );
            }

            return X;
        }

        static Vector teiler( BigInteger X )
        {
            var teiler = new ArrayList();

            var b2 = BigInteger.valueOf( 2L );

            while ( X.mod( b2 ).Equals( BigInteger.ZERO ) )
            {
                teiler.Add( Symbol.TWO );

                X = X.divide( b2 );
            }

            var b3 = BigInteger.valueOf( 3L );

            while ( X.mod( b3 ).Equals( BigInteger.ZERO ) )
            {
                teiler.Add( Symbol.THREE );

                X = X.divide( b3 );
            }

            var b5 = BigInteger.valueOf( 5L );

            while ( X.mod( b5 ).Equals( BigInteger.ZERO ) )
            {
                teiler.Add( new Number( 5.0 ) );

                X = X.divide( b5 );
            }

            var f = BigInteger.valueOf( 7L );

            while ( !X.Equals( BigInteger.ONE ) )
            {
                f = kleinsterTeiler( X, f );

                if ( f == null )
                {
                    return null;
                }

                teiler.Add( new Number( f ) );

                X = X.divide( f );
            }

            return Vector.Create( teiler );
        }
    }
}

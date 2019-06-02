using System;

namespace Tiny.Science.Numeric
{
    public sealed partial class Math
    {
        public static readonly double PI = BitConverter.Int64BitsToDouble( 0x400921fb54442d18L );

        public const double E = 2.7182818284590452354;

        private static Random random_Renamed;

        public static int abs( int x )
        {
            return ( ( x < 0 ) ? -x : x );
        }

        public static long abs( long x )
        {
            return ( ( x < 0L ) ? -x : x );
        }

        public static float abs( float x )
        {
            return ( ( x <= 0.0f ) ? 0.0f - x : x );
        }

        public static double abs( double x )
        {
            return ( ( x <= 0.0 ) ? 0.0 - x : x );
        }

        public static int min( int x, int y )
        {
            return ( ( x < y ) ? x : y );
        }

        public static long min( long x, long y )
        {
            return ( ( x < y ) ? x : y );
        }

        public static float min( float x, float y )
        {
            if ( float.IsNaN( x ) )
            {
                return x;
            }

            float ans = ( ( x <= y ) ? x : y );

            if ( ans == 0.0f && y == 1.0f / float.NegativeInfinity )
            {
                ans = y;
            }

            return ans;
        }

        public static double min( double x, double y )
        {
            if ( double.IsNaN( x ) )
            {
                return x;
            }

            double ans = ( ( x <= y ) ? x : y );

            if ( x == 0.0 && y == 0.0 && y == 1.0 / double.NegativeInfinity )
            {
                ans = y;
            }

            return ans;
        }

        public static int max( int x, int y )
        {
            return ( ( x > y ) ? x : y );
        }

        public static long max( long x, long y )
        {
            return ( ( x > y ) ? x : y );
        }

        public static float max( float x, float y )
        {
            if ( float.IsNaN( x ) )
            {
                return x;
            }

            float ans = ( ( x >= y ) ? x : y );

            if ( ans == 0.0f && y == 1.0f / float.NegativeInfinity )
            {
                ans = y;
            }

            return ans;
        }

        public static double max( double x, double y )
        {
            if ( double.IsNaN( x ) )
            {
                return x;
            }

            double ans = ( ( x >= y ) ? x : y );

            if ( x == 0.0 && y == 0.0 && y == 1.0 / double.NegativeInfinity )
            {
                ans = y;
            }

            return ans;
        }

        public static int round( float x )
        {
            return ( int ) floor( x + 0.5f );
        }

        public static long round( double x )
        {
            return ( long ) floor( x + 0.5 );
        }

        public static double random()
        {
            lock ( typeof( Math ) )
            {
                if ( random_Renamed == null )
                {
                    random_Renamed = new Random();
                }

                return random_Renamed.NextDouble();
            }
        }

        private const double huge = 1.0e+300;
        private const double tiny = 1.0e-300;

        public static double ceil( double x )
        {
            int exp, sign;
            long ix;

            if ( x == 0 )
            {
                return x;
            }

            ix = BitConverter.DoubleToInt64Bits( x );

            sign = ( int ) ( ( ix >> 63 ) & 1 );

            exp = ( ( int ) ( ix >> 52 ) & 0x7ff ) - 0x3ff;

            if ( exp < 0 )
            {
                if ( x < 0.0 )
                {
                    return NEGATIVE_ZERO;
                }
                else if ( x == 0.0 )
                {
                    return x;
                }
                else
                {
                    return 1.0;
                }
            }
            else if ( exp < 53 )
            {
                // TODO: Check this
                long mask = ( long ) ( 0x000fffffffffffffL >> exp );

                if ( ( mask & ix ) == 0 )
                {
                    return x;
                }

                if ( x > 0.0 )
                {
                    ix += 0x0010000000000000L >> exp;
                }

                ix = ix & ( ~mask );
            }
            else if ( exp == 1024 )
            {
                return x;
            }

            return BitConverter.Int64BitsToDouble( ix );
        }

        public static double floor( double x )
        {
            int exp, sign;
            long ix;

            if ( x == 0 )
            {
                return x;
            }

            ix = BitConverter.DoubleToInt64Bits( x );

            sign = ( int ) ( ( ix >> 63 ) & 1 );

            exp = ( ( int ) ( ix >> 52 ) & 0x7ff ) - 0x3ff;

            if ( exp < 0 )
            {
                if ( x < 0.0 )
                {
                    return -1.0;
                }
                else if ( x == 0.0 )
                {
                    return x;
                }
                else
                {
                    return 0.0;
                }
            }
            else if ( exp < 53 )
            {
                // TODO: Check this
                long mask = ( long ) ( 0x000fffffffffffffL >> exp );

                if ( ( mask & ix ) == 0 )
                {
                    return x;
                }

                if ( x < 0.0 )
                {
                    ix += 0x0010000000000000L >> exp;
                }

                ix = ix & ( ~mask );
            }
            else if ( exp == 1024 )
            {
                return x;
            }

            return BitConverter.Int64BitsToDouble( ix );
        }

        private static readonly double[] TWO52 =
        {
        BitConverter.Int64BitsToDouble( 0x4330000000000000L ),
        BitConverter.ToDouble( BitConverter.GetBytes( 0xc330000000000000 ), 0 )
    };

        private static readonly double NEGATIVE_ZERO = 1.0 / double.NegativeInfinity;

        const ulong SignBit = 0x8000000000000000;

        public static double rint( double x )
        {
            int exp, sign;
            long ix;
            double w;

            if ( x == 0 )
            {
                return x;
            }

            ix = BitConverter.DoubleToInt64Bits( x );

            sign = ( int ) ( ( ix >> 63 ) & 1 );

            exp = ( ( int ) ( ix >> 52 ) & 0x7ff ) - 0x3ff;

            if ( exp < 0 )
            {
                if ( x < -0.5 )
                {
                    return -1.0;
                }
                else if ( x > 0.5 )
                {
                    return 1.0;
                }
                else if ( sign == 0 )
                {
                    return 0.0;
                }
                else
                {
                    return NEGATIVE_ZERO;
                }
            }
            else if ( exp < 53 )
            {
                // TODO: Check this
                long mask = ( ( long ) ( 0x000fffffffffffffL >> exp ) );

                if ( ( mask & ix ) == 0 )
                {
                    return x;
                }
            }
            else if ( exp == 1024 )
            {
                return x;
            }

            x = BitConverter.Int64BitsToDouble( ix );

            w = TWO52[ sign ] + x;

            return w - TWO52[ sign ];
        }

        public static double IEEEremainder( double x, double p )
        {
            int hx, hp;
            int sx, lx, lp;
            double p_half;

            hx = __HI( x );
            lx = __LO( x );
            hp = __HI( p );
            lp = __LO( p );

            sx = hx & unchecked(( int ) 0x80000000);

            hp &= 0x7fffffff;
            hx &= 0x7fffffff;

            if ( ( hp | lp ) == 0 )
            {
                return ( x * p ) / ( x * p );
            }

            if ( ( hx >= 0x7ff00000 ) || ( ( hp >= 0x7ff00000 ) && ( ( ( hp - 0x7ff00000 ) | lp ) != 0 ) ) )
            {
                return ( x * p ) / ( x * p );
            }

            if ( hp <= 0x7fdfffff )
            {
                x = x % ( p + p );
            }

            if ( ( ( hx - hp ) | ( lx - lp ) ) == 0 )
            {
                return zero * x;
            }

            x = abs( x );
            p = abs( p );

            if ( hp < 0x00200000 )
            {
                if ( x + x > p )
                {
                    x -= p;

                    if ( x + x >= p )
                    {
                        x -= p;
                    }
                }
            }
            else
            {
                p_half = 0.5 * p;

                if ( x > p_half )
                {
                    x -= p;

                    if ( x >= p_half )
                    {
                        x -= p;
                    }
                }
            }

            lx = __HI( x );
            lx ^= sx;

            return setHI( x, lx );
        }

        public static double Sqrt( double x )
        {
            long ix = BitConverter.DoubleToInt64Bits( x );

            if ( ( ix & 0x7ff0000000000000L ) == 0x7ff0000000000000L )
            {
                return x * x + x;
            }

            if ( x < 0.0 )
            {
                return double.NaN;
            }
            else if ( x == 0.0 )
            {
                return x;
            }

            long m = ( ix >> 52 );

            ix &= 0x000fffffffffffffL;

            if ( m != 0 )
            {
                ix |= 0x0010000000000000L;
            }

            m -= 1023L;

            if ( ( m & 1 ) != 0 )
            {
                ix += ix;
            }

            m >>= 1;
            m += 1023L;
            ix += ix;

            var q = 0L;
            var s = 0L;
            var r = 0x0020000000000000L;

            while ( r != 0 )
            {
                var t = s + r;

                if ( t <= ix )
                {
                    s = t + r;
                    ix -= t;
                    q += r;
                }

                ix += ix;
                r >>= 1;
            }

            if ( ix != 0 )
            {
                q += ( q & 1L );
            }

            ix = ( m << 52 ) | ( 0x000fffffffffffffL & ( q >> 1 ) );

            return BitConverter.Int64BitsToDouble( ix );
        }

        private static readonly double[] halF = { 0.5, -0.5 };

        private static readonly double twom1000 = BitConverter.Int64BitsToDouble( 0x0170000000000000L );
        private static readonly double o_threshold = BitConverter.Int64BitsToDouble( 0x40862e42fefa39efL );

        private static readonly double u_threshold = BitConverter.ToDouble( BitConverter.GetBytes( 0xc0874910d52d3051 ), 0 );

        private static readonly double[] ln2HI =
        {
        BitConverter.Int64BitsToDouble( 0x3fe62e42fee00000L ),
        BitConverter.ToDouble( BitConverter.GetBytes( 0xbfe62e42fee00000 ), 0 )
    };

        private static readonly double[] ln2LO =
        {
        BitConverter.Int64BitsToDouble( 0x3dea39ef35793c76L ),
        BitConverter.ToDouble( BitConverter.GetBytes( 0xbdea39ef35793c76 ), 0 )
    };

        private static readonly double invln2 = BitConverter.Int64BitsToDouble( 0x3ff71547652b82feL );

        private static readonly double P1 = BitConverter.Int64BitsToDouble( 0x3fc555555555553eL );
        private static readonly double P2 = BitConverter.ToDouble( BitConverter.GetBytes( 0xbf66c16c16bebd93 ), 0 );
        private static readonly double P3 = BitConverter.Int64BitsToDouble( 0x3f11566aaf25de2cL );
        private static readonly double P4 = BitConverter.ToDouble( BitConverter.GetBytes( 0xbebbbd41c5d26bf1 ), 0 );
        private static readonly double P5 = BitConverter.Int64BitsToDouble( 0x3e66376972bea4d0L );

        public static double exp( double x )
        {
            double y, hi = 0, lo = 0, c, t;
            int k = 0, xsb;
            int hx;

            hx = __HI( x );
            xsb = ( ( int ) ( ( uint ) hx >> 31 ) ) & 1;

            hx &= 0x7fffffff;

            if ( hx >= 0x40862E42 )
            {
                if ( hx >= 0x7ff00000 )
                {
                    if ( ( ( hx & 0xfffff ) | __LO( x ) ) != 0 )
                    {
                        return x + x;
                    }
                    else
                    {
                        return ( ( xsb == 0 ) ? x : 0.0 );
                    }
                }

                if ( x > o_threshold )
                {
                    return huge * huge;
                }

                if ( x < u_threshold )
                {
                    return twom1000 * twom1000;
                }
            }

            if ( hx > 0x3fd62e42 )
            {
                if ( hx < 0x3FF0A2B2 )
                {
                    hi = x - ln2HI[ xsb ];
                    lo = ln2LO[ xsb ];
                    k = 1 - xsb - xsb;
                }
                else
                {
                    k = ( int ) ( invln2 * x + halF[ xsb ] );
                    t = k;
                    hi = x - t * ln2HI[ 0 ];
                    lo = t * ln2LO[ 0 ];
                }

                x = hi - lo;
            }
            else if ( hx < 0x3e300000 )
            {
                if ( huge + x > one )
                {
                    return one + x;
                }
            }
            else
            {
                k = 0;
            }

            t = x * x;
            c = x - t * ( P1 + t * ( P2 + t * ( P3 + t * ( P4 + t * P5 ) ) ) );

            if ( k == 0 )
            {
                return one - ( ( x * c ) / ( c - 2.0 ) - x );
            }
            else
            {
                y = one - ( ( lo - ( x * c ) / ( 2.0 - c ) ) - hi );
            }

            long iy = BitConverter.DoubleToInt64Bits( y );

            if ( k >= -1021 )
            {
                iy += ( ( long ) k << 52 );
            }
            else
            {
                iy += ( ( k + 1000L ) << 52 );
            }

            return BitConverter.Int64BitsToDouble( iy );
        }

        private static readonly double ln2_hi = BitConverter.Int64BitsToDouble( 0x3fe62e42fee00000L );
        private static readonly double ln2_lo = BitConverter.Int64BitsToDouble( 0x3dea39ef35793c76L );
        private static readonly double Lg1 = BitConverter.Int64BitsToDouble( 0x3fe5555555555593L );
        private static readonly double Lg2 = BitConverter.Int64BitsToDouble( 0x3fd999999997fa04L );
        private static readonly double Lg3 = BitConverter.Int64BitsToDouble( 0x3fd2492494229359L );
        private static readonly double Lg4 = BitConverter.Int64BitsToDouble( 0x3fcc71c51d8e78afL );
        private static readonly double Lg5 = BitConverter.Int64BitsToDouble( 0x3fc7466496cb03deL );
        private static readonly double Lg6 = BitConverter.Int64BitsToDouble( 0x3fc39a09d078c69fL );
        private static readonly double Lg7 = BitConverter.Int64BitsToDouble( 0x3fc2f112df3e5244L );

        public static double log( double x )
        {
            double hfsq, f, s, z, R, w, t1, t2, dk;
            int k, hx, i, j;
            int lx;
            hx = __HI( x );
            lx = __LO( x );
            k = 0;
            if ( hx < 0x00100000 )
            {
                if ( ( ( hx & 0x7fffffff ) | lx ) == 0 )
                {
                    return -two54 / zero;
                }
                if ( hx < 0 )
                {
                    return ( x - x ) / zero;
                }
                k -= 54;
                x *= two54;
                hx = __HI( x );
            }
            if ( hx >= 0x7ff00000 )
            {
                return x + x;
            }
            k += ( hx >> 20 ) - 1023;
            hx &= 0x000fffff;
            i = ( hx + 0x95f64 ) & 0x100000;
            x = setHI( x, hx | ( i ^ 0x3ff00000 ) );
            k += ( i >> 20 );
            f = x - 1.0;
            if ( ( 0x000fffff & ( 2 + hx ) ) < 3 )
            {
                if ( f == zero )
                {
                    if ( k == 0 )
                    {
                        return zero;
                    }
                    else
                    {
                        dk = ( double ) k;
                    }
                    return dk * ln2_hi + dk * ln2_lo;
                }
                R = f * f * ( 0.5 - 0.33333333333333333 * f );
                if ( k == 0 )
                {
                    return f - R;
                }
                else
                {
                    dk = ( double ) k;
                    return dk * ln2_hi - ( ( R - dk * ln2_lo ) - f );
                }
            }
            s = f / ( 2.0 + f );
            dk = ( double ) k;
            z = s * s;
            i = hx - 0x6147a;
            w = z * z;
            j = 0x6b851 - hx;
            t1 = w * ( Lg2 + w * ( Lg4 + w * Lg6 ) );
            t2 = z * ( Lg1 + w * ( Lg3 + w * ( Lg5 + w * Lg7 ) ) );
            i |= j;
            R = t2 + t1;
            if ( i > 0 )
            {
                hfsq = 0.5 * f * f;
                if ( k == 0 )
                {
                    return f - ( hfsq - s * ( hfsq + R ) );
                }
                else
                {
                    return dk * ln2_hi - ( ( hfsq - ( s * ( hfsq + R ) + dk * ln2_lo ) ) - f );
                }
            }
            else
            {
                if ( k == 0 )
                {
                    return f - s * ( f - R );
                }
                else
                {
                    return dk * ln2_hi - ( ( s * ( f - R ) - dk * ln2_lo ) - f );
                }
            }
        }
        public static double sin( double x )
        {
            double[] y = new double[ 2 ];
            double z = 0.0;
            int n;
            int ix = __HI( x );
            ix &= 0x7fffffff;
            if ( ix <= 0x3fe921fb )
            {
                return __kernel_sin( x, z, 0 );
            }
            else if ( ix >= 0x7ff00000 )
            {
                return x - x;
            }
            else
            {
                n = __ieee754_rem_pio2( x, y );
                switch ( n & 3 )
                {
                    case 0:
                        return __kernel_sin( y[ 0 ], y[ 1 ], 1 );
                    case 1:
                        return __kernel_cos( y[ 0 ], y[ 1 ] );
                    case 2:
                        return -__kernel_sin( y[ 0 ], y[ 1 ], 1 );
                    default:
                        return -__kernel_cos( y[ 0 ], y[ 1 ] );
                }
            }
        }

        private static double S1 = -1.66666666666666324348e-01;
        private static double S2 = 8.33333333332248946124e-03;
        private static double S3 = -1.98412698298579493134e-04;
        private static double S4 = 2.75573137070700676789e-06;
        private static double S5 = -2.50507602534068634195e-08;
        private static double S6 = 1.58969099521155010221e-10;

        internal static double __kernel_sin( double x, double y, int iy )
        {
            double z, r, v;
            int ix;
            ix = __HI( x ) & 0x7fffffff;
            if ( ix < 0x3e400000 )
            {
                if ( ( int ) x == 0 )
                {
                    return x;
                }
            }
            z = x * x;
            v = z * x;
            r = S2 + z * ( S3 + z * ( S4 + z * ( S5 + z * S6 ) ) );
            if ( iy == 0 )
            {
                return x + v * ( S1 + z * r );
            }
            else
            {
                return x - ( ( z * ( half * y - v * r ) - y ) - v * S1 );
            }
        }

        public static double cos( double x )
        {
            double z = 0.0;
            double[] y = new double[ 2 ];
            int n, ix;
            ix = __HI( x );
            ix &= 0x7fffffff;
            if ( ix <= 0x3fe921fb )
            {
                return __kernel_cos( x, z );
            }
            else if ( ix >= 0x7ff00000 )
            {
                return x - x;
            }
            else
            {
                n = __ieee754_rem_pio2( x, y );
                switch ( n & 3 )
                {
                    case 0:
                        return __kernel_cos( y[ 0 ], y[ 1 ] );
                    case 1:
                        return -__kernel_sin( y[ 0 ], y[ 1 ], 1 );
                    case 2:
                        return -__kernel_cos( y[ 0 ], y[ 1 ] );
                    default:
                        return __kernel_sin( y[ 0 ], y[ 1 ], 1 );
                }
            }
        }

        private static readonly double one = BitConverter.Int64BitsToDouble( 0x3ff0000000000000L );
        private static readonly double C1 = BitConverter.Int64BitsToDouble( 0x3fa555555555554cL );
        private static readonly double C2 = BitConverter.ToDouble( BitConverter.GetBytes( 0xbf56c16c16c15177 ), 0 );
        private static readonly double C3 = BitConverter.Int64BitsToDouble( 0x3efa01a019cb1590L );
        private static readonly double C4 = BitConverter.ToDouble( BitConverter.GetBytes( 0xbe927e4f809c52ad ), 0 );
        private static readonly double C5 = BitConverter.Int64BitsToDouble( 0x3e21ee9ebdb4b1c4L );
        private static readonly double C6 = BitConverter.ToDouble( BitConverter.GetBytes( 0xbda8fae9be8838d4 ), 0 );

        private static double __kernel_cos( double x, double y )
        {
            double a, hz, z, r, qx = zero;
            int ix;
            ix = __HI( x ) & 0x7fffffff;
            if ( ix < 0x3e400000 )
            {
                if ( ( ( int ) x ) == 0 )
                {
                    return one;
                }
            }
            z = x * x;
            r = z * ( C1 + z * ( C2 + z * ( C3 + z * ( C4 + z * ( C5 + z * C6 ) ) ) ) );
            if ( ix < 0x3FD33333 )
            {
                return one - ( 0.5 * z - ( z * r - x * y ) );
            }
            else
            {
                if ( ix > 0x3fe90000 )
                {
                    qx = 0.28125;
                }
                else
                {
                    qx = set( ix - 0x00200000, 0 );
                }
                hz = 0.5 * z - qx;
                a = one - qx;
                return a - ( hz - ( z * r - x * y ) );
            }
        }
        public static double tan( double x )
        {
            double z = zero;
            int n;
            int ix = __HI( x );
            ix &= 0x7fffffff;
            if ( ix <= 0x3fe921fb )
            {
                return __kernel_tan( x, z, 1 );
            }
            else if ( ix >= 0x7ff00000 )
            {
                return x - x;
            }
            else
            {
                double[] y = new double[ 2 ];
                n = __ieee754_rem_pio2( x, y );
                return __kernel_tan( y[ 0 ], y[ 1 ], 1 - ( ( n & 1 ) << 1 ) );
            }
        }

        private static readonly double pio4 = BitConverter.Int64BitsToDouble( 0x3fe921fb54442d18L );
        private static readonly double pio4lo = BitConverter.Int64BitsToDouble( 0x3c81a62633145c07L );

        private static readonly double[] T =
        {
        BitConverter.Int64BitsToDouble(0x3fd5555555555563L),
        BitConverter.Int64BitsToDouble(0x3fc111111110fe7aL),
        BitConverter.Int64BitsToDouble(0x3faba1ba1bb341feL),
        BitConverter.Int64BitsToDouble(0x3f9664f48406d637L),
        BitConverter.Int64BitsToDouble(0x3f8226e3e96e8493L),
        BitConverter.Int64BitsToDouble(0x3f6d6d22c9560328L),
        BitConverter.Int64BitsToDouble(0x3f57dbc8fee08315L),
        BitConverter.Int64BitsToDouble(0x3f4344d8f2f26501L),
        BitConverter.Int64BitsToDouble(0x3f3026f71a8d1068L),
        BitConverter.Int64BitsToDouble(0x3f147e88a03792a6L),
        BitConverter.Int64BitsToDouble(0x3f12b80f32f0a7e9L),
        BitConverter.ToDouble( BitConverter.GetBytes( 0xbef375cbdb605373 ), 0 ),
        BitConverter.Int64BitsToDouble(0x3efb2a7074bf7ad4L)
    };

        private static double __kernel_tan( double x, double y, int iy )
        {
            double z, r, v, w, s;
            int ix, hx;
            hx = __HI( x );
            ix = hx & 0x7fffffff;
            if ( ix < 0x3e300000 )
            {
                if ( ( int ) x == 0 )
                {
                    if ( ( ( ix | __LO( x ) ) | ( iy + 1 ) ) == 0 )
                    {
                        return one / abs( x );
                    }
                    else
                    {
                        return ( iy == 1 ) ? x : -one / x;
                    }
                }
            }

            if ( ix >= 0x3FE59428 )
            {
                if ( hx < 0 )
                {
                    x = -x;
                    y = -y;
                }
                z = pio4 - x;
                w = pio4lo - y;
                x = z + w;
                y = 0.0;
            }
            z = x * x;
            w = z * z;
            r = T[ 1 ] + w * ( T[ 3 ] + w * ( T[ 5 ] + w * ( T[ 7 ] + w * ( T[ 9 ] + w * T[ 11 ] ) ) ) );
            v = z * ( T[ 2 ] + w * ( T[ 4 ] + w * ( T[ 6 ] + w * ( T[ 8 ] + w * ( T[ 10 ] + w * T[ 12 ] ) ) ) ) );
            s = z * x;
            r = y + z * ( s * ( r + v ) + y );
            r += T[ 0 ] * s;
            w = x + r;
            if ( ix >= 0x3FE59428 )
            {
                v = ( double ) iy;
                return ( double ) ( 1 - ( ( hx >> 30 ) & 2 ) ) * ( v - 2.0 * ( x - ( w * w / ( w + v ) - r ) ) );
            }
            if ( iy == 1 )
            {
                return w;
            }
            else
            {
                double a, t;
                z = w;
                z = setLO( z, 0 );
                v = r - ( z - x );
                t = a = -1.0 / w;
                t = setLO( t, 0 );
                s = 1.0 + t * z;
                return t + a * ( s + t * v );
            }
        }

        private static readonly double pio2_hi = BitConverter.Int64BitsToDouble( 0x3FF921FB54442D18L );
        private static readonly double pio2_lo = BitConverter.Int64BitsToDouble( 0x3C91A62633145C07L );
        private static readonly double pio4_hi = BitConverter.Int64BitsToDouble( 0x3FE921FB54442D18L );
        private static readonly double pS0 = BitConverter.Int64BitsToDouble( 0x3fc5555555555555L );
        private static readonly double pS1 = BitConverter.ToDouble( BitConverter.GetBytes( 0xbfd4d61203eb6f7d ), 0 );
        private static readonly double pS2 = BitConverter.Int64BitsToDouble( 0x3fc9c1550e884455L );
        private static readonly double pS3 = BitConverter.ToDouble( BitConverter.GetBytes( 0xbfa48228b5688f3b ), 0 );
        private static readonly double pS4 = BitConverter.Int64BitsToDouble( 0x3f49efe07501b288L );
        private static readonly double pS5 = BitConverter.Int64BitsToDouble( 0x3f023de10dfdf709L );
        private static readonly double qS1 = BitConverter.ToDouble( BitConverter.GetBytes( 0xc0033a271c8a2d4b ), 0 );
        private static readonly double qS2 = BitConverter.Int64BitsToDouble( 0x40002ae59c598ac8L );
        private static readonly double qS3 = BitConverter.ToDouble( BitConverter.GetBytes( 0xbfe6066c1b8d0159 ), 0 );
        private static readonly double qS4 = BitConverter.Int64BitsToDouble( 0x3fb3b8c5b12e9282L );

        public static double asin( double x )
        {
            double t = zero, w, p, q, c, r, s;
            int hx, ix;
            hx = __HI( x );
            ix = hx & 0x7fffffff;
            if ( ix >= 0x3ff00000 )
            {
                if ( ( ( ix - 0x3ff00000 ) | __LO( x ) ) == 0 )
                {
                    return x * pio2_hi + x * pio2_lo;
                }
                return ( x - x ) / ( x - x );
            }
            else if ( ix < 0x3fe00000 )
            {
                if ( ix < 0x3e400000 )
                {
                    if ( huge + x > one )
                    {
                        return x;
                    }
                }
                else
                {
                    t = x * x;
                }
                p = t * ( pS0 + t * ( pS1 + t * ( pS2 + t * ( pS3 + t * ( pS4 + t * pS5 ) ) ) ) );
                q = one + t * ( qS1 + t * ( qS2 + t * ( qS3 + t * qS4 ) ) );
                w = p / q;
                return x + x * w;
            }
            w = one - abs( x );
            t = w * 0.5;
            p = t * ( pS0 + t * ( pS1 + t * ( pS2 + t * ( pS3 + t * ( pS4 + t * pS5 ) ) ) ) );
            q = one + t * ( qS1 + t * ( qS2 + t * ( qS3 + t * qS4 ) ) );
            s = Sqrt( t );
            if ( ix >= 0x3FEF3333 )
            {
                w = p / q;
                t = pio2_hi - ( 2.0 * ( s + s * w ) - pio2_lo );
            }
            else
            {
                w = s;
                w = setLO( w, 0 );
                c = ( t - w * w ) / ( s + w );
                r = p / q;
                p = 2.0 * s * r - ( pio2_lo - 2.0 * c );
                q = pio4_hi - 2.0 * w;
                t = pio4_hi - ( p - q );
            }
            return ( ( hx > 0 ) ? t : -t );
        }

        public static double acos( double x )
        {
            double z, p, q, r, w, s, c, df;
            int hx, ix;
            hx = __HI( x );
            ix = hx & 0x7fffffff;
            if ( ix >= 0x3ff00000 )
            {
                if ( ( ( ix - 0x3ff00000 ) | __LO( x ) ) == 0 )
                {
                    if ( hx > 0 )
                    {
                        return 0.0;
                    }
                    else
                    {
                        return PI + 2.0 * pio2_lo;
                    }
                }
                return ( x - x ) / ( x - x );
            }
            if ( ix < 0x3fe00000 )
            {
                if ( ix <= 0x3c600000 )
                {
                    return pio2_hi + pio2_lo;
                }
                z = x * x;
                p = z * ( pS0 + z * ( pS1 + z * ( pS2 + z * ( pS3 + z * ( pS4 + z * pS5 ) ) ) ) );
                q = one + z * ( qS1 + z * ( qS2 + z * ( qS3 + z * qS4 ) ) );
                r = p / q;
                return pio2_hi - ( x - ( pio2_lo - x * r ) );
            }
            else if ( hx < 0 )
            {
                z = ( one + x ) * 0.5;
                p = z * ( pS0 + z * ( pS1 + z * ( pS2 + z * ( pS3 + z * ( pS4 + z * pS5 ) ) ) ) );
                q = one + z * ( qS1 + z * ( qS2 + z * ( qS3 + z * qS4 ) ) );
                s = Sqrt( z );
                r = p / q;
                w = r * s - pio2_lo;
                return PI - 2.0 * ( s + w );
            }
            else
            {
                z = ( one - x ) * 0.5;
                s = Sqrt( z );
                df = s;
                df = setLO( df, 0 );
                c = ( z - df * df ) / ( s + df );
                p = z * ( pS0 + z * ( pS1 + z * ( pS2 + z * ( pS3 + z * ( pS4 + z * pS5 ) ) ) ) );
                q = one + z * ( qS1 + z * ( qS2 + z * ( qS3 + z * qS4 ) ) );
                r = p / q;
                w = r * s + c;
                return 2.0 * ( df + w );
            }
        }

        private static readonly double[] atanhi =
        {
        BitConverter.Int64BitsToDouble(0x3fddac670561bb4fL),
        BitConverter.Int64BitsToDouble(0x3fe921fb54442d18L),
        BitConverter.Int64BitsToDouble(0x3fef730bd281f69bL),
        BitConverter.Int64BitsToDouble(0x3ff921fb54442d18L)
    };

        private static readonly double[] atanlo =
        {
        BitConverter.Int64BitsToDouble(0x3c7a2b7f222f65e2L),
        BitConverter.Int64BitsToDouble(0x3c81a62633145c07L),
        BitConverter.Int64BitsToDouble(0x3c7007887af0cbbdL),
        BitConverter.Int64BitsToDouble(0x3c91a62633145c07L)
    };

        private static readonly double[] aT =
        {
        BitConverter.Int64BitsToDouble(0x3fd555555555550dL),
        BitConverter.ToDouble( BitConverter.GetBytes(0xbfc999999998ebc4 ), 0),
        BitConverter.Int64BitsToDouble(0x3fc24924920083ffL),
        BitConverter.ToDouble( BitConverter.GetBytes(0xbfbc71c6fe231671 ), 0 ),
        BitConverter.Int64BitsToDouble(0x3fb745cdc54c206eL),
        BitConverter.ToDouble( BitConverter.GetBytes(0xbfb3b0f2af749a6d ), 0 ),
        BitConverter.Int64BitsToDouble(0x3fb10d66a0d03d51L),
        BitConverter.ToDouble( BitConverter.GetBytes(0xbfadde2d52defd9a ), 0 ),
        BitConverter.Int64BitsToDouble(0x3fa97b4b24760debL),
        BitConverter.ToDouble( BitConverter.GetBytes(0xbfa2b4442c6a6c2f ), 0 ),
        BitConverter.ToDouble( BitConverter.GetBytes(0x3f90ad3ae322da11 ), 0 )
    };

        public static double atan( double x )
        {
            double w, s1, s2, z;
            int ix, hx, id;
            hx = __HI( x );
            ix = hx & 0x7fffffff;
            if ( ix >= 0x44100000 )
            {
                if ( ix > 0x7ff00000 || ( ix == 0x7ff00000 && ( __LO( x ) != 0 ) ) )
                {
                    return x + x;
                }
                if ( hx > 0 )
                {
                    return atanhi[ 3 ] + atanlo[ 3 ];
                }
                else
                {
                    return -atanhi[ 3 ] - atanlo[ 3 ];
                }
            }
            if ( ix < 0x3fdc0000 )
            {
                if ( ix < 0x3e200000 )
                {
                    if ( huge + x > one )
                    {
                        return x;
                    }
                }
                id = -1;
            }
            else
            {
                x = abs( x );
                if ( ix < 0x3ff30000 )
                {
                    if ( ix < 0x3fe60000 )
                    {
                        id = 0;
                        x = ( 2.0 * x - one ) / ( 2.0 + x );
                    }
                    else
                    {
                        id = 1;
                        x = ( x - one ) / ( x + one );
                    }
                }
                else
                {
                    if ( ix < 0x40038000 )
                    {
                        id = 2;
                        x = ( x - 1.5 ) / ( one + 1.5 * x );
                    }
                    else
                    {
                        id = 3;
                        x = -1.0 / x;
                    }
                }
            }
            z = x * x;
            w = z * z;
            s1 = z * ( aT[ 0 ] + w * ( aT[ 2 ] + w * ( aT[ 4 ] + w * ( aT[ 6 ] + w * ( aT[ 8 ] + w * aT[ 10 ] ) ) ) ) );
            s2 = w * ( aT[ 1 ] + w * ( aT[ 3 ] + w * ( aT[ 5 ] + w * ( aT[ 7 ] + w * aT[ 9 ] ) ) ) );
            if ( id < 0 )
            {
                return x - x * ( s1 + s2 );
            }
            else
            {
                z = atanhi[ id ] - ( ( x * ( s1 + s2 ) - atanlo[ id ] ) - x );
                return ( hx < 0 ) ? -z : z;
            }
        }

        private static readonly double pi_o_4 = BitConverter.Int64BitsToDouble( 0x3fe921fb54442d18L );
        private static readonly double pi_o_2 = BitConverter.Int64BitsToDouble( 0x3ff921fb54442d18L );
        private static readonly double pi_lo = BitConverter.Int64BitsToDouble( 0x3ca1a62633145c07L );

        public static double atan2( double y, double x )
        {
            double z;
            int k, m, hx, hy, ix, iy;
            int lx, ly;
            hx = __HI( x );
            ix = hx & 0x7fffffff;
            lx = __LO( x );
            hy = __HI( y );
            iy = hy & 0x7fffffff;
            ly = __LO( y );
            if ( ( ( ix | ( ( int ) ( ( uint ) ( lx | -lx ) >> 31 ) ) ) > 0x7ff00000 ) || ( ( iy | ( ( int ) ( ( uint ) ( ly | -ly ) >> 31 ) ) ) > 0x7ff00000 ) )
            {
                return x + y;
            }
            if ( ( hx - 0x3ff00000 | lx ) == 0 )
            {
                return atan( y );
            }
            m = ( ( hy >> 31 ) & 1 ) | ( ( hx >> 30 ) & 2 );
            if ( ( iy | ly ) == 0 )
            {
                switch ( m )
                {
                    case 0:
                    case 1:
                        return y;
                    case 2:
                        return PI + tiny;
                    case 3:
                        return -PI - tiny;
                }
            }
            if ( ( ix | lx ) == 0 )
            {
                return ( ( hy < 0 ) ? -pi_o_2 - tiny : pi_o_2 + tiny );
            }
            if ( ix == 0x7ff00000 )
            {
                if ( iy == 0x7ff00000 )
                {
                    switch ( m )
                    {
                        case 0:
                            return pi_o_4 + tiny;
                        case 1:
                            return -pi_o_4 - tiny;
                        case 2:
                            return 3.0 * pi_o_4 + tiny;
                        case 3:
                            return -3.0 * pi_o_4 - tiny;
                    }
                }
                else
                {
                    switch ( m )
                    {
                        case 0:
                            return zero;
                        case 1:
                            return -zero;
                        case 2:
                            return PI + tiny;
                        case 3:
                            return -PI - tiny;
                    }
                }
            }
            if ( iy == 0x7ff00000 )
            {
                return ( hy < 0 ) ? -pi_o_2 - tiny : pi_o_2 + tiny;
            }
            k = ( iy - ix ) >> 20;
            if ( k > 60 )
            {
                z = pi_o_2 + 0.5 * pi_lo;
            }
            else if ( hx < 0 && k < -60 )
            {
                z = 0.0;
            }
            else
            {
                z = atan( abs( y / x ) );
            }
            switch ( m )
            {
                case 0:
                    return z;
                case 1:
                    // TODO: Check this.
                    return setHI( z, __HI( z ) ^ BitConverter.ToInt32( BitConverter.GetBytes( 0x80000000 ), 0 ) );
                case 2:
                    return PI - ( z - pi_lo );
                default:
                    return ( z - pi_lo ) - PI;
            }
        }

        private static readonly int[] two_over_pi = { 0xa2f983, 0x6e4e44, 0x1529fc, 0x2757d1, 0xf534dd, 0xc0db62, 0x95993c, 0x439041, 0xfe5163, 0xabdebb, 0xc561b7, 0x246e3a, 0x424dd2, 0xe00649, 0x2eea09, 0xd1921c, 0xfe1deb, 0x1cb129, 0xa73ee8, 0x8235f5, 0x2ebb44, 0x84e99c, 0x7026b4, 0x5f7e41, 0x3991d6, 0x398353, 0x39f49c, 0x845f8b, 0xbdf928, 0x3b1ff8, 0x97ffde, 0x05980f, 0xef2f11, 0x8b5a0a, 0x6d1f6d, 0x367ecf, 0x27cb09, 0xb74f46, 0x3f669e, 0x5fea2d, 0x7527ba, 0xc7ebe5, 0xf17b3d, 0x0739f7, 0x8a5292, 0xea6bfb, 0x5fb11f, 0x8d5d08, 0x560330, 0x46fc7b, 0x6babf0, 0xcfbc20, 0x9af436, 0x1da9e3, 0x91615e, 0xe61b08, 0x659985, 0x5f14a0, 0x68408d, 0xffd880, 0x4d7327, 0x310606, 0x1556ca, 0x73a8c9, 0x60e27b, 0xc08c6b };
        private static readonly int[] npio2_hw = { 0x3ff921fb, 0x400921fb, 0x4012d97c, 0x401921fb, 0x401f6a7a, 0x4022d97c, 0x4025fdbb, 0x402921fb, 0x402c463a, 0x402f6a7a, 0x4031475c, 0x4032d97c, 0x40346b9c, 0x4035fdbb, 0x40378fdb, 0x403921fb, 0x403ab41b, 0x403c463a, 0x403dd85a, 0x403f6a7a, 0x40407e4c, 0x4041475c, 0x4042106c, 0x4042d97c, 0x4043a28c, 0x40446b9c, 0x404534ac, 0x4045fdbb, 0x4046c6cb, 0x40478fdb, 0x404858eb, 0x404921fb };

        private const double zero = 0.00000000000000000000e+00;

        private static readonly double half = BitConverter.Int64BitsToDouble( 0x3fe0000000000000L );
        private static readonly double two24 = BitConverter.Int64BitsToDouble( 0x4170000000000000L );
        private static readonly double invpio2 = BitConverter.Int64BitsToDouble( 0x3fe45f306dc9c883L );
        private static readonly double pio2_1 = BitConverter.Int64BitsToDouble( 0x3ff921fb54400000L );
        private static readonly double pio2_1t = BitConverter.Int64BitsToDouble( 0x3dd0b4611a626331L );
        private static readonly double pio2_2 = BitConverter.Int64BitsToDouble( 0x3dd0b4611a600000L );
        private static readonly double pio2_2t = BitConverter.Int64BitsToDouble( 0x3ba3198a2e037073L );
        private static readonly double pio2_3 = BitConverter.Int64BitsToDouble( 0x3ba3198a2e000000L );
        private static readonly double pio2_3t = BitConverter.Int64BitsToDouble( 0x397b839a252049c1L );

        private static int __ieee754_rem_pio2( double x, double[] y )
        {
            double z = zero, w, t, r, fn;
            double[] tx = new double[ 3 ];
            int i, j, nx, n, ix, hx;
            hx = __HI( x );
            ix = hx & 0x7fffffff;
            if ( ix <= 0x3fe921fb )
            {
                y[ 0 ] = x;
                y[ 1 ] = 0;
                return 0;
            }
            if ( ix < 0x4002d97c )
            {
                if ( hx > 0 )
                {
                    z = x - pio2_1;
                    if ( ix != 0x3ff921fb )
                    {
                        y[ 0 ] = z - pio2_1t;
                        y[ 1 ] = ( z - y[ 0 ] ) - pio2_1t;
                    }
                    else
                    {
                        z -= pio2_2;
                        y[ 0 ] = z - pio2_2t;
                        y[ 1 ] = ( z - y[ 0 ] ) - pio2_2t;
                    }
                    return 1;
                }
                else
                {
                    z = x + pio2_1;
                    if ( ix != 0x3ff921fb )
                    {
                        y[ 0 ] = z + pio2_1t;
                        y[ 1 ] = ( z - y[ 0 ] ) + pio2_1t;
                    }
                    else
                    {
                        z += pio2_2;
                        y[ 0 ] = z + pio2_2t;
                        y[ 1 ] = ( z - y[ 0 ] ) + pio2_2t;
                    }
                    return -1;
                }
            }
            if ( ix <= 0x413921fb )
            {
                t = abs( x );
                n = ( int ) ( t * invpio2 + half );
                fn = ( double ) n;
                r = t - fn * pio2_1;
                w = fn * pio2_1t;
                if ( n < 32 && ix != npio2_hw[ n - 1 ] )
                {
                    y[ 0 ] = r - w;
                }
                else
                {
                    j = ix >> 20;
                    y[ 0 ] = r - w;
                    i = j - ( ( ( __HI( y[ 0 ] ) ) >> 20 ) & 0x7ff );
                    if ( i > 16 )
                    {
                        t = r;
                        w = fn * pio2_2;
                        r = t - w;
                        w = fn * pio2_2t - ( ( t - r ) - w );
                        y[ 0 ] = r - w;
                        i = j - ( ( ( __HI( y[ 0 ] ) ) >> 20 ) & 0x7ff );
                        if ( i > 49 )
                        {
                            t = r;
                            w = fn * pio2_3;
                            r = t - w;
                            w = fn * pio2_3t - ( ( t - r ) - w );
                            y[ 0 ] = r - w;
                        }
                    }
                }
                y[ 1 ] = ( r - y[ 0 ] ) - w;
                if ( hx < 0 )
                {
                    y[ 0 ] = -y[ 0 ];
                    y[ 1 ] = -y[ 1 ];
                    return -n;
                }
                else
                {
                    return n;
                }
            }
            if ( ix >= 0x7ff00000 )
            {
                y[ 0 ] = y[ 1 ] = x - x;
                return 0;
            }
            long lx = BitConverter.DoubleToInt64Bits( x );
            long exp = ( 0x7ff0000000000000L & lx ) >> 52;
            exp -= 1046;
            lx -= ( exp << 52 );
            lx &= 0x7fffffffffffffffL;
            z = BitConverter.Int64BitsToDouble( lx );
            for ( i = 0; i < 2; i++ )
            {
                tx[ i ] = ( double ) ( ( int ) ( z ) );
                z = ( z - tx[ i ] ) * two24;
            }
            tx[ 2 ] = z;
            nx = 3;
            while ( tx[ nx - 1 ] == zero )
            {
                nx--;
            }
            n = __kernel_rem_pio2( tx, y, ( int ) exp, nx );
            if ( hx < 0 )
            {
                y[ 0 ] = -y[ 0 ];
                y[ 1 ] = -y[ 1 ];
                return -n;
            }
            return n;
        }

        private static readonly double[] PIo2 =
        {
        BitConverter.Int64BitsToDouble(0x3ff921fb40000000L),
        BitConverter.Int64BitsToDouble(0x3e74442d00000000L),
        BitConverter.Int64BitsToDouble(0x3cf8469880000000L),
        BitConverter.Int64BitsToDouble(0x3b78cc5160000000L),
        BitConverter.Int64BitsToDouble(0x39f01b8380000000L),
        BitConverter.Int64BitsToDouble(0x387a252040000000L),
        BitConverter.Int64BitsToDouble(0x36e3822280000000L),
        BitConverter.Int64BitsToDouble(0x3569f31d00000000L)
    };

        private static readonly double twon24 = BitConverter.Int64BitsToDouble( 0x3E70000000000000L );

        private static int __kernel_rem_pio2( double[] x, double[] y, int e0, int nx )
        {
            int jz, jx, jv, jp, jk, carry, n, i, j, k, m, q0, ih;
            double z, fw;
            double[] f = new double[ 20 ];
            double[] q = new double[ 20 ];
            double[] fq = new double[ 20 ];
            int[] iq = new int[ 20 ];
            jk = 4;
            jp = jk;
            jx = nx - 1;
            jv = ( e0 - 3 ) / 24;
            if ( jv < 0 )
            {
                jv = 0;
            }
            q0 = e0 - 24 * ( jv + 1 );
            j = jv - jx;
            m = jx + jk;
            for ( i = 0; i <= m; i++, j++ )
            {
                f[ i ] = ( ( j < 0 ) ? zero : ( double ) two_over_pi[ j ] );
            }
            for ( i = 0; i <= jk; i++ )
            {
                for ( j = 0, fw = 0.0; j <= jx; j++ )
                {
                    fw += x[ j ] * f[ jx + i - j ];
                }
                q[ i ] = fw;
            }
            jz = jk;
            while ( true )
            {
                for ( i = 0, j = jz, z = q[ jz ]; j > 0; i++, j-- )
                {
                    fw = ( double ) ( ( int ) ( twon24 * z ) );
                    iq[ i ] = ( int ) ( z - two24 * fw );
                    z = q[ j - 1 ] + fw;
                }
                z = scalbn( z, q0 );
                z -= 8.0 * floor( z * 0.125 );
                n = ( int ) z;
                z -= ( double ) n;
                ih = 0;
                if ( q0 > 0 )
                {
                    i = ( iq[ jz - 1 ] >> ( 24 - q0 ) );
                    n += i;
                    iq[ jz - 1 ] -= i << ( 24 - q0 );
                    ih = iq[ jz - 1 ] >> ( 23 - q0 );
                }
                else if ( q0 == 0 )
                {
                    ih = iq[ jz - 1 ] >> 23;
                }
                else if ( z >= 0.5 )
                {
                    ih = 2;
                }
                if ( ih > 0 )
                {
                    n += 1;
                    carry = 0;
                    for ( i = 0; i < jz; i++ )
                    {
                        j = iq[ i ];
                        if ( carry == 0 )
                        {
                            if ( j != 0 )
                            {
                                carry = 1;
                                iq[ i ] = 0x1000000 - j;
                            }
                        }
                        else
                        {
                            iq[ i ] = 0xffffff - j;
                        }
                    }
                    if ( q0 > 0 )
                    {
                        switch ( q0 )
                        {
                            case 1:
                                iq[ jz - 1 ] &= 0x7fffff;
                                break;
                            case 2:
                                iq[ jz - 1 ] &= 0x3fffff;
                                break;
                        }
                    }
                    if ( ih == 2 )
                    {
                        z = one - z;
                        if ( carry != 0 )
                        {
                            z -= scalbn( one, q0 );
                        }
                    }
                }
                if ( z == zero )
                {
                    j = 0;
                    for ( i = jz - 1; i >= jk; i-- )
                    {
                        j |= iq[ i ];
                    }
                    if ( j == 0 )
                    {
                        for ( k = 1; iq[ jk - k ] == 0; k++ )
                        {
                            ;
                        }
                        for ( i = jz + 1; i <= jz + k; i++ )
                        {
                            f[ jx + i ] = ( double ) two_over_pi[ jv + i ];
                            for ( j = 0, fw = 0.0; j <= jx; j++ )
                            {
                                fw += x[ j ] * f[ jx + i - j ];
                            }
                            q[ i ] = fw;
                        }
                        jz += k;
                        continue;
                    }
                }
                break;
            }
            if ( z == 0.0 )
            {
                jz--;
                q0 -= 24;
                while ( iq[ jz ] == 0 )
                {
                    jz--;
                    q0 -= 24;
                }
            }
            else
            {
                z = scalbn( z, -q0 );
                if ( z >= two24 )
                {
                    fw = ( double ) ( ( int ) ( twon24 * z ) );
                    iq[ jz ] = ( int ) ( z - two24 * fw );
                    jz++;
                    q0 += 24;
                    iq[ jz ] = ( int ) fw;
                }
                else
                {
                    iq[ jz ] = ( int ) z;
                }
            }
            fw = scalbn( one, q0 );
            for ( i = jz; i >= 0; i-- )
            {
                q[ i ] = fw * ( double ) iq[ i ];
                fw *= twon24;
            }
            for ( i = jz; i >= 0; i-- )
            {
                for ( fw = 0.0, k = 0; k <= jp && k <= jz - i; k++ )
                {
                    fw += PIo2[ k ] * q[ i + k ];
                }
                fq[ jz - i ] = fw;
            }
            fw = 0.0;
            for ( i = jz; i >= 0; i-- )
            {
                fw += fq[ i ];
            }
            y[ 0 ] = ( ih == 0 ) ? fw : -fw;
            fw = fq[ 0 ] - fw;
            for ( i = 1; i <= jz; i++ )
            {
                fw += fq[ i ];
            }
            y[ 1 ] = ( ( ih == 0 ) ? fw : -fw );
            return n & 7;
        }

        private static readonly double[] bp = { 1.0, 1.5 };
        private static readonly double[] dp_h = { 0.0, BitConverter.Int64BitsToDouble( 0x3fe2b80340000000L ) };
        private static readonly double[] dp_l = { 0.0, BitConverter.Int64BitsToDouble( 0x3e4cfdeb43cfd006L ) };

        private static readonly double two53 = BitConverter.Int64BitsToDouble( 0x4340000000000000L );

        private static readonly double L1 = BitConverter.Int64BitsToDouble( 0x3fe3333333333303L );
        private static readonly double L2 = BitConverter.Int64BitsToDouble( 0x3fdb6db6db6fabffL );
        private static readonly double L3 = BitConverter.Int64BitsToDouble( 0x3fd55555518f264dL );
        private static readonly double L4 = BitConverter.Int64BitsToDouble( 0x3fd17460a91d4101L );
        private static readonly double L5 = BitConverter.Int64BitsToDouble( 0x3fcd864a93c9db65L );
        private static readonly double L6 = BitConverter.Int64BitsToDouble( 0x3fca7e284a454eefL );
        private static readonly double lg2 = BitConverter.Int64BitsToDouble( 0x3fe62e42fefa39efL );
        private static readonly double lg2_h = BitConverter.Int64BitsToDouble( 0x3fe62e4300000000L );

        private const double lg2_l = -1.90465429995776804525e-09;
        private const double ovt = 8.0085662595372944372e-17;

        private static readonly double cp = BitConverter.Int64BitsToDouble( 0x3feec709dc3a03fdL );
        private static readonly double cp_h = BitConverter.Int64BitsToDouble( 0x3feec709e0000000L );
        private static readonly double cp_l = BitConverter.ToDouble( BitConverter.GetBytes( 0xbe3e2fe0145b01f5 ), 0 );
        private static readonly double ivln2 = BitConverter.Int64BitsToDouble( 0x3ff71547652b82feL );
        private static readonly double ivln2_h = BitConverter.Int64BitsToDouble( 0x3ff7154760000000L );
        private static readonly double ivln2_l = BitConverter.Int64BitsToDouble( 0x3e54ae0bf85ddf44L );

        public static double pow( double x, double y )
        {
            double z, ax, z_h, z_l, p_h, p_l;
            double y1, t1, t2, r, s, t, u, v, w;
            int i, j, k, yisint, n;
            int hx, hy, ix, iy;
            int lx, ly;
            hx = __HI( x );
            lx = __LO( x );
            hy = __HI( y );
            ly = __LO( y );
            ix = hx & 0x7fffffff;
            iy = hy & 0x7fffffff;
            if ( ( iy | ly ) == 0 )
            {
                return one;
            }
            if ( ix > 0x7ff00000 || ( ( ix == 0x7ff00000 ) && ( lx != 0 ) ) || iy > 0x7ff00000 || ( ( iy == 0x7ff00000 ) && ( ly != 0 ) ) )
            {
                return x + y;
            }
            yisint = 0;
            if ( hx < 0 )
            {
                if ( iy >= 0x43400000 )
                {
                    yisint = 2;
                }
                else if ( iy >= 0x3ff00000 )
                {
                    k = ( iy >> 20 ) - 0x3ff;
                    if ( k > 20 )
                    {
                        j = ( int ) ( ( uint ) ly >> ( 52 - k ) );
                        if ( ( j << ( 52 - k ) ) == ly )
                        {
                            yisint = 2 - ( j & 1 );
                        }
                    }
                    else if ( ly == 0 )
                    {
                        j = iy >> ( 20 - k );
                        if ( ( j << ( 20 - k ) ) == iy )
                        {
                            yisint = 2 - ( j & 1 );
                        }
                    }
                }
            }
            if ( ly == 0 )
            {
                if ( iy == 0x7ff00000 )
                {
                    if ( ( ( ix - 0x3ff00000 ) | lx ) == 0 )
                    {
                        return y - y;
                    }
                    else if ( ix >= 0x3ff00000 )
                    {
                        return ( hy >= 0 ) ? y : zero;
                    }
                    else
                    {
                        return ( hy < 0 ) ? -y : zero;
                    }
                }
                if ( iy == 0x3ff00000 )
                {
                    if ( hy < 0 )
                    {
                        return one / x;
                    }
                    else
                    {
                        return x;
                    }
                }
                if ( hy == 0x40000000 )
                {
                    return x * x;
                }
                if ( hy == 0x3fe00000 )
                {
                    if ( hx >= 0 )
                    {
                        return Sqrt( x );
                    }
                }
            }
            ax = abs( x );
            if ( lx == 0 )
            {
                if ( ix == 0x7ff00000 || ix == 0 || ix == 0x3ff00000 )
                {
                    z = ax;
                    if ( hy < 0 )
                    {
                        z = one / z;
                    }
                    if ( hx < 0 )
                    {
                        if ( ( ( ix - 0x3ff00000 ) | yisint ) == 0 )
                        {
                            z = ( z - z ) / ( z - z );
                        }
                        else if ( yisint == 1 )
                        {
                            z = -z;
                        }
                    }
                    return z;
                }
            }
            if ( ( ( ( hx >> 31 ) + 1 ) | yisint ) == 0 )
            {
                return ( x - x ) / ( x - x );
            }
            if ( iy > 0x41e00000 )
            {
                if ( iy > 0x43f00000 )
                {
                    if ( ix <= 0x3fefffff )
                    {
                        return ( ( hy < 0 ) ? huge * huge : tiny * tiny );
                    }
                    if ( ix >= 0x3ff00000 )
                    {
                        return ( ( hy > 0 ) ? huge * huge : tiny * tiny );
                    }
                }
                if ( ix < 0x3fefffff )
                {
                    return ( ( hy < 0 ) ? huge * huge : tiny * tiny );
                }
                if ( ix > 0x3ff00000 )
                {
                    return ( ( hy > 0 ) ? huge * huge : tiny * tiny );
                }
                t = x - 1;
                w = ( t * t ) * ( 0.5 - t * ( 0.3333333333333333333333 - t * 0.25 ) );
                u = ivln2_h * t;
                v = t * ivln2_l - w * ivln2;
                t1 = u + v;
                t1 = setLO( t1, 0 );
                t2 = v - ( t1 - u );
            }
            else
            {
                double s2, s_h, s_l, t_h, t_l;
                n = 0;
                if ( ix < 0x00100000 )
                {
                    ax *= two53;
                    n -= 53;
                    ix = __HI( ax );
                }
                n += ( ( ix ) >> 20 ) - 0x3ff;
                j = ix & 0x000fffff;
                ix = j | 0x3ff00000;
                if ( j <= 0x3988E )
                {
                    k = 0;
                }
                else if ( j < 0xBB67A )
                {
                    k = 1;
                }
                else
                {
                    k = 0;
                    n += 1;
                    ix -= 0x00100000;
                }
                ax = setHI( ax, ix );
                u = ax - bp[ k ];
                v = one / ( ax + bp[ k ] );
                s = u * v;
                s_h = s;
                s_h = setLO( s_h, 0 );
                t_h = zero;
                t_h = setHI( t_h, ( ( ix >> 1 ) | 0x20000000 ) + 0x00080000 + ( k << 18 ) );
                t_l = ax - ( t_h - bp[ k ] );
                s_l = v * ( ( u - s_h * t_h ) - s_h * t_l );
                s2 = s * s;
                r = s2 * s2 * ( L1 + s2 * ( L2 + s2 * ( L3 + s2 * ( L4 + s2 * ( L5 + s2 * L6 ) ) ) ) );
                r += s_l * ( s_h + s );
                s2 = s_h * s_h;
                t_h = 3.0 + s2 + r;
                t_h = setLO( t_h, 0 );
                t_l = r - ( ( t_h - 3.0 ) - s2 );
                u = s_h * t_h;
                v = s_l * t_h + t_l * s;
                p_h = u + v;
                p_h = setLO( p_h, 0 );
                p_l = v - ( p_h - u );
                z_h = cp_h * p_h;
                z_l = cp_l * p_h + p_l * cp + dp_l[ k ];
                t = ( double ) n;
                t1 = ( ( ( z_h + z_l ) + dp_h[ k ] ) + t );
                t1 = setLO( t1, 0 );
                t2 = z_l - ( ( ( t1 - t ) - dp_h[ k ] ) - z_h );
            }
            s = one;
            if ( ( ( ( hx >> 31 ) + 1 ) | ( yisint - 1 ) ) == 0 )
            {
                s = -one;
            }
            y1 = y;
            y1 = setLO( y1, 0 );
            p_l = ( y - y1 ) * t1 + y * t2;
            p_h = y1 * t1;
            z = p_l + p_h;
            j = __HI( z );
            i = __LO( z );
            if ( j >= 0x40900000 )
            {
                if ( ( ( j - 0x40900000 ) | i ) != 0 )
                {
                    return s * huge * huge;
                }
                else
                {
                    if ( p_l + ovt > z - p_h )
                    {
                        return s * huge * huge;
                    }
                }
            }
            else if ( ( j & 0x7fffffff ) >= 0x4090cc00 )
            {
                if ( ( ( j - 0xc090cc00 ) | i ) != 0 )
                {
                    return s * tiny * tiny;
                }
                else
                {
                    if ( p_l <= z - p_h )
                    {
                        return s * tiny * tiny;
                    }
                }
            }
            i = j & 0x7fffffff;
            k = ( i >> 20 ) - 0x3ff;
            n = 0;
            if ( i > 0x3fe00000 )
            {
                n = j + ( 0x00100000 >> ( k + 1 ) );
                k = ( ( n & 0x7fffffff ) >> 20 ) - 0x3ff;
                t = zero;
                t = setHI( t, ( n & ~( 0x000fffff >> k ) ) );
                n = ( ( n & 0x000fffff ) | 0x00100000 ) >> ( 20 - k );
                if ( j < 0 )
                {
                    n = -n;
                }
                p_h -= t;
            }
            t = p_l + p_h;
            t = setLO( t, 0 );
            u = t * lg2_h;
            v = ( p_l - ( t - p_h ) ) * lg2 + t * lg2_l;
            z = u + v;
            w = v - ( z - u );
            t = z * z;
            t1 = z - t * ( P1 + t * ( P2 + t * ( P3 + t * ( P4 + t * P5 ) ) ) );
            r = ( z * t1 ) / ( t1 - 2.0 ) - ( w + z * w );
            z = one - ( r - z );
            j = __HI( z );
            j += ( n << 20 );
            if ( ( j >> 20 ) <= 0 )
            {
                z = scalbn( z, n );
            }
            else
            {
                i = __HI( z );
                i += ( n << 20 );
                z = setHI( z, i );
            }
            return s * z;
        }

        private static readonly long SignMask = BitConverter.DoubleToInt64Bits( -0.0 ) ^ BitConverter.DoubleToInt64Bits( +0.0 );

        private static double copysign( double x, double y )
        {
            long ix = BitConverter.DoubleToInt64Bits( x );
            long iy = BitConverter.DoubleToInt64Bits( y );

            // TODO: Check this.
            ix = ( 0x7fffffffffffffffL & ix ) | ( SignMask & iy );

            return BitConverter.Int64BitsToDouble( ix );
        }

        private static readonly double two54 = BitConverter.Int64BitsToDouble( 0x4350000000000000L );
        private static readonly double twom54 = BitConverter.Int64BitsToDouble( 0x3c90000000000000L );

        private static double scalbn( double x, int n )
        {
            int k, hx, lx;
            hx = __HI( x );
            lx = __LO( x );
            k = ( hx & 0x7ff00000 ) >> 20;
            if ( k == 0 )
            {
                if ( ( lx | ( hx & 0x7fffffff ) ) == 0 )
                {
                    return x;
                }
                x *= two54;
                hx = __HI( x );
                k = ( ( hx & 0x7ff00000 ) >> 20 ) - 54;
                if ( n < -50000 )
                {
                    return tiny * x;
                }
            }
            if ( k == 0x7ff )
            {
                return x + x;
            }
            k = k + n;
            if ( k > 0x7fe )
            {
                return huge * copysign( huge, x );
            }
            if ( k > 0 )
            {
                return setHI( x, ( hx & unchecked(( int ) 0x800fffff) ) | ( k << 20 ) );
            }
            if ( k <= -54 )
            {
                if ( n > 50000 )
                {
                    return huge * copysign( huge, x );
                }
            }
            else
            {
                return tiny * copysign( tiny, x );
            }

            k += 54;

            return twom54 * setHI( x, ( hx & unchecked(( int ) 0x800fffff) ) | ( k << 20 ) );
        }

        private static double set( int newHiPart, int newLowPart )
        {
            return BitConverter.Int64BitsToDouble( ( ( ( long ) newHiPart ) << 32 ) | newLowPart );
        }

        private static double setLO( double x, int newLowPart )
        {
            long lx = BitConverter.DoubleToInt64Bits( x );

            // TODO: Check this.
            lx &= BitConverter.ToInt64( BitConverter.GetBytes( 0xFFFFFFFF00000000 ), 0 );
            lx |= newLowPart;

            return BitConverter.Int64BitsToDouble( lx );
        }

        private static double setHI( double x, int newHiPart )
        {
            long lx = BitConverter.DoubleToInt64Bits( x );

            lx &= 0x00000000FFFFFFFFL;

            lx |= ( ( ( long ) newHiPart ) << 32 );

            return BitConverter.Int64BitsToDouble( lx );
        }

        private static int __HI( double x )
        {
            return unchecked(( int ) ( 0xFFFFFFFF & ( BitConverter.DoubleToInt64Bits( x ) >> 32 ) ));
        }

        private static int __LO( double x )
        {
            return unchecked(( int ) ( 0xFFFFFFFF & BitConverter.DoubleToInt64Bits( x ) ));
        }

        // Copy from the MPN.cs.
        public static int add_1( int[] dest, int[] x, int size, int y )
        {
            long carry = ( long ) y & 0xffffffffL;
            for ( int i = 0; i < size; i++ )
            {
                carry += ( ( long ) x[ i ] & 0xffffffffL );
                dest[ i ] = ( int ) carry;
                carry >>= 32;
            }
            return ( int ) carry;
        }
        public static int add_n( int[] dest, int[] x, int[] y, int len )
        {
            long carry = 0;
            for ( int i = 0; i < len; i++ )
            {
                carry += ( ( long ) x[ i ] & 0xffffffffL ) + ( ( long ) y[ i ] & 0xffffffffL );
                dest[ i ] = ( int ) carry;
                carry = ( long ) ( ( ulong ) carry >> 32 );
            }
            return ( int ) carry;
        }
        public static int sub_n( int[] dest, int[] X, int[] Y, int size )
        {
            int cy = 0;
            for ( int i = 0; i < size; i++ )
            {
                int y = Y[ i ];
                int x = X[ i ];
                y += cy;
                cy = ( y ^ 0x80000000 ) < ( cy ^ 0x80000000 ) ? 1 : 0;
                y = x - y;
                cy += ( y ^ 0x80000000 ) > ( x ^ 0x80000000 ) ? 1 : 0;
                dest[ i ] = y;
            }
            return cy;
        }
        public static int mul_1( int[] dest, int[] x, int len, int y )
        {
            long yword = ( long ) y & 0xffffffffL;
            long carry = 0;
            for ( int j = 0; j < len; j++ )
            {
                carry += ( ( long ) x[ j ] & 0xffffffffL ) * yword;
                dest[ j ] = ( int ) carry;
                carry = ( long ) ( ( ulong ) carry >> 32 );
            }
            return ( int ) carry;
        }
        public static void mul( int[] dest, int[] x, int xlen, int[] y, int ylen )
        {
            dest[ xlen ] = mul_1( dest, x, xlen, y[ 0 ] );
            for ( int i = 1; i < ylen; i++ )
            {
                long yword = ( long ) y[ i ] & 0xffffffffL;
                long carry = 0;
                for ( int j = 0; j < xlen; j++ )
                {
                    carry += ( ( long ) x[ j ] & 0xffffffffL ) * yword + ( ( long ) dest[ i + j ] & 0xffffffffL );
                    dest[ i + j ] = ( int ) carry;
                    carry = ( long ) ( ( ulong ) carry >> 32 );
                }
                dest[ i + xlen ] = ( int ) carry;
            }
        }
        public static long udiv_qrnnd( long N, int D )
        {
            long q, r;
            long a1 = ( long ) ( ( ulong ) N >> 32 );
            long a0 = N & 0xffffffffL;
            if ( D >= 0 )
            {
                if ( a1 < ( ( D - a1 - ( ( long ) ( ( ulong ) a0 >> 31 ) ) ) & 0xffffffffL ) )
                {
                    q = N / D;
                    r = N % D;
                }
                else
                {
                    long c = N - ( ( long ) D << 31 );
                    q = c / D;
                    r = c % D;
                    q += 1 << 31;
                }
            }
            else
            {
                long b1 = ( int ) ( ( uint ) D >> 1 );
                long c = ( long ) ( ( ulong ) N >> 1 );
                if ( a1 < b1 || ( a1 >> 1 ) < b1 )
                {
                    if ( a1 < b1 )
                    {
                        q = c / b1;
                        r = c % b1;
                    }
                    else
                    {
                        c = ~( c - ( b1 << 32 ) );
                        q = c / b1;
                        r = c % b1;
                        q = ( ~q ) & 0xffffffffL;
                        r = ( b1 - 1 ) - r;
                    }
                    r = 2 * r + ( a0 & 1 );
                    if ( ( D & 1 ) != 0 )
                    {
                        if ( r >= q )
                        {
                            r = r - q;
                        }
                        else if ( q - r <= ( ( long ) D & 0xffffffffL ) )
                        {
                            r = r - q + D;
                            q -= 1;
                        }
                        else
                        {
                            r = r - q + D + D;
                            q -= 2;
                        }
                    }
                }
                else
                {
                    if ( a0 >= ( ( long ) ( -D ) & 0xffffffffL ) )
                    {
                        q = -1;
                        r = a0 + D;
                    }
                    else
                    {
                        q = -2;
                        r = a0 + D + D;
                    }
                }
            }
            return ( r << 32 ) | ( q & 0xFFFFFFFFL );
        }
        public static int divmod_1( int[] quotient, int[] dividend, int len, int divisor )
        {
            int i = len - 1;
            long r = dividend[ i ];
            if ( ( r & 0xffffffffL ) >= ( ( long ) divisor & 0xffffffffL ) )
            {
                r = 0;
            }
            else
            {
                quotient[ i-- ] = 0;
                r <<= 32;
            }
            for ( ; i >= 0; i-- )
            {
                int n0 = dividend[ i ];
                r = ( r & ~0xffffffffL ) | ( n0 & 0xffffffffL );
                r = udiv_qrnnd( r, divisor );
                quotient[ i ] = ( int ) r;
            }
            return ( int ) ( r >> 32 );
        }
        public static int submul_1( int[] dest, int offset, int[] x, int len, int y )
        {
            long yl = ( long ) y & 0xffffffffL;
            int carry = 0;
            int j = 0;
            do
            {
                long prod = ( ( long ) x[ j ] & 0xffffffffL ) * yl;
                int prod_low = ( int ) prod;
                int prod_high = ( int ) ( prod >> 32 );
                prod_low += carry;
                carry = ( ( prod_low ^ 0x80000000 ) < ( carry ^ 0x80000000 ) ? 1 : 0 ) + prod_high;
                int x_j = dest[ offset + j ];
                prod_low = x_j - prod_low;
                if ( ( prod_low ^ 0x80000000 ) > ( x_j ^ 0x80000000 ) )
                {
                    carry++;
                }
                dest[ offset + j ] = prod_low;
            } while ( ++j < len );
            return carry;
        }
        public static void divide( int[] zds, int nx, int[] y, int ny )
        {
            int j = nx;
            do
            {
                int qhat;
                if ( zds[ j ] == y[ ny - 1 ] )
                {
                    qhat = -1;
                }
                else
                {
                    long w = ( ( ( long ) ( zds[ j ] ) ) << 32 ) + ( ( long ) zds[ j - 1 ] & 0xffffffffL );
                    qhat = ( int ) udiv_qrnnd( w, y[ ny - 1 ] );
                }
                if ( qhat != 0 )
                {
                    int borrow = submul_1( zds, j - ny, y, ny, qhat );
                    int save = zds[ j ];
                    long num = ( ( long ) save & 0xffffffffL ) - ( ( long ) borrow & 0xffffffffL );
                    while ( num != 0 )
                    {
                        qhat--;
                        long carry = 0;
                        for ( int i = 0; i < ny; i++ )
                        {
                            carry += ( ( long ) zds[ j - ny + i ] & 0xffffffffL ) + ( ( long ) y[ i ] & 0xffffffffL );
                            zds[ j - ny + i ] = ( int ) carry;
                            carry = ( long ) ( ( ulong ) carry >> 32 );
                        }
                        zds[ j ] += ( int ) carry;
                        num = carry - 1;
                    }
                }
                zds[ j ] = qhat;
            } while ( --j >= ny );
        }
        public static int chars_per_word( int radix )
        {
            if ( radix < 10 )
            {
                if ( radix < 8 )
                {
                    if ( radix <= 2 )
                    {
                        return 32;
                    }
                    else if ( radix == 3 )
                    {
                        return 20;
                    }
                    else if ( radix == 4 )
                    {
                        return 16;
                    }
                    else
                    {
                        return 18 - radix;
                    }
                }
                else
                {
                    return 10;
                }
            }
            else if ( radix < 12 )
            {
                return 9;
            }
            else if ( radix <= 16 )
            {
                return 8;
            }
            else if ( radix <= 23 )
            {
                return 7;
            }
            else if ( radix <= 40 )
            {
                return 6;
            }
            else if ( radix <= 256 )
            {
                return 4;
            }
            else
            {
                return 1;
            }
        }
        public static int count_leading_zeros( int i )
        {
            if ( i == 0 )
            {
                return 32;
            }
            int count = 0;
            for ( int k = 16; k > 0; k = k >> 1 )
            {
                int j = ( int ) ( ( uint ) i >> k );
                if ( j == 0 )
                {
                    count += k;
                }
                else
                {
                    i = j;
                }
            }
            return count;
        }
        public static int set_str( int[] dest, sbyte[] str, int str_len, int @base )
        {
            int size = 0;
            if ( ( @base & ( @base - 1 ) ) == 0 )
            {
                int next_bitpos = 0;
                int bits_per_indigit = 0;
                for ( int i = @base; ( i >>= 1 ) != 0; )
                {
                    bits_per_indigit++;
                }
                int res_digit = 0;
                for ( int i = str_len; --i >= 0; )
                {
                    int inp_digit = str[ i ];
                    res_digit |= inp_digit << next_bitpos;
                    next_bitpos += bits_per_indigit;
                    if ( next_bitpos >= 32 )
                    {
                        dest[ size++ ] = res_digit;
                        next_bitpos -= 32;
                        res_digit = inp_digit >> ( bits_per_indigit - next_bitpos );
                    }
                }
                if ( res_digit != 0 )
                {
                    dest[ size++ ] = res_digit;
                }
            }
            else
            {
                int indigits_per_limb = chars_per_word( @base );
                int str_pos = 0;
                while ( str_pos < str_len )
                {
                    int chunk = str_len - str_pos;
                    if ( chunk > indigits_per_limb )
                    {
                        chunk = indigits_per_limb;
                    }
                    int res_digit = str[ str_pos++ ];
                    int big_base = @base;
                    while ( --chunk > 0 )
                    {
                        res_digit = res_digit * @base + str[ str_pos++ ];
                        big_base *= @base;
                    }
                    int cy_limb;
                    if ( size == 0 )
                    {
                        cy_limb = res_digit;
                    }
                    else
                    {
                        cy_limb = mul_1( dest, dest, size, big_base );
                        cy_limb += add_1( dest, dest, size, res_digit );
                    }
                    if ( cy_limb != 0 )
                    {
                        dest[ size++ ] = cy_limb;
                    }
                }
            }
            return size;
        }
        public static int cmp( int[] x, int[] y, int size )
        {
            while ( --size >= 0 )
            {
                int x_word = x[ size ];
                int y_word = y[ size ];
                if ( x_word != y_word )
                {
                    return ( x_word ^ 0x80000000 ) > ( y_word ^ 0x80000000 ) ? 1 : -1;
                }
            }
            return 0;
        }
        public static int cmp( int[] x, int xlen, int[] y, int ylen )
        {
            return xlen > ylen ? 1 : xlen < ylen ? -1 : cmp( x, y, xlen );
        }
        public static int rshift( int[] dest, int[] x, int x_start, int len, int count )
        {
            int count_2 = 32 - count;
            int low_word = x[ x_start ];
            int retval = low_word << count_2;
            int i = 1;
            for ( ; i < len; i++ )
            {
                int high_word = x[ x_start + i ];
                dest[ i - 1 ] = ( ( int ) ( ( uint ) low_word >> count ) ) | ( high_word << count_2 );
                low_word = high_word;
            }
            dest[ i - 1 ] = ( int ) ( ( uint ) low_word >> count );
            return retval;
        }
        public static void rshift0( int[] dest, int[] x, int x_start, int len, int count )
        {
            if ( count > 0 )
            {
                rshift( dest, x, x_start, len, count );
            }
            else
            {
                for ( int i = 0; i < len; i++ )
                {
                    dest[ i ] = x[ i + x_start ];
                }
            }
        }
        public static long rshift_long( int[] x, int len, int count )
        {
            int wordno = count >> 5;
            count &= 31;
            int sign = x[ len - 1 ] < 0 ? -1 : 0;
            int w0 = wordno >= len ? sign : x[ wordno ];
            wordno++;
            int w1 = wordno >= len ? sign : x[ wordno ];
            if ( count != 0 )
            {
                wordno++;
                int w2 = wordno >= len ? sign : x[ wordno ];
                w0 = ( ( int ) ( ( uint ) w0 >> count ) ) | ( w1 << ( 32 - count ) );
                w1 = ( ( int ) ( ( uint ) w1 >> count ) ) | ( w2 << ( 32 - count ) );
            }
            return ( ( long ) w1 << 32 ) | ( ( long ) w0 & 0xffffffffL );
        }
        public static int lshift( int[] dest, int d_offset, int[] x, int len, int count )
        {
            int count_2 = 32 - count;
            int i = len - 1;
            int high_word = x[ i ];
            int retval = ( int ) ( ( uint ) high_word >> count_2 );
            d_offset++;
            while ( --i >= 0 )
            {
                int low_word = x[ i ];
                dest[ d_offset + i ] = ( high_word << count ) | ( ( int ) ( ( uint ) low_word >> count_2 ) );
                high_word = low_word;
            }
            dest[ d_offset + i ] = high_word << count;
            return retval;
        }
        public static int findLowestBit( int word )
        {
            int i = 0;
            while ( ( word & 0xF ) == 0 )
            {
                word >>= 4;
                i += 4;
            }
            if ( ( word & 3 ) == 0 )
            {
                word >>= 2;
                i += 2;
            }
            if ( ( word & 1 ) == 0 )
            {
                i += 1;
            }
            return i;
        }
        public static int findLowestBit( int[] words )
        {
            for ( int i = 0; ; i++ )
            {
                if ( words[ i ] != 0 )
                {
                    return 32 * i + findLowestBit( words[ i ] );
                }
            }
        }
        public static int gcd( int[] x, int[] y, int len )
        {
            int i, word;
            for ( i = 0; ; i++ )
            {
                word = x[ i ] | y[ i ];
                if ( word != 0 )
                {
                    break;
                }
            }
            int initShiftWords = i;
            int initShiftBits = findLowestBit( word );
            len -= initShiftWords;
            rshift0( x, x, initShiftWords, len, initShiftBits );
            rshift0( y, y, initShiftWords, len, initShiftBits );
            int[] odd_arg;
            int[] other_arg;
            if ( ( x[ 0 ] & 1 ) != 0 )
            {
                odd_arg = x;
                other_arg = y;
            }
            else
            {
                odd_arg = y;
                other_arg = x;
            }
            for (; ; )
            {
                for ( i = 0; other_arg[ i ] == 0; )
                {
                    i++;
                }
                if ( i > 0 )
                {
                    int j;
                    for ( j = 0; j < len - i; j++ )
                    {
                        other_arg[ j ] = other_arg[ j + i ];
                    }
                    for ( ; j < len; j++ )
                    {
                        other_arg[ j ] = 0;
                    }
                }
                i = findLowestBit( other_arg[ 0 ] );
                if ( i > 0 )
                {
                    rshift( other_arg, other_arg, 0, len, i );
                }
                i = cmp( odd_arg, other_arg, len );
                if ( i == 0 )
                {
                    break;
                }
                if ( i > 0 )
                {
                    sub_n( odd_arg, odd_arg, other_arg, len );
                    int[] tmp = odd_arg;
                    odd_arg = other_arg;
                    other_arg = tmp;
                }
                else
                {
                    sub_n( other_arg, other_arg, odd_arg, len );
                } while ( odd_arg[ len - 1 ] == 0 && other_arg[ len - 1 ] == 0 )
                    len--;
            }
            if ( initShiftWords + initShiftBits > 0 )
            {
                if ( initShiftBits > 0 )
                {
                    int sh_out = lshift( x, initShiftWords, x, len, initShiftBits );
                    if ( sh_out != 0 )
                    {
                        x[ ( len++ ) + initShiftWords ] = sh_out;
                    }
                }
                else
                {
                    for ( i = len; --i >= 0; )
                    {
                        x[ i + initShiftWords ] = x[ i ];
                    }
                }
                for ( i = initShiftWords; --i >= 0; )
                {
                    x[ i ] = 0;
                }
                len += initShiftWords;
            }
            return len;
        }
        public static int intLength( int i )
        {
            return 32 - count_leading_zeros( i < 0 ? ~i : i );
        }
        public static int intLength( int[] words, int len )
        {
            len--;
            return intLength( words[ len ] ) + 32 * len;
        }
    }
}

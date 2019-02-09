using System;
using System.Text;

public class NumberFormatException : Exception
{
}

public class BigInteger
{
    internal virtual char Character_forDigit( int digit, int radix )
    {
        if ( digit < 0 || digit >= radix )
        {
            return '\u0000';
        }
        if ( digit < 10 )
        {
            return ( char ) ( digit + '0' );
        }
        return ( char ) ( digit - 10 + 'a' );
    }

    [NonSerialized]
    private int ival;

    [NonSerialized]
    private int[] words;

    private sbyte[] magnitude;

    private int bitCount_Renamed = -1;
    private int bitLength_Renamed = -1;
    private int firstNonzeroByteNum = -2;
    private int lowestSetBit = -2;
    private int signum_Renamed;

    private const long serialVersionUID = -8287574255936472291L;
    private const int minFixNum = 0;
    private const int maxFixNum = 10;

    private static readonly int numFixNum = maxFixNum - minFixNum + 1;
    private static readonly BigInteger[] smallFixNums = new BigInteger[ numFixNum ];

    public static readonly BigInteger ZERO;
    public static readonly BigInteger ONE;
    public static readonly BigInteger TEN;

    private const int FLOOR = 1;
    private const int CEILING = 2;
    private const int TRUNCATE = 3;
    private const int ROUND = 4;

    private static readonly int[] primes = { 2, 3, 5, 7 };
    private static readonly int[] k = { 100, 150, 200, 250, 300, 350, 400, 500, 600, 800, 1250, int.MaxValue };
    private static readonly int[] t = { 27, 18, 15, 12, 9, 8, 7, 6, 5, 4, 3, 2 };

    static BigInteger()
    {
        for ( var i = numFixNum; --i >= 0; )
        {
            smallFixNums[i] = new BigInteger( i + minFixNum );
        }

        ZERO = smallFixNums[ 0 - minFixNum ];
        ONE = smallFixNums[ 1 - minFixNum ];
        TEN = smallFixNums[ 10 - minFixNum ];
    }

    private BigInteger()
    {
    }

    private BigInteger( int value ) : this()
    {
        ival = value;
    }

    public BigInteger( string val, int radix ) : this()
    {
        var result = valueOf( val, radix );

        ival = result.ival;
        words = result.words;
    }

    public BigInteger( string val ) : this( val, 10 )
    {
    }

    public BigInteger( sbyte[] val ) : this()
    {
        if ( val == null || val.Length < 1 )
        {
            throw new NumberFormatException();
        }

        words = byteArrayToIntArray( val, val[ 0 ] < 0 ? -1 : 0 );

        var result = make( words, words.Length );

        ival = result.ival;
        words = result.words;
    }

    public BigInteger( int signum, sbyte[] magnitude ) : this()
    {
        if ( magnitude == null || signum > 1 || signum < -1 )
        {
            throw new NumberFormatException();
        }

        if ( signum == 0 )
        {
            int i;

            for ( i = magnitude.Length - 1; i >= 0 && magnitude[i] == 0; --i )
            {
                ;
            }

            if ( i >= 0 )
            {
                throw new NumberFormatException();
            }

            return;
        }

        words = byteArrayToIntArray( magnitude, 0 );

        var result = make( words, words.Length );

        ival = result.ival;
        words = result.words;

        if ( signum < 0 )
        {
            setNegative();
        }
    }

    public static BigInteger valueOf( long val )
    {
        if ( val >= minFixNum && val <= maxFixNum )
        {
            return smallFixNums[ ( int ) val - minFixNum ];
        }

        var i = ( int ) val;

        if ( i == val )
        {
            return new BigInteger(i);
        }

        var result = alloc(2);

        result.ival = 2;

        result.words[0] = i;
        result.words[1] = ( int ) ( val >> 32 );

        return result;
    }

    private static BigInteger make( int[] words, int len )
    {
        if ( words == null )
        {
            return valueOf( len );
        }

        len = wordsNeeded( words, len );

        if ( len <= 1 )
        {
            return len == 0 ? ZERO : valueOf( words[0] );
        }

        var num = new BigInteger
        {
            words = words,
            ival = len
        };

        return num;
    }

    private static int[] byteArrayToIntArray( sbyte[] bytes, int sign )
    {
        var words = new int[ bytes.Length / 4 + 1 ];

        int nwords = words.Length;
        int bptr = 0;
        int word = sign;

        for ( int i = bytes.Length % 4; i > 0; --i, bptr++ )
        {
            word = ( word << 8 ) | ( bytes[ bptr ] & 0xff );
        }

        words[ --nwords ] = word;

        while ( nwords > 0 )
        {
            words[ --nwords ] = bytes[ bptr++ ] << 24 | ( bytes[ bptr++ ] & 0xff ) << 16 | ( bytes[ bptr++ ] & 0xff ) << 8 | ( bytes[ bptr++ ] & 0xff );
        }

        return words;
    }

    private static BigInteger alloc( int nwords )
    {
        var result = new BigInteger();

        if ( nwords > 1 )
        {
            result.words = new int[ nwords ];
        }

        return result;
    }

    private void realloc( int nwords )
    {
        if ( nwords == 0 )
        {
            if ( words != null )
            {
                if ( ival > 0 )
                {
                    ival = words[0];
                }

                words = null;
            }
        }
        else if ( words == null || words.Length < nwords || words.Length > nwords + 2 )
        {
            var new_words = new int[ nwords ];

            if ( words == null )
            {
                new_words[0] = ival;
                ival = 1;
            }
            else
            {
                if ( nwords < ival )
                {
                    ival = nwords;
                }

                Array.Copy( words, 0, new_words, 0, ival );
            }

            words = new_words;
        }
    }

    private bool Negative
    {
        get { return ( words == null ? ival : words[ ival - 1 ] ) < 0; }
    }

    public virtual int signum()
    {
        var top = words == null ? ival : words[ ival - 1 ];

        if ( top == 0 && words == null )
        {
            return 0;
        }

        return top < 0 ? -1 : 1;
    }

    private static int compareTo( BigInteger x, BigInteger y )
    {
        if ( x.words == null && y.words == null )
        {
            return x.ival < y.ival ? -1 : x.ival > y.ival ? 1 : 0;
        }

        bool x_negative = x.Negative;
        bool y_negative = y.Negative;

        if ( x_negative != y_negative )
        {
            return x_negative ? -1 : 1;
        }

        int x_len = x.words == null ? 1 : x.ival;
        int y_len = y.words == null ? 1 : y.ival;

        if ( x_len != y_len )
        {
            return ( x_len > y_len ) != x_negative ? 1 : -1;
        }

        return MPN.cmp( x.words, y.words, x_len );
    }

    public virtual int compareTo( BigInteger val )
    {
        return compareTo( this, val );
    }

    public virtual BigInteger min( BigInteger val )
    {
        return compareTo( this, val ) < 0 ? this : val;
    }

    public virtual BigInteger max( BigInteger val )
    {
        return compareTo( this, val ) > 0 ? this : val;
    }

    private bool Zero
    {
        get
        {
            return words == null && ival == 0;
        }
    }

    private bool One
    {
        get
        {
            return words == null && ival == 1;
        }
    }

    private static int wordsNeeded( int[] words, int len )
    {
        int i = len;

        if ( i > 0 )
        {
            int word = words[ --i ];

            if ( word == -1 )
            {
                while ( i > 0 && ( word = words[ i - 1 ] ) < 0 )
                {
                    i--;
                    if ( word != -1 )
                    {
                        break;
                    }
                }
            }
            else
            {
                while ( word == 0 && i > 0 && ( word = words[ i - 1 ] ) >= 0 )
                {
                    i--;
                }
            }
        }

        return i + 1;
    }

    private BigInteger canonicalize()
    {
        if ( words != null && ( ival = wordsNeeded( words, ival ) ) <= 1 )
        {
            if ( ival == 1 )
            {
                ival = words[0];
            }

            words = null;
        }

        if ( words == null && ival >= minFixNum && ival <= maxFixNum )
        {
            return smallFixNums[ ival - minFixNum ];
        }

        return this;
    }

    private static BigInteger add( int x, int y )
    {
        return valueOf( ( long ) x + ( long ) y );
    }

    private static BigInteger add( BigInteger x, int y )
    {
        if ( x.words == null )
        {
            return add( x.ival, y );
        }

        var result = new BigInteger(0);

        result.setAdd( x, y );

        return result.canonicalize();
    }

    private void setAdd( BigInteger x, int y )
    {
        if ( x.words == null )
        {
            set( ( long ) x.ival + ( long ) y );

            return;
        }

        int len = x.ival;

        realloc( len + 1 );

        long carry = y;

        for ( int i = 0; i < len; i++ )
        {
            carry += ( ( long ) x.words[i] & 0xffffffffL );
            words[i] = ( int ) carry;
            carry >>= 32;
        }

        if ( x.words[ len - 1 ] < 0 )
        {
            carry--;
        }

        words[ len ] = ( int ) carry;

        ival = wordsNeeded( words, len + 1 );
    }

    private int Add
    {
        set
        {
            setAdd( this, value );
        }
    }

    private void set( long y )
    {
        int i = ( int ) y;

        if ( ( long ) i == y )
        {
            ival = i;
            words = null;
        }
        else
        {
            realloc( 2 );

            words[0] = i;
            words[1] = ( int ) ( y >> 32 );

            ival = 2;
        }
    }

    private void set( int[] words, int length )
    {
        this.ival = length;
        this.words = words;
    }

    private void set( BigInteger y )
    {
        if ( y.words == null )
        {
            set( y.ival );
        }
        else if ( this != y )
        {
            realloc( y.ival );

            Array.Copy( y.words, 0, words, 0, y.ival );

            ival = y.ival;
        }
    }

    private static BigInteger add( BigInteger x, BigInteger y, int k )
    {
        if ( x.words == null && y.words == null )
        {
            return valueOf( ( long ) k * ( long ) y.ival + ( long ) x.ival );
        }

        if ( k != 1 )
        {
            if ( k == -1 )
            {
                y = neg(y);
            }
            else
            {
                y = times( y, valueOf(k) );
            }
        }

        if ( x.words == null )
        {
            return add( y, x.ival );
        }

        if ( y.words == null )
        {
            return add( x, y.ival );
        }

        if ( y.ival > x.ival )
        {
            var tmp = x;
            x = y;
            y = tmp;
        }

        var result = alloc( x.ival + 1 );

        int i = y.ival;

        long carry = MPN.add_n( result.words, x.words, y.words, i );

        long y_ext = y.words[ i - 1 ] < 0 ? 0xffffffffL : 0;

        for ( ; i < x.ival; i++ )
        {
            carry += ( ( long ) x.words[ i ] & 0xffffffffL ) + y_ext;

            result.words[i] = ( int ) carry;

            carry = ( long ) ( ( ulong ) carry >> 32 );
        }

        if ( x.words[ i - 1 ] < 0 )
        {
            y_ext--;
        }

        result.words[i] = ( int ) ( carry + y_ext );
        result.ival = i + 1;

        return result.canonicalize();
    }

    public virtual BigInteger add( BigInteger val )
    {
        return add( this, val, 1 );
    }

    public virtual BigInteger subtract( BigInteger val )
    {
        return add( this, val, -1 );
    }

    private static BigInteger times( BigInteger x, int y )
    {
        if ( y == 0 )
        {
            return ZERO;
        }

        if ( y == 1 )
        {
            return x;
        }

        var xwords = x.words;
        int xlen = x.ival;

        if ( xwords == null )
        {
            return valueOf( ( long ) xlen * ( long ) y );
        }

        bool negative;

        var result = alloc( xlen + 1 );

        if ( xwords[ xlen - 1 ] < 0 )
        {
            negative = true;
            negate( result.words, xwords, xlen );
            xwords = result.words;
        }
        else
        {
            negative = false;
        }

        if ( y < 0 )
        {
            negative = !negative;
            y = -y;
        }

        result.words[ xlen ] = MPN.mul_1( result.words, xwords, xlen, y );
        result.ival = xlen + 1;

        if ( negative )
        {
            result.setNegative();
        }

        return result.canonicalize();
    }

    private static BigInteger times( BigInteger x, BigInteger y )
    {
        if ( y.words == null )
        {
            return times( x, y.ival );
        }

        if ( x.words == null )
        {
            return times( y, x.ival );
        }

        bool negative = false;

        int xlen = x.ival;
        int ylen = y.ival;

        int[] xwords;
        int[] ywords;

        if ( x.Negative )
        {
            negative = true;
            xwords = new int[ xlen ];

            negate( xwords, x.words, xlen );
        }
        else
        {
            negative = false;
            xwords = x.words;
        }

        if ( y.Negative )
        {
            negative = !negative;
            ywords = new int[ ylen ];

            negate( ywords, y.words, ylen );
        }
        else
        {
            ywords = y.words;
        }

        if ( xlen < ylen )
        {
            int[] twords = xwords;

            xwords = ywords;
            ywords = twords;

            int tlen = xlen;
            xlen = ylen;
            ylen = tlen;
        }

        var result = alloc( xlen + ylen );

        MPN.mul( result.words, xwords, xlen, ywords, ylen );

        result.ival = xlen + ylen;

        if ( negative )
        {
            result.setNegative();
        }

        return result.canonicalize();
    }

    public virtual BigInteger multiply( BigInteger y )
    {
        return times( this, y );
    }

    private static void divide( long x, long y, BigInteger quotient, BigInteger remainder, int rounding_mode )
    {
        bool xNegative, yNegative;

        if ( x < 0 )
        {
            xNegative = true;

            if ( x == long.MinValue )
            {
                divide( valueOf(x), valueOf(y), quotient, remainder, rounding_mode );
                return;
            }

            x = -x;
        }
        else
        {
            xNegative = false;
        }

        if ( y < 0 )
        {
            yNegative = true;

            if ( y == long.MinValue )
            {
                if ( rounding_mode == TRUNCATE )
                {
                    if ( quotient != null )
                    {
                        quotient.set(0);
                    }
                    if ( remainder != null )
                    {
                        remainder.set(x);
                    }
                }
                else
                {
                    divide( valueOf(x), valueOf(y), quotient, remainder, rounding_mode );
                }
                return;
            }

            y = -y;
        }
        else
        {
            yNegative = false;
        }

        long q = x / y;
        long r = x % y;
        bool qNegative = xNegative ^ yNegative;
        bool add_one = false;

        if ( r != 0 )
        {
            switch ( rounding_mode )
            {
                case TRUNCATE:
                    break;
                case CEILING:
                case FLOOR:
                    if ( qNegative == ( rounding_mode == FLOOR ) )
                    {
                        add_one = true;
                    }
                    break;
                case ROUND:
                    add_one = r > ( ( y - ( q & 1 ) ) >> 1 );
                    break;
            }
        }

        if ( quotient != null )
        {
            if ( add_one )
            {
                q++;
            }

            if ( qNegative )
            {
                q = -q;
            }

            quotient.set(q);
        }

        if ( remainder != null )
        {
            if ( add_one )
            {
                r = y - r;
                xNegative = !xNegative;
            }

            if ( xNegative )
            {
                r = -r;
            }

            remainder.set(r);
        }
    }

    private static void divide( BigInteger x, BigInteger y, BigInteger quotient, BigInteger remainder, int rounding_mode )
    {
        if ( ( x.words == null || x.ival <= 2 ) && ( y.words == null || y.ival <= 2 ) )
        {
            long x_l = x.longValue();
            long y_l = y.longValue();

            if ( x_l != long.MinValue && y_l != long.MinValue )
            {
                divide( x_l, y_l, quotient, remainder, rounding_mode );
                return;
            }
        }

        bool xNegative = x.Negative;
        bool yNegative = y.Negative;
        bool qNegative = xNegative ^ yNegative;

        int ylen = y.words == null ? 1 : y.ival;

        int[] ywords = new int[ ylen ];

        y.getAbsolute( ywords );

        while ( ylen > 1 && ywords[ ylen - 1 ] == 0 )
        {
            ylen--;
        }

        int xlen = x.words == null ? 1 : x.ival;

        int[] xwords = new int[ xlen + 2 ];

        x.getAbsolute( xwords );

        while ( xlen > 1 && xwords[ xlen - 1 ] == 0 )
        {
            xlen--;
        }

        int qlen, rlen;
        int cmpval = MPN.cmp( xwords, xlen, ywords, ylen );

        if ( cmpval < 0 )
        {
            int[] rwords = xwords;

            xwords = ywords;
            ywords = rwords;
            rlen = xlen;
            qlen = 1;
            xwords[0] = 0;
        }
        else if ( cmpval == 0 )
        {
            xwords[0] = 1;
            qlen = 1;
            ywords[0] = 0;
            rlen = 1;
        }
        else if ( ylen == 1 )
        {
            qlen = xlen;

            if ( ywords[0] == 1 && xwords[ xlen - 1 ] < 0 )
            {
                qlen++;
            }

            rlen = 1;
            ywords[0] = MPN.divmod_1( xwords, xwords, xlen, ywords[0] );
        }
        else
        {
            int nshift = MPN.count_leading_zeros( ywords[ ylen - 1 ] );

            if ( nshift != 0 )
            {
                MPN.lshift( ywords, 0, ywords, ylen, nshift );

                int x_high = MPN.lshift( xwords, 0, xwords, xlen, nshift );

                xwords[ xlen++ ] = x_high;
            }

            if ( xlen == ylen )
            {
                xwords[ xlen++ ] = 0;
            }

            MPN.divide( xwords, xlen, ywords, ylen );

            rlen = ylen;

            MPN.rshift0( ywords, xwords, 0, rlen, nshift );

            qlen = xlen + 1 - ylen;

            if ( quotient != null )
            {
                for ( int i = 0; i < qlen; i++ )
                {
                    xwords[i] = xwords[ i + ylen ];
                }
            }
        }

        if ( ywords[ rlen - 1 ] < 0 )
        {
            ywords[ rlen ] = 0;
            rlen++;
        }

        bool add_one = false;

        if ( rlen > 1 || ywords[ 0 ] != 0 )
        {
            switch ( rounding_mode )
            {
                case TRUNCATE:
                    break;
                case CEILING:
                case FLOOR:
                    if ( qNegative == ( rounding_mode == FLOOR ) )
                    {
                        add_one = true;
                    }
                    break;
                case ROUND:
                    var tmp = remainder ?? new BigInteger();
                    tmp.set( ywords, rlen );
                    tmp = shift( tmp, 1 );
                    if ( yNegative )
                    {
                        tmp.setNegative();
                    }
                    int cmp = compareTo( tmp, y );
                    if ( yNegative )
                    {
                        cmp = -cmp;
                    }
                    add_one = ( cmp == 1 ) || ( cmp == 0 && ( xwords[ 0 ] & 1 ) != 0 );
                    break;
            }
        }

        if ( quotient != null )
        {
            quotient.set( xwords, qlen );

            if ( qNegative )
            {
                if ( add_one )
                {
                    quotient.setInvert();
                }
                else
                {
                    quotient.setNegative();
                }
            }
            else if ( add_one )
            {
                quotient.Add = 1;
            }
        }

        if ( remainder != null )
        {
            remainder.set( ywords, rlen );

            if ( add_one )
            {
                BigInteger tmp;

                if ( y.words == null )
                {
                    tmp = remainder;
                    tmp.set( yNegative ? ywords[ 0 ] + y.ival : ywords[ 0 ] - y.ival );
                }
                else
                {
                    tmp = add( remainder, y, yNegative ? 1 : -1 );
                }

                if ( xNegative )
                {
                    remainder.setNegative( tmp );
                }
                else
                {
                    remainder.set( tmp );
                }
            }
            else
            {
                if ( xNegative )
                {
                    remainder.setNegative();
                }
            }
        }
    }

    public virtual BigInteger divide( BigInteger val )
    {
        if ( val.Zero )
        {
            throw new ArithmeticException( "divisor is zero" );
        }

        var quot = new BigInteger();

        divide( this, val, quot, null, TRUNCATE );

        return quot.canonicalize();
    }

    public virtual BigInteger remainder( BigInteger val )
    {
        if ( val.Zero )
        {
            throw new ArithmeticException( "divisor is zero" );
        }

        var rem = new BigInteger();

        divide( this, val, null, rem, TRUNCATE );

        return rem.canonicalize();
    }

    public virtual BigInteger[] divideAndRemainder( BigInteger val )
    {
        if ( val.Zero )
        {
            throw new ArithmeticException( "divisor is zero" );
        }

        var result = new BigInteger[2];

        result[0] = new BigInteger();
        result[1] = new BigInteger();

        divide( this, val, result[0], result[1], TRUNCATE );

        result[0].canonicalize();
        result[1].canonicalize();

        return result;
    }

    public virtual BigInteger mod( BigInteger m )
    {
        if ( m.Negative || m.Zero )
        {
            throw new ArithmeticException( "non-positive modulus" );
        }

        var rem = new BigInteger();

        divide( this, m, null, rem, FLOOR );

        return rem.canonicalize();
    }

    public virtual BigInteger pow( int exponent )
    {
        if ( exponent <= 0 )
        {
            if ( exponent == 0 )
            {
                return ONE;
            }

            throw new ArithmeticException( "negative exponent" );
        }

        if ( Zero )
        {
            return this;
        }

        int plen = words == null ? 1 : ival;
        int blen = ( ( bitLength() * exponent ) >> 5 ) + 2 * plen;

        bool negative = Negative && ( exponent & 1 ) != 0;

        int[] pow2 = new int[ blen ];
        int[] rwords = new int[ blen ];
        int[] work = new int[ blen ];

        getAbsolute( pow2 );

        int rlen = 1;

        rwords[0] = 1;

        for ( ; ; )
        {
            if ( ( exponent & 1 ) != 0 )
            {
                MPN.mul( work, pow2, plen, rwords, rlen );

                int[] temp = work;

                work = rwords;
                rwords = temp;
                rlen += plen;

                while ( rwords[ rlen - 1 ] == 0 )
                {
                    rlen--;
                }
            }

            exponent >>= 1;

            if ( exponent == 0 )
            {
                break;
            }

            MPN.mul( work, pow2, plen, pow2, plen );

            {
                int[] temp = work;
                work = pow2;
                pow2 = temp;
                plen *= 2;

                while ( pow2[ plen - 1 ] == 0 )
                {
                    plen--;
                }
            }
        }

        if ( rwords[ rlen - 1 ] < 0 )
        {
            rlen++;
        }

        if ( negative )
        {
            negate( rwords, rwords, rlen );
        }

        return make( rwords, rlen );
    }

    private static int[] euclidInv( int a, int b, int prevDiv )
    {
        if ( b == 0 )
        {
            throw new ArithmeticException( "not invertible" );
        }

        if ( b == 1 )
        {
            return new[] { -prevDiv, 1 };
        }

        var xy = euclidInv( b, a % b, a / b );

        a = xy[0];

        xy[0] = a * -prevDiv + xy[1];
        xy[1] = a;

        return xy;
    }

    private static void euclidInv( BigInteger a, BigInteger b, BigInteger prevDiv, BigInteger[] xy )
    {
        if ( b.Zero )
        {
            throw new ArithmeticException( "not invertible" );
        }

        if ( b.One )
        {
            xy[0] = neg( prevDiv );
            xy[1] = ONE;

            return;
        }

        if ( a.words == null )
        {
            var xyInt = euclidInv( b.ival, a.ival % b.ival, a.ival / b.ival );

            xy[0] = new BigInteger( xyInt[0] );
            xy[1] = new BigInteger( xyInt[1] );
        }
        else
        {
            var rem = new BigInteger();
            var quot = new BigInteger();

            divide( a, b, quot, rem, FLOOR );

            rem.canonicalize();
            quot.canonicalize();

            euclidInv( b, rem, quot, xy );
        }

        var t = xy[0];

        xy[0] = add( xy[1], times( t, prevDiv ), -1 );
        xy[1] = t;
    }

    public virtual BigInteger modInverse( BigInteger y )
    {
        if ( y.Negative || y.Zero )
        {
            throw new ArithmeticException( "non-positive modulo" );
        }

        if ( y.One )
        {
            return ZERO;
        }

        if ( One )
        {
            return ONE;
        }

        var result = new BigInteger();

        bool swapped = false;

        if ( y.words == null )
        {
            int xval = ( words != null || Negative ) ? mod( y ).ival : ival;
            int yval = y.ival;

            if ( yval > xval )
            {
                int tmp = xval;
                xval = yval;
                yval = tmp;
                swapped = true;
            }

            result.ival = euclidInv( yval, xval % yval, xval / yval )[ swapped ? 0 : 1 ];

            if ( result.ival < 0 )
            {
                result.ival += y.ival;
            }
        }
        else
        {
            var x = Negative ? this.mod(y) : this;

            if ( x.compareTo( y ) < 0 )
            {
                result = x;
                x = y;
                y = result;
                swapped = true;
            }

            var rem = new BigInteger();
            var quot = new BigInteger();

            divide( x, y, quot, rem, FLOOR );

            rem.canonicalize();
            quot.canonicalize();

            var xy = new BigInteger[2];

            euclidInv( y, rem, quot, xy );

            result = swapped ? xy[0] : xy[1];

            if ( result.Negative )
            {
                result = add( result, swapped ? x : y, 1 );
            }
        }

        return result;
    }

    public virtual BigInteger modPow( BigInteger exponent, BigInteger m )
    {
        if ( m.Negative || m.Zero )
        {
            throw new ArithmeticException( "non-positive modulo" );
        }

        if ( exponent.Negative )
        {
            return modInverse(m);
        }

        if ( exponent.One )
        {
            return mod(m);
        }

        var s = ONE;
        var t = this;
        var u = exponent;

        while ( !u.Zero )
        {
            if ( u.and( ONE ).One )
            {
                s = times( s, t ).mod(m);
            }

            u = u.shiftRight(1);
            t = times( t, t ).mod(m);
        }

        return s;
    }

    private static int gcd( int a, int b )
    {
        int tmp;

        if ( b > a )
        {
            tmp = a;
            a = b;
            b = tmp;
        }

        for ( ; ; )
        {
            if ( b == 0 )
            {
                return a;
            }
            if ( b == 1 )
            {
                return b;
            }

            tmp = b;
            b = a % b;
            a = tmp;
        }
    }

    public virtual BigInteger gcd( BigInteger y )
    {
        int xval = ival;
        int yval = y.ival;

        if ( words == null )
        {
            if ( xval == 0 )
            {
                return abs( y );
            }

            if ( y.words == null && xval != int.MinValue && yval != int.MinValue )
            {
                if ( xval < 0 )
                {
                    xval = -xval;
                }

                if ( yval < 0 )
                {
                    yval = -yval;
                }

                return valueOf( gcd( xval, yval ) );
            }

            xval = 1;
        }

        if ( y.words == null )
        {
            if ( yval == 0 )
            {
                return abs( this );
            }

            yval = 1;
        }

        int len = ( xval > yval ? xval : yval ) + 1;

        var xwords = new int[ len ];
        var ywords = new int[ len ];

        getAbsolute( xwords );

        y.getAbsolute( ywords );

        len = MPN.gcd( xwords, ywords, len );

        var result = new BigInteger(0)
        {
            ival = len,
            words = xwords
        };

        if ( result.Negative && len < xwords.Length )
        {
            xwords[ len ] = 0;
            result.ival++;
        }

        return result.canonicalize();
    }

    public virtual bool isProbablePrime( int certainty )
    {
        if ( certainty < 1 )
        {
            return true;
        }

        var rem = new BigInteger();

        int i;

        for ( i = 0; i < primes.Length; i++ )
        {
            if ( words == null && ival == primes[i] )
            {
                return true;
            }

            divide( this, smallFixNums[ primes[i] - minFixNum ], null, rem, TRUNCATE );

            if ( rem.canonicalize().Zero )
            {
                return false;
            }
        }

        var pMinus1 = add( this, -1 );

        int b = pMinus1.LowestSetBit;

        var m = pMinus1.divide( valueOf( 2L << b - 1 ) );

        int bits = bitLength();

        for ( i = 0; i < k.Length; i++ )
        {
            if ( bits <= k[i] )
            {
                break;
            }
        }

        int trials = t[i];

        if ( certainty > 80 )
        {
            trials *= 2;
        }

        BigInteger z;

        for ( var k = 0; k < trials; k++ )
        {
            z = smallFixNums[ primes[k] - minFixNum ].modPow( m, this );

            if ( z.One || z.Equals( pMinus1 ) )
            {
                continue;
            }

            for ( i = 0; i < b; )
            {
                if ( z.One )
                {
                    return false;
                }

                i++;

                if ( z.Equals( pMinus1 ) )
                {
                    break;
                }

                z = z.modPow( valueOf( 2 ), this );
            }

            if ( i == b && !z.Equals( pMinus1 ) )
            {
                return false;
            }
        }

        return true;
    }

    private void setInvert()
    {
        if ( words == null )
        {
            ival = ~ival;
        }
        else
        {
            for ( int i = ival; --i >= 0; )
            {
                words[i] = ~words[i];
            }
        }
    }

    private void setShiftLeft( BigInteger x, int count )
    {
        int[] xwords;
        int xlen;

        if ( x.words == null )
        {
            if ( count < 32 )
            {
                set( ( long ) x.ival << count );
                return;
            }

            xwords = new int[1];
            xwords[ 0 ] = x.ival;
            xlen = 1;
        }
        else
        {
            xwords = x.words;
            xlen = x.ival;
        }

        int word_count = count >> 5;

        count &= 31;

        int new_len = xlen + word_count;

        if ( count == 0 )
        {
            realloc( new_len );

            for ( int i = xlen; --i >= 0; )
            {
                words[ i + word_count ] = xwords[i];
            }
        }
        else
        {
            new_len++;
            realloc( new_len );

            int shift_out = MPN.lshift( words, word_count, xwords, xlen, count );

            count = 32 - count;
            words[ new_len - 1 ] = ( shift_out << count ) >> count;
        }

        ival = new_len;

        for ( int i = word_count; --i >= 0; )
        {
            words[i] = 0;
        }
    }

    private void setShiftRight( BigInteger x, int count )
    {
        if ( x.words == null )
        {
            set( count < 32 ? x.ival >> count : x.ival < 0 ? -1 : 0 );
        }
        else if ( count == 0 )
        {
            set(x);
        }
        else
        {
            bool neg = x.Negative;
            int word_count = count >> 5;
            count &= 31;

            int d_len = x.ival - word_count;

            if ( d_len <= 0 )
            {
                set( neg ? -1 : 0 );
            }
            else
            {
                if ( words == null || words.Length < d_len )
                {
                    realloc( d_len );
                }

                MPN.rshift0( words, x.words, word_count, d_len, count );

                ival = d_len;

                if ( neg )
                {
                    words[ d_len - 1 ] |= -2 << ( 31 - count );
                }
            }
        }
    }

    private void setShift( BigInteger x, int count )
    {
        if ( count > 0 )
        {
            setShiftLeft( x, count );
        }
        else
        {
            setShiftRight( x, -count );
        }
    }

    private static BigInteger shift( BigInteger x, int count )
    {
        if ( x.words == null )
        {
            if ( count <= 0 )
            {
                return valueOf( count > -32 ? x.ival >> ( -count ) : x.ival < 0 ? -1 : 0 );
            }

            if ( count < 32 )
            {
                return valueOf( ( long ) x.ival << count );
            }
        }

        if ( count == 0 )
        {
            return x;
        }

        var result = new BigInteger(0);

        result.setShift( x, count );

        return result.canonicalize();
    }

    public virtual BigInteger shiftLeft( int n )
    {
        return shift( this, n );
    }

    public virtual BigInteger shiftRight( int n )
    {
        return shift( this, -n );
    }

    private void format( int radix, StringBuilder buffer )
    {
        if ( words == null )
        {
            buffer.Append( Convert.ToString( ival, radix ) );
        }
        else if ( ival <= 2 )
        {
            buffer.Append( Convert.ToString( longValue(), radix ) );
        }
        else
        {
            bool neg = Negative;
            int[] work;

            if ( neg || radix != 16 )
            {
                work = new int[ ival ];
                getAbsolute( work );
            }
            else
            {
                work = words;
            }

            int len = ival;

            if ( radix == 16 )
            {
                if ( neg )
                {
                    buffer.Append( '-' );
                }

                int buf_start = buffer.Length;

                for ( int i = len; --i >= 0; )
                {
                    int word = work[i];

                    for ( int j = 8; --j >= 0; )
                    {
                        int hex_digit = ( word >> ( 4 * j ) ) & 0xF;

                        if ( hex_digit > 0 || buffer.Length > buf_start )
                        {
                            buffer.Append( Character_forDigit( hex_digit, 16 ) );
                        }
                    }
                }
            }
            else
            {
                int i = buffer.Length;

                for ( ; ; )
                {
                    int digit = MPN.divmod_1( work, work, len, radix );

                    buffer.Append( Character_forDigit( digit, radix ) );

                    while ( len > 0 && work[ len - 1 ] == 0 )
                    {
                        len--;
                    }

                    if ( len == 0 )
                    {
                        break;
                    }
                }

                if ( neg )
                {
                    buffer.Append( '-' );
                }

                int j = buffer.Length - 1;

                while ( i < j )
                {
                    char tmp = buffer[i];

                    buffer[i] = buffer[j];
                    buffer[j] = tmp;

                    i++;
                    j--;
                }
            }
        }
    }

    public override string ToString()
    {
        return ToString( 10 );
    }

    public virtual string ToString( int radix )
    {
        if ( words == null )
        {
            return Convert.ToString( ival, radix );
        }

        if ( ival <= 2 )
        {
            return Convert.ToString( longValue(), radix );
        }

        int buf_size = ival * ( MPN.chars_per_word( radix ) + 1 );

        var buffer = new StringBuilder( buf_size );

        format( radix, buffer );

        return buffer.ToString();
    }

    public virtual int intValue()
    {
        if ( words == null )
        {
            return ival;
        }

        return words[0];
    }

    public virtual long longValue()
    {
        if ( words == null )
        {
            return ival;
        }

        if ( ival == 1 )
        {
            return words[ 0 ];
        }

        return ( ( long ) words[ 1 ] << 32 ) + ( ( long ) words[ 0 ] & 0xffffffffL );
    }

    public override int GetHashCode()
    {
        return words == null ? ival : ( words[0] + words[ ival - 1 ] );
    }

    private static bool Equals( BigInteger x, BigInteger y )
    {
        if ( x.words == null && y.words == null )
        {
            return x.ival == y.ival;
        }

        if ( x.words == null || y.words == null || x.ival != y.ival )
        {
            return false;
        }

        for ( int i = x.ival; --i >= 0; )
        {
            if ( x.words[i] != y.words[i] )
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals( object obj )
    {
        if ( !( obj is BigInteger ) )
        {
            return false;
        }

        return Equals( this, ( BigInteger ) obj );
    }

    private static BigInteger valueOf( string s, int radix )
    {
        int len = s.Length;

        if ( len <= 15 && radix <= 16 )
        {
            return valueOf( Convert.ToInt64( s, radix ) );
        }

        int byte_len = 0;

        sbyte[] bytes = new sbyte[ len ];

        bool negative = false;

        for ( int i = 0; i < len; i++ )
        {
            char ch = s[i];

            if ( ch == '-' )
            {
                negative = true;
            }
            else if ( ch == '_' || ( byte_len == 0 && ( ch == ' ' || ch == '\t' ) ) )
            {
                continue;
            }
            else
            {
                int digit = Convert.ToInt32( ch.ToString(), radix );

                if ( digit < 0 )
                {
                    break;
                }

                bytes[ byte_len++ ] = ( sbyte ) digit;
            }
        }

        return valueOf( bytes, byte_len, negative, radix );
    }

    private static BigInteger valueOf( sbyte[] digits, int byte_len, bool negative, int radix )
    {
        int chars_per_word = MPN.chars_per_word( radix );

        int[] words = new int[ byte_len / chars_per_word + 1 ];

        int size = MPN.set_str( words, digits, byte_len, radix );

        if ( size == 0 )
        {
            return ZERO;
        }

        if ( words[ size - 1 ] < 0 )
        {
            words[ size++ ] = 0;
        }

        if ( negative )
        {
            negate( words, words, size );
        }

        return make( words, size );
    }

    public virtual double doubleValue()
    {
        if ( words == null )
        {
            return ( double ) ival;
        }

        if ( ival <= 2 )
        {
            return ( double ) longValue();
        }

        if ( Negative )
        {
            return neg( this ).roundToDouble( 0, true, false );
        }

        return roundToDouble( 0, false, false );
    }

    public virtual float floatValue()
    {
        return ( float ) doubleValue();
    }

    private bool checkBits( int n )
    {
        if ( n <= 0 )
        {
            return false;
        }

        if ( words == null )
        {
            return n > 31 || ( ( ival & ( ( 1 << n ) - 1 ) ) != 0 );
        }

        int i;

        for ( i = 0; i < ( n >> 5 ); i++ )
        {
            if ( words[i] != 0 )
            {
                return true;
            }
        }

        return ( n & 31 ) != 0 && ( words[ i ] & ( ( 1 << ( n & 31 ) ) - 1 ) ) != 0;
    }

    private double roundToDouble( int exp, bool neg, bool remainder )
    {
        int il = bitLength();

        exp += il - 1;

        if ( exp < -1075 )
        {
            return neg ? -0.0 : 0.0;
        }

        if ( exp > 1023 )
        {
            return neg ? double.NegativeInfinity : double.PositiveInfinity;
        }

        int ml = ( exp >= -1022 ? 53 : 53 + exp + 1022 );

        long m;

        int excess_bits = il - ( ml + 1 );

        if ( excess_bits > 0 )
        {
            m = ( ( words == null ) ? ival >> excess_bits : MPN.rshift_long( words, ival, excess_bits ) );
        }
        else
        {
            m = longValue() << ( -excess_bits );
        }

        if ( exp == 1023 && ( ( m >> 1 ) == ( 1L << 53 ) - 1 ) )
        {
            if ( remainder || checkBits( il - ml ) )
            {
                return neg ? double.NegativeInfinity : double.PositiveInfinity;
            }
            else
            {
                return neg ? -double.MaxValue : double.MaxValue;
            }
        }

        if ( ( m & 1 ) == 1 && ( ( m & 2 ) == 2 || remainder || checkBits( excess_bits ) ) )
        {
            m += 2;

            if ( ( m & ( 1L << 54 ) ) != 0 )
            {
                exp++;
                m >>= 1;
            }
            else if ( ml == 52 && ( m & ( 1L << 53 ) ) != 0 )
            {
                exp++;
            }
        }

        m >>= 1;

        long bits_sign = neg ? ( 1L << 63 ) : 0;

        exp += 1023;

        long bits_exp = ( exp <= 0 ) ? 0 : ( ( long ) exp ) << 52;
        long bits_mant = m & ~( 1L << 52 );

        return BitConverter.Int64BitsToDouble( bits_sign | bits_exp | bits_mant );
    }

    private void getAbsolute( int[] words )
    {
        int len;

        if ( this.words == null )
        {
            len = 1;
            words[0] = this.ival;
        }
        else
        {
            len = this.ival;

            for ( int i = len; --i >= 0; )
            {
                words[ i ] = this.words[ i ];
            }
        }

        if ( words[ len - 1 ] < 0 )
        {
            negate( words, words, len );
        }

        for ( int i = words.Length; --i > len; )
        {
            words[ i ] = 0;
        }
    }

    private static bool negate( int[] dest, int[] src, int len )
    {
        long carry = 1;
        bool negative = src[ len - 1 ] < 0;

        for ( int i = 0; i < len; i++ )
        {
            carry += ( ( long ) ( ~src[i] ) & 0xffffffffL );
            dest[i] = ( int ) carry;
            carry >>= 32;
        }

        return ( negative && dest[ len - 1 ] < 0 );
    }

    private void setNegative( BigInteger x )
    {
        int len = x.ival;

        if ( x.words == null )
        {
            if ( len == int.MinValue )
            {
                set( -( long ) len );
            }
            else
            {
                set( -len );
            }

            return;
        }

        realloc( len + 1 );

        if ( negate( words, x.words, len ) )
        {
            words[ len++ ] = 0;
        }

        ival = len;
    }

    private void setNegative()
    {
        setNegative( this );
    }

    private static BigInteger abs( BigInteger x )
    {
        return x.Negative ? neg( x ) : x;
    }

    public virtual BigInteger abs()
    {
        return abs( this );
    }

    private static BigInteger neg( BigInteger x )
    {
        if ( x.words == null && x.ival != int.MinValue )
        {
            return valueOf( -x.ival );
        }

        var result = new BigInteger(0);

        result.setNegative(x);

        return result.canonicalize();
    }

    public virtual BigInteger negate()
    {
        return neg( this );
    }

    public virtual int bitLength()
    {
        if ( words == null )
        {
            return MPN.intLength( ival );
        }

        return MPN.intLength( words, ival );
    }

    public virtual sbyte[] toByteArray()
    {
        sbyte[] bytes = new sbyte[ ( bitLength() + 1 + 7 ) / 8 ];
        int nbytes = bytes.Length;
        int wptr = 0;
        int word;

        while ( nbytes > 4 )
        {
            word = words[ wptr++ ];

            for ( int i = 4; i > 0; --i, word >>= 8 )
            {
                bytes[ --nbytes ] = ( sbyte ) word;
            }
        }

        word = ( words == null ) ? ival : words[ wptr ];

        for ( ; nbytes > 0; word >>= 8 )
        {
            bytes[ --nbytes ] = ( sbyte ) word;
        }

        return bytes;
    }

    private static int swappedOp( int op )
    {
        return "\x0000\x0001\x0004\x0005\x0002\x0003\x0006\x0007\x0008\x0009\x000C\x000D\x000A\x000B\x000E\x000F"[ op ];
    }

    private static BigInteger bitOp( int op, BigInteger x, BigInteger y )
    {
        switch ( op )
        {
            case 0:
                return ZERO;
            case 1:
                return x.and( y );
            case 3:
                return x;
            case 5:
                return y;
            case 15:
                return valueOf( -1 );
        }

        var result = new BigInteger();

        setBitOp( result, op, x, y );

        return result.canonicalize();
    }

    private static void setBitOp( BigInteger result, int op, BigInteger x, BigInteger y )
    {
        if ( y.words == null )
        {
            ;
        }
        else if ( x.words == null || x.ival < y.ival )
        {
            var temp = x;
            x = y;
            y = temp;
            op = swappedOp( op );
        }
        int xi;
        int yi;
        int xlen, ylen;
        if ( y.words == null )
        {
            yi = y.ival;
            ylen = 1;
        }
        else
        {
            yi = y.words[ 0 ];
            ylen = y.ival;
        }
        if ( x.words == null )
        {
            xi = x.ival;
            xlen = 1;
        }
        else
        {
            xi = x.words[ 0 ];
            xlen = x.ival;
        }
        if ( xlen > 1 )
        {
            result.realloc( xlen );
        }
        int[] w = result.words;
        int i = 0;
        int finish = 0;
        int ni;
        switch ( op )
        {
            case 0:
                ni = 0;
                break;
            case 1:
                for ( ; ; )
                {
                    ni = xi & yi;
                    if ( i + 1 >= ylen )
                    {
                        break;
                    }
                    w[ i++ ] = ni;
                    xi = x.words[ i ];
                    yi = y.words[ i ];
                }
                if ( yi < 0 )
                {
                    finish = 1;
                }
                break;
            case 2:
                for ( ; ; )
                {
                    ni = xi & ~yi;
                    if ( i + 1 >= ylen )
                    {
                        break;
                    }
                    w[ i++ ] = ni;
                    xi = x.words[ i ];
                    yi = y.words[ i ];
                }
                if ( yi >= 0 )
                {
                    finish = 1;
                }
                break;
            case 3:
                ni = xi;
                finish = 1;
                break;
            case 4:
                for ( ; ; )
                {
                    ni = ~xi & yi;
                    if ( i + 1 >= ylen )
                    {
                        break;
                    }
                    w[ i++ ] = ni;
                    xi = x.words[ i ];
                    yi = y.words[ i ];
                }
                if ( yi < 0 )
                {
                    finish = 2;
                }
                break;
            case 5:
                for ( ; ; )
                {
                    ni = yi;
                    if ( i + 1 >= ylen )
                    {
                        break;
                    }
                    w[ i++ ] = ni;
                    xi = x.words[ i ];
                    yi = y.words[ i ];
                }
                break;
            case 6:
                for ( ; ; )
                {
                    ni = xi ^ yi;
                    if ( i + 1 >= ylen )
                    {
                        break;
                    }
                    w[ i++ ] = ni;
                    xi = x.words[ i ];
                    yi = y.words[ i ];
                }
                finish = yi < 0 ? 2 : 1;
                break;
            case 7:
                for ( ; ; )
                {
                    ni = xi | yi;
                    if ( i + 1 >= ylen )
                    {
                        break;
                    }
                    w[ i++ ] = ni;
                    xi = x.words[ i ];
                    yi = y.words[ i ];
                }
                if ( yi >= 0 )
                {
                    finish = 1;
                }
                break;
            case 8:
                for ( ; ; )
                {
                    ni = ~( xi | yi );
                    if ( i + 1 >= ylen )
                    {
                        break;
                    }
                    w[ i++ ] = ni;
                    xi = x.words[ i ];
                    yi = y.words[ i ];
                }
                if ( yi >= 0 )
                {
                    finish = 2;
                }
                break;
            case 9:
                for ( ; ; )
                {
                    ni = ~( xi ^ yi );
                    if ( i + 1 >= ylen )
                    {
                        break;
                    }
                    w[ i++ ] = ni;
                    xi = x.words[ i ];
                    yi = y.words[ i ];
                }
                finish = yi >= 0 ? 2 : 1;
                break;
            case 10:
                for ( ; ; )
                {
                    ni = ~yi;
                    if ( i + 1 >= ylen )
                    {
                        break;
                    }
                    w[ i++ ] = ni;
                    xi = x.words[ i ];
                    yi = y.words[ i ];
                }
                break;
            case 11:
                for ( ; ; )
                {
                    ni = xi | ~yi;
                    if ( i + 1 >= ylen )
                    {
                        break;
                    }
                    w[ i++ ] = ni;
                    xi = x.words[ i ];
                    yi = y.words[ i ];
                }
                if ( yi < 0 )
                {
                    finish = 1;
                }
                break;
            case 12:
                ni = ~xi;
                finish = 2;
                break;
            case 13:
                for ( ; ; )
                {
                    ni = ~xi | yi;
                    if ( i + 1 >= ylen )
                    {
                        break;
                    }
                    w[ i++ ] = ni;
                    xi = x.words[ i ];
                    yi = y.words[ i ];
                }
                if ( yi >= 0 )
                {
                    finish = 2;
                }
                break;
            case 14:
                for ( ; ; )
                {
                    ni = ~( xi & yi );
                    if ( i + 1 >= ylen )
                    {
                        break;
                    }
                    w[ i++ ] = ni;
                    xi = x.words[ i ];
                    yi = y.words[ i ];
                }
                if ( yi < 0 )
                {
                    finish = 2;
                }
                break;
            default:
                goto case 15;
            case 15:
                ni = -1;
                break;
        }
        if ( i + 1 == xlen )
        {
            finish = 0;
        }
        switch ( finish )
        {
            case 0:
                if ( i == 0 && w == null )
                {
                    result.ival = ni;
                    return;
                }
                w[ i++ ] = ni;
                break;
            case 1:
                w[ i ] = ni;
                while ( ++i < xlen )
                {
                    w[ i ] = x.words[ i ];
                }
                break;
            case 2:
                w[ i ] = ni;
                while ( ++i < xlen )
                {
                    w[ i ] = ~x.words[ i ];
                }
                break;
        }
        result.ival = i;
    }

    private static BigInteger and( BigInteger x, int y )
    {
        if ( x.words == null )
        {
            return valueOf( x.ival & y );
        }

        if ( y >= 0 )
        {
            return valueOf( x.words[ 0 ] & y );
        }

        int len = x.ival;
        int[] words = new int[ len ];

        words[ 0 ] = x.words[ 0 ] & y;

        while ( --len > 0 )
        {
            words[ len ] = x.words[ len ];
        }

        return make( words, x.ival );
    }

    public virtual BigInteger and( BigInteger y )
    {
        if ( y.words == null )
        {
            return and( this, y.ival );
        }

        if ( this.words == null )
        {
            return and( y, ival );
        }

        var x = this;

        if ( ival < y.ival )
        {
            var temp = this;
            x = y;
            y = temp;
        }

        int i;
        int len = y.Negative ? x.ival : y.ival;
        int[] words = new int[ len ];

        for ( i = 0; i < y.ival; i++ )
        {
            words[i] = x.words[i] & y.words[i];
        }

        for ( ; i < len; i++ )
        {
            words[i] = x.words[i];
        }

        return make( words, len );
    }

    public virtual BigInteger or( BigInteger y )
    {
        return bitOp( 7, this, y );
    }

    public virtual BigInteger xor( BigInteger y )
    {
        return bitOp( 6, this, y );
    }

    public virtual BigInteger not()
    {
        return bitOp( 12, this, ZERO );
    }

    public virtual BigInteger andNot( BigInteger val )
    {
        return and( val.not() );
    }

    public virtual BigInteger clearBit( int n )
    {
        if ( n < 0 )
        {
            throw new ArithmeticException();
        }

        return and( ONE.shiftLeft(n).not() );
    }

    public virtual BigInteger setBit( int n )
    {
        if ( n < 0 )
        {
            throw new ArithmeticException();
        }

        return or( ONE.shiftLeft(n) );
    }

    public virtual bool testBit( int n )
    {
        if ( n < 0 )
        {
            throw new ArithmeticException();
        }

        return !and( ONE.shiftLeft(n) ).Zero;
    }

    public virtual BigInteger flipBit( int n )
    {
        if ( n < 0 )
        {
            throw new ArithmeticException();
        }

        return xor( ONE.shiftLeft(n) );
    }

    public virtual int LowestSetBit
    {
        get
        {
            if ( Zero )
            {
                return -1;
            }

            if ( words == null )
            {
                return MPN.findLowestBit( ival );
            }
            else
            {
                return MPN.findLowestBit( words );
            }
        }
    }

    private static readonly sbyte[] bit4_count = { 0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4 };

    private static int bitCount( int i )
    {
        int count = 0;

        while ( i != 0 )
        {
            count += bit4_count[ i & 15 ];
            i = ( int ) ( ( uint ) i >> 4 );
        }

        return count;
    }

    private static int bitCount( int[] x, int len )
    {
        int count = 0;

        while ( --len >= 0 )
        {
            count += bitCount( x[ len ] );
        }

        return count;
    }

    public virtual int bitCount()
    {
        int i, x_len;
        int[] x_words = words;

        if ( x_words == null )
        {
            x_len = 1;
            i = bitCount( ival );
        }
        else
        {
            x_len = ival;
            i = bitCount( x_words, x_len );
        }

        return Negative ? x_len * 32 - i : i;
    }
}

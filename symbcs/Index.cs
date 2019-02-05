using System.Collections;

public class Index
{
    internal int[] row;
    internal int[] col;
    internal int row_max, col_max;
    public Index( int[] row, int[] col )
    {
        this.row = row;
        this.col = col;
        row_max = maxint( row );
        col_max = maxint( col );
    }
    public Index( int row, int col, Algebraic x )
    {
        int width = 1, height = 1;
        if ( x is Vektor )
        {
            width = ( ( Vektor ) x ).length();
        }
        else if ( x is Matrix )
        {
            width = ( ( Matrix ) x ).nrow();
            height = ( ( Matrix ) x ).ncol();
        }
        this.row = series( row, row + height - 1 );
        this.col = series( col, col + width - 1 );
        row_max = maxint( this.row );
        col_max = maxint( this.col );
    }
    private int maxint( int[] c )
    {
        int max = c[ 0 ];
        for ( int i = 1; i < c.Length; i++ )
        {
            if ( c[ i ] > max )
            {
                max = c[ i ];
            }
        }
        return max;
    }
    public override string ToString()
    {
        string s = "Index = \nRows: ";
        for ( int i = 0; i < row.Length; i++ )
        {
            s += "  " + row[ i ];
        }
        s += "\nColumns: ";
        for ( int k = 0; k < col.Length; k++ )
        {
            s += "  " + col[ k ];
        }
        return s;
    }
    //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
    //ORIGINAL LINE: public static Index createIndex(Algebraic idx_in, Matrix x) throws JasymcaException
    public static Index createIndex( Algebraic idx_in, Matrix x )
    {
        if ( !idx_in.constantq() )
        {
            throw new JasymcaException( "Index not constant: " + idx_in );
        }
        idx_in = idx_in.reduce();
        if ( idx_in is Zahl && ( ( Zahl ) idx_in ).integerq() )
        {
            int[] row = new int[] { 1 };
            int[] col = new int[] { ( ( Zahl ) idx_in ).intval() };
            return new Index( row, col );
        }
        else if ( idx_in is Vektor && ( ( Vektor ) idx_in ).length() == 2 )
        {
            Algebraic r = ( ( Vektor ) idx_in ).get( 0 );
            Algebraic c = ( ( Vektor ) idx_in ).get( 1 );
            if ( r is Zahl && ( ( Zahl ) r ).integerq() && c is Zahl && ( ( Zahl ) c ).integerq() )
            {
                int[] row = new int[] { ( ( Zahl ) r ).intval() };
                int[] col = new int[] { ( ( Zahl ) c ).intval() };
                return new Index( row, col );
            }
        }
        throw new JasymcaException( "Not a legel index: " + idx_in );
    }
    //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
    //ORIGINAL LINE: public static Index createIndex(Stack st, Matrix x) throws ParseException, JasymcaException
    public static Index createIndex( Stack st, Matrix x )
    {
        int[] row, col;
        int length = Lambda.getNarg( st );
        object rdx, cdx;
        if ( length > 1 )
        {
            rdx = st.Pop();
            if ( ":".Equals( rdx ) )
            {
                row = series( 1, x.nrow() );
            }
            else
            {
                row = setseries( ( Algebraic ) rdx );
            }
            cdx = st.Pop();
        }
        else
        {
            cdx = st.Pop();
            row = new int[ 1 ];
            row[ 0 ] = 1;
        }
        if ( ":".Equals( cdx ) )
        {
            col = series( 1, x.ncol() );
        }
        else
        {
            col = setseries( ( Algebraic ) cdx );
        }
        return new Index( row, col );
    }
    //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
    //ORIGINAL LINE: static int[] setseries(Algebraic c) throws ParseException, JasymcaException
    internal static int[] setseries( Algebraic c )
    {
        int[] s;
        if ( c is Zahl && ( ( Zahl ) c ).integerq() )
        {
            s = new int[ 1 ];
            s[ 0 ] = ( ( Zahl ) c ).intval();
        }
        else if ( c is Vektor )
        {
            s = new int[ ( ( Vektor ) c ).length() ];
            for ( int i = 0; i < s.Length; i++ )
            {
                Algebraic a = ( ( Vektor ) c ).get( i );
                if ( a is Zahl && ( ( Zahl ) a ).integerq() )
                {
                    s[ i ] = ( ( Zahl ) a ).intval();
                }
                else
                {
                    throw new ParseException( "Not a legal index: " + a );
                }
            }
        }
        else
        {
            throw new ParseException( "Not a legal index: " + c );
        }
        return s;
    }
    internal static int[] series( int a, int b )
    {
        int[] c = new int[ b + 1 - a ];
        for ( int i = 0; i < c.Length; i++ )
        {
            c[ i ] = a + i;
        }
        return c;
    }
}
internal class REFX : Lambda
{
    //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
    //ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
    public override int lambda( Stack st )
    {
        int narg = getNarg( st );
        Algebraic x = getAlgebraic( st );
        Algebraic index_in = CreateVector.crv( st );
        if ( index_in.constantq() )
        {
            Matrix mx = new Matrix( ( Algebraic ) x );
            Index idx = Index.createIndex( index_in, mx );
            mx = mx.extract( idx );
            x = mx.reduce();
        }
        else
        {
            MatRef mr = new MatRef( ( Algebraic ) x );
            x = new Polynomial( new FunctionVariable( "MR(" + x + ")", index_in, mr ) );
        }
        st.Push( x );
        return 0;
    }
}
internal class REFM : Lambda
{
    //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
    //ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
    public override int lambda( Stack st )
    {
        int narg = getNarg( st );
        Algebraic x = getAlgebraic( st );
        Matrix mx = new Matrix( ( Algebraic ) x );
        Index idx = Index.createIndex( st, mx );
        mx = mx.extract( idx );
        st.Push( mx.reduce() );
        return 0;
    }
}
internal class MatRef : LambdaAlgebraic
{
    internal Matrix mx;
    public MatRef( Algebraic x )
    {
        this.mx = new Matrix( x );
    }
    //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
    //ORIGINAL LINE: Algebraic f_exakt(Algebraic x) throws JasymcaException
    internal override Algebraic f_exakt( Algebraic x )
    {
        if ( x.constantq() )
        {
            Index idx = Index.createIndex( x, mx );
            Matrix m = mx.extract( idx );
            return m.reduce();
        }
        return null;
    }
}
using System.Collections;
using System.Linq;

namespace Tiny.Science.Symbolic
{
    public class Index
    {

        #region Internal fields

        internal int row_max, col_max;
        internal int[] row;
        internal int[] col;

        #endregion

        #region Constructors

        public Index( int[] row, int[] col )
        {
            this.row = row;
            this.col = col;

            row_max = row.Max();
            col_max = col.Max();
        }

        public Index( int row, int col, Algebraic x )
        {
            int width = 1, height = 1;

            if ( x is Vector )
            {
                width = ( ( Vector ) x ).Length();
            }
            else if ( x is Matrix )
            {
                width = ( ( Matrix ) x ).Rows();
                height = ( ( Matrix ) x ).Cols();
            }

            this.row = series( row, row + height - 1 );
            this.col = series( col, col + width - 1 );

            row_max = this.row.Max();
            col_max = this.col.Max();
        }

        #endregion

        public override string ToString()
        {
            var s = row.Aggregate( "Index = \nRows: ", ( current, t ) => current + ( "  " + t ) );

            s += "\nColumns: ";

            return col.Aggregate( s, ( current, t ) => current + ( "  " + t ) );
        }

        public static Index createIndex( Algebraic indx, Matrix x )
        {
            if ( !indx.IsConstant() )
            {
                throw new SymbolicException( "Index not constant: " + indx );
            }

            indx = indx.Reduce();

            if ( indx is Symbol && ( ( Symbol ) indx ).IsInteger() )
            {
                int[] row = { 1 };
                int[] col = { ( ( Symbol ) indx ).ToInt() };

                return new Index( row, col );
            }
            else if ( indx is Vector && ( ( Vector ) indx ).Length() == 2 )
            {
                var r = ( ( Vector ) indx )[0];
                var c = ( ( Vector ) indx )[1];

                if ( r is Symbol && ( ( Symbol ) r ).IsInteger() && c is Symbol && ( ( Symbol ) c ).IsInteger() )
                {
                    int[] row = { ( ( Symbol ) r ).ToInt() };
                    int[] col = { ( ( Symbol ) c ).ToInt() };

                    return new Index( row, col );
                }
            }

            throw new SymbolicException( "Not a legel index: " + indx );
        }

        public static Index createIndex( Stack stack, Matrix x )
        {
            int[] row, col;

            int length = Lambda.GetNarg( stack );

            object rdx, cdx;

            if ( length > 1 )
            {
                rdx = stack.Pop();

                row = rdx.Equals( ":" ) ? series( 1, x.Rows() ) : setseries( ( Algebraic ) rdx );

                cdx = stack.Pop();
            }
            else
            {
                cdx = stack.Pop();

                row = new int[1];

                row[0] = 1;
            }

            col = cdx.Equals( ":" ) ? series( 1, x.Cols() ) : setseries( ( Algebraic ) cdx );

            return new Index( row, col );
        }

        internal static int[] setseries( Algebraic c )
        {
            int[] s;

            if ( c is Symbol && ( ( Symbol ) c ).IsInteger() )
            {
                s = new int[1];

                s[0] = ( ( Symbol ) c ).ToInt();
            }
            else if ( c is Vector )
            {
                s = new int[ ( ( Vector ) c ).Length() ];

                for ( int i = 0; i < s.Length; i++ )
                {
                    var a = ( ( Vector ) c )[i];

                    if ( a is Symbol && ( ( Symbol ) a ).IsInteger() )
                    {
                        s[i] = ( ( Symbol ) a ).ToInt();
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
                c[i] = a + i;
            }

            return c;
        }
    }


    public class REFX : Lambda
    {
        public override int Eval( Stack st )
        {
            int narg = GetNarg( st );

            var x = GetAlgebraic( st );

            var index_in = CreateVector.Create( st );

            if ( index_in.IsConstant() )
            {
                var mx = new Matrix( ( Algebraic ) x );

                var idx = Index.createIndex( index_in, mx );

                mx = mx.Extract( idx );
                x = mx.Reduce();
            }
            else
            {
                var mr = new MatRef( ( Algebraic ) x );

                x = new Polynomial( new FunctionVariable( "MR(" + x + ")", index_in, mr ) );
            }

            st.Push(x);

            return 0;
        }
    }

    public class REFM : Lambda
    {
        public override int Eval( Stack st )
        {
            int narg = GetNarg( st );
            var x = GetAlgebraic( st );

            var mx = new Matrix( ( Algebraic ) x );
            var idx = Index.createIndex( st, mx );

            mx = mx.Extract( idx );
            st.Push( mx.Reduce() );

            return 0;
        }
    }

    internal class MatRef : LambdaAlgebraic
    {
        internal Matrix mx;

        public MatRef( Algebraic x )
        {
            mx = new Matrix(x);
        }

        internal override Algebraic SymEval( Algebraic x )
        {
            if ( x.IsConstant() )
            {
                var idx = Index.createIndex( x, mx );
                var m = mx.Extract( idx );

                return m.Reduce();
            }

            return null;
        }
    }
}

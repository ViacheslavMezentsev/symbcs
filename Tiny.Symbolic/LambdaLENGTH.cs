using System.Collections;

using Math = Tiny.Numeric.Math;

namespace Tiny.Symbolic
{
    internal class LambdaLENGTH : Lambda
    {
        public override int Eval( Stack st )
        {
            int narg = GetNarg( st );
            Algebraic x = GetAlgebraic( st );
            if ( x.IsScalar() && !x.IsConstant() )
            {
                throw new SymbolicException( "Unknown variable dimension: " + x );
            }
            Matrix m = new Matrix( x );
            st.Push( new Complex( ( double ) Math.max( m.Cols(), m.Rows() ) ) );
            return 0;
        }
    }
    internal class LambdaPROD : Lambda
    {
        public override int Eval( Stack st )
        {
            int narg = GetNarg( st );
            Algebraic x = GetAlgebraic( st );
            if ( x.IsScalar() && !x.IsConstant() )
            {
                throw new SymbolicException( "Unknown variable dimension: " + x );
            }
            Matrix mx = new Matrix( x );
            Algebraic s = mx.col( 1 );
            for ( int i = 2; i <= mx.Cols(); i++ )
            {
                s = s * mx.col( i );
            }
            st.Push( s );
            return 0;
        }
    }
    internal class LambdaSIZE : Lambda
    {
        public override int Eval( Stack st )
        {
            int narg = GetNarg( st );
            Algebraic x = GetAlgebraic( st );
            if ( x.IsScalar() && !x.IsConstant() )
            {
                throw new SymbolicException( "Unknown variable dimension: " + x );
            }
            Matrix mx = new Matrix( x );
            Complex nr = new Complex( ( double ) mx.Rows() ), nc = new Complex( ( double ) mx.Cols() );
            if ( length == 2 )
            {
                st.Push( nr );
                st.Push( nc );
                length = 1;
            }
            else
            {
                st.Push( new Vector( new Algebraic[] { nr, nc } ) );
            }
            return 0;
        }
    }
    internal class LambdaMIN : Lambda
    {
        public override int Eval( Stack st )
        {
            int narg = GetNarg( st );
            Algebraic x = GetAlgebraic( st );
            Matrix mx;
            if ( x is Vector )
            {
                mx = Matrix.Column( ( Vector ) x );
            }
            else
            {
                mx = new Matrix( x );
            }
            st.Push( mx.min() );
            return 0;
        }
    }
    internal class LambdaMAX : Lambda
    {
        public override int Eval( Stack st )
        {
            int narg = GetNarg( st );
            Algebraic x = GetAlgebraic( st );
            Matrix mx;
            if ( x is Vector )
            {
                mx = Matrix.Column( ( Vector ) x );
            }
            else
            {
                mx = new Matrix( x );
            }
            st.Push( mx.max() );
            return 0;
        }
    }
    internal class LambdaFIND : Lambda
    {
        public override int Eval( Stack st )
        {
            int narg = GetNarg( st );
            Algebraic x = GetAlgebraic( st );
            Matrix mx = new Matrix( x );
            st.Push( mx.find() );
            return 0;
        }
    }
}

using System;
using System.Collections;

namespace Tiny.Science.Symbolic
{
    public abstract class LambdaAlgebraic : Lambda
    {
        public override int Eval( Stack stack )
        {
            int narg = GetNarg( stack );

            switch ( narg )
            {
                case 0:
                    throw new SymbolicException( "Lambda functions expect argument." );

                case 1:
                    var arg = GetAlgebraic( stack );

                    stack.Push( arg.Map( this, null ) );

                    break;

                case 2:
                    var arg2 = GetAlgebraic( stack );
                    var arg1 = GetAlgebraic( stack );

                    arg1 = arg1.Promote( arg2 );

                    stack.Push( arg1.Map( this, arg2 ) );

                    break;

                default:

                    var args = new Algebraic[ narg ];

                    for ( int i = narg - 1; i >= 0; i-- )
                    {
                        args[ i ] = GetAlgebraic( stack );
                    }

                    stack.Push( SymEval( args ) );

                    break;
            }

            return 0;
        }

        internal virtual Symbol PreEval( Symbol s )
        {
            return s;
        }

        internal virtual Algebraic SymEval( Algebraic a )
        {
            return null;
        }

        internal virtual Algebraic SymEval( Symbol sx, Symbol sy )
        {
            return SymEval( sx as Algebraic, sy );
        }

        internal virtual Algebraic SymEval( Algebraic ax, Algebraic ay )
        {
            return null;
        }

        internal virtual Algebraic SymEval( Algebraic[] ax )
        {
            return null;
        }

        public virtual Algebraic Integrate( Algebraic va, Variable vx )
        {
            if ( !va.Depends( vx ) )
            {
                throw new SymbolicException( "Expression in function does not depend on Variable." );
            }

            if ( !( va is Polynomial ) || ( ( Polynomial ) va ).Degree() != 1 || !( ( Polynomial ) va ).IsRat( vx ) || intrule == null )
            {
                throw new SymbolicException( "Can not integrate function " );
            }

            try
            {
                var y = evalx( intrule, va );

                return y / ( ( Polynomial ) va )[1];
            }
            catch ( Exception )
            {
                throw new SymbolicException( "Error integrating function" );
            }
        }
    }
}

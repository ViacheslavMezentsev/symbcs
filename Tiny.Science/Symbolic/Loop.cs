using System;
using System.Collections;
using System.Text;
using System.Threading;

using Tiny.Science.Engine;

namespace Tiny.Science.Symbolic
{
    internal class LambdaERROR : Lambda
    {
        public override int Eval( Stack stack )
        {
            LambdaPRINTF.printf( stack );

            return Processor.ERROR;
        }
    }

    internal class LambdaEVAL : Lambda
    {
        public override int Eval( Stack stack )
        {
            int narg = GetNarg( stack );

            var s_in = stack.Pop();

            if ( !( s_in is string ) )
            {
                throw new SymbolicException( "Argument to EVAL must be string." );
            }

            var pgm = Session.Parser.compile( ( string ) s_in );

            return Session.Proc.ProcessList( pgm, true );
        }
    }

    internal class LambdaBLOCK : Lambda
    {
        public override int Eval( Stack stack )
        {
            int narg = GetNarg( stack );

            var local = Session.Proc.Store.Clone();
            var code = GetList( stack );

            var ups = new Stack();

            int ret = UserProgram.process_block( code, ups, local, false );

            Session.Proc.Store.Update( local );

            if ( ret != Processor.ERROR && ups.Count > 0 )
            {
                var y = ups.Pop();

                stack.Push( y );
            }
            else
            {
                throw new SymbolicException( "Error processing block." );
            }

            return 0;
        }
    }

    internal class LambdaBRANCH : Lambda
    {
        public override int Eval( Stack stack )
        {
            int narg = GetNarg( stack ), sel;

            List cond, b_true, b_false;

            switch ( narg )
            {
                case 2:
                    cond = GetList( stack );
                    b_true = GetList( stack );
                    Session.Proc.ProcessList( cond, true );
                    sel = GetInteger( Session.Proc.Stack );

                    if ( sel == 1 )
                    {
                        return Session.Proc.ProcessList( b_true, true );
                    }
                    else if ( sel != 0 )
                    {
                        throw new SymbolicException( "Branch requires boolean type." );
                    }

                    break;

                case 3:

                    cond = GetList( stack );
                    b_true = GetList( stack );
                    b_false = GetList( stack );

                    Session.Proc.ProcessList( cond, true );

                    sel = GetInteger( Session.Proc.Stack );

                    if ( sel == 1 )
                    {
                        return Session.Proc.ProcessList( b_true, true );
                    }
                    else if ( sel == 0 )
                    {
                        return Session.Proc.ProcessList( b_false, true );
                    }
                    else
                    {
                        throw new SymbolicException( "Branch requires boolean type, got " + sel );
                    }
                default:
                    throw new SymbolicException( "Wrong number of arguments to branch." );
            }

            return 0;
        }
    }

    internal class LambdaFOR : Lambda
    {
        public override int Eval( Stack stack )
        {
            int narg = GetNarg( stack );

            var cond = GetList( stack );
            var body = GetList( stack );

            Session.Proc.ProcessList( cond, true );

            if ( Session.Proc.Stack.Count == 0 || !( Session.Proc.Stack.Peek() is Vector ) || ( ( Algebraic ) Session.Proc.Stack.Peek() ).Name == null )
            {
                throw new ParseException( "Wrong format in for-loop." );
            }

            var vals = ( Vector ) Session.Proc.Stack.Pop();

            for ( int i = 0; i < vals.Length(); i++ )
            {
                Session.Proc.Store.PutValue( vals.Name, vals[ i ] );

                int ret = Session.Proc.ProcessList( body, true );

                switch ( ret )
                {
                    case Processor.BREAK:
                        return 0;

                    case Processor.RETURN:
                    case Processor.EXIT:
                    case Processor.ERROR:
                        return ret;

                    case Processor.CONTINUE:
                        break;
                }
            }

            return 0;
        }
    }

    internal class LambdaXFOR : Lambda
    {
        public override int Eval( Stack stack )
        {
            int narg = GetNarg( stack );

            var cond = GetList( stack );
            var step_in = GetList( stack );
            var thru_in = GetList( stack );
            var body = GetList( stack );

            Session.Proc.ProcessList( cond, true );

            if ( Session.Proc.Stack.Count == 0 || !( Session.Proc.Stack.Peek() is Symbol ) || ( ( Algebraic ) Session.Proc.Stack.Peek() ).Name == null )
            {
                throw new ParseException( "Non-constant initializer in for loop." );
            }

            var x = ( Symbol ) Session.Proc.Stack.Pop();

            var xname = x.Name;

            Session.Proc.ProcessList( step_in, true );

            if ( Session.Proc.Stack.Count == 0 || !( Session.Proc.Stack.Peek() is Symbol ) )
            {
                throw new ParseException( "Step size must be constant." );
            }

            var step = ( Symbol ) Session.Proc.Stack.Pop();

            Session.Proc.ProcessList( thru_in, true );

            if ( Session.Proc.Stack.Count == 0 || !( Session.Proc.Stack.Peek() is Symbol ) )
            {
                throw new ParseException( "Wrong format in for-loop." );
            }

            var thru = ( Symbol ) Session.Proc.Stack.Pop();

            var pos = !step.Smaller( Symbol.ZERO );

            while ( true )
            {
                if ( ( pos ? thru.Smaller( x ) : x.Smaller( thru ) ) )
                {
                    break;
                }

                Session.Proc.Store.PutValue( xname, x );

                int ret = Session.Proc.ProcessList( body, true );

                switch ( ret )
                {
                    case Processor.BREAK:
                        return 0;

                    case Processor.RETURN:
                    case Processor.EXIT:
                    case Processor.ERROR:
                        return ret;

                    case Processor.CONTINUE:
                        break;
                }

                x = ( Symbol ) ( x + step );
            }

            return 0;
        }
    }

    internal class LambdaWHILE : Lambda
    {
        public override int Eval( Stack stack )
        {
            int narg = GetNarg( stack );

            var cond = GetList( stack );
            var body = GetList( stack );

            while ( true )
            {
                Session.Proc.ProcessList( cond, true );

                var c = ( Symbol ) Session.Proc.Stack.Pop();

                if ( Equals( c, Symbol.ZERO ) )
                {
                    break;
                }
                else if ( !Equals( c, Symbol.ONE ) )
                {
                    throw new SymbolicException( "Not boolean: " + c );
                }

                int ret = Session.Proc.ProcessList( body, true );

                switch ( ret )
                {
                    case Processor.BREAK:
                        return 0;

                    case Processor.RETURN:
                    case Processor.EXIT:
                    case Processor.ERROR:
                        return ret;

                    case Processor.CONTINUE:
                        break;
                }
            }

            return 0;
        }
    }

    internal class LambdaPRINTF : Lambda
    {
        public override int Eval( Stack st )
        {
            printf( st );
            return 0;
        }

        internal static void printf( Stack st )
        {
            int narg = GetNarg( st );

            var s_in = st.Pop();

            if ( !( s_in is string ) )
            {
                throw new SymbolicException( "Argument to PRINTF must be string." );
            }

            string fmt = ( string ) s_in;

            int idx, i = 1;
            var cs = "%f";

            while ( ( idx = fmt.IndexOf( cs, StringComparison.Ordinal ) ) != -1 && st.Count > 0 && narg-- > 1 )
            {
                var n = st.Pop();

                if ( n != null )
                {
                    var sb = new StringBuilder( fmt );

                    sb.Remove( idx, idx + cs.Length - idx );
                    sb.Insert( idx, n.ToString() );

                    fmt = sb.ToString();
                }
                else
                {
                    break;
                }
            }

            while ( ( idx = fmt.IndexOf( "\\n", StringComparison.Ordinal ) ) != -1 )
            {
                var sb = new StringBuilder( fmt );

                sb.Remove( idx, idx + "\\n".Length - idx );
                sb.Insert( idx, "\n" );

                fmt = sb.ToString();
            }

            Session.Proc.print( fmt );
        }
    }

    internal class LambdaPAUSE : Lambda
    {
        public override int Eval( Stack st )
        {
            int narg = GetNarg( st );

            int millis = Math.Abs( GetInteger( st ) );

            try
            {
                Thread.Sleep( millis );
            }
            catch ( Exception )
            {
            }

            return 0;
        }
    }
}

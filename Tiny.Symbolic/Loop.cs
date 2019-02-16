using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace Tiny.Symbolic
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

            var pgm = Globals.Parser.compile( ( string ) s_in );

            return Globals.Proc.process_list( pgm, true );
        }
    }

    internal class LambdaBLOCK : Lambda
    {
        public override int Eval( Stack stack )
        {
            int narg = GetNarg( stack );

            var local = Globals.Proc.Store.copy();
            var code = GetList( stack );

            var ups = new Stack();

            int ret = UserProgram.process_block( code, ups, local, false );

            Globals.Proc.Store.update( local );

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
                    Globals.Proc.process_list( cond, true );
                    sel = GetInteger( Globals.Proc.Stack );

                    if ( sel == 1 )
                    {
                        return Globals.Proc.process_list( b_true, true );
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

                    Globals.Proc.process_list( cond, true );

                    sel = GetInteger( Globals.Proc.Stack );

                    if ( sel == 1 )
                    {
                        return Globals.Proc.process_list( b_true, true );
                    }
                    else if ( sel == 0 )
                    {
                        return Globals.Proc.process_list( b_false, true );
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

            Globals.Proc.process_list( cond, true );

            if ( Globals.Proc.Stack.Count == 0 || !( Globals.Proc.Stack.Peek() is Vector ) || ( ( Algebraic ) Globals.Proc.Stack.Peek() ).Name == null )
            {
                throw new ParseException( "Wrong format in for-loop." );
            }

            var vals = ( Vector ) Globals.Proc.Stack.Pop();

            for ( int i = 0; i < vals.Length(); i++ )
            {
                Globals.Proc.Store.putValue( vals.Name, vals[ i ] );

                int ret = Globals.Proc.process_list( body, true );

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

            Globals.Proc.process_list( cond, true );

            if ( Globals.Proc.Stack.Count == 0 || !( Globals.Proc.Stack.Peek() is Symbol ) || ( ( Algebraic ) Globals.Proc.Stack.Peek() ).Name == null )
            {
                throw new ParseException( "Non-constant initializer in for loop." );
            }

            var x = ( Symbol ) Globals.Proc.Stack.Pop();

            var xname = x.Name;

            Globals.Proc.process_list( step_in, true );

            if ( Globals.Proc.Stack.Count == 0 || !( Globals.Proc.Stack.Peek() is Symbol ) )
            {
                throw new ParseException( "Step size must be constant." );
            }

            var step = ( Symbol ) Globals.Proc.Stack.Pop();

            Globals.Proc.process_list( thru_in, true );

            if ( Globals.Proc.Stack.Count == 0 || !( Globals.Proc.Stack.Peek() is Symbol ) )
            {
                throw new ParseException( "Wrong format in for-loop." );
            }

            var thru = ( Symbol ) Globals.Proc.Stack.Pop();

            var pos = !step.Smaller( Symbol.ZERO );

            while ( true )
            {
                if ( ( pos ? thru.Smaller( x ) : x.Smaller( thru ) ) )
                {
                    break;
                }

                Globals.Proc.Store.putValue( xname, x );

                int ret = Globals.Proc.process_list( body, true );

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
                Globals.Proc.process_list( cond, true );

                var c = ( Symbol ) Globals.Proc.Stack.Pop();

                if ( Equals( c, Symbol.ZERO ) )
                {
                    break;
                }
                else if ( !Equals( c, Symbol.ONE ) )
                {
                    throw new SymbolicException( "Not boolean: " + c );
                }

                int ret = Globals.Proc.process_list( body, true );

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

            Globals.Proc.print( fmt );
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

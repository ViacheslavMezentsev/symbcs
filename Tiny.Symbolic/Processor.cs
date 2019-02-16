using System;
using System.Collections;

namespace Tiny.Symbolic
{
    public interface IPrintable
    {
        void print( string format, params object[] list );
    }

    public class Processor: IPrintable
    {
        internal const int LIST = 1, MATRIX = 2, SCALAR = 3, STRING = 4, FUNCTION = 5, LVALUE = 6, SYMBOL = 7, NARG = 8, PDIR = 9, COLON = 10, SYMREF = 11;
        internal const int BREAK = 1, CONTINUE = 2, RETURN = 3;
        public const int EXIT = 4;
        internal const int ERROR = 5;

        internal bool interrupt_flag;

        public Processor( Store store )
        {
            Stack = new Stack();
            this.Store = store;
            Globals.Proc = this;
        }

        internal Stack Stack { get; set; }
        internal Store Store { get; set; }

        internal virtual bool CheckInterrupt()
        {
            return interrupt_flag;
        }

        public virtual void SetInterrupt( bool flag )
        {
            interrupt_flag = flag;
        }

        internal virtual int instruction_type( object x )
        {
            if ( x is List )
            {
                return LIST;
            }

            if ( x is Matrix || x is Vector )
            {
                return MATRIX;
            }

            if ( x is Algebraic )
            {
                return SCALAR;
            }

            if ( x.Equals( ":" ) )
            {
                return COLON;
            }

            if ( x is string )
            {
                var s = ( string ) x;

                switch ( s[ 0 ] )
                {
                    case '@':
                        return SYMREF;
                    case ' ':
                        return STRING;
                    case '$':
                        return LVALUE;
                    case '#':
                        return PDIR;
                    default:
                        return SYMBOL;
                }
            }

            if ( x is int? )
            {
                return NARG;
            }

            if ( x is Lambda )
            {
                return FUNCTION;
            }

            return 0;
        }

        public virtual int ProcessInstruction( object x, bool canon )
        {
            if ( interrupt_flag )
            {
                SetInterrupt( false );

                throw new SymbolicException( "Interrupted." );
            }

            switch ( instruction_type( x ) )
            {
                case LIST:
                case SCALAR:
                case NARG:
                case STRING:
                case LVALUE:
                case COLON:
                    Stack.Push( x );
                    return 0;

                case MATRIX:
                    Stack.Push( x );
                    return 0;

                case FUNCTION:
                    return ( ( Lambda ) x ).Eval( Stack );

                case SYMREF:
                    var s = ( ( string ) x ).Substring( 1 );

                    var val = Store.getValue( s );

                    if ( val == null )
                    {
                        try
                        {
                            LambdaLOADFILE.ReadFile( s + ".m" );

                            val = Store.getValue( s );
                        }
                        catch ( Exception )
                        {
                        }
                    }

                    if ( val is Lambda )
                    {
                        return ( ( Lambda ) val ).Eval( Stack );
                    }

                    if ( val is Algebraic )
                    {
                        var mx = new Matrix( ( Algebraic ) val );

                        var idx = Index.createIndex( Stack, mx );

                        mx = mx.Extract( idx );

                        Stack.Push( mx.Reduce() );

                        return 0;
                    }

                    if ( val is string && ( ( string ) val ).Length > 1 )
                    {
                        s = ( ( string ) val ).Substring( 1 );

                        val = Store.getValue( s );

                        if ( val == null )
                        {
                            try
                            {
                                LambdaLOADFILE.ReadFile( s + ".m" );

                                val = Store.getValue( s );
                            }
                            catch ( Exception )
                            {
                            }
                        }

                        if ( val is Lambda )
                        {
                            return ( ( Lambda ) val ).Eval( Stack );
                        }
                    }

                    throw new ParseException( "Unknown symbol or incorrect symbol type: " + x );

                case SYMBOL:

                    val = Store.getValue( ( string ) x );

                    if ( val == null )
                    {
                        try
                        {
                            LambdaLOADFILE.ReadFile( ( string ) x + ".m" );

                            return 0;
                        }
                        catch ( Exception )
                        {
                            throw new ParseException( "Unknown symbol: " + x );
                        }
                    }

                    return ProcessInstruction( val, canon );

                case PDIR:

                    var selector = ( ( string ) x ).Substring( 1 );

                    if ( selector.Equals( ";" ) )
                    {
                        clearStack();
                    }
                    else if ( selector.Equals( "," ) )
                    {
                        printStack();
                    }
                    else if ( selector.Equals( "brk" ) )
                    {
                        return BREAK;
                    }
                    else if ( selector.Equals( "exit" ) )
                    {
                        return EXIT;
                    }
                    else if ( selector.Equals( "cont" ) )
                    {
                        return CONTINUE;
                    }
                    else if ( selector.Equals( "ret" ) )
                    {
                        return RETURN;
                    }
                    else
                    {
                        var nout = Convert.ToInt32( selector );

                        Lambda.length = nout;
                    }

                    return 0;
            }

            throw new SymbolicException( "Unrecognized instruction type: " + x );
        }

        public virtual int process_list( List x, bool canon )
        {
            lock ( this )
            {
                int n = x.Count, i = 0;

                try
                {
                    for ( i = 0; i < n; i++ )
                    {
                        var z = x[ i ];

                        int ret = ProcessInstruction( z, canon );

                        if ( ret != 0 )
                        {
                            return ret;
                        }
                    }

                    return 0;
                }
                catch ( ParseException p )
                {
                    throw p;
                }
                catch ( SymbolicException j )
                {
                    throw j;
                }
            }
        }

        public virtual void clearStack()
        {
            while ( Stack.Count > 0 )
            {
                var x = Stack.Pop();

                if ( Stack.Count == 0 && x is Algebraic )
                {
                    Store.putValue( "ans", x );
                }
            }
        }

        public virtual void printStack()
        {
            while ( Stack.Count > 0 )
            {
                var x = Stack.Pop();

                if ( x is Algebraic )
                {
                    var vname = "ans";

                    Store.putValue( vname, x );

                    if ( ( ( Algebraic ) x ).Name != null )
                    {
                        vname = ( ( Algebraic ) x ).Name;
                    }

                    var s = vname + " = ";

                    print( s );

                    ( ( Algebraic ) x ).Print();

                    println();
                }
                else if ( x is string )
                {
                    println( ( string ) x );
                }
            }
        }

        public virtual void print( string format, params object[] list )
        {
        }

        public void println( string format, params object[] list )
        {
            print( format + Environment.NewLine, list );
        }

        public void println()
        {
            println( "" );
        }
    }
}

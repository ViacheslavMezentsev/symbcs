using System;
using System.Collections;

using Tiny.Science.Symbolic;

namespace Tiny.Science.Engine
{
    public class Processor
    {
        public const int LIST = 1, MATRIX = 2, SCALAR = 3, STRING = 4, FUNCTION = 5, LVALUE = 6, SYMBOL = 7, NARG = 8, PDIR = 9, COLON = 10, SYMREF = 11;
        public const int BREAK = 1, CONTINUE = 2, RETURN = 3;
        public const int EXIT = 4;
        public const int ERROR = 5;

        private IPrintable _writer;
        internal bool interrupt_flag;

        public Processor( Store store )
        {
            Stack = new Stack();
            this.Store = store;
        }

        public void AttachOutput( IPrintable writer )
        {
            _writer = writer;
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

        internal virtual int GetInstructionType( object op )
        {
            if ( op is List )
            {
                return LIST;
            }

            if ( op is Matrix || op is Vector )
            {
                return MATRIX;
            }

            if ( op is Algebraic )
            {
                return SCALAR;
            }

            if ( op.Equals( ":" ) )
            {
                return COLON;
            }

            if ( op is string )
            {
                var s = ( string ) op;

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

            if ( op is int? )
            {
                return NARG;
            }

            if ( op is Lambda )
            {
                return FUNCTION;
            }

            return 0;
        }

        public virtual int ProcessInstruction( object op, bool canon )
        {
            if ( interrupt_flag )
            {
                SetInterrupt( false );

                throw new SymbolicException( "Interrupted." );
            }

            switch ( GetInstructionType( op ) )
            {
                case LIST:
                case SCALAR:
                case NARG:
                case STRING:
                case LVALUE:
                case COLON:
                    Stack.Push( op );
                    return 0;

                case MATRIX:
                    Stack.Push( op );
                    return 0;

                case FUNCTION:
                    return ( ( Lambda ) op ).Eval( Stack );

                case SYMREF:
                    var s = ( ( string ) op ).Substring( 1 );

                    var val = Store.GetValue( s );

                    if ( val == null )
                    {
                        try
                        {
                            LambdaLOADFILE.ReadFile( s + ".m" );

                            val = Store.GetValue( s );
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

                        val = Store.GetValue( s );

                        if ( val == null )
                        {
                            try
                            {
                                LambdaLOADFILE.ReadFile( s + ".m" );

                                val = Store.GetValue( s );
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

                    throw new ParseException( "Unknown symbol or incorrect symbol type: " + op );

                case SYMBOL:

                    val = Store.GetValue( ( string ) op );

                    if ( val == null )
                    {
                        try
                        {
                            LambdaLOADFILE.ReadFile( ( string ) op + ".m" );

                            return 0;
                        }
                        catch ( Exception )
                        {
                            throw new ParseException( "Unknown symbol: " + op );
                        }
                    }

                    return ProcessInstruction( val, canon );

                case PDIR:

                    var selector = ( ( string ) op ).Substring( 1 );

                    if ( selector.Equals( ";" ) )
                    {
                        ClearStack();
                    }
                    else if ( selector.Equals( "," ) )
                    {
                        PrintStack();
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

            throw new SymbolicException( "Unrecognized instruction type: " + op );
        }

        public virtual int ProcessList( List x, bool canon )
        {
            lock ( this )
            {
                try
                {
                    for ( var n = 0; n < x.Count; n++ )
                    {
                        var z = x[n];

                        int ret = ProcessInstruction( z, canon );

                        if ( ret != 0 )
                        {
                            return ret;
                        }
                    }

                    return 0;
                }
                catch ( ParseException ex )
                {
                    throw ex;
                }
                catch ( SymbolicException ex )
                {
                    throw ex;
                }
            }
        }

        public virtual void ClearStack()
        {
            while ( Stack.Count > 0 )
            {
                var x = Stack.Pop();

                if ( Stack.Count == 0 && x is Algebraic )
                {
                    Store.PutValue( "ans", x );
                }
            }
        }

        public virtual void PrintStack()
        {
            while ( Stack.Count > 0 )
            {
                var x = Stack.Pop();

                if ( x is Algebraic )
                {
                    var a = ( Algebraic ) x;

                    var vname = "ans";

                    Store.PutValue( vname, x );

                    if ( a.Name != null ) vname = a.Name;

                    print( vname + " = " );

                    a.Print();

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
            _writer.print( format, list );
        }

        public virtual void println( string format, params object[] list )
        {
            print( format + Environment.NewLine, list );
        }

        public virtual void println()
        {
            println( "" );
        }
    }
}

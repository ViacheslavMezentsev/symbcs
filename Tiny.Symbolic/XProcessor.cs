using System;

namespace Tiny.Symbolic
{
    public class XProcessor : Processor
    {
        public XProcessor( Store env ) : base( env )
        {
        }
        internal new const int LIST = 1, MATRIX = 2, SCALAR = 3, STRING = 4, FUNCTION = 5, LVALUE = 6, SYMBOL = 7, NARG = 8, PDIR = 9, COLON = 10;
        internal new const int BREAK = 1, CONTINUE = 2, RETURN = 3, EXIT = 4, ERROR = 5;

        internal override int instruction_type( object x )
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
                string s = ( string ) x;

                if ( s.StartsWith( " ", StringComparison.Ordinal ) )
                {
                    return STRING;
                }

                if ( s.StartsWith( "@", StringComparison.Ordinal ) )
                {
                    return FUNCTION;
                }

                if ( s.StartsWith( "Lambda", StringComparison.Ordinal ) )
                {
                    return FUNCTION;
                }

                if ( s.StartsWith( "$", StringComparison.Ordinal ) )
                {
                    return LVALUE;
                }

                if ( s.StartsWith( "#", StringComparison.Ordinal ) )
                {
                    return PDIR;
                }

                return SYMBOL;
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

        public override int ProcessInstruction( object x, bool canon )
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

                    if ( !( x is Lambda ) )
                    {
                        x = Store.getValue( ( string ) x );
                    }

                    return ( ( Lambda ) x ).Eval( Stack );

                case SYMBOL:

                    object val = Store.getValue( ( string ) x );

                    if ( val != null )
                    {
                        return ProcessInstruction( val, canon );
                    }
                    else if ( canon )
                    {
                        x = new Polynomial( new SimpleVariable( ( string ) x ) );

                        Stack.Push( x );
                    }
                    else
                    {
                        Stack.Push( x );
                    }
                    return 0;

                case PDIR:

                    string selector = ( ( string ) x ).Substring( 1 );

                    if ( selector.Equals( ";" ) )
                    {
                        printStack();
                    }
                    else if ( selector.Equals( "," ) )
                    {
                        clearStack();
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
                        int nout = Convert.ToInt32( selector );

                        Lambda.length = nout;
                    }

                    return 0;
            }

            throw new SymbolicException( "Unrecognized instruction type: " + x );
        }

        public override void clearStack()
        {
            while ( Stack.Count > 0 )
            {
                object x = Stack.Pop();
            }
        }

        internal int result = 1;

        public override void printStack()
        {
            while ( Stack.Count > 0 )
            {
                object x = Stack.Pop();

                if ( x is Algebraic )
                {
                    string vname;

                    if ( ( ( Algebraic ) x ).Name != null )
                    {
                        vname = ( ( Algebraic ) x ).Name;
                    }
                    else
                    {
                        vname = "d" + result++;
                        Store.putValue( vname, x );
                    }

                    var s = "    " + vname + " = ";

                    print( s );

                    ( ( Algebraic ) x ).Print();

                    println( "" );
                }
                else if ( x is string )
                {
                    println( ( string ) x );
                }
            }
        }
    }
}

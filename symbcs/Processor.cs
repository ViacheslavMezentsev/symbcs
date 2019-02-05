using System;
using System.Collections;

public class Processor : Constants
{
    internal Stack stack;
    internal Environment env;
    internal PrintStream ps = null;

    public Processor( Environment env )
    {
        stack = new Stack();
        this.env = env;
        Lambda.pc = this;
    }

    internal virtual Environment Environment
    {
        set
        {
            this.env = value;
        }
        get
        {
            return env;
        }
    }

    internal virtual PrintStream PrintStream
    {
        set
        {
            this.ps = value;
        }
    }

    internal bool interrupt_flag = false;

    internal virtual bool check_interrupt()
    {
        return interrupt_flag;
    }

    internal virtual void set_interrupt( bool flag )
    {
        interrupt_flag = flag;
    }

    internal const int LIST = 1, MATRIX = 2, SCALAR = 3, STRING = 4, FUNCTION = 5, LVALUE = 6, SYMBOL = 7, NARG = 8, PDIR = 9, COLON = 10, SYMREF = 11;
    internal const int BREAK = 1, CONTINUE = 2, RETURN = 3, EXIT = 4, ERROR = 5;

    internal virtual int instruction_type( object x )
    {
        if ( x is List )
        {
            return LIST;
        }
        if ( x is Matrix || x is Vektor )
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

    //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
    //ORIGINAL LINE: public int process_instruction(Object x, boolean canon) throws ParseException, JasymcaException
    public virtual int process_instruction( object x, bool canon )
    {
        if ( interrupt_flag )
        {
            set_interrupt( false );
            throw new JasymcaException( "Interrupted." );
        }
        switch ( instruction_type( x ) )
        {
            case LIST:
            case SCALAR:
            case NARG:
            case STRING:
            case LVALUE:
            case COLON:
                stack.Push( x );
                return 0;
            case MATRIX:
                stack.Push( x );
                return 0;
            case FUNCTION:
                return ( ( Lambda ) x ).lambda( stack );
            case SYMREF:
                string s = ( ( string ) x ).Substring( 1 );
                object val = env.getValue( s );
                if ( val == null )
                {
                    try
                    {
                        LambdaLOADFILE.readFile( s + ".m" );
                        val = env.getValue( s );
                    }
                    catch ( Exception )
                    {
                    }
                }
                if ( val is Lambda )
                {
                    return ( ( Lambda ) val ).lambda( stack );
                }
                if ( val is Algebraic )
                {
                    Matrix mx = new Matrix( ( Algebraic ) val );
                    Index idx = Index.createIndex( stack, mx );
                    mx = mx.extract( idx );
                    stack.Push( mx.reduce() );
                    return 0;
                }
                if ( val is string && ( ( string ) val ).Length > 1 )
                {
                    s = ( ( string ) val ).Substring( 1 );
                    val = env.getValue( s );
                    if ( val == null )
                    {
                        try
                        {
                            LambdaLOADFILE.readFile( s + ".m" );
                            val = env.getValue( s );
                        }
                        catch ( Exception )
                        {
                        }
                    }
                    if ( val is Lambda )
                    {
                        return ( ( Lambda ) val ).lambda( stack );
                    }
                }
                throw new ParseException( "Unknown symbol or incorrect symbol type: " + x );
            case SYMBOL:
                val = env.getValue( ( string ) x );
                if ( val == null )
                {
                    try
                    {
                        LambdaLOADFILE.readFile( ( string ) x + ".m" );
                        return 0;
                    }
                    catch ( Exception )
                    {
                        throw new ParseException( "Unknown symbol: " + x );
                    }
                }
                return process_instruction( val, canon );
            case PDIR:
                string selector = ( ( string ) x ).Substring( 1 );
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
        throw new JasymcaException( "Unrecognized instruction type: " + x );
    }

    //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
    //ORIGINAL LINE: synchronized public int process_list(List x, boolean canon) throws JasymcaException, ParseException
    public virtual int process_list( List x, bool canon )
    {
        lock ( this )
        {
            int n = x.Count, i = 0;
            try
            {
                for ( i = 0; i < n; i++ )
                {
                    object z = x[ i ];
                    int ret = process_instruction( z, canon );
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
            catch ( JasymcaException j )
            {
                throw j;
            }
        }
    }

    internal virtual void clearStack()
    {
        while ( stack.Count > 0 )
        {
            object x = stack.Pop();
            if ( stack.Count == 0 && x is Algebraic )
            {
                env.putValue( "ans", x );
            }
        }
    }

    public virtual void printStack()
    {
        while ( stack.Count > 0 )
        {
            object x = stack.Pop();
            if ( x is Algebraic )
            {
                string vname = "ans";
                env.putValue( vname, x );
                if ( ( ( Algebraic ) x ).name != null )
                {
                    vname = ( ( Algebraic ) x ).name;
                }
                if ( ps != null )
                {
                    string s = vname + " = ";
                    ps.print( s );
                    ( ( Algebraic ) x ).print( ps );
                    ps.println( "" );
                }
            }
            else if ( x is string )
            {
                ps.println( ( string ) x );
            }
        }
    }
}

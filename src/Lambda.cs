using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

public abstract class Lambda : Constants
{
    internal static int length = 1;
    internal static bool DEBUG = false;
    internal static Processor pc;
    internal static Parser pr;
    internal static Environment sandbox;
    internal string diffrule = null, intrule = null, trigrule = null;

    internal static void Debug( string text )
    {
        if ( !DEBUG ) return;

        Console.WriteLine( text );
    }

    public virtual int Eval( Stack stack )
    {
        return 0;
    }

    internal static Algebraic GetAlgebraic( Stack stack )
    {
        var arg = stack.Pop();

        if ( !( arg is Algebraic ) )
        {
            pc.ProcessInstruction( arg, true );

            arg = stack.Pop();
        }

        if ( !( arg is Algebraic ) )
        {
            throw new JasymcaException( "Expected algebraic, got: " + arg );
        }

        return ( Algebraic ) arg;
    }

    internal static Symbolic GetNumber( Stack stack )
    {
        var arg = stack.Pop();

        if ( arg is Algebraic )
        {
            arg = new ExpandConstants().SymEval( ( Algebraic ) arg );
        }

        if ( !( arg is Symbolic ) )
        {
            throw new ParseException( "Expected number, got " + arg );
        }

        return ( Symbolic ) arg;
    }

    internal static int GetNarg( Stack stack )
    {
        var arg = stack.Pop();

        if ( !( arg is int? ) )
        {
            throw new JasymcaException( "Expected Integer, got: " + arg );
        }

        return ( int ) ( int? ) arg;
    }

    internal static string GetSymbol( Stack stack )
    {
        var arg = stack.Pop();

        if ( !( arg is string ) || ( ( string ) arg ).Length == 0 || ( ( string ) arg )[0] == ' ' )
        {
            throw new JasymcaException( "Expected Symbol, got: " + arg );
        }

        return ( string ) arg;
    }

    internal static Polynomial GetPolynomial( Stack stack )
    {
        object arg = GetAlgebraic( stack );

        if ( !( arg is Polynomial ) )
        {
            throw new ParseException( "Expected polynomial, got " + arg );
        }

        return ( Polynomial ) arg;
    }

    internal static Vector GetVektor( Stack stack )
    {
        var arg = stack.Pop();

        if ( !( arg is Vector ) )
        {
            throw new ParseException( "Expected vector, got " + arg );
        }

        return ( Vector ) arg;
    }

    internal static Variable GetVariable( Stack stack )
    {
        var p = GetPolynomial( stack );

        return p._v;
    }

    internal static int GetInteger( Stack stack )
    {
        var arg = stack.Pop();

        if ( !( arg is Symbolic ) || !( ( Symbolic ) arg ).IsInteger() )
        {
            throw new ParseException( "Expected integer, got " + arg );
        }

        return ( ( Symbolic ) arg ).ToInt();
    }

    internal static int GetInteger( Algebraic arg )
    {
        if ( !( arg is Symbolic ) || !( ( Symbolic ) arg ).IsInteger() )
        {
            throw new ParseException( "Expected integer, got " + arg );
        }

        return ( ( Symbolic ) arg ).ToInt();
    }

    internal static List GetList( Stack stack )
    {
        var arg = stack.Pop();

        if ( !( arg is List ) )
        {
            throw new ParseException( "Expected list, got " + arg );
        }

        return ( List ) arg;
    }    

    internal static Algebraic evalx( string rule, Algebraic x )
    {
        try
        {
            var pgm = pr.compile( rule );

            var save = pc.Environment;

            if ( sandbox == null )
            {
                sandbox = new Environment();

                sandbox.putValue( "x", new Polynomial( new SimpleVariable( "x" ) ) );
                sandbox.putValue( "X", new Polynomial( new SimpleVariable( "X" ) ) );
                sandbox.putValue( "a", new Polynomial( new SimpleVariable( "a" ) ) );
                sandbox.putValue( "b", new Polynomial( new SimpleVariable( "b" ) ) );
                sandbox.putValue( "c", new Polynomial( new SimpleVariable( "c" ) ) );
            }

            pc.Environment = sandbox;
            pc.process_list( pgm, true );
            pc.Environment = save;

            var y = GetAlgebraic( pc.stack );

            y = y.Value( new SimpleVariable( "x" ), x );

            return y;
        }
        catch ( Exception ex )
        {
            throw new JasymcaException( string.Format( "Could not evaluate expression {0}: {1}", rule, ex.Message ) );
        }
    }
}

public abstract class LambdaAlgebraic : Lambda
{
    public override int Eval( Stack stack )
    {
        int narg = GetNarg( stack );

        switch ( narg )
        {
            case 0:
                throw new JasymcaException( "Lambda functions expect argument." );

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
                    args[i] = GetAlgebraic( stack );
                }

                stack.Push( SymEval( args ) );

                break;
        }

        return 0;
    }

    internal virtual Symbolic PreEval( Symbolic x )
    {
        return x;
    }

    internal virtual Algebraic SymEval( Algebraic x )
    {
        return null;
    }

    internal virtual Algebraic SymEval( Symbolic x, Symbolic y )
    {
        return SymEval( x as Algebraic, y );
    }

    internal virtual Algebraic SymEval( Algebraic x, Algebraic y )
    {
        return null;
    }

    internal virtual Algebraic SymEval( Algebraic[] x )
    {
        return null;
    }    

    public virtual Algebraic Integrate( Algebraic arg, Variable x )
    {
        if ( !arg.Depends(x) )
        {
            throw new JasymcaException( "Expression in function does not depend on Variable." );
        }

        if ( !( arg is Polynomial ) || ( ( Polynomial ) arg ).Degree() != 1 || !( ( Polynomial ) arg ).IsRat(x) || intrule == null )
        {
            throw new JasymcaException( "Can not integrate function " );
        }

        try
        {
            var y = evalx( intrule, arg );

            return y / ( ( Polynomial ) arg )[1];
        }
        catch ( Exception )
        {
            throw new JasymcaException( "Error integrating function" );
        }
    }
}

internal class LambdaFUNC : Lambda
{
    public override int Eval( Stack stack )
    {
        int narg = GetNarg( stack );

        if ( narg != 3 )
        {
            throw new ParseException( "Wrong function definition." );
        }

        var ret = GetList( stack );
        var prot = GetList( stack );

        if ( prot.Count < 1 || !( prot[ prot.Count - 1 ] is string ) )
        {
            throw new ParseException( "Wrong function definition." );
        }

        var fname = ( ( string ) prot[ prot.Count - 1 ] ).Substring( 1 );

        var vars_in = prot.Take( 0, prot.Count - 1 );
        var code_in = GetList( stack );

        SimpleVariable[] vars = null;

        if ( vars_in.Count != 0 )
        {
            int fnarg = ( int ) ( ( int? ) vars_in[ vars_in.Count - 1 ] );

            vars = new SimpleVariable[ fnarg ];

            for ( int i = 0; i < vars.Length; i++ )
            {
                vars[ i ] = new SimpleVariable( ( string ) vars_in[ vars.Length - i - 1 ] );
            }
        }

        var result = new SimpleVariable( ( ( string ) ret[ 0 ] ).Substring( 1 ) );

        Lambda func = null;

        var env = new Environment();
        var ups = new Stack();

        foreach (var t in vars)
        {
            env.putValue( t.name, new Polynomial( t ) );
        }

        object y = null;

        if ( vars.Length == 1 )
        {
            int res = UserProgram.process_block( code_in, ups, env, true );

            if ( res != Processor.ERROR )
            {
                y = env.getValue( result.name );
            }
        }

        if ( y is Algebraic )
        {
            func = new UserFunction( fname, vars, ( Algebraic ) y, result, env );
        }
        else
        {
            func = new UserProgram( fname, vars, code_in, result, env, ups );
        }

        pc.env.putValue( fname, func );
        stack.Push( func );

        return 0;
    }
}

internal class UserProgram : Lambda
{
    internal string fname;
    internal List body;
    internal SimpleVariable[] args;
    internal SimpleVariable result;
    internal Environment env = null;
    internal Stack ups = null;

    public UserProgram()
    {
    }

    public UserProgram( string fname, SimpleVariable[] args, List body, SimpleVariable result, Environment env, Stack ups )
    {
        this.fname = fname;
        this.args = args;
        this.body = body;
        this.result = result;
        this.env = env;
        this.ups = ups;
    }

    public override int Eval( Stack st )
    {
        int narg = GetNarg( st );

        if ( args.Length != narg )
        {
            throw new JasymcaException( fname + " requires " + args.Length + " Arguments." );
        }

        foreach (var t in args)
        {
            var a = st.Pop();

            env.putValue( t.name, a );
        }

        int ret = process_block( body, ups, env, result != null );

        if ( ret != Processor.ERROR )
        {
            var y = result != null ? env.getValue( result.name ) : ups.Pop();

            if ( y is Algebraic && result != null )
            {
                ( ( Algebraic ) y ).Name = result.name;
            }

            if ( y != null )
            {
                st.Push( y );
            }
        }

        return 0;
    }

    internal static int process_block( List code, Stack st, Environment env, bool clear_stack )
    {
        var global = pc.Environment;
        var old_stack = pc.stack;

        pc.Environment = env;
        pc.stack = st;

        int ret;

        try
        {
            ret = pc.process_list( code, true );
        }
        catch ( Exception )
        {
            ret = Processor.ERROR;
        }

        pc.stack = old_stack;
        pc.Environment = global;

        if ( clear_stack )
        {
            while ( st.Count > 0 )
            {
                st.Pop();
            }
        }

        return ret;
    }
}

internal class UserFunction : LambdaAlgebraic
{
    internal string fname;
    internal Algebraic body;
    internal SimpleVariable[] sv;
    internal SimpleVariable result;
    internal Environment env = null;

    public UserFunction()
    {
    }

    public UserFunction( string fname, SimpleVariable[] sv, Algebraic body, SimpleVariable result, Environment env )
    {
        this.fname = fname;
        this.sv = sv;
        this.body = body;
        this.result = result;
        this.env = env;
    }

    internal override Symbolic PreEval( Symbolic x )
    {
        var y = SymEval( x );

        if ( y is Symbolic )
        {
            return ( Symbolic ) y;
        }

        y = ( new ExpandConstants() ).SymEval( y );

        if ( y is Symbolic )
        {
            return ( Symbolic ) y;
        }

        throw new JasymcaException( "Can not evaluate Function " + fname + " to number, got " + y + " for " + x );
    }

    internal override Algebraic SymEval( Algebraic x )
    {
        if ( sv.Length != 1 )
        {
            throw new JasymcaException( "Wrong number of arguments." );
        }

        var y = body.Value( sv[ 0 ], x );

        return y;
    }

    internal override Algebraic SymEval( Algebraic x, Algebraic y )
    {
        if ( sv.Length != 2 )
        {
            throw new JasymcaException( "Wrong number of arguments." );
        }

        var z = body.Value( sv[ 0 ], y );

        z = z.Value( sv[ 1 ], x );

        return z;
    }

    internal override Algebraic SymEval( Algebraic[] x )
    {
        if ( sv.Length != x.Length )
        {
            throw new JasymcaException( "Wrong number of arguments." );
        }

        var y = body;

        for ( int i = 0; i < x.Length; i++ )
        {
            y = y.Value( sv[ x.Length - i - 1 ], x[ i ] );
        }

        return y;
    }

    internal virtual Algebraic fv( Vector x )
    {
        var env = pc.env;

        pc.env = env;

        var r = body;

        pc.env = env;

        for ( int i = 0; i < sv.Length; i++ )
        {
            r = r.Value( sv[i], x[i] );
        }

        return r;
    }

    public override Algebraic Integrate( Algebraic arg, Variable x )
    {
        if ( !( body is Algebraic ) )
        {
            throw new JasymcaException( "Can not integrate function " + fname );
        }

        if ( !( arg.Depends(x) ) )
        {
            throw new JasymcaException( "Expression in function does not depend on Variable." );
        }

        if ( sv.Length == 1 )
        {
            return body.Value( sv[0], arg ).Integrate( x );
        }

        if ( arg is Vector && ( ( Vector ) arg ).Length() == sv.Length )
        {
            return fv( ( Vector ) arg ).Integrate(x);
        }

        throw new JasymcaException( "Wrong argument to function " + fname );
    }
}

internal class LambdaFLOAT : LambdaAlgebraic
{
    internal double eps = 1.0e-8;

    public override int Eval( Stack stack )
    {
        int narg = GetNarg( stack );

        var exp = GetAlgebraic( stack );

        var a = pc.env.getnum( "algepsilon" );

        if ( a != null )
        {
            var epstry = a.ToComplex().Re;

            if ( epstry > 0 )
            {
                eps = epstry;
            }
        }

        exp = new ExpandConstants().SymEval( exp );

        stack.Push( exp.Map( this ) );

        return 0;
    }

    internal override Symbolic PreEval( Symbolic x )
    {
        var f = x.ToComplex();

        if ( f.Equals( Symbolic.ZERO ) )
        {
            return f;
        }

        var abs = ( ( Complex ) f.Abs() ).Re;
        var r = f.Re;

        if ( Math.Abs( r / abs ) < eps )
        {
            r = 0.0;
        }

        var i = f.Im;

        if ( Math.Abs( i / abs ) < eps )
        {
            i = 0.0;
        }

        return new Complex( r, i );
    }

    internal override Algebraic SymEval( Algebraic x )
    {
        return x.Map( this );
    }
}

internal class LambdaMATRIX : Lambda
{
    public override int Eval( Stack stack )
    {
        int narg = GetNarg( stack );

        var a = new Algebraic[ narg ][];

        for ( int i = 0; i < narg; i++ )
        {
            var b = GetAlgebraic( stack );

            if ( b is Vector )
            {
                a[ i ] = ( ( Vector ) b ).ToArray();
            }
            else
            {
                a[ i ] = new Algebraic[ 1 ];
                a[ i ][ 0 ] = b;
            }

            if ( a[ i ].GetLength( 0 ) != a[ 0 ].GetLength( 0 ) )
            {
                throw new JasymcaException( "Matrix rows must have equal length." );
            }
        }

        stack.Push( ( new Matrix( a ) ).Reduce() );

        return 0;
    }
}

internal class LambdaFORMAT : Lambda
{
    public override int Eval( Stack stack )
    {
        int nargs = GetNarg( stack );

        if ( nargs == 1 )
        {
            var arg = stack.Pop();

            if ( "$short".Equals( arg.ToString() ) )
            {
                Jasymca.fmt = new NumFmtVar( 10, 5 );

                return 0;
            }
            else if ( "$long".Equals( arg.ToString() ) )
            {
                Jasymca.fmt = new NumFmtJava();

                return 0;
            }

            throw new JasymcaException( "Usage: format long | short | base significant" );
        }
        else if ( nargs == 2 )
        {
            int bas = GetInteger( stack );
            int nsign = GetInteger( stack );

            if ( bas < 2 || nsign < 1 )
            {
                throw new JasymcaException( "Invalid variables." );
            }

            Jasymca.fmt = new NumFmtVar( bas, nsign );

            return 0;
        }

        throw new JasymcaException( "Usage: format long | short | base significant" );
    }
}

internal class LambdaSYMS : Lambda
{
    public override int Eval( Stack stack )
    {
        int nargs = GetNarg( stack );

        while ( nargs-- > 0 )
        {
            var arg = stack.Pop();

            if ( arg is string )
            {
                var s = ( ( string ) arg ).Substring( 1 );

                pc.env.putValue( s, new Polynomial( new SimpleVariable( s ) ) );
            }
        }

        return 0;
    }
}

internal class LambdaCLEAR : Lambda
{
    public override int Eval( Stack stack )
    {
        int nargs = GetNarg( stack );

        while ( nargs-- > 0 )
        {
            var arg = stack.Pop();

            if ( arg is string )
            {
                var s = ( ( string ) arg ).Substring(1);

                pc.env.Remove(s);
            }
        }

        return 0;
    }
}

internal class CreateVector : Lambda
{
    public override int Eval( Stack stack )
    {
        int nr = GetNarg( stack );
        int nrow = 1, ncol = 1;

        var m = new Matrix( nr, 1 );

        while ( nr-- > 0 )
        {
            var row = crv( stack );

            var idx = new Index( nrow, ncol, row );

            m.Insert( new Matrix( row ), idx );

            nrow = idx.row_max + 1;
        }

        stack.Push( m.Reduce() );

        return 0;
    }

    internal static Algebraic crv( Stack stack )
    {
        int nc = GetNarg( stack );

        if ( nc == 1 )
        {
            return GetAlgebraic( stack );
        }

        var m = new Matrix( 1, nc );

        int nrow = 1, ncol = 1;

        while ( nc-- > 0 )
        {
            var x = GetAlgebraic( stack );

            var idx = new Index( nrow, ncol, x );

            m.Insert( new Matrix( x ), idx );

            ncol = idx.col_max + 1;
        }

        return m.Reduce();
    }
}

internal class CR1 : Lambda
{
    public override int Eval( Stack stack )
    {
        int narg = GetNarg( stack );

        Algebraic a, b, c = GetAlgebraic( stack );

        if ( narg == 2 )
        {
            b = Symbolic.ONE;
            a = GetAlgebraic( stack );
        }
        else
        {
            b = GetAlgebraic( stack );
            a = GetAlgebraic( stack );
        }

        var na = ( c - a ) / b;

        if ( !( na is Symbolic ) )
        {
            na = ( new ExpandConstants() ).SymEval( na );
        }

        if ( !( na is Symbolic ) )
        {
            throw new ParseException( "CreateVector requires numbers." );
        }

        int n = ( int ) ( ( ( Symbolic ) na ).ToComplex().Re + 1.0 );

        var coord = new Algebraic[ n ];

        for ( int i = 0; i < n; i++ )
        {
            coord[i] = a + b * new Complex(i);
        }

        stack.Push( new Vector( coord ) );

        return 0;
    }
}

internal class LambdaEYE : Lambda
{
    public override int Eval( Stack stack )
    {
        int narg = GetNarg( stack );

        if ( narg < 1 )
        {
            throw new JasymcaException( "Usage: EYE( nrow, ncol )." );
        }

        int nrow = GetInteger( stack );
        int ncol = nrow;

        if ( narg > 1 )
        {
            ncol = GetInteger( stack );
        }

        stack.Push( Matrix.Eye( nrow, ncol ).Reduce() );

        return 0;
    }
}

internal class LambdaZEROS : Lambda
{
    public override int Eval( Stack stack )
    {
        int narg = GetNarg( stack );

        if ( narg < 1 )
        {
            throw new JasymcaException( "Usage: ZEROS( nrow, ncol )." );
        }

        int nrow = GetInteger( stack );
        int ncol = nrow;

        if ( narg > 1 )
        {
            ncol = GetInteger( stack );
        }

        stack.Push( ( new Matrix( nrow, ncol ) ).Reduce() );

        return 0;
    }
}

internal class LambdaONES : Lambda
{
    public override int Eval( Stack stack )
    {
        int narg = GetNarg( stack );

        if ( narg < 1 )
        {
            throw new JasymcaException( "Usage: ONES( nrow, ncol )." );
        }

        int nrow = GetInteger( stack );
        int ncol = nrow;

        if ( narg > 1 )
        {
            ncol = GetInteger( stack );
        }

        stack.Push( ( new Matrix( Symbolic.ONE, nrow, ncol ) ).Reduce() );

        return 0;
    }
}

internal class LambdaRAND : Lambda
{
    public override int Eval( Stack stack )
    {
        int narg = GetNarg( stack );

        if ( narg < 1 )
        {
            throw new JasymcaException( "Usage: RAND( nrow, ncol )." );
        }

        int nrow = GetInteger( stack );
        int ncol = nrow;

        if ( narg > 1 )
        {
            ncol = GetInteger( stack );
        }

        var a = Matrix.CreateRectangularArray<Algebraic>( nrow, ncol );

        for ( int i = 0; i < nrow; i++ )
        {
            for ( int k = 0; k < ncol; k++ )
            {
                a[i][k] = new Complex( JMath.random() );
            }
        }

        stack.Push( new Matrix(a).Reduce() );

        return 0;
    }
}
internal class LambdaDIAG : Lambda
{
    public override int Eval( Stack stack )
    {
        int narg = GetNarg( stack );

        if ( narg < 1 )
        {
            throw new JasymcaException( "Usage: DIAG( matrix, k )." );
        }

        var x = GetAlgebraic( stack ).Reduce();

        int k = 0;

        if ( narg > 1 )
        {
            k = GetInteger( stack );
        }

        if ( x.IsScalar() )
        {
            x = new Vector( new[] { x } );
        }

        if ( x is Vector )
        {
            var xv = ( Vector ) x;

            if ( k >= 0 )
            {
                var m = new Matrix( xv.Length() + k, xv.Length() + k );

                for ( int i = 0; i < xv.Length(); i++ )
                {
                    m[ i, i + k ] = xv[i];
                }

                stack.Push( m );
            }
            else
            {
                var m = new Matrix( xv.Length() - k, xv.Length() - k );

                for ( int i = 0; i < xv.Length(); i++ )
                {
                    m[ i - k, i ] = xv[i];
                }

                stack.Push(m);
            }
        }
        else if ( x is Matrix )
        {
            var xm = ( Matrix ) x;

            if ( k >= 0 && k < xm.Cols() )
            {
                var a = new Algebraic[ xm.Cols() - k ];

                for ( int i = 0; i < a.Length; i++ )
                {
                    a[i] = xm[ i, i + k ];
                }

                stack.Push( new Vector(a) );
            }
            else if ( k < 0 && ( -k ) < xm.Rows() )
            {
                var a = new Algebraic[ xm.Rows() + k ];

                for ( int i = 0; i < a.Length; i++ )
                {
                    a[i] = xm[ i - k, i ];
                }

                stack.Push( new Vector(a) );
            }
            else
            {
                throw new JasymcaException( "Argument k to DIAG out of range." );
            }
        }
        else
        {
            throw new JasymcaException( "Argument to DIAG must be vector or matrix." );
        }

        return 0;
    }
}

internal class LambdaGCD : Lambda
{
    public override int Eval( Stack stack )
    {
        int narg = GetNarg( stack );

        if ( narg < 2 )
        {
            throw new ParseException( "GCD requires at least 2 arguments." );
        }

        var _gcd = GetAlgebraic( stack );

        for ( int i = 1; i < narg; i++ )
        {
            _gcd = gcd( _gcd, GetAlgebraic( stack ) );
        }

        stack.Push( _gcd );

        return 0;
    }

    internal virtual Algebraic gcd( Algebraic x, Algebraic y )
    {
        if ( !x.IsNumber() )
        {
            x = ( new LambdaRAT() ).SymEval( x );
        }

        if ( !y.IsNumber() )
        {
            y = ( new LambdaRAT() ).SymEval( y );
        }

        if ( x is Symbolic && y is Symbolic )
        {
            return ( ( Symbolic ) x ).gcd( ( Symbolic ) y );
        }

        if ( x is Polynomial )
        {
            var gcd_x = ( ( Polynomial ) x ).gcd_coeff();

            if ( y is Polynomial )
            {
                var gcd_y = ( ( Polynomial ) y ).gcd_coeff();

                return Poly.poly_gcd( x, y ) * gcd_x.gcd( gcd_y );
            }

            if ( y is Symbolic )
            {
                return gcd_x.gcd( ( Symbolic ) y );
            }
        }

        if ( y is Polynomial && x is Symbolic )
        {
            return gcd( y, x );
        }

        throw new JasymcaException( "Not implemented." );
    }
}

internal class LambdaEXPAND : Lambda
{
    public override int Eval( Stack stack )
    {
        int narg = GetNarg( stack );

        object x = stack.Pop();

        if ( x is List )
        {
            pc.process_list( ( List ) x, true );

            x = pc.stack.Pop();
        }

        if ( x is Algebraic )
        {
            x = ( new SqrtExpand() ).SymEval( ( Algebraic ) x );
        }

        stack.Push( x );

        return 0;
    }
}

internal class LambdaREALPART : Lambda
{
    public override int Eval( Stack stack )
    {
        int narg = GetNarg( stack );

        stack.Push( GetAlgebraic( stack ).RealPart() );

        return 0;
    }
}

internal class LambdaIMAGPART : Lambda
{
    public override int Eval( Stack stack )
    {
        int narg = GetNarg( stack );

        stack.Push( GetAlgebraic( stack ).ImagPart() );

        return 0;
    }
}

internal class LambdaCONJ : Lambda
{
    public override int Eval( Stack stack )
    {
        int narg = GetNarg( stack );

        stack.Push( GetAlgebraic( stack ).Conj() );

        return 0;
    }
}

internal class LambdaANGLE : Lambda
{
    public override int Eval( Stack stack )
    {
        int narg = GetNarg( stack );

        var x = GetAlgebraic( stack );

        object atan2 = pc.env.getValue( "atan2" );

        if ( !( atan2 is LambdaAlgebraic ) )
        {
            throw new JasymcaException( "Function ATAN2 not installed." );
        }

        stack.Push( ( ( LambdaAlgebraic ) atan2 ).SymEval( x.ImagPart(), x.RealPart() ) );

        return 0;
    }
}

internal class LambdaCFS : Lambda
{
    public override int Eval( Stack stack )
    {
        int narg = GetNarg( stack );

        var y = GetAlgebraic( stack ).Rat();

        if ( !( y is Number ) )
        {
            throw new ParseException( "Argument must be exact number" );
        }

        double eps = 1.0e-5;

        if ( narg > 1 )
        {
            eps = GetNumber( stack ).ToComplex().Re;
        }

        stack.Push( ( ( Number ) y ).cfs( eps ) );

        return 0;
    }
}

internal class LambdaDIFF : Lambda
{
    public override int Eval( Stack stack )
    {
        int narg = GetNarg( stack );

        if ( narg == 0 )
        {
            throw new ParseException( "Argument to diff missing." );
        }

        var f = GetAlgebraic( stack );

        Variable v;

        if ( narg > 1 )
        {
            v = GetVariable( stack );
        }
        else
        {
            if ( f is Polynomial )
            {
                v = ( ( Polynomial ) f )._v;
            }
            else if ( f is Rational )
            {
                v = ( ( Rational ) f ).den._v;
            }
            else
            {
                throw new ParseException( "Could not determine Variable." );
            }
        }

        var df = f.Derive( v );

        if ( df is Rational && !df.IsNumber() )
        {
            df = ( new LambdaRAT() ).SymEval( df );
        }

        stack.Push( df );

        return 0;
    }
}

internal class LambdaSUBST : Lambda
{
    public override int Eval( Stack stack )
    {
        int narg = GetNarg( stack );

        if ( narg != 3 )
        {
            throw new ParseException( "Usage: SUBST (a, b, c), substitutes a for b in c" );
        }

        var a = GetAlgebraic( stack );
        var b = GetPolynomial( stack );
        var c = GetAlgebraic( stack );

        var bx = b._v;

        while ( bx is FunctionVariable )
        {
            var arg = ( ( FunctionVariable ) bx ).Var;

            if ( !( arg is Polynomial ) )
            {
                throw new JasymcaException( "Can not solve " + b + " for a variable." );
            }

            bx = ( ( Polynomial ) arg )._v;
        }

        var sol = LambdaSOLVE.solve( a - b, bx );

        var res = new Algebraic[ sol.Length() ];

        for ( int i = 0; i < sol.Length(); i++ )
        {
            var y = sol[i];

            res[ i ] = c.Value( bx, y );
        }

        stack.Push( ( new Vector( res ) ).Reduce() );

        return 0;
    }
}

internal class LambdaCOEFF : Lambda
{
    public override int Eval( Stack stack )
    {
        int narg = GetNarg( stack );

        if ( narg != 3 )
        {
            throw new ParseException( "Usage: COEFF (a, b, c), find coeff of b^c in a" );
        }

        var a = GetPolynomial( stack );
        var b = GetVariable( stack );
        var c_in = GetAlgebraic( stack );

        if ( c_in.IsScalar() )
        {
            stack.Push( a.coefficient( b, GetInteger( c_in ) ) );
        }
        else if ( c_in is Vector )
        {
            var c = ( Vector ) c_in;

            var v = new Algebraic[ c.Length() ];

            for ( int i = 0; i < v.Length; i++ )
            {
                v[ i ] = a.coefficient( b, GetInteger( c[i] ) );
            }

            stack.Push( new Vector( v ) );
        }
        else
        {
            throw new ParseException( "Usage: COEFF (a, b, c), find coeff of b^c in a" );
        }

        return 0;
    }
}

internal class LambdaSUM : Lambda
{
    public override int Eval( Stack stack )
    {
        int narg = GetNarg( stack );

        if ( narg == 1 )
        {
            var x = GetAlgebraic( stack );

            if ( x.IsScalar() && !x.IsConstant() )
            {
                throw new JasymcaException( "Unknown variable dimension: " + x );
            }

            var m = new Matrix( x );

            bool addcols = ( m.Cols() > 1 );

            if ( narg > 1 )
            {
                if ( GetInteger( stack ) == 2 )
                {
                    addcols = false;
                }
            }
            if ( addcols )
            {
                var s = m.col( 1 );

                for ( int i = 2; i <= m.Cols(); i++ )
                {
                    s = s + m.col(i);
                }

                stack.Push( s );
            }
            else
            {
                var s = m.row( 1 );

                for ( int i = 2; i <= m.Rows(); i++ )
                {
                    s = s + m.row(i);
                }

                stack.Push( s );
            }

            return 0;
        }

        if ( narg != 4 )
        {
            throw new ParseException( "Usage: SUM (exp, ind, lo, hi)" );
        }

        var exp = GetAlgebraic( stack );
        var v = GetVariable( stack );

        int lo = GetInteger( stack );
        int hi = GetInteger( stack );

        Algebraic sum = Symbolic.ZERO;

        for ( ; lo <= hi; lo++ )
        {
            sum = sum + exp.Value( v, new Complex( lo ) );
        }

        stack.Push( sum );

        return 0;
    }
}

internal class LambdaLSUM : Lambda
{
    public override int Eval( Stack stack )
    {
        int narg = GetNarg( stack );

        if ( narg != 3 )
        {
            throw new ParseException( "Usage: LSUM (exp, ind, list)" );
        }

        var exp = GetAlgebraic( stack );
        var v = GetVariable( stack );
        var list = GetVektor( stack );

        Algebraic sum = Symbolic.ZERO;

        for ( int i = 0; i < list.Length(); i++ )
        {
            sum = sum + exp.Value( v, list[i] );
        }

        stack.Push( sum );

        return 0;
    }
}

internal class LambdaDIVIDE : Lambda
{
    public override int Eval( Stack stack )
    {
        int size = GetNarg( stack );

        if ( size != 3 && size != 2 )
        {
            throw new ParseException( "Usage: DIVIDE (p1, p2, var)" );
        }

        var p1 = GetAlgebraic( stack );

        if ( !p1.IsNumber() )
        {
            p1 = ( new LambdaRAT() ).SymEval( p1 );
        }

        var p2 = GetAlgebraic( stack );

        if ( !p2.IsNumber() )
        {
            p2 = ( new LambdaRAT() ).SymEval( p2 );
        }

        Algebraic[] a = { p1, p2 };

        if ( size == 3 )
        {
            var v = GetVariable( stack );

            Poly.polydiv( a, v );
        }
        else
        {
            if ( p1 is Symbolic && p2 is Symbolic )
            {
                a = ( ( Symbolic ) p1 ).Div( p2, a );
            }
            else
            {
                a[ 0 ] = Poly.polydiv( p1, p2 );
                a[ 1 ] = ( p1 - a[0] ) * p2;
            }
        }

        stack.Push( new Vector( a ) );

        return 0;
    }
}

internal class LambdaTAYLOR : Lambda
{
    public override int Eval( Stack stack )
    {
        int narg = GetNarg( stack );

        if ( narg != 4 )
        {
            throw new ParseException( "Usage: TAYLOR (exp, var, pt, pow)" );
        }

        var exp = GetAlgebraic( stack );
        var v = GetVariable( stack );
        var pt = GetAlgebraic( stack );

        int n = GetInteger( stack );

        var r = exp.Value( v, pt );
        var t = ( new Polynomial( v ) ) - pt;

        double nf = 1.0;

        for ( int i = 1; i <= n; i++ )
        {
            exp = exp.Derive(v);

            nf *= i;
            r = r + ( exp.Value( v, pt ) * t^i ) / new Complex( nf );
        }

        stack.Push( r );

        return 0;
    }
}

internal class LambdaSAVE : Lambda
{
    public override int Eval( Stack stack )
    {
        int size = GetNarg( stack );

        if ( size < 2 )
        {
            throw new ParseException( "Usage: SAVE (filename,arg1, arg2,...,argi)" );
        }

        var filename = stack.Pop();

        try
        {
            var stream = Jasymca.GetFileOutputStream( ( string ) filename, true );

            for ( var i = 1; i < size; i++ )
            {
                var name = ( string ) stack.Pop();

                if ( "ALL".Equals( name, StringComparison.CurrentCultureIgnoreCase ) )
                {
                    var en = pc.env.Keys.GetEnumerator();

                    while ( en.MoveNext() )
                    {
                        var key = en.Current;

                        if ( "pi".Equals( ( string ) key, StringComparison.CurrentCultureIgnoreCase ) ) continue;

                        var val = pc.env.getValue( ( string ) key );

                        if ( val is Lambda ) continue;

                        var line = string.Format( "{0}:{1};\n", key, val );

                        var bytes = Encoding.UTF8.GetBytes( line );

                        stream.Write( bytes, 0, bytes.Length );
                    }
                }
                else
                {
                    var val = pc.env.getValue( name );

                    var line = string.Format( "{0}:{1};\n", name, val );

                    var bytes = Encoding.UTF8.GetBytes( line );

                    stream.Write( bytes, 0, bytes.Length );
                }
            }

            stream.Close();

            Console.WriteLine( "Wrote variables to " + filename );
        }
        catch ( Exception ex )
        {
            throw new JasymcaException( "Could not write to " + filename + " : " + ex.Message );
        }

        return 0;
    }
}

internal class LambdaLOADFILE : Lambda
{
    public override int Eval( Stack stack )
    {
        int narg = GetNarg( stack );

        if ( narg != 1 )
        {
            throw new ParseException( "Usage: LOADFILE (filename)" );
        }

        var filename = stack.Pop();

        if ( !( filename is string ) )
        {
            throw new JasymcaException( filename + " not a valid filename." );
        }

        try
        {
            ReadFile( ( string ) filename );

            Console.WriteLine( "Loaded Variables from " + filename );
        }
        catch ( Exception e )
        {
            throw new JasymcaException( "Could not read from " + filename + " :" + e.ToString() );
        }

        return 0;
    }

    public static void ReadFile( string fname )
    {
        // Try to load from resources.
        var assembly = Assembly.GetExecutingAssembly();

        var names = assembly.GetManifestResourceNames();

        var resName = names.FirstOrDefault( p => p == "symcs.inc." + fname );

        if ( resName != null )
        {
            Read( assembly.GetManifestResourceStream( resName ) );

            return;
        }

        foreach ( string path in Environment.Paths )
        {
            var full = Path.GetFullPath( Path.Combine( path, fname ) );

            if ( !File.Exists( full ) ) continue;

            Stream stream = new FileStream( full, FileMode.Open );

            Read( stream );

            return;
        }    

        throw new IOException( "Could not find " + fname + "." );
    }

    public static void Read( Stream stream )
    {
        var stack = pc.stack;

        pc.stack = new Stack();

        try
        {
            while ( true )
            {
                var code = pr.compile( stream, null );

                if ( code == null )
                {
                    break;
                }

                pc.process_list( code, true );
            }

            stream.Close();

            pc.stack = stack;
        }
        catch ( Exception ex )
        {
            pc.stack = stack;

            throw new JasymcaException( ex.Message );
        }
    }
}

internal class LambdaRAT : LambdaAlgebraic
{
    public override int Eval( Stack stack )
    {
        int narg = GetNarg( stack );

        var arg = GetAlgebraic( stack ).Reduce();

        if ( arg is Complex )
        {
            stack.Push( PreEval( ( Symbolic ) arg ) );
        }
        else if ( arg is Number )
        {
            stack.Push( arg );
        }
        else
        {
            stack.Push( FunctionVariable.Create( GetType().Name.Substring( "Lambda".Length ).ToLower(), arg ) );
        }

        return 0;
    }

    internal override Algebraic SymEval( Algebraic a )
    {
        if ( a is Symbolic )
        {
            return ( Symbolic ) a.Rat();
        }

        return a.Map( this );
    }

    internal override Symbolic PreEval( Symbolic s )
    {
        return ( Symbolic ) s.Rat();
    }
}

internal class LambdaSQFR : Lambda
{
    public override int Eval( Stack stack )
    {
        int narg = GetNarg( stack );

        var f = GetAlgebraic( stack );

        if ( f is Symbolic )
        {
            stack.Push( f );
            return 0;
        }

        if ( !( f is Polynomial ) )
        {
            throw new ParseException( "Argument to sqfr() must be polynomial." );
        }

        f = ( ( Polynomial ) f ).Rat();

        var fs = ( ( Polynomial ) f ).square_free_dec( ( ( Polynomial ) f )._v );

        if ( fs == null )
        {
            stack.Push( f );
            return 0;
        }

        stack.Push( new Vector( fs ) );

        return 0;
    }
}

internal class LambdaALLROOTS : Lambda
{
    public override int Eval( Stack stack )
    {
        int narg = GetNarg( stack );
        var x = GetAlgebraic( stack );

        if ( x is Vector )
        {
            x = new Polynomial( new SimpleVariable( "x" ), ( Vector ) x );
        }

        if ( !( x is Polynomial ) )
        {
            throw new JasymcaException( "Argument to allroots must be polynomial." );
        }

        var p = ( Polynomial ) ( ( Polynomial ) x ).Rat();
        var ps = p.square_free_dec( p._v );

        Vector r;
        var v = new ArrayList();

        for ( int i = 0; i < ps.Length; i++ )
        {
            if ( ps[ i ] is Polynomial )
            {
                r = ( ( Polynomial ) ps[ i ] ).Monic().roots();

                for ( int k = 0; r != null && k < r.Length(); k++ )
                {
                    for ( int j = 0; j <= i; j++ )
                    {
                        v.Add( r[k] );
                    }
                }
            }
        }

        stack.Push( Vector.Create( v ) );

        return 0;
    }
}

internal class LambdaDET : Lambda
{
    public override int Eval( Stack stack )
    {
        int narg = GetNarg( stack );

        var m = new Matrix( GetAlgebraic( stack ) );

        stack.Push( m.det() );

        return 0;
    }
}

internal class LambdaEIG : Lambda
{
    public override int Eval( Stack stack )
    {
        int narg = GetNarg( stack );

        var m = new Matrix( GetAlgebraic( stack ) );

        stack.Push( m.EigenValues() );

        return 0;
    }
}

internal class LambdaINV : Lambda
{
    public override int Eval( Stack stack )
    {
        int narg = GetNarg( stack );

        var m = new Matrix( GetAlgebraic( stack ) );

        stack.Push( m.invert() );

        return 0;
    }
}

internal class LambdaPINV : Lambda
{
    public override int Eval( Stack stack )
    {
        int narg = GetNarg( stack );

        var m = new Matrix( GetAlgebraic( stack ) );

        stack.Push( m.pseudoinverse() );

        return 0;
    }
}

internal class LambdaHILB : Lambda
{
    public override int Eval( Stack stack )
    {
        int narg = GetNarg( stack );
        int n = GetInteger( stack );

        var a = Matrix.CreateRectangularArray<Algebraic>( n, n );

        for ( int i = 0; i < n; i++ )
        {
            for ( int k = 0; k < n; k++ )
            {
                a[ i ][ k ] = new Number( 1L, ( long ) ( i + k + 1 ) );
            }
        }

        stack.Push( new Matrix( a ) );

        return 0;
    }
}

internal class LambdaLU : Lambda
{
    public override int Eval( Stack stack )
    {
        int narg = GetNarg( stack );

        var m = ( new Matrix( GetAlgebraic( stack ) ) ).copy();

        var B = new Matrix( 1, 1 );
        var P = new Matrix( 1, 1 );

        m.rank_decompose( B, P );

        if ( length != 2 && length != 3 )
        {
            throw new JasymcaException( "Usage: [l,u,p] = LU( Matrix )." );
        }

        if ( length >= 2 )
        {
            stack.Push( B );
            stack.Push( m );

            if ( length == 3 )
            {
                stack.Push( P );
            }
        }

        length = 1;

        return 0;
    }
}

internal class LambdaSQRT : LambdaAlgebraic
{
    public LambdaSQRT()
    {
        diffrule = "1/(2*sqrt(x))";
        intrule = "2/3*x*sqrt(x)";
    }

    internal static string intrule2 = "(2*a*x+b)*sqrt(X)/(4*a)+(4*a*c-b*b)/(8*a*sqrt(a))*log(2*sqrt(a*X)+2*a*x+b)";

    public override Algebraic Integrate( Algebraic arg, Variable x )
    {
        try
        {
            return base.Integrate( arg, x );
        }
        catch ( JasymcaException )
        {
            if ( !( arg.Depends( x ) ) )
            {
                throw new JasymcaException( "Expression in function does not depend on Variable." );
            }

            if ( !( arg is Polynomial ) || ( ( Polynomial ) arg ).Degree() != 2 || !( ( Polynomial ) arg ).IsRat( x ) )
            {
                throw new JasymcaException( "Can not integrate function " );
            }

            Algebraic xp = new Polynomial( x );

            var X = ( Polynomial ) arg;
            var y = evalx( intrule2, xp );

            y = y.Value( new SimpleVariable( "X" ), X );
            y = y.Value( new SimpleVariable( "a" ), X[ 2 ] );
            y = y.Value( new SimpleVariable( "b" ), X[ 1 ] );
            y = y.Value( new SimpleVariable( "c" ), X[ 0 ] );

            y = ( new SqrtExpand() ).SymEval( y );

            return y;
        }
    }

    internal override Symbolic PreEval( Symbolic x )
    {
        var z = x.ToComplex();

        if ( z.Im == 0.0 )
        {
            if ( z.Re < 0.0 )
            {
                return new Complex( 0, Math.Sqrt( -z.Re ) );
            }

            return new Complex( Math.Sqrt( z.Re ) );
        }

        var sr = Math.Sqrt( Math.Sqrt( z.Re * z.Re + z.Im * z.Im ) );

        var phi = JMath.atan2( z.Im, z.Re ) / 2.0;

        return new Complex( sr * Math.Cos( phi ), sr * Math.Sin( phi ) );
    }

    internal override Algebraic SymEval( Algebraic x )
    {
        if ( x.Equals( Symbolic.ONE ) || x.Equals( Symbolic.ZERO ) )
        {
            return x;
        }

        if ( x.Equals( Symbolic.MINUS ) )
        {
            return Symbolic.IONE;
        }

        if ( x is Symbolic )
        {
            return fzexakt( ( Symbolic ) x );
        }

        if ( x is Polynomial 
            && ( ( Polynomial ) x ).Degree() == 1 
            && ( ( Polynomial ) x )[0].Equals( Symbolic.ZERO ) 
            && ( ( Polynomial ) x )[1].Equals( Symbolic.ONE ) 
            && ( ( Polynomial ) x )._v is FunctionVariable 
            && ( ( FunctionVariable ) ( ( Polynomial ) x )._v ).Name.Equals( "exp" ) )
        {
            return FunctionVariable.Create( "exp", ( ( FunctionVariable ) ( ( Polynomial ) x )._v ).Var / Symbolic.TWO );
        }

        return null;
    }

    internal virtual Algebraic fzexakt( Symbolic x )
    {
        if ( x is Number && !x.IsComplex() )
        {
            if ( x < Symbolic.ZERO )
            {
                var r = fzexakt( ( Symbolic ) ( -x ) );

                if ( r != null )
                {
                    return Symbolic.IONE * r;
                }

                return r;
            }

            var nom = ( ( Number ) x ).real[ 0 ].longValue();
            var den = ( ( Number ) x ).real[ 1 ].longValue();

            long a0 = introot( nom ), a1 = nom / ( a0 * a0 );
            long b0 = introot( den ), b1 = den / ( b0 * b0 );

            var br = new[] { BigInteger.valueOf( a0 ), BigInteger.valueOf( b0 * b1 ) };

            var r1 = new Number( br );

            a0 = a1 * b1;

            if ( a0 == 1L )
            {
                return r1;
            }

            return r1 * new Polynomial( new FunctionVariable( "sqrt", new Number( BigInteger.valueOf( a0 ) ), this ) );
        }

        return null;
    }

    internal virtual long introot( long x )
    {
        long s = 1L; 
        long f; 
        long g;
         
        long[] t = { 2L, 3L, 5L };

        foreach (long t1 in t)
        {
            g = t1;
            f = g * g;

            while ( x % f == 0L && x != 1L )
            {
                s *= g;
                x /= f;
            }
        }

        for ( long i = 6L; x != 1L; i += 6L )
        {
            g = i + 1;
            f = g * g;

            while ( x % f == 0L && x != 1L )
            {
                s *= g;
                x /= f;
            }

            g = i + 5;
            f = g * g;

            while ( x % f == 0L && x != 1L )
            {
                s *= g;
                x /= f;
            }

            if ( f > x )
            {
                break;
            }
        }

        return s;
    }
}

internal class LambdaSIGN : LambdaAlgebraic
{
    public LambdaSIGN()
    {
        diffrule = "x-x";
        intrule = "x*sign(x)";
    }

    internal override Algebraic SymEval( Algebraic a )
    {
        if ( a is Symbolic )
        {
            return PreEval( ( Symbolic ) a );
        }

        return null;
    }

    internal override Symbolic PreEval( Symbolic s )
    {
        return s.Smaller( Symbolic.ZERO ) ? Symbolic.MINUS : Symbolic.ONE;
    }
}

internal class LambdaABS : LambdaAlgebraic
{
    public LambdaABS()
    {
        diffrule = "sign(x)";
        intrule = "sign(x)*x^2/2";
    }

    internal override Algebraic SymEval( Algebraic a )
    {
        if ( a is Symbolic )
        {
            return PreEval( ( Symbolic ) a );
        }

        return FunctionVariable.Create( "sqrt", a * a.Conj() );
    }

    internal override Symbolic PreEval( Symbolic x )
    {
        return new Complex( x.Norm() );
    }
}

internal class ExpandUser : LambdaAlgebraic
{
    internal override Algebraic SymEval( Algebraic x1 )
    {
        if ( !( x1 is Polynomial ) )
        {
            return x1.Map( this );
        }

        var p = ( Polynomial ) x1;

        if ( p._v is SimpleVariable )
        {
            return p.Map( this );
        }

        var f = ( FunctionVariable ) p._v;

        var lx = pc.env.getValue( f.Name );

        if ( !( lx is UserFunction ) )
        {
            return p.Map( this );
        }

        var la = ( UserFunction ) lx;

        if ( !( la.body is Algebraic ) )
        {
            return x1;
        }

        var body = la.body;

        Algebraic x;

        if ( la.sv.Length == 1 )
        {
            x = body.Value( la.sv[ 0 ], f.Var );
        }
        else if ( f.Var is Vector && ( ( Vector ) f.Var ).Length() == la.sv.Length )
        {
            x = la.fv( ( Vector ) f.Var );
        }
        else
        {
            throw new JasymcaException( "Wrong argument to function " + la.fname );
        }

        Algebraic r = Symbolic.ZERO;

        for ( int i = p.Coeffs.Length - 1; i > 0; i-- )
        {
            r = ( r + SymEval( p[i] ) ) * x;
        }
        if ( p.Coeffs.Length > 0 )
        {
            r = r + SymEval( p[0] );
        }

        return r;
    }
}

internal class ASS : Lambda
{
    public override int Eval( Stack stack )
    {
        int narg = GetNarg( stack );

        var val = new object[ narg ];

        for ( int i = 0; i < narg; i++ )
        {
            val[ i ] = stack.Pop();
        }

        for ( int i = narg - 1; i >= 0; i-- )
        {
            var name = GetSymbol( stack );

            if ( !name.StartsWith( "$", StringComparison.Ordinal ) )
            {
                throw new JasymcaException( "Illegal lvalue: " + name );
            }

            name = name.Substring( 1 );

            bool idxq = stack.Count > 0 && stack.Peek() is int?;

            if ( !idxq )
            {
                pc.env.putValue( name, val[ i ] );

                if ( val[ i ] is Algebraic )
                {
                    ( ( Algebraic ) val[ i ] ).Name = name;
                }
            }
            else
            {
                if ( !( val[ i ] is Algebraic ) )
                {
                    throw new JasymcaException( "No index allowed here: " + val[ i ] );
                }

                var rhs = new Matrix( ( Algebraic ) val[ i ] );
                var lhs = new Matrix( ( Algebraic ) pc.env.getValue( name ) );

                var idx = Index.createIndex( stack, lhs );

                lhs.Insert( rhs, idx );

                val[ i ] = lhs.Reduce();

                pc.env.putValue( name, val[ i ] );
            }
        }

        for ( int i = 0; i < narg; i++ )
        {
            stack.Push( val[ i ] );
        }

        return 0;
    }

    internal static int lambdap( Stack stack, Lambda op )
    {
        int narg = GetNarg( stack );

        var y = stack.Pop();

        var name = GetSymbol( stack );

        if ( !name.StartsWith( "$", StringComparison.Ordinal ) )
        {
            throw new JasymcaException( "Illegal lvalue: " + name );
        }

        var t = new List { name, name.Substring(1), y, 2, op, 1, Operator.get( "=" ).Lambda };

        pc.process_list( t, true );

        return 0;
    }

    internal static int lambdai( Stack st, bool sign, bool pre )
    {
        int narg = GetNarg( st );

        var name = GetSymbol( st );

        if ( !name.StartsWith( "$", StringComparison.Ordinal ) )
        {
            throw new JasymcaException( "Illegal lvalue: " + name );
        }

        object p = null;

        if ( !pre )
        {
            p = pc.env.getValue( name.Substring( 1 ) );
        }

        var t = new List { name, name.Substring(1), Symbolic.ONE, 2, Operator.get( sign ? "+" : "-" ).Lambda, 1, Operator.get( "=" ).Lambda };

        pc.process_list( t, true );

        if ( !pre && p != null )
        {
            if ( p is Algebraic )
            {
                ( ( Algebraic ) p ).Name = null;
            }

            st.Pop();
            st.Push( p );
        }

        return 0;
    }
}

internal class LambdaWHO : Lambda
{
    public override int Eval( Stack stack )
    {
        if ( pc.ps != null )
        {
            pc.ps.println( pc.env.ToString() );
        }

        return 0;
    }
}

internal class LambdaADDPATH : Lambda
{
    public override int Eval( Stack stack )
    {
        int narg = GetNarg( stack );

        while ( narg-- > 0 )
        {
            var s = stack.Pop();

            if ( !( s is string ) )
            {
                throw new JasymcaException( "Usage: ADDPATH( dir1, dir2, ... )" );
            }

            pc.env.addPath( ( ( string ) s ).Substring(1) );
        }

        return 0;
    }
}

internal class LambdaPATH : Lambda
{
    public override int Eval( Stack stack )
    {
        var s = string.Join( ":", Environment.Paths.Cast<string>().ToArray() );

        if ( pc.ps != null )
        {
            pc.ps.println(s);
        }

        return 0;
    }
}

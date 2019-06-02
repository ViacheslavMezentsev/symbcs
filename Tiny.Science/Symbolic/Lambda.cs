using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using Tiny.Science.Engine;
using Math = Tiny.Science.Numeric.Math;
using BigInteger = Tiny.Science.Numeric.BigInteger;

namespace Tiny.Science.Symbolic
{
    public static class Flags
    {
        public const int Unary = 1, Binary = 2, Ternary = 4, LValue = 8, List = 16;
        public const int LeftRight = 0, RightLeft = 1;
        public const int Start = 0, Mid = 1, End = 2;
        public const int KEYWORD = 1, REF_PAREN = 2, REF_BRACK = 4;
    }

    public abstract class Lambda
    {
        internal static int length = 1;
        internal static bool DEBUG = false;
        internal string diffrule, intrule, trigrule = null;

        internal static void Debug( string text )
        {
            if ( !DEBUG ) return;

            Session.Proc?.println( text );
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
                Session.Proc.ProcessInstruction( arg, true );

                arg = stack.Pop();
            }

            if ( !( arg is Algebraic ) )
            {
                throw new SymbolicException( $"Expected algebraic, got: {arg}" );
            }

            return ( Algebraic ) arg;
        }

        internal static Symbol GetNumber( Stack stack )
        {
            var arg = stack.Pop();

            if ( arg is Algebraic )
            {
                arg = new ExpandConstants().SymEval( ( Algebraic ) arg );
            }

            if ( !( arg is Symbol ) )
            {
                throw new ParseException( $"Expected number, got {arg}" );
            }

            return ( Symbol ) arg;
        }

        internal static int GetNarg( Stack stack )
        {
            var arg = stack.Pop();

            if ( !( arg is int? ) )
            {
                throw new SymbolicException( $"Expected Integer, got: {arg}" );
            }

            return ( int ) ( int? ) arg;
        }

        internal static string GetSymbol( Stack stack )
        {
            var arg = stack.Pop();

            if ( !( arg is string ) || ( ( string ) arg ).Length == 0 || ( ( string ) arg )[0] == ' ' )
            {
                throw new SymbolicException( $"Expected Symbol, got: {arg}" );
            }

            return ( string ) arg;
        }

        internal static Polynomial GetPolynomial( Stack stack )
        {
            object arg = GetAlgebraic( stack );

            if ( !( arg is Polynomial ) )
            {
                throw new ParseException( $"Expected polynomial, got {arg}" );
            }

            return ( Polynomial ) arg;
        }

        internal static Vector GetVector( Stack stack )
        {
            var arg = stack.Pop();

            if ( !( arg is Vector ) )
            {
                throw new ParseException( $"Expected vector, got {arg}" );
            }

            return ( Vector ) arg;
        }

        internal static Variable GetVariable( Stack stack )
        {
            var p = GetPolynomial( stack );

            return p.Var;
        }

        internal static int GetInteger( Stack stack )
        {
            var arg = stack.Pop();

            if ( !( arg is Symbol ) || !( ( Symbol ) arg ).IsInteger() )
            {
                throw new ParseException( $"Expected integer, got {arg}" );
            }

            return ( ( Symbol ) arg ).ToInt();
        }

        internal static int GetInteger( Algebraic arg )
        {
            if ( !( arg is Symbol ) || !( ( Symbol ) arg ).IsInteger() )
            {
                throw new ParseException( $"Expected integer, got {arg}" );
            }

            return ( ( Symbol ) arg ).ToInt();
        }

        internal static List GetList( Stack stack )
        {
            var arg = stack.Pop();

            if ( !( arg is List ) )
            {
                throw new ParseException( $"Expected list, got {arg}" );
            }

            return ( List ) arg;
        }

        internal static Algebraic evalx( string rule, Algebraic x )
        {
            try
            {
                var list = Session.Parser.compile( rule );

                var save = Session.Proc.Store;

                if ( Session.Store == null )
                {
                    Session.Store = new Store();

                    var values = new[] { "x", "X", "a", "b", "c" };

                    foreach ( var value in values )
                    {
                        Session.Store.PutValue( value, new Polynomial( new SimpleVariable( value ) ) );
                    }
                }

                Session.Proc.Store = Session.Store;
                Session.Proc.ProcessList( list, true );
                Session.Proc.Store = save;

                var res = GetAlgebraic( Session.Proc.Stack );

                res = res.Value( new SimpleVariable( "x" ), x );

                return res;
            }
            catch ( Exception ex )
            {
                throw new SymbolicException( $"Could not evaluate expression {rule}: {ex.Message}" );
            }
        }
    }


    internal class LambdaFUNC : Lambda
    {
        public override int Eval( Stack stack )
        {
            var narg = GetNarg( stack );

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

            var fname = ( ( string ) prot[ prot.Count - 1 ] ).Substring(1);

            var vars_in = prot.Take( 0, prot.Count - 1 );
            var code_in = GetList( stack );

            var vars = new SimpleVariable[0];
            var env = new Store();
            var ups = new Stack();

            if ( vars_in.Count != 0 )
            {
                var fnarg = ( int ) ( ( int? ) vars_in[ vars_in.Count - 1 ] );

                vars = new SimpleVariable[ fnarg ];

                for ( var n = 0; n < vars.Length; n++ )
                {
                    vars[n] = new SimpleVariable( ( string ) vars_in[ vars.Length - n - 1 ] );

                    env.PutValue( vars[n].name, new Polynomial( vars[n] ) );
                }
            }

            var result = new SimpleVariable( ( ( string ) ret[0] ).Substring(1) );

            object y = null;

            if ( vars.Length == 1 )
            {
                var res = UserProgram.process_block( code_in, ups, env, true );

                if ( res != Processor.ERROR )
                {
                    y = env.GetValue( result.name );
                }
            }

            Lambda func;

            if ( y is Algebraic )
            {
                func = new UserFunction( fname, vars, ( Algebraic ) y, result, env );
            }
            else
            {
                func = new UserProgram( fname, vars, code_in, result, env, ups );
            }

            Session.Proc.Store.PutValue( fname, func );
            stack.Push( func );

            return 0;
        }
    }

    internal class UserProgram : Lambda
    {
        private string _fname;
        private List _body;
        private SimpleVariable[] _args;
        private SimpleVariable _result;
        private Store _env;
        private Stack _ups;

        public UserProgram()
        {
        }

        public UserProgram( string fname, SimpleVariable[] args, List body, SimpleVariable result, Store env, Stack ups )
        {
            _fname = fname;
            _args = args;
            _body = body;
            _result = result;
            _env = env;
            _ups = ups;
        }

        public override int Eval( Stack stack )
        {
            var narg = GetNarg( stack );

            if ( _args.Length != narg )
            {
                throw new SymbolicException( $"{_fname} requires {_args.Length} Arguments." );
            }

            foreach ( var arg in _args )
            {
                var value = stack.Pop();

                _env.PutValue( arg.name, value );
            }

            var ret = process_block( _body, _ups, _env, _result != null );

            if ( ret != Processor.ERROR )
            {
                var y = _result != null ? _env.GetValue( _result.name ) : _ups.Pop();

                if ( y is Algebraic && _result != null )
                {
                    ( ( Algebraic ) y ).Name = _result.name;
                }

                if ( y != null )
                {
                    stack.Push( y );
                }
            }

            return 0;
        }

        internal static int process_block( List code, Stack stack, Store env, bool clear_stack )
        {
            var save = Session.Proc.Store;
            var old_stack = Session.Proc.Stack;

            Session.Proc.Store = env;
            Session.Proc.Stack = stack;

            int ret;

            try
            {
                ret = Session.Proc.ProcessList( code, true );
            }
            catch
            {
                ret = Processor.ERROR;
            }

            Session.Proc.Stack = old_stack;
            Session.Proc.Store = save;

            if ( clear_stack )
            {
                stack.Clear();
            }

            return ret;
        }
    }

    internal class UserFunction : LambdaAlgebraic
    {        
        private SimpleVariable _result;
        private Store _store;

        public string Name { get; }
        public Algebraic Body { get; }
        public SimpleVariable[] Args { get; }

        public UserFunction()
        {
            Args = new SimpleVariable[0];
        }

        public UserFunction( string name, SimpleVariable[] args, Algebraic body, SimpleVariable result, Store store )
        {
            _result = result;
            _store = store;

            Name = name;
            Args = args;
            Body = body;
        }

        internal override Symbol PreEval( Symbol x )
        {
            var y = SymEval(x);

            if ( y is Symbol )
            {
                return ( Symbol ) y;
            }

            y = ( new ExpandConstants() ).SymEval(y);

            if ( y is Symbol )
            {
                return ( Symbol ) y;
            }

            throw new SymbolicException( $"Can not evaluate Function {Name} to number, got {y} for {x}" );
        }

        internal override Algebraic SymEval( Algebraic x )
        {
            if ( Args.Length != 1 )
            {
                throw new SymbolicException( "Wrong number of arguments." );
            }

            var y = Body.Value( Args[0], x );

            return y;
        }

        internal override Algebraic SymEval( Algebraic x, Algebraic y )
        {
            if ( Args.Length != 2 )
            {
                throw new SymbolicException( "Wrong number of arguments." );
            }

            var z = Body.Value( Args[0], y );

            z = z.Value( Args[1], x );

            return z;
        }

        internal override Algebraic SymEval( Algebraic[] x )
        {
            if ( Args.Length != x.Length )
            {
                throw new SymbolicException( "Wrong number of arguments." );
            }

            var y = Body;

            for ( var n = 0; n < x.Length; n++ )
            {
                y = y.Value( Args[ x.Length - n - 1 ], x[n] );
            }

            return y;
        }

        internal virtual Algebraic fv( Vector x )
        {
            var save = Session.Proc.Store;

            Session.Proc.Store = _store;

            var r = Body;

            for ( var n = 0; n < Args.Length; n++ )
            {
                r = r.Value( Args[n], x[n] );
            }

            Session.Proc.Store = save;

            return r;
        }

        public override Algebraic Integrate( Algebraic arg, Variable x )
        {
            if ( !( Body is Algebraic ) )
            {
                throw new SymbolicException( $"Can not integrate function {Name}" );
            }

            if ( !( arg.Depends(x) ) )
            {
                throw new SymbolicException( "Expression in function does not depend on Variable." );
            }

            if ( Args.Length == 1 )
            {
                return Body.Value( Args[0], arg ).Integrate(x);
            }

            if ( arg is Vector && ( ( Vector ) arg ).Length() == Args.Length )
            {
                return fv( ( Vector ) arg ).Integrate(x);
            }

            throw new SymbolicException( $"Wrong argument to function {Name}" );
        }
    }

    internal class LambdaFLOAT : LambdaAlgebraic
    {
        internal double eps = 1.0e-8;

        public override int Eval( Stack stack )
        {
            var narg = GetNarg( stack );

            var exp = GetAlgebraic( stack );

            var a = Session.Proc.Store.GetNum( "algepsilon" );

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

        internal override Symbol PreEval( Symbol x )
        {
            var f = x.ToComplex();

            if ( f.Equals( Symbol.ZERO ) )
            {
                return f;
            }

            var abs = ( ( Complex ) f.Abs() ).Re;
            var r = f.Re;

            if ( Math.abs( r / abs ) < eps )
            {
                r = 0.0;
            }

            var i = f.Im;

            if ( Math.abs( i / abs ) < eps )
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
            var narg = GetNarg( stack );

            var a = new Algebraic[ narg ][];

            for ( var n = 0; n < narg; n++ )
            {
                var b = GetAlgebraic( stack );

                if ( b is Vector )
                {
                    a[n] = ( ( Vector ) b ).ToArray();
                }
                else
                {
                    a[n] = new[] {b};
                }

                if ( a[n].GetLength(0) != a[0].GetLength(0) )
                {
                    throw new SymbolicException( "Matrix rows must have equal length." );
                }
            }

            stack.Push( ( new Matrix(a) ).Reduce() );

            return 0;
        }
    }

    internal class LambdaFORMAT : Lambda
    {
        public override int Eval( Stack stack )
        {
            var nargs = GetNarg( stack );

            if ( nargs == 1 )
            {
                var arg = stack.Pop().ToString();

                if ( arg.Equals( "$short" ) )
                {
                    Session.Fmt = new NumFmtVar( 10, 5 );

                    return 0;
                }

                if ( arg.Equals( "$long" ) )
                {
                    Session.Fmt = new NumFmtLong();

                    return 0;
                }

                throw new SymbolicException( "Usage: format long | short | base significant" );
            }

            if ( nargs == 2 )
            {
                var bas = GetInteger( stack );
                var nsign = GetInteger( stack );

                if ( bas < 2 || nsign < 1 )
                {
                    throw new SymbolicException( "Invalid variables." );
                }

                Session.Fmt = new NumFmtVar( bas, nsign );

                return 0;
            }

            throw new SymbolicException( "Usage: format long | short | base significant" );
        }
    }

    internal class LambdaSYMS : Lambda
    {
        public override int Eval( Stack stack )
        {
            var nargs = GetNarg( stack );

            while ( nargs-- > 0 )
            {
                var arg = stack.Pop();

                if ( arg is string )
                {
                    var value = ( ( string ) arg ).Substring(1);

                    Session.Proc.Store.PutValue( value, new Polynomial( new SimpleVariable( value ) ) );
                }
            }

            return 0;
        }
    }

    internal class LambdaCLEAR : Lambda
    {
        public override int Eval( Stack stack )
        {
            var nargs = GetNarg( stack );

            while ( nargs-- > 0 )
            {
                var arg = stack.Pop();

                if ( arg is string )
                {
                    var s = ( ( string ) arg ).Substring(1);

                    Session.Proc.Store.Remove(s);
                }
            }

            return 0;
        }
    }

    public class CreateVector : Lambda
    {
        public override int Eval( Stack stack )
        {
            var narg = GetNarg( stack );
            int nrow = 1, ncol = 1;

            var m = new Matrix( narg, 1 );

            while ( narg-- > 0 )
            {
                var row = Create( stack );

                var idx = new Index( nrow, ncol, row );

                m.Insert( new Matrix( row ), idx );

                nrow = idx.row_max + 1;
            }

            stack.Push( m.Reduce() );

            return 0;
        }

        internal static Algebraic Create( Stack stack )
        {
            var narg = GetNarg( stack );

            if ( narg == 1 )
            {
                return GetAlgebraic( stack );
            }

            var m = new Matrix( 1, narg );

            int nrow = 1, ncol = 1;

            while ( narg-- > 0 )
            {
                var x = GetAlgebraic( stack );

                var idx = new Index( nrow, ncol, x );

                m.Insert( new Matrix(x), idx );

                ncol = idx.col_max + 1;
            }

            return m.Reduce();
        }
    }

    internal class CR1 : Lambda
    {
        public override int Eval( Stack stack )
        {
            var narg = GetNarg( stack );

            Algebraic a, b, c = GetAlgebraic( stack );

            if ( narg == 2 )
            {
                b = Symbol.ONE;
                a = GetAlgebraic( stack );
            }
            else
            {
                b = GetAlgebraic( stack );
                a = GetAlgebraic( stack );
            }

            var na = ( c - a ) / b;

            if ( !( na is Symbol ) )
            {
                na = ( new ExpandConstants() ).SymEval( na );
            }

            if ( !( na is Symbol ) )
            {
                throw new ParseException( "CreateVector requires numbers." );
            }

            var n = ( int ) ( ( ( Symbol ) na ).ToComplex().Re + 1.0 );

            var coord = new Algebraic[n];

            for ( var i = 0; i < n; i++ )
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
            var narg = GetNarg( stack );

            if ( narg < 1 )
            {
                throw new SymbolicException( "Usage: EYE( nrow, ncol )." );
            }

            var nrow = GetInteger( stack );
            var ncol = nrow;

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
            var narg = GetNarg( stack );

            if ( narg < 1 )
            {
                throw new SymbolicException( "Usage: ZEROS( nrow, ncol )." );
            }

            var nrow = GetInteger( stack );
            var ncol = nrow;

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
            var narg = GetNarg( stack );

            if ( narg < 1 )
            {
                throw new SymbolicException( "Usage: ONES( nrow, ncol )." );
            }

            var nrow = GetInteger( stack );
            var ncol = nrow;

            if ( narg > 1 )
            {
                ncol = GetInteger( stack );
            }

            stack.Push( ( new Matrix( Symbol.ONE, nrow, ncol ) ).Reduce() );

            return 0;
        }
    }

    internal class LambdaRAND : Lambda
    {
        public override int Eval( Stack stack )
        {
            var narg = GetNarg( stack );

            if ( narg < 1 )
            {
                throw new SymbolicException( "Usage: RAND( nrow, ncol )." );
            }

            var nrow = GetInteger( stack );
            var ncol = nrow;

            if ( narg > 1 )
            {
                ncol = GetInteger( stack );
            }

            var m = Matrix.CreateRectangularArray<Algebraic>( nrow, ncol );

            for ( var n = 0; n < nrow; n++ )
            {
                for ( var k = 0; k < ncol; k++ )
                {
                    m[n][k] = new Complex( Math.random() );
                }
            }

            stack.Push( new Matrix(m).Reduce() );

            return 0;
        }
    }

    internal class LambdaDIAG : Lambda
    {
        public override int Eval( Stack stack )
        {
            var narg = GetNarg( stack );

            if ( narg < 1 )
            {
                throw new SymbolicException( "Usage: DIAG( matrix, k )." );
            }

            var x = GetAlgebraic( stack ).Reduce();

            var k = 0;

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
                var v = ( Vector ) x;

                if ( k >= 0 )
                {
                    var m = new Matrix( v.Length() + k, v.Length() + k );

                    for ( var n = 0; n < v.Length(); n++ )
                    {
                        m[ n, n + k ] = v[n];
                    }

                    stack.Push(m);
                }
                else
                {
                    var m = new Matrix( v.Length() - k, v.Length() - k );

                    for ( var n = 0; n < v.Length(); n++ )
                    {
                        m[ n - k, n ] = v[n];
                    }

                    stack.Push(m);
                }
            }
            else if ( x is Matrix )
            {
                var m = ( Matrix ) x;

                if ( k >= 0 && k < m.Cols() )
                {
                    var a = new Algebraic[ m.Cols() - k ];

                    for ( var n = 0; n < a.Length; n++ )
                    {
                        a[n] = m[ n, n + k ];
                    }

                    stack.Push( new Vector(a) );
                }
                else if ( k < 0 && ( -k ) < m.Rows() )
                {
                    var a = new Algebraic[ m.Rows() + k ];

                    for ( var n = 0; n < a.Length; n++ )
                    {
                        a[n] = m[ n - k, n ];
                    }

                    stack.Push( new Vector(a) );
                }
                else
                {
                    throw new SymbolicException( "Argument k to DIAG out of range." );
                }
            }
            else
            {
                throw new SymbolicException( "Argument to DIAG must be vector or matrix." );
            }

            return 0;
        }
    }

    internal class LambdaGCD : Lambda
    {
        public override int Eval( Stack stack )
        {
            var narg = GetNarg( stack );

            if ( narg < 2 )
            {
                throw new ParseException( "GCD requires at least 2 arguments." );
            }

            var a = GetAlgebraic( stack );

            for ( var n = 1; n < narg; n++ )
            {
                a = gcd( a, GetAlgebraic( stack ) );
            }

            stack.Push(a);

            return 0;
        }

        private Algebraic gcd( Algebraic x, Algebraic y )
        {
            if ( !x.IsNumber() )
            {
                x = ( new LambdaRAT() ).SymEval(x);
            }

            if ( !y.IsNumber() )
            {
                y = ( new LambdaRAT() ).SymEval(y);
            }

            if ( x is Symbol && y is Symbol )
            {
                return ( ( Symbol ) x ).gcd( ( Symbol ) y );
            }

            if ( x is Polynomial )
            {
                var gcd_x = ( ( Polynomial ) x ).gcd_coeff();

                if ( y is Polynomial )
                {
                    var gcd_y = ( ( Polynomial ) y ).gcd_coeff();

                    return Poly.poly_gcd( x, y ) * gcd_x.gcd( gcd_y );
                }

                if ( y is Symbol )
                {
                    return gcd_x.gcd( ( Symbol ) y );
                }
            }

            if ( y is Polynomial && x is Symbol )
            {
                return gcd( y, x );
            }

            throw new SymbolicException( "Not implemented." );
        }
    }

    internal class LambdaEXPAND : Lambda
    {
        public override int Eval( Stack stack )
        {
            var narg = GetNarg( stack );

            var x = stack.Pop();

            if ( x is List )
            {
                Session.Proc.ProcessList( ( List ) x, true );

                x = Session.Proc.Stack.Pop();
            }

            if ( x is Algebraic )
            {
                x = ( new SqrtExpand() ).SymEval( ( Algebraic ) x );
            }

            stack.Push(x);

            return 0;
        }
    }

    internal class LambdaREALPART : Lambda
    {
        public override int Eval( Stack stack )
        {
            var narg = GetNarg( stack );

            stack.Push( GetAlgebraic( stack ).RealPart() );

            return 0;
        }
    }

    internal class LambdaIMAGPART : Lambda
    {
        public override int Eval( Stack stack )
        {
            var narg = GetNarg( stack );

            stack.Push( GetAlgebraic( stack ).ImagPart() );

            return 0;
        }
    }

    internal class LambdaCONJ : Lambda
    {
        public override int Eval( Stack stack )
        {
            var narg = GetNarg( stack );

            var arg = GetAlgebraic( stack );
            
            var res = arg.Conj();

            stack.Push( res );

            return 0;
        }
    }

    internal class LambdaANGLE : Lambda
    {
        public override int Eval( Stack stack )
        {
            var narg = GetNarg( stack );

            var x = GetAlgebraic( stack );

            var atan2 = Session.Proc.Store.GetValue( "atan2" );

            if ( !( atan2 is LambdaAlgebraic ) )
            {
                throw new SymbolicException( "Function ATAN2 not installed." );
            }

            stack.Push( ( ( LambdaAlgebraic ) atan2 ).SymEval( x.ImagPart(), x.RealPart() ) );

            return 0;
        }
    }

    internal class LambdaCFS : Lambda
    {
        public override int Eval( Stack stack )
        {
            var narg = GetNarg( stack );

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
            var narg = GetNarg( stack );

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
                    v = ( ( Polynomial ) f ).Var;
                }
                else if ( f is Rational )
                {
                    v = ( ( Rational ) f ).den.Var;
                }
                else
                {
                    throw new ParseException( "Could not determine Variable." );
                }
            }

            var df = f.Derive(v);

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
            var narg = GetNarg( stack );

            if ( narg != 3 )
            {
                throw new ParseException( "Usage: SUBST (a, b, c), substitutes a for b in c" );
            }

            var a = GetAlgebraic( stack );
            var b = GetPolynomial( stack );
            var c = GetAlgebraic( stack );

            var bx = b.Var;

            while ( bx is FunctionVariable )
            {
                var arg = ( ( FunctionVariable ) bx ).Var;

                if ( !( arg is Polynomial ) )
                {
                    throw new SymbolicException( $"Can not solve {b} for a variable." );
                }

                bx = ( ( Polynomial ) arg ).Var;
            }

            var sol = LambdaSOLVE.solve( a - b, bx );

            var res = new Algebraic[ sol.Length() ];

            for ( var n = 0; n < sol.Length(); n++ )
            {
                var y = sol[n];

                res[n] = c.Value( bx, y );
            }

            stack.Push( ( new Vector( res ) ).Reduce() );

            return 0;
        }
    }

    internal class LambdaCOEFF : Lambda
    {
        public override int Eval( Stack stack )
        {
            var narg = GetNarg( stack );

            if ( narg != 3 )
            {
                throw new ParseException( "Usage: COEFF (a, b, c), find coeff of b^c in a" );
            }

            var a = GetPolynomial( stack );
            var b = GetVariable( stack );
            var c_in = GetAlgebraic( stack );

            if ( c_in.IsScalar() )
            {
                stack.Push( a.GetCoeff( b, GetInteger( c_in ) ) );
            }
            else if ( c_in is Vector )
            {
                var c = ( Vector ) c_in;

                var v = new Algebraic[ c.Length() ];

                for ( var n = 0; n < v.Length; n++ )
                {
                    v[n] = a.GetCoeff( b, GetInteger( c[n] ) );
                }

                stack.Push( new Vector(v) );
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
            var narg = GetNarg( stack );

            if ( narg == 1 )
            {
                var x = GetAlgebraic( stack );

                if ( x.IsScalar() && !x.IsConstant() )
                {
                    throw new SymbolicException( $"Unknown variable dimension: {x}" );
                }

                var m = new Matrix(x);

                var addcols = ( m.Cols() > 1 );

                if ( narg > 1 )
                {
                    if ( GetInteger( stack ) == 2 )
                    {
                        addcols = false;
                    }
                }

                if ( addcols )
                {
                    var s = m.col(1);

                    for ( var n = 2; n <= m.Cols(); n++ )
                    {
                        s = s + m.col(n);
                    }

                    stack.Push(s);
                }
                else
                {
                    var s = m.row(1);

                    for ( var n = 2; n <= m.Rows(); n++ )
                    {
                        s = s + m.row(n);
                    }

                    stack.Push(s);
                }

                return 0;
            }

            if ( narg != 4 )
            {
                throw new ParseException( "Usage: SUM (exp, ind, lo, hi)" );
            }

            var exp = GetAlgebraic( stack );
            var v = GetVariable( stack );

            var lo = GetInteger( stack );
            var hi = GetInteger( stack );

            Algebraic sum = Symbol.ZERO;

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
            var narg = GetNarg( stack );

            if ( narg != 3 )
            {
                throw new ParseException( "Usage: LSUM (exp, ind, list)" );
            }

            var exp = GetAlgebraic( stack );
            var v = GetVariable( stack );
            var list = GetVector( stack );

            Algebraic sum = Symbol.ZERO;

            for ( var n = 0; n < list.Length(); n++ )
            {
                sum = sum + exp.Value( v, list[n] );
            }

            stack.Push( sum );

            return 0;
        }
    }

    internal class LambdaDIVIDE : Lambda
    {
        public override int Eval( Stack stack )
        {
            var size = GetNarg( stack );

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
                if ( p1 is Symbol && p2 is Symbol )
                {
                    a = ( ( Symbol ) p1 ).Div( p2, a );
                }
                else
                {
                    a[0] = Poly.polydiv( p1, p2 );
                    a[1] = ( p1 - a[0] ) * p2;
                }
            }

            stack.Push( new Vector(a) );

            return 0;
        }
    }

    internal class LambdaTAYLOR : Lambda
    {
        public override int Eval( Stack stack )
        {
            var narg = GetNarg( stack );

            if ( narg != 4 )
            {
                throw new ParseException( "Usage: TAYLOR (exp, var, pt, pow)" );
            }

            var exp = GetAlgebraic( stack );
            var v = GetVariable( stack );
            var pt = GetAlgebraic( stack );

            var pow = GetInteger( stack );

            var r = exp.Value( v, pt );
            var t = ( new Polynomial(v) ) - pt;

            double nf = 1.0;

            for ( var n = 1; n <= pow; n++ )
            {
                exp = exp.Derive(v);

                nf *= n;
                r = r + ( exp.Value( v, pt ) * t ^ n ) / new Complex( nf );
            }

            stack.Push(r);

            return 0;
        }
    }

    internal class LambdaSAVE : Lambda
    {
        public override int Eval( Stack stack )
        {
            var size = GetNarg( stack );

            if ( size < 2 )
            {
                throw new ParseException( "Usage: SAVE (filename,arg1,arg2,...,argi)" );
            }

            var filename = stack.Pop();

            try
            {
                var stream = Session.GetFileOutputStream( ( string ) filename, true );

                for ( var n = 1; n < size; n++ )
                {
                    var name = ( string ) stack.Pop();

                    if ( name.Equals( "ALL" ) )
                    {
                        var en = Session.Proc.Store.Keys.GetEnumerator();

                        while ( en.MoveNext() )
                        {
                            var key = en.Current;

                            if ( "pi".Equals( ( string ) key ) ) continue;

                            var val = Session.Proc.Store.GetValue( ( string ) key );

                            if ( val is Lambda ) continue;

                            var bytes = Encoding.UTF8.GetBytes( $"{key}:{val};\n" );

                            stream.Write( bytes, 0, bytes.Length );
                        }
                    }
                    else
                    {
                        var val = Session.Proc.Store.GetValue( name );

                        var bytes = Encoding.UTF8.GetBytes( $"{name}:{val};\n" );

                        stream.Write( bytes, 0, bytes.Length );
                    }
                }

                stream.Close();

                Session.Proc?.println( $"Wrote variables to {filename}" );
            }
            catch ( Exception ex )
            {
                throw new SymbolicException( $"Could not write to {filename} : {ex.Message}" );
            }

            return 0;
        }
    }

    internal class LambdaLOADFILE : Lambda
    {
        public override int Eval( Stack stack )
        {
            var narg = GetNarg( stack );

            if ( narg != 1 )
            {
                throw new ParseException( "Usage: LOADFILE (filename)" );
            }

            var filename = stack.Pop();

            if ( !( filename is string ) )
            {
                throw new SymbolicException( $"{filename} not a valid filename." );
            }

            try
            {
                ReadFile( ( string ) filename );

                Session.Proc?.println( $"Loaded Variables from {filename}" );
            }
            catch ( Exception ex )
            {
                throw new SymbolicException( $"Could not read from {filename}: {ex}" );
            }

            return 0;
        }

        public static void ReadFile( string fname )
        {
            // Try to load from resources.
            var assembly = Assembly.GetExecutingAssembly();

            var names = assembly.GetManifestResourceNames();

            var resName = names.FirstOrDefault( p => p == $"symcs.inc.{fname}" );

            if ( resName != null )
            {
                Read( assembly.GetManifestResourceStream( resName ) );

                return;
            }

            foreach ( string path in Store.Paths )
            {
                var full = Path.GetFullPath( Path.Combine( path, fname ) );

                if ( !File.Exists( full ) ) continue;

                Stream stream = new FileStream( full, FileMode.Open );

                Read( stream );

                return;
            }

            throw new IOException( $"Could not find {fname}." );
        }

        public static void Read( Stream stream )
        {
            var stack = Session.Proc.Stack;

            Session.Proc.Stack = new Stack();

            try
            {
                while ( true )
                {
                    var code = Session.Parser.compile( stream );

                    if ( code == null )
                    {
                        break;
                    }

                    Session.Proc.ProcessList( code, true );
                }

                stream.Close();

                Session.Proc.Stack = stack;
            }
            catch ( Exception ex )
            {
                Session.Proc.Stack = stack;

                throw new SymbolicException( ex.Message );
            }
        }
    }

    internal class LambdaRAT : LambdaAlgebraic
    {
        public override int Eval( Stack stack )
        {
            var narg = GetNarg( stack );

            var arg = GetAlgebraic( stack ).Reduce();

            if ( arg is Complex )
            {
                stack.Push( PreEval( ( Symbol ) arg ) );
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
            if ( a is Symbol )
            {
                return ( Symbol ) a.Rat();
            }

            return a.Map( this );
        }

        internal override Symbol PreEval( Symbol s )
        {
            return ( Symbol ) s.Rat();
        }
    }

    internal class LambdaSQFR : Lambda
    {
        public override int Eval( Stack stack )
        {
            var narg = GetNarg( stack );

            var f = GetAlgebraic( stack );

            if ( f is Symbol )
            {
                stack.Push(f);
                return 0;
            }

            if ( !( f is Polynomial ) )
            {
                throw new ParseException( "Argument to sqfr() must be polynomial." );
            }

            var poly = ( Polynomial ) f;

            f = poly.Rat();

            var fs = poly.square_free_dec( poly.Var );

            if ( fs == null )
            {
                stack.Push(f);
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
            var narg = GetNarg( stack );
            var x = GetAlgebraic( stack );

            if ( x is Vector )
            {
                x = new Polynomial( new SimpleVariable( "x" ), ( Vector ) x );
            }

            if ( !( x is Polynomial ) )
            {
                throw new SymbolicException( "Argument to allroots must be polynomial." );
            }

            var p = ( Polynomial ) ( ( Polynomial ) x ).Rat();
            var ps = p.square_free_dec( p.Var );

            Vector r;
            var v = new ArrayList();

            for ( var i = 0; i < ps.Length; i++ )
            {
                if ( ps[i] is Polynomial )
                {
                    r = ( ( Polynomial ) ps[i] ).Monic().roots();

                    for ( var k = 0; r != null && k < r.Length(); k++ )
                    {
                        for ( var j = 0; j <= i; j++ )
                        {
                            v.Add( r[k] );
                        }
                    }
                }
            }

            stack.Push( Vector.Create(v) );

            return 0;
        }
    }

    internal class LambdaDET : Lambda
    {
        public override int Eval( Stack stack )
        {
            var narg = GetNarg( stack );

            var m = new Matrix( GetAlgebraic( stack ) );

            stack.Push( m.det() );

            return 0;
        }
    }

    internal class LambdaEIG : Lambda
    {
        public override int Eval( Stack stack )
        {
            var narg = GetNarg( stack );

            var m = new Matrix( GetAlgebraic( stack ) );

            stack.Push( m.EigenValues() );

            return 0;
        }
    }

    internal class LambdaINV : Lambda
    {
        public override int Eval( Stack stack )
        {
            var narg = GetNarg( stack );

            var m = new Matrix( GetAlgebraic( stack ) );

            stack.Push( m.invert() );

            return 0;
        }
    }

    internal class LambdaPINV : Lambda
    {
        public override int Eval( Stack stack )
        {
            var narg = GetNarg( stack );

            var m = new Matrix( GetAlgebraic( stack ) );

            stack.Push( m.pseudoinverse() );

            return 0;
        }
    }

    internal class LambdaHILB : Lambda
    {
        public override int Eval( Stack stack )
        {
            var narg = GetNarg( stack );
            var n = GetInteger( stack );

            var a = Matrix.CreateRectangularArray<Algebraic>( n, n );

            for ( var i = 0; i < n; i++ )
            {
                for ( var k = 0; k < n; k++ )
                {
                    a[i][k] = new Number( 1L, i + k + 1 );
                }
            }

            stack.Push( new Matrix(a) );

            return 0;
        }
    }

    internal class LambdaLU : Lambda
    {
        public override int Eval( Stack stack )
        {
            var narg = GetNarg( stack );

            var m = ( new Matrix( GetAlgebraic( stack ) ) ).copy();

            var B = new Matrix( 1, 1 );
            var P = new Matrix( 1, 1 );

            m.rank_decompose( B, P );

            if ( length != 2 && length != 3 )
            {
                throw new SymbolicException( "Usage: [l,u,p] = LU( Matrix )." );
            }

            if ( length >= 2 )
            {
                stack.Push(B);
                stack.Push(m);

                if ( length == 3 )
                {
                    stack.Push(P);
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
            catch ( SymbolicException )
            {
                if ( !( arg.Depends(x) ) )
                {
                    throw new SymbolicException( "Expression in function does not depend on Variable." );
                }

                if ( !( arg is Polynomial ) || ( ( Polynomial ) arg ).Degree() != 2 || !( ( Polynomial ) arg ).IsRat(x) )
                {
                    throw new SymbolicException( "Can not integrate function " );
                }

                Algebraic xp = new Polynomial(x);

                var X = ( Polynomial ) arg;
                var y = evalx( intrule2, xp );

                y = y.Value( new SimpleVariable( "X" ), X );
                y = y.Value( new SimpleVariable( "a" ), X[2] );
                y = y.Value( new SimpleVariable( "b" ), X[1] );
                y = y.Value( new SimpleVariable( "c" ), X[0] );

                y = ( new SqrtExpand() ).SymEval(y);

                return y;
            }
        }

        internal override Symbol PreEval( Symbol x )
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

            var phi = Math.atan2( z.Im, z.Re ) / 2.0;

            return new Complex( sr * Math.cos( phi ), sr * Math.sin( phi ) );
        }

        internal override Algebraic SymEval( Algebraic x )
        {
            if ( x.Equals( Symbol.ONE ) || x.Equals( Symbol.ZERO ) )
            {
                return x;
            }

            if ( x.Equals( Symbol.MINUS ) )
            {
                return Symbol.IONE;
            }

            if ( x is Symbol )
            {
                return fzexakt( ( Symbol ) x );
            }

            if ( x is Polynomial
                && ( ( Polynomial ) x ).Degree() == 1
                && ( ( Polynomial ) x )[0].Equals( Symbol.ZERO )
                && ( ( Polynomial ) x )[1].Equals( Symbol.ONE )
                && ( ( Polynomial ) x ).Var is FunctionVariable
                && ( ( FunctionVariable ) ( ( Polynomial ) x ).Var ).Name.Equals( "exp" ) )
            {
                return FunctionVariable.Create( "exp", ( ( FunctionVariable ) ( ( Polynomial ) x ).Var ).Var / Symbol.TWO );
            }

            return null;
        }

        internal virtual Algebraic fzexakt( Symbol x )
        {
            if ( x is Number && !x.IsComplex() )
            {
                if ( x < Symbol.ZERO )
                {
                    var r = fzexakt( ( Symbol ) ( -x ) );

                    if ( r != null )
                    {
                        return Symbol.IONE * r;
                    }

                    return r;
                }

                var nom = ( ( Number ) x ).real[0].longValue();
                var den = ( ( Number ) x ).real[1].longValue();

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

            foreach ( long t1 in t )
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
            if ( a is Symbol )
            {
                return PreEval( ( Symbol ) a );
            }

            return null;
        }

        internal override Symbol PreEval( Symbol s )
        {
            return s.Smaller( Symbol.ZERO ) ? Symbol.MINUS : Symbol.ONE;
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
            if ( a is Symbol )
            {
                return PreEval( ( Symbol ) a );
            }

            return FunctionVariable.Create( "sqrt", a * a.Conj() );
        }

        internal override Symbol PreEval( Symbol x )
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

            if ( p.Var is SimpleVariable )
            {
                return p.Map( this );
            }

            var f = ( FunctionVariable ) p.Var;

            var lx = Session.Proc.Store.GetValue( f.Name );

            if ( !( lx is UserFunction ) )
            {
                return p.Map( this );
            }

            var la = ( UserFunction ) lx;

            if ( !( la.Body is Algebraic ) )
            {
                return x1;
            }

            var body = la.Body;

            Algebraic x;

            if ( la.Args.Length == 1 )
            {
                x = body.Value( la.Args[0], f.Var );
            }
            else if ( f.Var is Vector && ( ( Vector ) f.Var ).Length() == la.Args.Length )
            {
                x = la.fv( ( Vector ) f.Var );
            }
            else
            {
                throw new SymbolicException( $"Wrong argument to function {la.Name}" );
            }

            Algebraic r = Symbol.ZERO;

            for ( var n = p.Coeffs.Length - 1; n > 0; n-- )
            {
                r = ( r + SymEval( p[n] ) ) * x;
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
            var narg = GetNarg( stack );

            var val = new object[ narg ];

            for ( var n = 0; n < narg; n++ )
            {
                val[n] = stack.Pop();
            }

            for ( var n = narg - 1; n >= 0; n-- )
            {
                var name = GetSymbol( stack );

                if ( !name.StartsWith( "$" ) )
                {
                    throw new SymbolicException( "Illegal lvalue: " + name );
                }

                name = name.Substring( 1 );

                var idxq = stack.Count > 0 && stack.Peek() is int?;

                if ( !idxq )
                {
                    Session.Proc.Store.PutValue( name, val[n] );

                    if ( val[n] is Algebraic )
                    {
                        ( ( Algebraic ) val[n] ).Name = name;
                    }
                }
                else
                {
                    if ( !( val[n] is Algebraic ) )
                    {
                        throw new SymbolicException( "No index allowed here: " + val[n] );
                    }

                    var rhs = new Matrix( ( Algebraic ) val[n] );
                    var lhs = new Matrix( ( Algebraic ) Session.Proc.Store.GetValue( name ) );

                    var idx = Index.createIndex( stack, lhs );

                    lhs.Insert( rhs, idx );

                    val[n] = lhs.Reduce();

                    Session.Proc.Store.PutValue( name, val[n] );
                }
            }

            for ( var n = 0; n < narg; n++ )
            {
                stack.Push( val[n] );
            }

            return 0;
        }

        internal static int lambdap( Stack stack, Lambda op )
        {
            var narg = GetNarg( stack );

            var y = stack.Pop();

            var name = GetSymbol( stack );

            if ( !name.StartsWith( "$" ) )
            {
                throw new SymbolicException( "Illegal lvalue: " + name );
            }

            var t = new List { name, name.Substring( 1 ), y, 2, op, 1, Operator.get( "=" ).Lambda };

            Session.Proc.ProcessList( t, true );

            return 0;
        }

        internal static int lambdai( Stack st, bool sign, bool pre )
        {
            var narg = GetNarg( st );

            var name = GetSymbol( st );

            if ( !name.StartsWith( "$" ) )
            {
                throw new SymbolicException( "Illegal lvalue: " + name );
            }

            object p = null;

            if ( !pre )
            {
                p = Session.Proc.Store.GetValue( name.Substring(1) );
            }

            var t = new List { name, name.Substring(1), Symbol.ONE, 2, Operator.get( sign ? "+" : "-" ).Lambda, 1, Operator.get( "=" ).Lambda };

            Session.Proc.ProcessList( t, true );

            if ( !pre && p != null )
            {
                if ( p is Algebraic )
                {
                    ( ( Algebraic ) p ).Name = null;
                }

                st.Pop();
                st.Push(p);
            }

            return 0;
        }
    }

    internal class LambdaWHO : Lambda
    {
        public override int Eval( Stack stack )
        {
            Session.Proc.println( Session.Proc.Store.ToString() );

            return 0;
        }
    }

    internal class LambdaADDPATH : Lambda
    {
        public override int Eval( Stack stack )
        {
            var narg = GetNarg( stack );

            while ( narg-- > 0 )
            {
                var s = stack.Pop();

                if ( !( s is string ) )
                {
                    throw new SymbolicException( "Usage: ADDPATH( dir1, dir2, ... )" );
                }

                Session.Proc.Store.AddPath( ( ( string ) s ).Substring(1) );
            }

            return 0;
        }
    }

    internal class LambdaPATH : Lambda
    {
        public override int Eval( Stack stack )
        {
            var s = string.Join( ":", Store.Paths.Cast<string>().ToArray() );

            Session.Proc.println(s);

            return 0;
        }
    }
}

using System;
using System.Collections;
using System.Linq;

using Tiny.Numeric;

namespace Tiny.Symbolic
{
    public class Operator
    {
        public int precedence;
        internal int associativity;
        internal int type;

        internal string mnemonic;
        public string symbol;

        internal Lambda func = null;

        public static Operator[] OPS = new Operator[ 0 ];

        public virtual bool unary()
        {
            return ( type & Flags.UNARY ) != 0;
        }

        public virtual bool binary()
        {
            return ( type & Flags.BINARY ) != 0;
        }

        public virtual bool ternary()
        {
            return ( type & Flags.TERNARY ) != 0;
        }

        public virtual bool lvalue()
        {
            return ( type & Flags.LVALUE ) != 0;
        }

        public virtual bool list()
        {
            return ( type & Flags.LIST ) != 0;
        }

        public virtual bool left_right()
        {
            return associativity == Flags.LEFT_RIGHT;
        }

        public Operator( string mnemonic, string symbol, int precedence, int associativity, int type )
        {
            this.mnemonic = mnemonic;
            this.symbol = symbol;
            this.precedence = precedence;
            this.associativity = associativity;
            this.type = type;
        }

        public override string ToString()
        {
            return symbol;
        }

        public static Operator get( object text_in )
        {
            if ( !( text_in is string ) )
            {
                return null;
            }

            var text = ( string ) text_in;

            return OPS.FirstOrDefault( op => text.StartsWith( op.symbol, StringComparison.Ordinal ) );

        }

        public static Operator get( object text_in, int pos )
        {
            if ( !( text_in is string ) )
            {
                return null;
            }

            var text = ( string ) text_in;

            foreach ( var op in OPS )
            {
                if ( text.StartsWith( op.symbol, StringComparison.Ordinal ) )
                {
                    switch ( pos )
                    {
                        case Flags.START:
                            if ( op.unary() && op.left_right() )
                            {
                                return op;
                            }
                            continue;

                        case Flags.END:
                            if ( op.unary() && !op.left_right() )
                            {
                                return op;
                            }
                            continue;

                        case Flags.MID:
                            if ( op.binary() || op.ternary() )
                            {
                                return op;
                            }
                            continue;
                    }
                }
            }

            return null;
        }

        public virtual Lambda Lambda
        {
            get
            {
                if ( func == null )
                {
                    try
                    {
                        var type = Type.GetType( mnemonic );

                        func = ( Lambda ) Activator.CreateInstance( type );
                    }
                    catch ( Exception )
                    {
                    }
                }

                return func;
            }
        }
    }

    internal class ADJ : Lambda
    {
        public override int Eval( Stack st )
        {
            int narg = GetNarg( st );

            var m = new Matrix( GetAlgebraic( st ) );

            st.Push( m.adjunkt().Reduce() );

            return 0;
        }
    }

    internal class TRN : Lambda
    {
        public override int Eval( Stack st )
        {
            int narg = GetNarg( st );
            var m = new Matrix( GetAlgebraic( st ) );
            st.Push( m.transpose().Reduce() );
            return 0;
        }
    }

    internal class FCT : LambdaAlgebraic
    {
        public override int Eval( Stack st )
        {
            int narg = GetNarg( st );
            var arg = GetAlgebraic( st );

            if ( arg is Symbol )
            {
                st.Push( PreEval( ( Symbol ) arg ) );
            }
            else
            {
                st.Push( FunctionVariable.Create( "factorial", arg ) );
            }

            return 0;
        }

        internal override Algebraic SymEval( Algebraic x )
        {
            if ( x is Symbol )
            {
                return PreEval( ( Symbol ) x );
            }

            return null;
        }

        internal override Symbol PreEval( Symbol x )
        {
            if ( !x.IsInteger() || x.Smaller( Symbol.ZERO ) )
            {
                throw new SymbolicException( "Argument to factorial must be a positive integer, is " + x );
            }

            Algebraic r = Symbol.ONE;

            while ( Symbol.ONE.Smaller( x ) )
            {
                r = r * x;
                x = ( Symbol ) ( x - Symbol.ONE );
            }

            return ( Symbol ) r;
        }
    }

    internal class LambdaFACTORIAL : FCT
    {
    }

    internal class FCN : Lambda
    {
        public override int Eval( Stack st )
        {
            int narg = GetNarg( st );
            var code_in = GetList( st );

            var fname = GetSymbol( st ).Substring( 1 );
            int nvar = GetNarg( st );

            var vars = new SimpleVariable[ nvar ];

            for ( int i = 0; i < nvar; i++ )
            {
                vars[ i ] = new SimpleVariable( GetSymbol( st ) );
            }

            Lambda func = null;
            var env = new Store();
            var ups = new Stack();

            object y = null;

            if ( nvar == 1 )
            {
                int res = UserProgram.process_block( code_in, ups, env, false );

                if ( res != Processor.ERROR )
                {
                    y = ups.Pop();
                }
            }

            if ( y is Algebraic )
            {
                func = new UserFunction( fname, vars, ( Algebraic ) y, null, null );
            }
            else
            {
                func = new UserProgram( fname, vars, code_in, null, env, ups );
            }

            Globals.Proc.Store.putValue( fname, func );
            st.Push( fname );

            return 0;
        }
    }

    internal class POW : LambdaAlgebraic
    {
        internal override Algebraic SymEval( Algebraic x, Algebraic y )
        {
            if ( x.Equals( Symbol.ZERO ) )
            {
                if ( y.Equals( Symbol.ZERO ) )
                {
                    return Symbol.ONE;
                }

                return Symbol.ZERO;
            }
            if ( y is Symbol && ( ( Symbol ) y ).IsInteger() )
            {
                return x.Pow( ( ( Symbol ) y ).ToInt() );
            }

            return FunctionVariable.Create( "exp", FunctionVariable.Create( "log", x ) * y );
        }
    }

    internal class PPR : Lambda
    {
        public override int Eval( Stack st )
        {
            return ASS.lambdai( st, true, false );
        }
    }

    internal class MMR : Lambda
    {
        public override int Eval( Stack st )
        {
            return ASS.lambdai( st, false, false );
        }
    }

    internal class PPL : Lambda
    {
        public override int Eval( Stack st )
        {
            return ASS.lambdai( st, true, true );
        }
    }

    internal class MML : Lambda
    {
        public override int Eval( Stack st )
        {
            return ASS.lambdai( st, false, true );
        }
    }

    internal class ADE : Lambda
    {
        public override int Eval( Stack st )
        {
            return ASS.lambdap( st, Operator.get( "+" ).Lambda );
        }
    }

    internal class SUE : Lambda
    {
        public override int Eval( Stack st )
        {
            return ASS.lambdap( st, Operator.get( "-" ).Lambda );
        }
    }

    internal class MUE : Lambda
    {
        public override int Eval( Stack st )
        {
            return ASS.lambdap( st, Operator.get( "*" ).Lambda );
        }
    }

    internal class DIE : Lambda
    {
        public override int Eval( Stack st )
        {
            return ASS.lambdap( st, Operator.get( "/" ).Lambda );
        }
    }

    internal class ADD : LambdaAlgebraic
    {
        internal override Algebraic SymEval( Algebraic x )
        {
            return x;
        }

        internal override Algebraic SymEval( Algebraic x, Algebraic y )
        {
            return x + y;
        }

        internal override Symbol PreEval( Symbol x )
        {
            return ( Symbol ) SymEval( x );
        }
    }

    internal class SUB : LambdaAlgebraic
    {
        internal override Algebraic SymEval( Algebraic x )
        {
            return -x;
        }

        internal override Algebraic SymEval( Algebraic x, Algebraic y )
        {
            return x - y;
        }

        internal override Symbol PreEval( Symbol x )
        {
            return ( Symbol ) SymEval( x );
        }
    }

    internal class MUL : LambdaAlgebraic
    {
        internal override Algebraic SymEval( Algebraic x, Algebraic y )
        {
            return x * y;
        }
    }

    internal class MMU : LambdaAlgebraic
    {
        public override int Eval( Stack stack )
        {
            int narg = GetNarg( stack );

            if ( narg != 2 )
            {
                throw new ParseException( "Wrong number of arguments for \"*\"." );
            }

            var b = GetAlgebraic( stack );
            var a = GetAlgebraic( stack );

            if ( b.IsScalar() )
            {
                stack.Push( a * b );
            }
            else if ( a.IsScalar() )
            {
                stack.Push( b * a );
            }
            else if ( a is Vector && b is Vector )
            {
                stack.Push( a * b );
            }
            else
            {
                stack.Push( ( new Matrix( a ) * new Matrix( b ) ).Reduce() );
            }

            return 0;
        }
    }

    internal class MPW : LambdaAlgebraic
    {
        public override int Eval( Stack stack )
        {
            int narg = GetNarg( stack );

            var a = GetAlgebraic( stack );
            var b = GetAlgebraic( stack );

            if ( a.IsScalar() && b.IsScalar() )
            {
                stack.Push( new POW().SymEval( b, a ) );

                return 0;
            }

            if ( !( a is Symbol ) || !( ( Symbol ) a ).IsInteger() )
            {
                throw new SymbolicException( "Wrong arguments to function Matrixpow." );
            }

            stack.Push( ( new Matrix( b ) ).mpow( ( ( Symbol ) a ).ToInt() ) );

            return 0;
        }
    }

    internal class DIV : LambdaAlgebraic
    {
        internal override Algebraic SymEval( Algebraic x, Algebraic y )
        {
            return x / y;
        }
    }

    internal class MDR : Lambda
    {
        public override int Eval( Stack stack )
        {
            int narg = GetNarg( stack );

            if ( narg != 2 )
            {
                throw new ParseException( "Wrong number of arguments for \"/\"." );
            }

            var b = GetAlgebraic( stack );

            var a = new Matrix( GetAlgebraic( stack ) );

            stack.Push( ( a / b ).Reduce() );

            return 0;
        }
    }

    internal class MDL : Lambda
    {
        public override int Eval( Stack stack )
        {
            int narg = GetNarg( stack );

            if ( narg != 2 )
            {
                throw new ParseException( "Wrong number of arguments for \"\\\"." );
            }

            var b = new Matrix( GetAlgebraic( stack ) );
            var a = new Matrix( GetAlgebraic( stack ) );

            stack.Push( ( ( Matrix ) ( b.transpose() / a.transpose() ) ).transpose().Reduce() );

            return 0;
        }
    }

    internal class EQU : LambdaAlgebraic
    {
        internal override Algebraic SymEval( Symbol x, Symbol y )
        {
            return y == x ? Symbol.ONE : Symbol.ZERO;
        }
    }

    internal class NEQ : LambdaAlgebraic
    {
        internal override Algebraic SymEval( Symbol x, Symbol y )
        {
            return y == x ? Symbol.ZERO : Symbol.ONE;
        }
    }

    internal class GEQ : LambdaAlgebraic
    {
        internal override Algebraic SymEval( Symbol x, Symbol y )
        {
            return x < y ? Symbol.ZERO : Symbol.ONE;
        }
    }

    internal class GRE : LambdaAlgebraic
    {
        internal override Algebraic SymEval( Symbol x, Symbol y )
        {
            return y < x ? Symbol.ONE : Symbol.ZERO;
        }
    }

    internal class LEQ : LambdaAlgebraic
    {
        internal override Algebraic SymEval( Symbol x, Symbol y )
        {
            return y < x ? Symbol.ZERO : Symbol.ONE;
        }
    }

    internal class LES : LambdaAlgebraic
    {
        internal override Algebraic SymEval( Symbol x, Symbol y )
        {
            return x < y ? Symbol.ONE : Symbol.ZERO;
        }
    }

    internal class NOT : LambdaAlgebraic
    {
        internal override Symbol PreEval( Symbol x )
        {
            return Equals( x, Symbol.ZERO ) ? Symbol.ONE : Symbol.ZERO;
        }
    }

    internal class OR : LambdaAlgebraic
    {
        internal override Algebraic SymEval( Symbol x, Symbol y )
        {
            return Equals( x, Symbol.ONE ) || Equals( y, Symbol.ONE ) ? Symbol.ONE : Symbol.ZERO;
        }
    }

    internal class AND : LambdaAlgebraic
    {
        internal override Algebraic SymEval( Symbol x, Symbol y )
        {
            return Equals( x, Symbol.ONE ) && Equals( y, Symbol.ONE ) ? Symbol.ONE : Symbol.ZERO;
        }
    }

    internal class LambdaGAMMA : LambdaAlgebraic
    {
        internal override Symbol PreEval( Symbol x )
        {
            return new Complex( Sfun.gamma( x.ToComplex().Re ) );
        }
    }

    internal class LambdaGAMMALN : LambdaAlgebraic
    {
        internal override Symbol PreEval( Symbol x )
        {
            return new Complex( Sfun.logGamma( x.ToComplex().Re ) );
        }
    }
}

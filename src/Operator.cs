using System;
using System.Collections;
using System.Linq;

public class Operator : Constants
{
    internal int precedence;
    internal int associativity;
    internal int type;

    internal string mnemonic;
    internal string symbol;

    internal Lambda func = null;

    internal static Operator[] OPS = new Operator[0];

    public virtual bool unary()
    {
        return ( type & Fields.UNARY ) != 0;
    }

    public virtual bool binary()
    {
        return ( type & Fields.BINARY ) != 0;
    }

    public virtual bool ternary()
    {
        return ( type & Fields.TERNARY ) != 0;
    }

    public virtual bool lvalue()
    {
        return ( type & Fields.LVALUE ) != 0;
    }

    public virtual bool list()
    {
        return ( type & Fields.LIST ) != 0;
    }

    public virtual bool left_right()
    {
        return associativity == Fields.LEFT_RIGHT;
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

    internal static Operator get( object text_in )
    {
        if ( !( text_in is string ) )
        {
            return null;
        }

        var text = ( string ) text_in;

        return OPS.FirstOrDefault( op => text.StartsWith( op.symbol, StringComparison.Ordinal ) );

    }

    internal static Operator get( object text_in, int pos )
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
                    case Fields.START:
                        if ( op.unary() && op.left_right() )
                        {
                            return op;
                        }
                        continue;

                    case Fields.END:
                        if ( op.unary() && !op.left_right() )
                        {
                            return op;
                        }
                        continue;

                    case Fields.MID:
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

    internal virtual Lambda Lambda
    {
        get
        {
            if ( func == null )
            {
                try
                {
                    var c = Type.GetType( mnemonic );

                    func = ( Lambda ) Activator.CreateInstance(c);
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
    public override int lambda( Stack st )
    {
        int narg = getNarg( st );

        var m = new Matrix( getAlgebraic( st ) );

        st.Push( m.adjunkt().reduce() );

        return 0;
    }
}

internal class TRN : Lambda
{
    public override int lambda( Stack st )
    {
        int narg = getNarg( st );
        var m = new Matrix( getAlgebraic( st ) );
        st.Push( m.transpose().reduce() );
        return 0;
    }
}

internal class FCT : LambdaAlgebraic
{
    public override int lambda( Stack st )
    {
        int narg = getNarg( st );
        var arg = getAlgebraic( st );

        if ( arg is Zahl )
        {
            st.Push( f( ( Zahl ) arg ) );
        }
        else
        {
            st.Push( FunctionVariable.create( "factorial", arg ) );
        }

        return 0;
    }

    internal override Algebraic f_exakt( Algebraic x )
    {
        if ( x is Zahl )
        {
            return f( ( Zahl ) x );
        }

        return null;
    }

    internal override Zahl f( Zahl x )
    {
        if ( !x.integerq() || x.smaller( Zahl.ZERO ) )
        {
            throw new JasymcaException( "Argument to factorial must be a positive integer, is " + x );
        }

        Algebraic r = Zahl.ONE;

        while ( Zahl.ONE.smaller(x) )
        {
            r = r.mult(x);
            x = ( Zahl ) x.sub( Zahl.ONE );
        }

        return ( Zahl ) r;
    }
}

internal class LambdaFACTORIAL : FCT
{
}

internal class FCN : Lambda
{
    public override int lambda( Stack st )
    {
        int narg = getNarg( st );
        var code_in = getList( st );

        var fname = getSymbol( st ).Substring( 1 );
        int nvar = getNarg( st );

        var vars = new SimpleVariable[ nvar ];

        for ( int i = 0; i < nvar; i++ )
        {
            vars[i] = new SimpleVariable( getSymbol( st ) );
        }

        Lambda func = null;
        var env = new Environment();
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

        pc.env.putValue( fname, func );
        st.Push( fname );

        return 0;
    }
}

internal class POW : LambdaAlgebraic
{
    internal override Algebraic f_exakt( Algebraic x, Algebraic y )
    {
        if ( x.Equals( Zahl.ZERO ) )
        {
            if ( y.Equals( Zahl.ZERO ) )
            {
                return Zahl.ONE;
            }

            return Zahl.ZERO;
        }
        if ( y is Zahl && ( ( Zahl ) y ).integerq() )
        {
            return x.pow_n( ( ( Zahl ) y ).intval() );
        }

        return FunctionVariable.create( "exp", FunctionVariable.create( "log", x ).mult( y ) );
    }
}

internal class PPR : Lambda
{
    public override int lambda( Stack st )
    {
        return ASS.lambdai( st, true, false );
    }
}

internal class MMR : Lambda
{
    public override int lambda( Stack st )
    {
        return ASS.lambdai( st, false, false );
    }
}

internal class PPL : Lambda
{
    public override int lambda( Stack st )
    {
        return ASS.lambdai( st, true, true );
    }
}

internal class MML : Lambda
{
    public override int lambda( Stack st )
    {
        return ASS.lambdai( st, false, true );
    }
}

internal class ADE : Lambda
{
    public override int lambda( Stack st )
    {
        return ASS.lambdap( st, Operator.get( "+" ).Lambda );
    }
}

internal class SUE : Lambda
{
    public override int lambda( Stack st )
    {
        return ASS.lambdap( st, Operator.get( "-" ).Lambda );
    }
}

internal class MUE : Lambda
{
    public override int lambda( Stack st )
    {
        return ASS.lambdap( st, Operator.get( "*" ).Lambda );
    }
}

internal class DIE : Lambda
{
    public override int lambda( Stack st )
    {
        return ASS.lambdap( st, Operator.get( "/" ).Lambda );
    }
}

internal class ADD : LambdaAlgebraic
{
    internal override Algebraic f_exakt( Algebraic x )
    {
        return x;
    }

    internal override Algebraic f_exakt( Algebraic x, Algebraic y )
    {
        return x.add(y);
    }

    internal override Zahl f( Zahl x )
    {
        return ( Zahl ) f_exakt(x);
    }
}

internal class SUB : LambdaAlgebraic
{
    internal override Algebraic f_exakt( Algebraic x )
    {
        return x.mult( Zahl.MINUS );
    }

    internal override Algebraic f_exakt( Algebraic x, Algebraic y )
    {
        return x.add( y.mult( Zahl.MINUS ) );
    }

    internal override Zahl f( Zahl x )
    {
        return ( Zahl ) f_exakt(x);
    }
}

internal class MUL : LambdaAlgebraic
{
    internal override Algebraic f_exakt( Algebraic x, Algebraic y )
    {
        return x.mult(y);
    }
}

internal class MMU : LambdaAlgebraic
{
    public override int lambda( Stack st )
    {
        int narg = getNarg( st );

        if ( narg != 2 )
        {
            throw new ParseException( "Wrong number of arguments for \"*\"." );
        }

        var b = getAlgebraic( st );
        var a = getAlgebraic( st );

        if ( b.scalarq() )
        {
            st.Push( a.mult( b ) );
        }
        else if ( a.scalarq() )
        {
            st.Push( b.mult( a ) );
        }
        else if ( a is Vektor && b is Vektor )
        {
            st.Push( a.mult( b ) );
        }
        else
        {
            st.Push( ( new Matrix(a) ).mult( new Matrix(b) ).reduce() );
        }

        return 0;
    }
}

internal class MPW : LambdaAlgebraic
{
    public override int lambda( Stack st )
    {
        int narg = getNarg( st );

        var a = getAlgebraic( st );
        var b = getAlgebraic( st );

        if ( a.scalarq() && b.scalarq() )
        {
            st.Push( ( new POW() ).f_exakt( b, a ) );

            return 0;
        }

        if ( !( a is Zahl ) || !( ( Zahl ) a ).integerq() )
        {
            throw new JasymcaException( "Wrong arguments to function Matrixpow." );
        }

        st.Push( ( new Matrix( b ) ).mpow( ( ( Zahl ) a ).intval() ) );

        return 0;
    }
}

internal class DIV : LambdaAlgebraic
{
    internal override Algebraic f_exakt( Algebraic x, Algebraic y )
    {
        return x.div( y );
    }
}

internal class MDR : Lambda
{
    public override int lambda( Stack st )
    {
        int narg = getNarg( st );

        if ( narg != 2 )
        {
            throw new ParseException( "Wrong number of arguments for \"/\"." );
        }

        var b = getAlgebraic( st );

        var a = new Matrix( getAlgebraic( st ) );

        st.Push( a.div( b ).reduce() );

        return 0;
    }
}

internal class MDL : Lambda
{
    public override int lambda( Stack st )
    {
        int narg = getNarg( st );

        if ( narg != 2 )
        {
            throw new ParseException( "Wrong number of arguments for \"\\\"." );
        }

        var b = new Matrix( getAlgebraic( st ) );
        var a = new Matrix( getAlgebraic( st ) );

        st.Push( ( ( Matrix ) b.transpose().div( a.transpose() ) ).transpose().reduce() );

        return 0;
    }
}

internal class EQU : LambdaAlgebraic
{
    internal override Algebraic f_exakt( Algebraic x1, Algebraic y1 )
    {
        var x = ensure_Zahl( x1 );
        var y = ensure_Zahl( y1 );

        return y.Equals( x ) ? Zahl.ONE : Zahl.ZERO;
    }
}

internal class NEQ : LambdaAlgebraic
{
    internal override Algebraic f_exakt( Algebraic x1, Algebraic y1 )
    {
        var x = ensure_Zahl( x1 );
        var y = ensure_Zahl( y1 );

        return y.Equals( x ) ? Zahl.ZERO : Zahl.ONE;
    }
}

internal class GEQ : LambdaAlgebraic
{
    internal override Algebraic f_exakt( Algebraic x1, Algebraic y1 )
    {
        var x = ensure_Zahl( x1 );
        var y = ensure_Zahl( y1 );

        return x.smaller( y ) ? Zahl.ZERO : Zahl.ONE;
    }
}

internal class GRE : LambdaAlgebraic
{
    internal override Algebraic f_exakt( Algebraic x1, Algebraic y1 )
    {
        var x = ensure_Zahl( x1 );
        var y = ensure_Zahl( y1 );

        return y.smaller( x ) ? Zahl.ONE : Zahl.ZERO;
    }
}

internal class LEQ : LambdaAlgebraic
{
    internal override Algebraic f_exakt( Algebraic x1, Algebraic y1 )
    {
        var x = ensure_Zahl( x1 );
        var y = ensure_Zahl( y1 );

        return y.smaller( x ) ? Zahl.ZERO : Zahl.ONE;
    }
}

internal class LES : LambdaAlgebraic
{
    internal override Algebraic f_exakt( Algebraic x1, Algebraic y1 )
    {
        var x = ensure_Zahl( x1 );
        var y = ensure_Zahl( y1 );

        return x.smaller( y ) ? Zahl.ONE : Zahl.ZERO;
    }
}

internal class NOT : LambdaAlgebraic
{
    internal override Zahl f( Zahl x )
    {
        return x.Equals( Zahl.ZERO ) ? Zahl.ONE : Zahl.ZERO;
    }
}

internal class OR : LambdaAlgebraic
{
    internal override Algebraic f_exakt( Algebraic x1, Algebraic y1 )
    {
        var x = ensure_Zahl( x1 );
        var y = ensure_Zahl( y1 );

        return x.Equals( Zahl.ONE ) || y.Equals( Zahl.ONE ) ? Zahl.ONE : Zahl.ZERO;
    }
}

internal class AND : LambdaAlgebraic
{
    internal override Algebraic f_exakt( Algebraic x1, Algebraic y1 )
    {
        var x = ensure_Zahl( x1 );
        var y = ensure_Zahl( y1 );

        return x.Equals( Zahl.ONE ) && y.Equals( Zahl.ONE ) ? Zahl.ONE : Zahl.ZERO;
    }
}

internal class LambdaGAMMA : LambdaAlgebraic
{
    internal override Zahl f( Zahl x )
    {
        return new Unexakt( Sfun.gamma( x.unexakt().real ) );
    }
}

internal class LambdaGAMMALN : LambdaAlgebraic
{
    internal override Zahl f( Zahl x )
    {
        return new Unexakt( Sfun.logGamma( x.unexakt().real ) );
    }
}

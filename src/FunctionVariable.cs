using System;

public class FunctionVariable : Variable
{

    #region Properties

    public string Name { get; private set; }
    public Algebraic Var { get; private set; }
    public LambdaAlgebraic AlgLambda { get; private set; }

    #endregion

    public FunctionVariable( string fname, Algebraic fvar, LambdaAlgebraic flambda )
    {
        this.Name = fname;
        this.Var = fvar;
        this.AlgLambda = flambda;
    }

    public override Algebraic Derive( Variable v )
    {
        if ( Equals(v) )
        {
            return Symbolic.ONE;
        }

        if ( !Var.Depends( v ) )
        {
            return Symbolic.ZERO;
        }

        if ( AlgLambda == null )
        {
            throw new JasymcaException( "Can not differentiate " + Name + "  : No definition." );
        }

        var diffrule = AlgLambda.diffrule;

        if ( diffrule == null )
        {
            throw new JasymcaException( "Can not differentiate " + Name + " : No rule available." );
        }

        var y = Lambda.evalx( diffrule, Var );

        return y * Var.Derive(v);
    }

    public virtual Algebraic Integrate( Variable x )
    {
        Var = Var.Reduce();

        if ( AlgLambda == null )
        {
            throw new JasymcaException( "Can not integrate " + Name );
        }

        return AlgLambda.Integrate( Var, x );
    }

    public static Algebraic Create( string f, Algebraic arg )
    {
        arg = arg.Reduce();

        var fl = Lambda.pc.env.getValue( f );

        if ( fl != null && fl is LambdaAlgebraic )
        {
            var r = ( ( LambdaAlgebraic ) fl ).SymEval( arg );

            if ( r != null )
            {
                return r;
            }

            if ( arg is Complex )
            {
                return ( ( LambdaAlgebraic ) fl ).PreEval( ( Symbolic ) arg );
            }
        }
        else
        {
            fl = null;
        }

        return new Polynomial( new FunctionVariable( f, arg, ( LambdaAlgebraic ) fl ) );
    }

    public override bool Equals( object x )
    {
        return x is FunctionVariable && Name.Equals( ( ( FunctionVariable ) x ).Name ) && Var.Equals( ( ( FunctionVariable ) x ).Var );
    }

    public override Algebraic Value( Variable @var, Algebraic x )
    {
        if ( Equals( @var ) )
        {
            return x;
        }
        else
        {
            x = Var.Value( @var, x );

            var r = AlgLambda.SymEval( x );

            if ( r != null )
            {
                return r;
            }

            if ( x is Complex )
            {
                return AlgLambda.PreEval( ( Symbolic ) x );
            }

            return new Polynomial( new FunctionVariable( Name, x, AlgLambda ) );
        }
    }

    public override bool Smaller( Variable v )
    {
        if ( v == SimpleVariable.top )
        {
            return true;
        }

        if ( v is SimpleVariable )
        {
            return false;
        }

        if ( !( ( FunctionVariable ) v ).Name.Equals( Name ) )
        {
            return Name.CompareTo( ( ( FunctionVariable ) v ).Name ) < 0;
        }

        if ( Var.Equals( ( ( FunctionVariable ) v ).Var ) )
        {
            return false;
        }

        if ( Var is Polynomial && ( ( FunctionVariable ) v ).Var is Polynomial )
        {
            var a = ( Polynomial ) Var;
            var b = ( Polynomial ) ( ( FunctionVariable ) v ).Var;

            if ( !a._v.Equals( b._v ) )
            {
                return a._v.Smaller( b._v );
            }

            if ( a.Degree() != b.Degree() )
            {
                return a.Degree() < b.Degree();
            }

            for ( int i = a.Coeffs.Length - 1; i >= 0; i-- )
            {
                if ( !a[i].Equals( b[i] ) )
                {
                    if ( a[i] is Symbolic && b[i] is Symbolic )
                    {
                        return ( ( Symbolic ) a[i] ).Smaller( ( Symbolic ) b[i] );
                    }

                    return a[i].Norm() < b[i].Norm();
                }
            }
        }

        return false;
    }

    public override Variable Conj()
    {
        if ( Name.Equals( "exp" ) || Name.Equals( "log" ) || Name.Equals( "sqrt" ) )
        {
            return new FunctionVariable( Name, Var.Conj(), AlgLambda );
        }

        throw new JasymcaException( "Can't calculate cc for Function " + Name );
    }

    public override string ToString()
    {
        var a = Var.ToString();

        if ( a.StartsWith( "(", StringComparison.Ordinal ) && a.EndsWith( ")", StringComparison.Ordinal ) )
        {
            return Name + a;
        }
        else
        {
            return Name + "(" + a + ")";
        }
    }
}

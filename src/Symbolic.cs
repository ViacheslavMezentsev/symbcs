using System;

public abstract class Symbolic : Algebraic
{
    public static Symbolic ZERO = new Complex( 0.0 );
    public static Symbolic HALF = new Complex( 0.5 );
    public static Symbolic ONE = new Complex( 1.0 );
    public static Symbolic TWO = new Complex( 2.0 );
    public static Symbolic THREE = new Complex( 3.0 );
    public static Symbolic MINUS = new Complex( -1.0 );
    public static Symbolic IONE = new Complex( 0.0, 1.0 );
    public static Symbolic IMINUS = new Complex( 0.0, -1.0 );
    
    public static Algebraic SQRT2 = new Polynomial( new Root( new Vector( new Algebraic[] { new Complex( -2.0 ), ZERO, ONE } ), 0 ) );
    public static Algebraic SQRT3 = new Polynomial( new Root( new Vector( new Algebraic[] { new Complex( -3.0 ), ZERO, ONE } ), 0 ) );

    public static Polynomial PI = new Polynomial( new Constant( "pi", Math.PI ) );

    public abstract bool IsImaginary();
    public abstract bool IsInteger();
    public abstract int ToInt();
    public abstract Symbolic Abs();
    public abstract bool Smaller( Symbolic a );

    public static bool operator <( Symbolic lhs, Symbolic rhs )
    {
        return lhs.Smaller( rhs );
    }

    public static bool operator >( Symbolic lhs, Symbolic rhs )
    {
        return rhs.Smaller( lhs );
    }

    public static bool operator <=( Symbolic lhs, Symbolic rhs )
    {
        return !rhs.Smaller( lhs );
    }

    public static bool operator >=( Symbolic lhs, Symbolic rhs )
    {
        return !lhs.Smaller( rhs );
    }

    public override bool IsConstant()
    {
        return true;
    }

    public override Algebraic Derive( Variable v )
    {
        return ZERO;
    }

    public override Algebraic Integrate( Variable v )
    {
        return this == ZERO ? this : new Polynomial(v) * this;
    }

    public override Algebraic Value( Variable item, Algebraic x )
    {
        return this;
    }

    public override Algebraic Conj()
    {
        return RealPart() - ImagPart();
    }

    public virtual Number ToNumber()
    {
        return this is Number ? ( Number ) this : new Number( ( ( Complex ) this ).Re, ( ( Complex ) this ).Im );
    }

    public virtual Complex ToComplex()
    {
        return this is Complex ? ( Complex ) this : ( ( Number ) this ).ToFloat();
    }

    public override Algebraic Map( LambdaAlgebraic lambda )
    {
        return lambda.PreEval( this );
    }

    public virtual Symbolic gcd( Symbolic x )
    {
        return ToNumber().gcd( x.ToNumber() );
    }    

    public virtual Algebraic[] Div( Algebraic q1, Algebraic[] result )
    {
        return ToNumber().Div( q1, result );
    }
}

using System;

public abstract class Zahl : Algebraic
{
    public static Zahl ZERO = new Unexakt( 0.0 );
    public static Zahl HALF = new Unexakt( 0.5 );
    public static Zahl ONE = new Unexakt( 1.0 );
    public static Zahl TWO = new Unexakt( 2.0 );
    public static Zahl THREE = new Unexakt( 3.0 );
    public static Zahl MINUS = new Unexakt( -1.0 );
    public static Zahl IONE = new Unexakt( 0.0, 1.0 );
    public static Zahl IMINUS = new Unexakt( 0.0, -1.0 );
    public static Polynomial PI = new Polynomial( new Constant( "pi", Math.PI ) );
    public static Algebraic SQRT2 = new Polynomial( new Root( new Vektor( new Algebraic[] { new Unexakt( -2.0 ), ZERO, ONE } ), 0 ) );
    public static Algebraic SQRT3 = new Polynomial( new Root( new Vektor( new Algebraic[] { new Unexakt( -3.0 ), ZERO, ONE } ), 0 ) );

    public override bool constantq()
    {
        return true;
    }

    public override Algebraic deriv( Variable item )
    {
        return ZERO;
    }

    public override Algebraic integrate( Variable item )
    {
        if ( Equals( ZERO ) )
        {
            return this;
        }

        return new Polynomial( item ).mult( this );
    }

    public abstract int intval();

    public abstract bool imagq();

    public override Algebraic value( Variable item, Algebraic x )
    {
        return this;
    }

    public override Algebraic cc()
    {
        return realpart().add( imagpart().mult( IMINUS ) );
    }

    public abstract Zahl abs();

    public virtual Exakt exakt()
    {
        return this is Exakt ? ( Exakt ) this : new Exakt( ( ( Unexakt ) this ).real, ( ( Unexakt ) this ).imag );
    }

    public virtual Unexakt unexakt()
    {
        return this is Unexakt ? ( Unexakt ) this : ( ( Exakt ) this ).tofloat();
    }

    public override Algebraic map( LambdaAlgebraic lambda )
    {
        return lambda.f( this );
    }

    public virtual Zahl gcd( Zahl x )
    {
        return exakt().gcd( x.exakt() );
    }

    public abstract bool smaller( Zahl x );

    public virtual Algebraic[] div( Algebraic q1, Algebraic[] result )
    {
        return exakt().div( q1, result );
    }

    public abstract bool integerq();
}

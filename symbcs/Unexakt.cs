using System;

public class Unexakt : Zahl
{
    public double real, imag;

    public Unexakt()
    {
    }

    public Unexakt( double real, double imag )
    {
        this.real = real;
        this.imag = imag;
    }

    public Unexakt( double real ) : this( real, 0.0 )
    {
    }

    public override double norm()
    {
        double r;

        if ( Math.Abs( real ) > Math.Abs( imag ) )
        {
            r = imag / real;
            r = Math.Abs( real ) * Math.Sqrt( 1 + r * r );
        }
        else if ( imag != 0 )
        {
            r = real / imag;
            r = Math.Abs( imag ) * Math.Sqrt( 1 + r * r );
        }
        else
        {
            r = 0.0;
        }

        return r;
    }

    public virtual Unexakt arg()
    {
        return new Unexakt( JMath.atan2( imag, real ) );
    }

    public override Algebraic add( Algebraic x )
    {
        if ( x is Unexakt )
        {
            return new Unexakt( real + ( ( Unexakt ) x ).real, imag + ( ( Unexakt ) x ).imag );
        }
        return x.add( this );
    }

    public override Algebraic mult( Algebraic x )
    {
        if ( x is Unexakt )
        {
            return new Unexakt( real * ( ( Unexakt ) x ).real - imag * ( ( Unexakt ) x ).imag, real * ( ( Unexakt ) x ).imag + imag * ( ( Unexakt ) x ).real );
        }

        return x.mult( this );
    }

    public override Algebraic div( Algebraic x )
    {
        if ( x is Unexakt )
        {
            Unexakt a = this, b = ( Unexakt ) x, c = new Unexakt( 0.0 );

            double ratio, den, abr, abi, cr;

            if ( ( abr = b.real ) < 0.0 )
            {
                abr = -abr;
            }

            if ( ( abi = b.imag ) < 0.0 )
            {
                abi = -abi;
            }

            if ( abr <= abi )
            {
                if ( abi == 0 )
                {
                    throw new JasymcaException( "Division by Zero." );
                }

                ratio = b.real / b.imag;
                den = b.imag * ( 1 + ratio * ratio );
                cr = ( a.real * ratio + a.imag ) / den;
                c.imag = ( a.imag * ratio - a.real ) / den;
            }
            else
            {
                ratio = b.imag / b.real;
                den = b.real * ( 1 + ratio * ratio );
                cr = ( a.real + a.imag * ratio ) / den;
                c.imag = ( a.imag - a.real * ratio ) / den;
            }

            c.real = cr;

            return c;
        }

        if ( x is Exakt )
        {
            return ( new Exakt( real, imag ) ).div( x );
        }

        return base.div( x );
    }

    public override string ToString()
    {
        if ( imag == 0.0 )
        {
            return Jasymca.fmt.ToString( real );
        }

        if ( real == 0.0 )
        {
            return Jasymca.fmt.ToString( imag ) + "i";
        }

        return "(" + Jasymca.fmt.ToString( real ) + ( imag > 0 ? "+" : "" ) + Jasymca.fmt.ToString( imag ) + "i)";
    }

    public override bool integerq()
    {
        return imag == 0.0 && JMath.round( real ) == real;
    }

    public override bool komplexq()
    {
        return imag != 0;
    }

    public override bool imagq()
    {
        return imag != 0 && real == 0;
    }

    public override Algebraic realpart()
    {
        return new Unexakt( real );
    }

    public override Algebraic imagpart()
    {
        return new Unexakt( imag );
    }
    public override bool Equals( object x )
    {
        if ( x is Unexakt )
        {
            return ( ( Unexakt ) x ).real == real && ( ( Unexakt ) x ).imag == imag;
        }
        if ( x is Exakt )
        {
            return ( ( Exakt ) x ).tofloat().Equals( this );
        }
        return false;
    }

    public override bool smaller( Zahl x )
    {
        Unexakt xu = x.unexakt();

        if ( real == xu.real )
        {
            return imag < xu.imag;
        }
        else
        {
            return real < xu.real;
        }
    }

    public override int intval()
    {
        return ( int ) real;
    }

    public override Zahl abs()
    {
        return new Unexakt( z_abs( real, imag ) );
    }

    private double z_abs( double real, double imag )
    {
        double temp;

        if ( real < 0 )
        {
            real = -real;
        }

        if ( imag < 0 )
        {
            imag = -imag;
        }

        if ( imag > real )
        {
            temp = real;
            real = imag;
            imag = temp;
        }

        if ( ( real + imag ) == real )
        {
            return ( real );
        }

        temp = imag / real;
        temp = real * Math.Sqrt( 1.0 + temp * temp );

        return ( temp );
    }

    public override Algebraic map_lambda( LambdaAlgebraic lambda, Algebraic arg2 )
    {
        if ( arg2 == null )
        {
            var r = lambda.f( this );

            if ( r != null )
            {
                return r;
            }
        }

        return base.map_lambda( lambda, arg2 );
    }

    public override Algebraic rat()
    {
        return new Exakt( real, imag );
    }
}

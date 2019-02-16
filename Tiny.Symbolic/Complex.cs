using Math = Tiny.Numeric.Math;

namespace Tiny.Symbolic
{
    public class Complex : Symbol
    {

        #region Constructors

        public Complex()
        {
        }

        public Complex( double real, double imag )
        {
            Re = real;
            Im = imag;
        }

        public Complex( double real ) : this( real, 0.0 )
        {
        }

        #endregion

        #region Private methods

        private double abs( double real, double imag )
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
            temp = real * System.Math.Sqrt( 1.0 + temp * temp );

            return ( temp );
        }

        #endregion

        #region Public methods

        public override double Norm()
        {
            double r;

            if ( System.Math.Abs( Re ) > System.Math.Abs( Im ) )
            {
                r = Im / Re;
                r = System.Math.Abs( Re ) * System.Math.Sqrt( 1 + r * r );
            }
            else if ( Im != 0 )
            {
                r = Re / Im;
                r = System.Math.Abs( Im ) * System.Math.Sqrt( 1 + r * r );
            }
            else
            {
                r = 0.0;
            }

            return r;
        }

        public virtual Complex Arg()
        {
            return new Complex( Math.atan2( Im, Re ) );
        }

        protected override Algebraic Add( Algebraic a )
        {
            if ( a is Complex )
            {
                var c = ( Complex ) a;

                return new Complex( Re + c.Re, Im + c.Im );
            }

            return a + this;
        }

        protected override Algebraic Mul( Algebraic a )
        {
            if ( a is Complex )
            {
                var c = ( Complex ) a;

                return new Complex( Re * c.Re - Im * c.Im, Re * c.Im + Im * c.Re );
            }

            return a * this;
        }

        protected override Algebraic Div( Algebraic x )
        {
            if ( x is Complex )
            {
                Complex a = this, b = ( Complex ) x, c = new Complex( 0.0 );

                double ratio, den, abr, abi, cr;

                if ( ( abr = b.Re ) < 0.0 )
                {
                    abr = -abr;
                }

                if ( ( abi = b.Im ) < 0.0 )
                {
                    abi = -abi;
                }

                if ( abr <= abi )
                {
                    if ( abi == 0 )
                    {
                        throw new SymbolicException( "Division by Zero." );
                    }

                    ratio = b.Re / b.Im;

                    den = b.Im * ( 1 + ratio * ratio );

                    cr = ( a.Re * ratio + a.Im ) / den;

                    c.Im = ( a.Im * ratio - a.Re ) / den;
                }
                else
                {
                    ratio = b.Im / b.Re;

                    den = b.Re * ( 1 + ratio * ratio );

                    cr = ( a.Re + a.Im * ratio ) / den;

                    c.Im = ( a.Im - a.Re * ratio ) / den;
                }

                c.Re = cr;

                return c;
            }

            if ( x is Number )
            {
                return new Number( Re, Im ) / x;
            }

            return this / x;
        }

        public override string ToString()
        {
            if ( Im == 0.0 )
            {
                return Globals.fmt.ToString( Re );
            }

            if ( Re == 0.0 )
            {
                return Globals.fmt.ToString( Im ) + "i";
            }

            return "(" + Globals.fmt.ToString( Re ) + ( Im > 0 ? "+" : "" ) + Globals.fmt.ToString( Im ) + "i)";
        }

        public override bool IsInteger()
        {
            return Im == 0.0 && Math.round( Re ) == Re;
        }

        public override bool IsComplex()
        {
            return Im != 0;
        }

        public override bool IsImaginary()
        {
            return Im != 0 && Re == 0;
        }

        public override Algebraic RealPart()
        {
            return new Complex( Re );
        }

        public override Algebraic ImagPart()
        {
            return new Complex( Im );
        }

        public override bool Equals( object x )
        {
            if ( x is Complex )
            {
                return ( ( Complex ) x ).Re == Re && ( ( Complex ) x ).Im == Im;
            }

            if ( x is Number )
            {
                return ( ( Number ) x ).ToFloat().Equals( this );
            }

            return false;
        }

        public override bool Smaller( Symbol x )
        {
            var xu = x.ToComplex();

            return Re == xu.Re ? Im < xu.Im : Re < xu.Re;
        }

        public override int ToInt()
        {
            return ( int ) Re;
        }

        public override Symbol Abs()
        {
            return new Complex( abs( Re, Im ) );
        }

        public override Algebraic Map( LambdaAlgebraic lambda, Algebraic arg )
        {
            if ( arg == null )
            {
                var r = lambda.PreEval( this );

                if ( r != null )
                {
                    return r;
                }
            }

            return base.Map( lambda, arg );
        }

        public override Algebraic Rat()
        {
            return new Number( Re, Im );
        }

        #endregion

        #region Properties

        public double Re
        {
            get; private set;
        }
        public double Im
        {
            get; private set;
        }

        #endregion

    }
}

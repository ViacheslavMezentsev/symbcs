using System;
using System.Collections;

internal class LambdaTRIGRAT : Lambda
{
    public override int Eval( Stack st )
    {
        int narg = GetNarg( st );

        var f = GetAlgebraic( st );

        f = f.Rat();

        Debug( "Rational: " + f );

        f = ( new ExpandUser() ).SymEval(f);

        Debug( "User Function expand: " + f );

        f = ( new TrigExpand() ).SymEval(f);

        Debug( "Trigexpand: " + f );

        f = ( new NormExp() ).SymEval(f);

        Debug( "Norm: " + f );

        f = ( new TrigInverseExpand() ).SymEval(f);

        Debug( "Triginverse: " + f );

        f = ( new SqrtExpand() ).SymEval(f);

        Debug( "Sqrtexpand: " + f );

        st.Push(f);

        return 0;
    }
}

internal class LambdaTRIGEXP : Lambda
{
    public override int Eval( Stack st )
    {
        int narg = GetNarg( st );
        var f = GetAlgebraic( st );

        f = f.Rat();

        Debug( "Rational: " + f );

        f = ( new ExpandUser() ).SymEval(f);

        Debug( "User Function expand: " + f );

        f = ( new TrigExpand() ).SymEval(f);

        Debug( "Trigexpand: " + f );

        f = ( new NormExp() ).SymEval(f);

        f = ( new SqrtExpand() ).SymEval(f);

        st.Push( f );

        return 0;
    }
}

internal class TrigExpand : LambdaAlgebraic
{
    internal override Algebraic SymEval( Algebraic x )
    {
        if ( x is Polynomial && ( ( Polynomial ) x )._v is FunctionVariable )
        {
            var xp = ( Polynomial ) x;

            var f = ( FunctionVariable ) xp._v;

            var la = pc.env.getValue( f.Name );

            if ( la is LambdaAlgebraic && ( ( LambdaAlgebraic ) la ).trigrule != null )
            {
                try
                {
                    var rule = ( ( LambdaAlgebraic ) la ).trigrule;

                    var fexp = evalx( rule, f.Var );

                    Algebraic r = Symbolic.ZERO;

                    for ( int i = xp.Coeffs.Length - 1; i > 0; i-- )
                    {
                        r = ( r + SymEval( xp[i] ) ) * fexp;
                    }

                    if ( xp.Coeffs.Length > 0 )
                    {
                        r = r + SymEval( xp[0] );
                    }

                    return r;
                }
                catch ( Exception e )
                {
                    throw new JasymcaException( e.ToString() );
                }
            }
        }

        return x.Map( this );
    }
}

internal class SqrtExpand : LambdaAlgebraic
{
    internal override Algebraic SymEval( Algebraic x )
    {
        if ( !( x is Polynomial ) )
        {
            return x.Map( this );
        }

        var xp = ( Polynomial ) x;

        var item = xp._v;

        if ( item is Root )
        {
            var cr = ( ( Root ) item ).poly;

            if ( cr.Length() == xp.Degree() + 1 )
            {
                var xr = new Algebraic[ xp.Degree() + 1 ];

                Algebraic ratio = null;

                for ( int i = xr.Length - 1; i >= 0; i-- )
                {
                    xr[i] = xp[i].Map( this );

                    if ( i == xr.Length - 1 )
                    {
                        ratio = xr[i];
                    }
                    else if ( i > 0 && ratio != null )
                    {
                        if ( !Equals( cr[i] * ratio, xr[i] ) )
                        {
                            ratio = null;
                        }
                    }
                }
                if ( ratio != null )
                {
                    return xr[0] - ratio * cr[0];
                }
                else
                {
                    return new Polynomial( item, xr );
                }
            }
        }

        Algebraic xf = null;

        if ( item is FunctionVariable && ( ( FunctionVariable ) item ).Name.Equals( "sqrt" ) && ( ( FunctionVariable ) item ).Var is Polynomial )
        {
            var arg = ( Polynomial ) ( ( FunctionVariable ) item ).Var;

            var sqfr = arg.square_free_dec( arg._v );

            var issquare = !(sqfr.Length > 0 && !sqfr[0].Equals( arg[ arg.Coeffs.Length - 1 ] ));

            for ( int i = 2; i < sqfr.Length && issquare; i++ )
            {
                if ( ( i + 1 ) % 2 == 1 && !sqfr[i].Equals( Symbolic.ONE ) )
                {
                    issquare = false;
                }
            }

            if ( issquare )
            {
                xf = Symbolic.ONE;

                for ( int i = 1; i < sqfr.Length; i += 2 )
                {
                    if ( !sqfr[i].Equals( Symbolic.ZERO ) )
                    {
                        xf = xf * sqfr[i] ^ ( ( i + 1 ) / 2 );
                    }
                }

                Algebraic r = Symbolic.ZERO;

                for ( int i = xp.Coeffs.Length - 1; i > 0; i-- )
                {
                    r = ( r + SymEval( xp[i] ) ) * xf;
                }

                if ( xp.Coeffs.Length > 0 )
                {
                    r = r + SymEval( xp[0] );
                }

                return r;
            }
        }

        if ( item is FunctionVariable && ( ( FunctionVariable ) item ).Name.Equals( "sqrt" ) && xp.Degree() > 1 )
        {
            xf = ( ( FunctionVariable ) item ).Var;

            var sq = new Polynomial( item );

            var r = SymEval( xp[0] );

            Algebraic xv = Symbolic.ONE;

            for ( int i = 1; i < xp.Coeffs.Length; i++ )
            {
                if ( i % 2 == 1 )
                {
                    r = r + SymEval( xp[i] ) * xv * sq;
                }
                else
                {
                    xv = xv * xf;

                    r = r + SymEval( xp[i] ) * xv;
                }
            }

            return r;
        }

        return x.Map( this );
    }
}

internal class TrigInverseExpand : LambdaAlgebraic
{
    public virtual Algebraic divExponential( Algebraic x, FunctionVariable fv, int n )
    {
        var a = new Algebraic[2];

        a[1] = x;

        Algebraic xk = Symbolic.ZERO;

        for ( int i = n; i >= 0; i-- )
        {
            var kf = FunctionVariable.Create( "exp", fv.Var ).Pow(i);

            a[0] = a[1];
            a[1] = kf;

            Poly.polydiv( a, fv );

            if ( !a[0].Equals( Symbolic.ZERO ) )
            {
                var kfi = FunctionVariable.Create( "exp", -fv.Var ) ^ ( n - i );

                xk = xk + a[0] * kfi;
            }

            if ( Equals(a[1], Symbolic.ZERO) )
            {
                break;
            }
        }

        return SymEval( xk );
    }

    internal override Algebraic SymEval( Algebraic x )
    {
        if ( x is Rational )
        {
            var xr = ( Rational ) x;

            if ( xr.den._v is FunctionVariable 
                && ( ( FunctionVariable ) xr.den._v ).Name.Equals( "exp" ) 
                && ( ( FunctionVariable ) xr.den._v ).Var.IsComplex() )
            {
                var fv = ( FunctionVariable ) xr.den._v;

                int maxdeg = Math.Max( Poly.Degree( xr.nom, fv ), Poly.Degree( xr.den, fv ) );

                if ( maxdeg % 2 == 0 )
                {
                    return divExponential( xr.nom, fv, maxdeg / 2 ) / divExponential( xr.den, fv, maxdeg / 2 );
                }
                else
                {
                    var fv2 = new FunctionVariable( "exp", ( ( FunctionVariable ) xr.den._v ).Var / Symbolic.TWO, ( ( FunctionVariable ) xr.den._v ).AlgLambda );

                    Algebraic ex = new Polynomial( fv2, new Algebraic[] { Symbolic.ZERO, Symbolic.ZERO, Symbolic.ONE } );

                    var xr1 = xr.nom.Value( xr.den._v, ex ) / xr.den.Value( xr.den._v, ex );

                    return SymEval( xr1 );
                }
            }
        }

        if ( x is Polynomial && ( ( Polynomial ) x )._v is FunctionVariable )
        {
            var xp = ( Polynomial ) x;

            Algebraic xf = null;

            var fvar = ( FunctionVariable ) xp._v;

            if ( fvar.Name.Equals( "exp" ) )
            {
                var re = fvar.Var.RealPart();
                var im = fvar.Var.ImagPart();

                if ( im != Symbolic.ZERO )

                {
                    bool _minus = minus( im );

                    if ( _minus )
                    {
                        im = -im;
                    }

                    var a = FunctionVariable.Create( "exp", re );
                    var b = FunctionVariable.Create( "cos", im );
                    var c = FunctionVariable.Create( "sin", im ) * Symbolic.IONE;

                    xf = a * ( _minus ? b - c : b + c );
                }
            }

            if ( fvar.Name.Equals( "log" ) )
            {
                var arg = fvar.Var;

                Algebraic factor = Symbolic.ONE, sum = Symbolic.ZERO;

                if ( arg is Polynomial 
                    && ( ( Polynomial ) arg ).Degree() == 1 
                    && ( ( Polynomial ) arg )._v is FunctionVariable 
                    && ( ( Polynomial ) arg )[0].Equals( Symbolic.ZERO ) 
                    && ( ( FunctionVariable ) ( ( Polynomial ) arg )._v ).Name.Equals( "sqrt" ) )
                {
                    sum = FunctionVariable.Create( "log", ( ( Polynomial ) arg )[1] );

                    factor = new Complex( 0.5 );

                    arg = ( ( FunctionVariable ) ( ( Polynomial ) arg )._v ).Var;

                    xf = FunctionVariable.Create( "log", arg );
                }

                try
                {
                    var re = arg.RealPart();
                    var im = arg.ImagPart();

                    if ( im != Symbolic.ZERO )
                    {
                        bool min_im = minus( im );

                        if ( min_im )
                        {
                            im = -im;
                        }

                        var a1 = new SqrtExpand().SymEval( arg * arg.Conj() );

                        var a = FunctionVariable.Create( "log", a1 ) / Symbolic.TWO;

                        var b1 = SymEval( re / im );

                        var b = FunctionVariable.Create( "atan", b1 ) * Symbolic.IONE;

                        xf = min_im ? a + b : a - b;

                        var pi2 = Symbolic.PI * Symbolic.IONE / Symbolic.TWO;

                        xf = min_im ? xf - pi2 : xf + pi2;
                    }
                }
                catch ( JasymcaException )
                {
                }

                if ( xf != null )
                {
                    xf = xf * factor + sum;
                }
            }

            if ( xf == null )
            {
                return x.Map( this );
            }

            Algebraic r = Symbolic.ZERO;

            for ( int i = xp.Coeffs.Length - 1; i > 0; i-- )
            {
                r = ( r + SymEval( xp[i] ) ) * xf;
            }

            if ( xp.Coeffs.Length > 0 )
            {
                r = r + SymEval( xp[0] );
            }

            return r;
        }

        return x.Map( this );
    }

    internal static bool minus( Algebraic x )
    {
        if ( x is Symbolic )
        {
            return ( ( Symbolic ) x ).Smaller( Symbolic.ZERO );
        }

        if ( x is Polynomial )
        {
            return minus( ( ( Polynomial ) x )[ ( ( Polynomial ) x ).Degree() ] );
        }

        if ( x is Rational )
        {
            var a = minus( ( ( Rational ) x ).nom );
            var b = minus( ( ( Rational ) x ).den );

            return ( a && !b ) || ( !a && b );
        }

        throw new JasymcaException( "minus not implemented for " + x );
    }
}

internal class LambdaSIN : LambdaAlgebraic
{
    public LambdaSIN()
    {
        diffrule = "cos(x)";
        intrule = "-cos(x)";
        trigrule = "1/(2*i)*(exp(i*x)-exp(-i*x))";
    }

    internal override Symbolic PreEval( Symbolic x )
    {
        var z = x.ToComplex();

        if ( z.Im == 0.0 )
        {
            return new Complex( Math.Sin( z.Re ) );
        }

        return ( Symbolic ) evalx( trigrule, z );
    }

    internal override Algebraic SymEval( Algebraic x )
    {
        if ( x.Equals( Symbolic.ZERO ) )
        {
            return Symbolic.ZERO;
        }

        return null;
    }
}

internal class LambdaCOS : LambdaAlgebraic
{
    public LambdaCOS()
    {
        diffrule = "-sin(x)";
        intrule = "sin(x)";
        trigrule = "1/2 *(exp(i*x)+exp(-i*x))";
    }
    internal override Symbolic PreEval( Symbolic x )
    {
        Complex z = x.ToComplex();
        if ( z.Im == 0.0 )
        {
            return new Complex( Math.Cos( z.Re ) );
        }
        return ( Symbolic ) evalx( trigrule, z );
    }
    internal override Algebraic SymEval( Algebraic x )
    {
        if ( x.Equals( Symbolic.ZERO ) )
        {
            return Symbolic.ONE;
        }
        return null;
    }
}
internal class LambdaTAN : LambdaAlgebraic
{
    public LambdaTAN()
    {
        diffrule = "1/(cos(x))^2";
        intrule = "-log(cos(x))";
        trigrule = "-i*(exp(i*x)-exp(-i*x))/(exp(i*x)+exp(-i*x))";
    }
    internal override Symbolic PreEval( Symbolic x )
    {
        Complex z = x.ToComplex();
        if ( z.Im == 0.0 )
        {
            return new Complex( Math.Tan( z.Re ) );
        }
        return ( Symbolic ) evalx( trigrule, z );
    }
    internal override Algebraic SymEval( Algebraic x )
    {
        if ( x.Equals( Symbolic.ZERO ) )
        {
            return Symbolic.ZERO;
        }
        return null;
    }
}
internal class LambdaATAN : LambdaAlgebraic
{
    public LambdaATAN()
    {
        diffrule = "1/(1+x^2)";
        intrule = "x*atan(x)-1/2*log(1+x^2)";
        trigrule = "-i/2*log((1+i*x)/(1-i*x))";
    }

    internal override Symbolic PreEval( Symbolic x )
    {
        var z = x.ToComplex();

        if ( z.Im == 0.0 )
        {
            return new Complex( JMath.atan( z.Re ) );
        }

        return ( Symbolic ) evalx( trigrule, z );
    }

    internal override Algebraic SymEval( Algebraic x )
    {
        return Equals(x, Symbolic.ZERO) ? Symbolic.ZERO : null;
    }
}

internal class LambdaASIN : LambdaAlgebraic
{
    public LambdaASIN()
    {
        diffrule = "1/sqrt(1-x^2)";
        intrule = "x*asin(x)+sqrt(1-x^2)";
        trigrule = "-i*log(i*x+i*sqrt(1-x^2))";
    }

    internal override Symbolic PreEval( Symbolic x )
    {
        var z = x.ToComplex();

        if ( z.Im == 0.0 )
        {
            return new Complex( JMath.asin( z.Re ) );
        }

        return ( Symbolic ) evalx( trigrule, z );
    }
}

internal class LambdaACOS : LambdaAlgebraic
{
    public LambdaACOS()
    {
        diffrule = "-1/sqrt(1-x^2)";
        intrule = "x*acos(x)-sqrt(1-x^2)";
        trigrule = "-i*log(x+i*sqrt(1-x^2))";
    }

    internal override Symbolic PreEval( Symbolic x )
    {
        var z = x.ToComplex();

        if ( z.Im == 0.0 )
        {
            return new Complex( JMath.acos( z.Re ) );
        }

        return ( Symbolic ) evalx( trigrule, z );
    }
}

internal class LambdaATAN2 : LambdaAlgebraic
{
    internal override Symbolic PreEval( Symbolic x )
    {
        return null;
    }

    internal override Algebraic SymEval( Algebraic x )
    {
        throw new JasymcaException( "Usage: ATAN2(y,x)." );
    }

    internal override Algebraic SymEval( Algebraic[] x )
    {
        throw new JasymcaException( "Usage: ATAN2(y,x)." );
    }

    internal override Algebraic SymEval( Algebraic x, Algebraic y )
    {
        if ( y is Complex && !y.IsComplex() && x is Complex && !x.IsComplex() )
        {
            return new Complex( JMath.atan2( ( ( Complex ) y ).Re, ( ( Complex ) x ).Re ) );
        }

        if ( Symbolic.ZERO != x )
        {
            return FunctionVariable.Create( "atan", y / x ) + FunctionVariable.Create( "sign", y ) * ( Symbolic.ONE - FunctionVariable.Create( "sign", x ) ) * Symbolic.PI / Symbolic.TWO;
        }
        else
        {
            return ( FunctionVariable.Create( "sign", y ) * Symbolic.PI ) / Symbolic.TWO;
        }
    }
}

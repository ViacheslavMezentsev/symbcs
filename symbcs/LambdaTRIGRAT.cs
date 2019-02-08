using System;
using System.Collections;

internal class LambdaTRIGRAT : Lambda
{
    public override int lambda( Stack st )
    {
        int narg = getNarg( st );

        var f = getAlgebraic( st );

        f = f.rat();

        debug( "Rational: " + f );

        f = ( new ExpandUser() ).f_exakt(f);

        debug( "User Function expand: " + f );

        f = ( new TrigExpand() ).f_exakt(f);

        debug( "Trigexpand: " + f );

        f = ( new NormExp() ).f_exakt(f);

        debug( "Norm: " + f );

        f = ( new TrigInverseExpand() ).f_exakt(f);

        debug( "Triginverse: " + f );

        f = ( new SqrtExpand() ).f_exakt(f);

        debug( "Sqrtexpand: " + f );

        st.Push(f);

        return 0;
    }
}

internal class LambdaTRIGEXP : Lambda
{
    public override int lambda( Stack st )
    {
        int narg = getNarg( st );
        var f = getAlgebraic( st );

        f = f.rat();

        debug( "Rational: " + f );

        f = ( new ExpandUser() ).f_exakt(f);

        debug( "User Function expand: " + f );

        f = ( new TrigExpand() ).f_exakt(f);

        debug( "Trigexpand: " + f );

        f = ( new NormExp() ).f_exakt(f);

        f = ( new SqrtExpand() ).f_exakt(f);

        st.Push( f );

        return 0;
    }
}

internal class TrigExpand : LambdaAlgebraic
{
    internal override Algebraic f_exakt( Algebraic x )
    {
        if ( x is Polynomial && ( ( Polynomial ) x ).v is FunctionVariable )
        {
            var xp = ( Polynomial ) x;

            var f = ( FunctionVariable ) xp.v;

            var la = pc.env.getValue( f.fname );

            if ( la is LambdaAlgebraic && ( ( LambdaAlgebraic ) la ).trigrule != null )
            {
                try
                {
                    var rule = ( ( LambdaAlgebraic ) la ).trigrule;

                    var fexp = evalx( rule, f.arg );

                    Algebraic r = Zahl.ZERO;

                    for ( int i = xp.a.Length - 1; i > 0; i-- )
                    {
                        r = r.add( f_exakt( xp.a[i] ) ).mult( fexp );
                    }

                    if ( xp.a.Length > 0 )
                    {
                        r = r.add( f_exakt( xp.a[0] ) );
                    }

                    return r;
                }
                catch ( Exception e )
                {
                    throw new JasymcaException( e.ToString() );
                }
            }
        }

        return x.map( this );
    }
}

internal class SqrtExpand : LambdaAlgebraic
{
    internal override Algebraic f_exakt( Algebraic x )
    {
        if ( !( x is Polynomial ) )
        {
            return x.map( this );
        }

        var xp = ( Polynomial ) x;

        var item = xp.v;

        if ( item is Root )
        {
            var cr = ( ( Root ) item ).poly;

            if ( cr.length() == xp.degree() + 1 )
            {
                var xr = new Algebraic[ xp.degree() + 1 ];

                Algebraic ratio = null;

                for ( int i = xr.Length - 1; i >= 0; i-- )
                {
                    xr[ i ] = xp.a[ i ].map( this );

                    if ( i == xr.Length - 1 )
                    {
                        ratio = xr[ i ];
                    }
                    else if ( i > 0 && ratio != null )
                    {
                        if ( !cr.get( i ).mult( ratio ).Equals( xr[ i ] ) )
                        {
                            ratio = null;
                        }
                    }
                }
                if ( ratio != null )
                {
                    return xr[0].sub( ratio.mult( cr.get(0) ) );
                }
                else
                {
                    return new Polynomial( item, xr );
                }
            }
        }

        Algebraic xf = null;

        if ( item is FunctionVariable && ( ( FunctionVariable ) item ).fname.Equals( "sqrt" ) && ( ( FunctionVariable ) item ).arg is Polynomial )
        {
            var arg = ( Polynomial ) ( ( FunctionVariable ) item ).arg;

            var sqfr = arg.square_free_dec( arg.v );

            var issquare = !(sqfr.Length > 0 && !sqfr[0].Equals( arg.a[ arg.a.Length - 1 ] ));

            for ( int i = 2; i < sqfr.Length && issquare; i++ )
            {
                if ( ( i + 1 ) % 2 == 1 && !sqfr[i].Equals( Zahl.ONE ) )
                {
                    issquare = false;
                }
            }

            if ( issquare )
            {
                xf = Zahl.ONE;

                for ( int i = 1; i < sqfr.Length; i += 2 )
                {
                    if ( !sqfr[i].Equals( Zahl.ZERO ) )
                    {
                        xf = xf.mult( sqfr[ i ].pow_n( ( i + 1 ) / 2 ) );
                    }
                }

                Algebraic r = Zahl.ZERO;

                for ( int i = xp.a.Length - 1; i > 0; i-- )
                {
                    r = r.add( f_exakt( xp.a[i] ) ).mult( xf );
                }

                if ( xp.a.Length > 0 )
                {
                    r = r.add( f_exakt( xp.a[0] ) );
                }

                return r;
            }
        }

        if ( item is FunctionVariable && ( ( FunctionVariable ) item ).fname.Equals( "sqrt" ) && xp.degree() > 1 )
        {
            xf = ( ( FunctionVariable ) item ).arg;

            var sq = new Polynomial( item );

            var r = f_exakt( xp.a[0] );

            Algebraic xv = Zahl.ONE;

            for ( int i = 1; i < xp.a.Length; i++ )
            {
                if ( i % 2 == 1 )
                {
                    r = r.add( f_exakt( xp.a[i] ).mult( xv ).mult( sq ) );
                }
                else
                {
                    xv = xv.mult( xf );
                    r = r.add( f_exakt( xp.a[i] ).mult( xv ) );
                }
            }

            return r;
        }

        return x.map( this );
    }
}

internal class TrigInverseExpand : LambdaAlgebraic
{
    public virtual Algebraic divExponential( Algebraic x, FunctionVariable fv, int n )
    {
        var a = new Algebraic[2];

        a[1] = x;

        Algebraic xk = Zahl.ZERO;

        for ( int i = n; i >= 0; i-- )
        {
            var kf = FunctionVariable.create( "exp", fv.arg ).pow_n(i);

            a[0] = a[1];
            a[1] = kf;

            Poly.polydiv( a, fv );

            if ( !a[0].Equals( Zahl.ZERO ) )
            {
                var kfi = FunctionVariable.create( "exp", fv.arg.mult( Zahl.MINUS ) ).pow_n( n - i );

                xk = xk.add( a[0].mult( kfi ) );
            }

            if ( a[1].Equals( Zahl.ZERO ) )
            {
                break;
            }
        }

        return f_exakt( xk );
    }

    internal override Algebraic f_exakt( Algebraic x )
    {
        if ( x is Rational )
        {
            var xr = ( Rational ) x;

            if ( xr.den.v is FunctionVariable 
                && ( ( FunctionVariable ) xr.den.v ).fname.Equals( "exp" ) 
                && ( ( FunctionVariable ) xr.den.v ).arg.komplexq() )
            {
                var fv = ( FunctionVariable ) xr.den.v;

                int maxdeg = Math.Max( Poly.degree( xr.nom, fv ), Poly.degree( xr.den, fv ) );

                if ( maxdeg % 2 == 0 )
                {
                    return divExponential( xr.nom, fv, maxdeg / 2 ).div( divExponential( xr.den, fv, maxdeg / 2 ) );
                }
                else
                {
                    var fv2 = new FunctionVariable( "exp", ( ( FunctionVariable ) xr.den.v ).arg.div( Zahl.TWO ), ( ( FunctionVariable ) xr.den.v ).la );

                    Algebraic ex = new Polynomial( fv2, new Algebraic[] { Zahl.ZERO, Zahl.ZERO, Zahl.ONE } );

                    var xr1 = xr.nom.value( xr.den.v, ex ).div( xr.den.value( xr.den.v, ex ) );

                    return f_exakt( xr1 );
                }
            }
        }

        if ( x is Polynomial && ( ( Polynomial ) x ).v is FunctionVariable )
        {
            var xp = ( Polynomial ) x;

            Algebraic xf = null;

            var fvar = ( FunctionVariable ) xp.v;

            if ( fvar.fname.Equals( "exp" ) )
            {
                var re = fvar.arg.realpart();
                var im = fvar.arg.imagpart();

                if ( !im.Equals( Zahl.ZERO ) )

                {
                    bool _minus = minus( im );

                    if ( _minus )
                    {
                        im = im.mult( Zahl.MINUS );
                    }

                    var a = FunctionVariable.create( "exp", re );
                    var b = FunctionVariable.create( "cos", im );
                    var c = FunctionVariable.create( "sin", im ).mult( Zahl.IONE );

                    xf = a.mult( _minus ? ( b.sub( c ) ) : b.add( c ) );
                }
            }

            if ( fvar.fname.Equals( "log" ) )
            {
                var arg = fvar.arg;

                Algebraic factor = Zahl.ONE, sum = Zahl.ZERO;

                if ( arg is Polynomial 
                    && ( ( Polynomial ) arg ).degree() == 1 
                    && ( ( Polynomial ) arg ).v is FunctionVariable 
                    && ( ( Polynomial ) arg ).a[0].Equals( Zahl.ZERO ) 
                    && ( ( FunctionVariable ) ( ( Polynomial ) arg ).v ).fname.Equals( "sqrt" ) )
                {
                    sum = FunctionVariable.create( "log", ( ( Polynomial ) arg ).a[1] );

                    factor = new Unexakt( 0.5 );

                    arg = ( ( FunctionVariable ) ( ( Polynomial ) arg ).v ).arg;

                    xf = FunctionVariable.create( "log", arg );
                }

                try
                {
                    var re = arg.realpart();
                    var im = arg.imagpart();

                    if ( !im.Equals( Zahl.ZERO ) )
                    {
                        bool min_im = minus( im );

                        if ( min_im )
                        {
                            im = im.mult( Zahl.MINUS );
                        }

                        var a1 = ( new SqrtExpand() ).f_exakt( arg.mult( arg.cc() ) );
                        var a = FunctionVariable.create( "log", a1 ).div( Zahl.TWO );

                        var b1 = f_exakt( re.div( im ) );
                        var b = FunctionVariable.create( "atan", b1 ).mult( Zahl.IONE );

                        xf = min_im ? a.add(b) : a.sub(b);

                        var pi2 = Zahl.PI.mult( Zahl.IONE ).div( Zahl.TWO );

                        xf = min_im ? xf.sub( pi2 ) : xf.add( pi2 );
                    }
                }
                catch ( JasymcaException )
                {
                }

                if ( xf != null )
                {
                    xf = xf.mult( factor ).add( sum );
                }
            }

            if ( xf == null )
            {
                return x.map( this );
            }

            Algebraic r = Zahl.ZERO;

            for ( int i = xp.a.Length - 1; i > 0; i-- )
            {
                r = r.add( f_exakt( xp.a[i] ) ).mult( xf );
            }

            if ( xp.a.Length > 0 )
            {
                r = r.add( f_exakt( xp.a[0] ) );
            }

            return r;
        }

        return x.map( this );
    }

    internal static bool minus( Algebraic x )
    {
        if ( x is Zahl )
        {
            return ( ( Zahl ) x ).smaller( Zahl.ZERO );
        }

        if ( x is Polynomial )
        {
            return minus( ( ( Polynomial ) x ).a[ ( ( Polynomial ) x ).degree() ] );
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

    internal override Zahl f( Zahl x )
    {
        var z = x.unexakt();

        if ( z.imag == 0.0 )
        {
            return new Unexakt( Math.Sin( z.real ) );
        }

        return ( Zahl ) evalx( trigrule, z );
    }

    internal override Algebraic f_exakt( Algebraic x )
    {
        if ( x.Equals( Zahl.ZERO ) )
        {
            return Zahl.ZERO;
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
    internal override Zahl f( Zahl x )
    {
        Unexakt z = x.unexakt();
        if ( z.imag == 0.0 )
        {
            return new Unexakt( Math.Cos( z.real ) );
        }
        return ( Zahl ) evalx( trigrule, z );
    }
    internal override Algebraic f_exakt( Algebraic x )
    {
        if ( x.Equals( Zahl.ZERO ) )
        {
            return Zahl.ONE;
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
    internal override Zahl f( Zahl x )
    {
        Unexakt z = x.unexakt();
        if ( z.imag == 0.0 )
        {
            return new Unexakt( Math.Tan( z.real ) );
        }
        return ( Zahl ) evalx( trigrule, z );
    }
    internal override Algebraic f_exakt( Algebraic x )
    {
        if ( x.Equals( Zahl.ZERO ) )
        {
            return Zahl.ZERO;
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
    internal override Zahl f( Zahl x )
    {
        Unexakt z = x.unexakt();
        if ( z.imag == 0.0 )
        {
            return new Unexakt( JMath.atan( z.real ) );
        }
        return ( Zahl ) evalx( trigrule, z );
    }
    internal override Algebraic f_exakt( Algebraic x )
    {
        if ( x.Equals( Zahl.ZERO ) )
        {
            return Zahl.ZERO;
        }
        return null;
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
    internal override Zahl f( Zahl x )
    {
        Unexakt z = x.unexakt();
        if ( z.imag == 0.0 )
        {
            return new Unexakt( JMath.asin( z.real ) );
        }
        return ( Zahl ) evalx( trigrule, z );
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
    internal override Zahl f( Zahl x )
    {
        Unexakt z = x.unexakt();
        if ( z.imag == 0.0 )
        {
            return new Unexakt( JMath.acos( z.real ) );
        }
        return ( Zahl ) evalx( trigrule, z );
    }
}
internal class LambdaATAN2 : LambdaAlgebraic
{
    internal override Zahl f( Zahl x )
    {
        return null;
    }
    internal override Algebraic f_exakt( Algebraic x )
    {
        throw new JasymcaException( "Usage: ATAN2(y,x)." );
    }
    internal override Algebraic f_exakt( Algebraic[] x )
    {
        throw new JasymcaException( "Usage: ATAN2(y,x)." );
    }
    internal override Algebraic f_exakt( Algebraic x, Algebraic y )
    {
        if ( y is Unexakt && !y.komplexq() && x is Unexakt && !x.komplexq() )
        {
            return new Unexakt( JMath.atan2( ( ( Unexakt ) y ).real, ( ( Unexakt ) x ).real ) );
        }
        if ( !Zahl.ZERO.Equals( x ) )
        {
            return FunctionVariable.create( "atan", y.div( x ) ).add( ( FunctionVariable.create( "sign", y ).mult( Zahl.ONE.sub( ( FunctionVariable.create( "sign", x ) ) ) ).mult( Zahl.PI ).div( Zahl.TWO ) ) );
        }
        else
        {
            return ( FunctionVariable.create( "sign", y ).mult( Zahl.PI ).div( Zahl.TWO ) );
        }
    }
}
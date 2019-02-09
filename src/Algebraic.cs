using System;

public abstract class Algebraic
{

    #region Internal fields

	internal string Name = null;

    #endregion

    #region Internal methods

    internal static void debug( string s )
    {
        Lambda.debug(s);
    }

    #endregion
    
    #region Public methods

    #region Abstract

    public abstract Algebraic add( Algebraic x );

    public abstract Algebraic mult( Algebraic x );

    public abstract Algebraic cc();

    public abstract Algebraic deriv( Variable var );

    public abstract Algebraic integrate( Variable var );

    public abstract double norm();

    public abstract Algebraic map( LambdaAlgebraic f );

    public abstract override bool Equals( object x );

    #endregion

    #region Virtual

    public virtual Algebraic sub( Algebraic x )
	{
		return add( x.mult( Zahl.MINUS ) );
	}

    public virtual Algebraic div( Algebraic x )
    {
        if ( x is Polynomial )
        {
            return new Rational( this, ( Polynomial ) x ).reduce();
        }

        if ( x is Rational )
        {
            return ( ( Rational ) x ).den.mult( this ).div( ( ( Rational ) x ).nom );
        }

        if ( !x.scalarq() )
        {
            return new Matrix( this ).div( x );
        }

        throw new JasymcaException( "Can not divide " + this + " through " + x );
    }

	public virtual Algebraic pow_n( int n )
	{
		Algebraic pow, x = this;

		if ( n <= 0 )
		{
			if ( n == 0 || Equals( Zahl.ONE ) )
			{
				return Zahl.ONE;
			}

			if ( Equals( Zahl.ZERO ) )
			{
				throw new JasymcaException( "Division by Zero." );
			}

			x = Zahl.ONE.div(x);

			n = -n;
		}

		for ( pow = Zahl.ONE; ; )
		{
			if ( ( n & 1 ) != 0 )
			{
				pow = pow.mult(x);
			}

			if ( ( n >>= 1 ) != 0 )
			{
				x = x.mult(x);
			}
			else
			{
				break;
			}
		}

		return pow;
	}

    public virtual Algebraic realpart()
    {
        return add( cc() ).div( Zahl.TWO );
    }

    public virtual Algebraic imagpart()
    {
        return sub( cc() ).div( Zahl.TWO ).div( Zahl.IONE );
    }

	public virtual Algebraic rat()
	{
		return map( new LambdaRAT() );
	}

	public virtual Algebraic reduce()
	{
		return this;
	}

    public virtual Algebraic value( Variable item, Algebraic x )
	{
		return this;
	}

    public virtual bool depends( Variable item )
	{
		return false;
	}

	public virtual bool ratfunc( Variable v )
	{
		return true;
	}

	public virtual bool depdir( Variable item )
	{
        return depends( item ) && ratfunc( item );
	}

	public virtual bool constantq()
	{
		return false;
	}

	public virtual bool komplexq()
	{
		return !imagpart().Equals( Zahl.ZERO );
	}

	public virtual bool scalarq()
	{
		return true;
	}

	public virtual bool exaktq()
	{
		return false;
	}

	public virtual Algebraic promote(Algebraic b)
	{
		if ( b.scalarq() )
		{
			return this;
		}

		if ( b is Vektor )
		{
		    var bv = ( Vektor ) b;

		    if ( this is Vektor && ( ( Vektor ) this ).length() == bv.length() )
			{
				return this;
			}

		    if ( scalarq() )
			{
			    return new Vektor( this, bv.length() );
			}
		}

	    if ( b is Matrix )
		{
		    var bm = ( Matrix ) b;

		    if ( this is Matrix && bm.equalsized( ( Matrix ) this ) )
			{
				return this;
			}

		    if ( scalarq() )
			{
			    return new Matrix( this, bm.nrow(), bm.ncol() );
			}
		}

		throw new JasymcaException( "Wrong argument type." );
	}

	public virtual void print( PrintStream p )
	{
		p.print( StringFmt.compact( ToString() ) );
	}


    public virtual Algebraic map( LambdaAlgebraic lambda, Algebraic arg )
    {
        if ( arg == null )
		{
		    var r = lambda.f_exakt( this );

		    if ( r != null )
			{
				return r;
			}
			
            var fname = lambda.GetType().Name;

		    if ( fname.StartsWith( "Lambda", StringComparison.Ordinal ) )
			{
			    fname = fname.Substring( "Lambda".Length );
				fname = fname.ToLower();

			    return FunctionVariable.create( fname, this );
			}

		    throw new JasymcaException( "Wrong type of arguments." );
		}

        return lambda.f_exakt( this, arg );
    }

    #endregion

    #endregion

}

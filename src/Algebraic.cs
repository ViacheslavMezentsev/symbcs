using System;

public abstract class Algebraic
{

    #region Internal fields

	internal string Name = null;

    #endregion

    #region Internal methods

    internal static void Debug( string s )
    {
        Lambda.Debug(s);
    }

    #endregion
    
    #region Public methods

    #region Abstract

    protected abstract Algebraic Add( Algebraic a );

    protected abstract Algebraic Mul( Algebraic a );

    public abstract Algebraic Conj();

    public abstract Algebraic Derive( Variable v );

    public abstract Algebraic Integrate( Variable v );

    public abstract double Norm();

    public abstract Algebraic Map( LambdaAlgebraic f );

    public abstract override bool Equals( object x );

    #endregion

    #region Operators

    public static Algebraic operator +( Algebraic lhs, Algebraic rhs )
    {
        return lhs.Add( rhs );
    }

    public static Algebraic operator -( Algebraic lhs, Algebraic rhs )
    {
        return lhs + rhs * Symbolic.MINUS;
    }

    public static Algebraic operator -( Algebraic self )
    {
        return self * Symbolic.MINUS;
    }

    public static Algebraic operator *( Algebraic lhs, Algebraic rhs )
    {
        return lhs.Mul( rhs );
    }

    public static Algebraic operator /( Algebraic lhs, Algebraic rhs )
    {
        return lhs.Div( rhs );
    }

    public static Algebraic operator ^( Algebraic lhs, int rhs )
    {
        return lhs.Pow( rhs );
    }

    #endregion

    #region Virtual

    protected virtual Algebraic Div( Algebraic x )
    {
        if ( x is Polynomial )
        {
            return new Rational( this, ( Polynomial ) x ).Reduce();
        }

        if ( x is Rational )
        {
            return ( ( Rational ) x ).den.Mul( this ).Div( ( ( Rational ) x ).nom );
        }

        if ( !x.IsScalar() )
        {
            return new Matrix( this ).Div(x);
        }

        throw new JasymcaException( "Can not divide " + this + " through " + x );
    }

	public virtual Algebraic Pow( int exp )
	{
		Algebraic pow, self = this;

		if ( exp <= 0 )
		{
			if ( exp == 0 || Equals(self, Symbolic.ONE) )
			{
				return Symbolic.ONE;
			}

			if ( Equals(self, Symbolic.ZERO) )
			{
				throw new JasymcaException( "Division by Zero." );
			}

			self = Symbolic.ONE / self;

			exp = -exp;
		}

		for ( pow = Symbolic.ONE; ; )
		{
			if ( ( exp & 1 ) != 0 )
			{
				pow = pow * self;
			}

			if ( ( exp >>= 1 ) != 0 )
			{
				self = self * self;
			}
			else
			{
				break;
			}
		}

		return pow;
	}

    public virtual Algebraic RealPart()
    {
        return ( this + Conj() ) / Symbolic.TWO;
    }

    public virtual Algebraic ImagPart()
    {
        return ( this - Conj() ) / Symbolic.TWO / Symbolic.IONE;
    }

	public virtual Algebraic Rat()
	{
		return Map( new LambdaRAT() );
	}

	public virtual Algebraic Reduce()
	{
		return this;
	}

    public virtual Algebraic Value( Variable v, Algebraic a )
	{
		return this;
	}

    public virtual bool Depends( Variable v )
	{
		return false;
	}

	public virtual bool IsRat( Variable v )
	{
		return true;
	}

	public virtual bool DepDir( Variable v )
	{
        return Depends(v) && IsRat(v);
	}

	public virtual bool IsConstant()
	{
		return false;
	}

	public virtual bool IsComplex()
	{
		return ImagPart() != Symbolic.ZERO;
	}

	public virtual bool IsScalar()
	{
		return true;
	}

	public virtual bool IsNumber()
	{
		return false;
	}

	public virtual Algebraic Promote( Algebraic a )
	{
		if ( a.IsScalar() )
		{
			return this;
		}

		if ( a is Vector )
		{
		    var bv = ( Vector ) a;

		    if ( this is Vector && ( ( Vector ) this ).Length() == bv.Length() )
			{
				return this;
			}

		    if ( IsScalar() )
			{
			    return new Vector( this, bv.Length() );
			}
		}

	    if ( a is Matrix )
		{
		    var bm = ( Matrix ) a;

		    if ( this is Matrix && bm.Equalsized( ( Matrix ) this ) )
			{
				return this;
			}

		    if ( IsScalar() )
			{
			    return new Matrix( this, bm.Rows(), bm.Cols() );
			}
		}

		throw new JasymcaException( "Wrong argument type." );
	}

    public override string ToString()
    {
        return StringFmt.Compact( base.ToString() );
    }

    public virtual void Print( PrintStream stream )
	{
		stream.print( ToString() );
	}

    public virtual Algebraic Map( LambdaAlgebraic lambda, Algebraic arg )
    {
        if ( arg == null )
		{
		    var r = lambda.SymEval( this );

		    if ( r != null )
			{
				return r;
			}
			
            var fname = lambda.GetType().Name;

		    if ( fname.StartsWith( "Lambda", StringComparison.Ordinal ) )
			{
			    fname = fname.Substring( "Lambda".Length );
				fname = fname.ToLower();

			    return FunctionVariable.Create( fname, this );
			}

		    throw new JasymcaException( "Wrong type of arguments." );
		}

        return lambda.SymEval( this, arg );
    }

    #endregion

    #endregion

}

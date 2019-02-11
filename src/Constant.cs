public class Constant : SimpleVariable
{
	private Complex value;

    public Constant( string name, double value ) : base( name )
    {
        this.value = new Complex( value );
    }

    public Constant( string name, Complex value ) : base( name )
    {
        this.value = value;
    }

	public override bool Smaller(Variable v)
	{
	    if ( v is Constant )
		{
		    return name.CompareTo( ( ( Constant ) v ).name ) < 0;
		}

		return true;
	}

	internal virtual Algebraic Value
	{
		get
		{
			return value;
		}
	}
}

internal class Root : Constant
{
    internal Vector poly;
    internal int n;

    public Root( Vector poly, int n ) : base( "Root", 0.0 )
    {
        this.poly = poly;
        this.n = n;
    }

    public override bool Smaller( Variable v )
    {
        if ( !( v is Root ) )
        {
            return base.Smaller(v);
        }

        if ( !poly.Equals( ( ( Root ) v ).poly ) )
        {
            return poly.Norm() < ( ( Root ) v ).poly.Norm();
        }

        return n < ( ( Root ) v ).n;
    }

    public override bool Equals( object x )
    {
        return x is Root && ( ( Root ) x ).poly.Equals( poly ) && ( ( Root ) x ).n == n;
    }

    public override string ToString()
    {
        return string.Format( "Root({0}, {1})", new Vector( poly ), n );
    }

    internal override Algebraic Value
    {
        get
        {
            var roots = ( new Polynomial( new SimpleVariable( "x" ), poly ) ).roots();

            return roots[n];
        }
    }
}

internal class ExpandConstants : LambdaAlgebraic
{
    internal override Algebraic SymEval( Algebraic f )
    {
        while ( f is Polynomial && ( ( Polynomial ) f )._v is Constant )
        {
            f = f.Value( ( ( Polynomial ) f )._v, ( ( Constant ) ( ( Polynomial ) f )._v ).Value );
        }

        return f.Map( this );
    }
}

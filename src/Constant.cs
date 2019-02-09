public class Constant : SimpleVariable
{
	private Unexakt value;

    public Constant( string name, double value ) : base( name )
    {
        this.value = new Unexakt( value );
    }

    public Constant( string name, Unexakt value ) : base( name )
    {
        this.value = value;
    }

	public override bool smaller(Variable v)
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
    internal Vektor poly;
    internal int n;

    public Root( Vektor poly, int n ) : base( "Root", 0.0 )
    {
        this.poly = poly;
        this.n = n;
    }

    public override bool smaller( Variable v )
    {
        if ( !( v is Root ) )
        {
            return base.smaller(v);
        }

        if ( !poly.Equals( ( ( Root ) v ).poly ) )
        {
            return poly.norm() < ( ( Root ) v ).poly.norm();
        }

        return n < ( ( Root ) v ).n;
    }

    public override bool Equals( object x )
    {
        return x is Root && ( ( Root ) x ).poly.Equals( poly ) && ( ( Root ) x ).n == n;
    }

    public override string ToString()
    {
        return "Root(" + new Vektor( poly ) + ", " + n + ")";
    }

    internal override Algebraic Value
    {
        get
        {
            var roots = ( new Polynomial( new SimpleVariable( "x" ), poly ) ).roots();

            return roots.get(n);
        }
    }
}

internal class ExpandConstants : LambdaAlgebraic
{
    internal override Algebraic f_exakt( Algebraic f )
    {
        while ( f is Polynomial && ( ( Polynomial ) f ).v is Constant )
        {
            f = f.value( ( ( Polynomial ) f ).v, ( ( Constant ) ( ( Polynomial ) f ).v ).Value );
        }
        return f.map( this );
    }
}

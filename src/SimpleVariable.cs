public class SimpleVariable : Variable
{
    internal string name;
    internal static SimpleVariable top = new SimpleVariable( "top" );

    public SimpleVariable( string name )
    {
        this.name = name;
    }

    public override Algebraic Derive( Variable x )
    {
        if ( Equals(x) )
        {
            return Symbolic.ONE;
        }
        else
        {
            return Symbolic.ZERO;
        }
    }

    public override bool Equals( object x )
    {
        return x is SimpleVariable && ( ( SimpleVariable ) x ).name.Equals( name );
    }

    public override string ToString()
    {
        return name;
    }

    public virtual object toPrefix()
    {
        return name;
    }

    public override bool Smaller( Variable v )
    {
        if ( v == top )
        {
            return true;
        }

        if ( this == top )
        {
            return false;
        }

        if ( v is Constant )
        {
            return false;
        }

        if ( !( v is SimpleVariable ) )
        {
            return true;
        }

        return name.CompareTo( ( ( SimpleVariable ) v ).name ) < 0;
    }

    public override Algebraic Value( Variable item, Algebraic x )
    {
        if ( item.Equals( this ) )
        {
            return x;
        }

        return new Polynomial( this );
    }
}

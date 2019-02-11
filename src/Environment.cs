using System;
using System.Collections;

public class Environment : Hashtable
{
    internal static ArrayList Paths = new ArrayList();
    internal static Hashtable Globals = new Hashtable();

    internal virtual void addPath(string s)
	{
		if ( !Paths.Contains(s) )
		{
			Paths.Add(s);
		}
	}

	public virtual Environment copy()
	{
		var e = new Environment();

		var k = Keys.GetEnumerator();

		while ( k.MoveNext() )
		{
			var key = k.Current;

			e[ key ] = this[ key ];
		}

		return e;
	}

	public virtual void update( Environment local )
	{
		var kl = local.Keys.GetEnumerator();

        while ( kl.MoveNext() )
		{
            var key = kl.Current;

			if ( this[ key ] != null )
			{
				this[ key ] = local[ key ];
			}
		}
	}

	public override string ToString()
	{
		var k = Keys.GetEnumerator();

		var s = "";

        while ( k.MoveNext() )
		{
            var key = k.Current;

			s += key + ": ";

			s += getValue( ( string ) key ) + "\n";
		}

		k = Globals.Keys.GetEnumerator();

		s += "Globals:\n";

        while ( k.MoveNext() )
		{
            var key = k.Current;

			s += key + ": ";
			s += getValue( ( string ) key ) + "\n";
		}

		return s;
	}

	public virtual void putValue( string name, object x )
	{
		if ( x.Equals( "null" ) )
		{
            Remove( name );
		}
		else
		{
			if ( x is Lambda )
			{
			    Globals[ name ] = x;
			}
			else
			{
			    this[ name ] = x;
			}
		}
	}

    public virtual object getValue( string name )
	{
	    if ( name.StartsWith( " ", StringComparison.Ordinal ) )
		{
            return name;
		}

        var r = this[ name ];

		if ( r != null )
		{
			return r;
		}

        r = Globals[ name ];

		if ( r != null )
		{
			return r;
		}

		try
		{
            var fname = "Lambda" + name.ToUpper();

			var c = Type.GetType( fname );

			var f = ( Lambda ) Activator.CreateInstance(c);

		    putValue( name, f );

			r = f;
		}
		catch {}

		return r;
	}

	public virtual Symbolic getnum( string name )
	{
        var r = this[ name ];

		if ( r == null )
		{
            r = Globals[ name ];
		}

		if ( r is Symbolic )
		{
			return ( Symbolic ) r;
		}

		return null;
	}
}

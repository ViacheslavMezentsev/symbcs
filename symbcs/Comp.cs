using System.Collections;

internal class Comp
{
	internal static List vec2list( ArrayList v )
	{
		return new List(v);
	}

	internal static List clonelist( List x )
	{
		var y = new List();

		y.AddRange(x);

		return y;
	}

	internal static void clear( List list, int from, int to )
	{
		for ( var j = from; j < to; j++ )
		{
			list.RemoveAt(from);
		}
	}
}

public class List : ArrayList
{
	public List()
	{
	}

	public List( ArrayList x )
	{
		for ( var i = 0; i < x.Count; i++ )
		{
			Add( x[i] );
		}
	}
	public virtual object get(int i)
	{
		return this[i];
	}

	public virtual void add(object x)
	{
		Add(x);
	}

	public virtual void add(int i, object x)
	{
		Insert( i, x );
	}

	public virtual List subList(int i, int k)
	{
		var list = new List();

		for ( var j = i; j < k; j++ )
		{
			list.Add( this[j] );
		}

		return list;
	}

	public virtual List addAll(List x)
	{
		for ( var i = 0; i < x.Count; i++ )
		{
			Add( x[i] );
		}
		return this;
	}

	public virtual void remove(object x)
	{
		Remove(x);
	}

	public virtual void remove(int i)
	{
		RemoveAt(i);
	}

	public virtual void clear()
	{
		Clear();
	}
}

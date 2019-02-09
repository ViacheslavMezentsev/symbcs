using System.Collections;

public class List : ArrayList
{
	public List()
	{
	}

	public List( IEnumerable x )
	{
	    foreach ( var t in x )
	    {
	        Add(t);
	    }
	}

    public virtual List clone()
    {
        var y = new List();

        y.AddRange( this );

        return y;
    }

	public virtual List take( int i, int k )
	{
		var list = new List();

		for ( var j = i; j < k; j++ )
		{
			list.Add( this[j] );
		}

		return list;
	}

    public virtual void clear( int from, int to )
    {
        for ( var j = from; j < to; j++ )
        {
            RemoveAt( from );
        }
    }
}

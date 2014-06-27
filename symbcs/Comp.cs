using System.Collections;

internal class Comp
{
	internal static List vec2list(ArrayList v)
	{
		return new List(v);
	}
	internal static List clonelist(List x)
	{
		List y = new List();
		y.AddRange(x);
		return y;
	}
	internal static void clear(List list, int from, int to)
	{
		for (int j = from; j < to; j++)
		{
			list.RemoveAt(from);
		}
	}
}
internal class List : ArrayList
{
	public List()
	{
	}
	public List(ArrayList x)
	{
		for (int i = 0; i < x.Count; i++)
		{
			this.Add(x[i]);
		}
	}
	public virtual object get(int i)
	{
		return this[i];
	}
	public virtual void add(object x)
	{
		this.Add(x);
	}
	public virtual void add(int i, object x)
	{
		this.Insert(i, x);
	}
	public virtual List subList(int i, int k)
	{
		List list = new List();
		for (int j = i; j < k; j++)
		{
			list.Add(this[j]);
		}
		return list;
	}
	public virtual List addAll(List x)
	{
		for (int i = 0; i < x.Count; i++)
		{
			this.Add(x[i]);
		}
		return this;
	}
	public virtual void remove(object x)
	{
		this.Remove(x);
	}
	public virtual void remove(int i)
	{
		this.RemoveAt(i);
	}
	public virtual void clear()
	{
		this.Clear();
	}
}
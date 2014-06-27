using System;
using System.Collections;

public class Environment : Hashtable
{
	internal static ArrayList path = new ArrayList();
	internal static Hashtable globals = new Hashtable();
	public Environment()
	{
	}
	internal virtual void addPath(string s)
	{
		if (!path.Contains(s))
		{
			path.Add(s);
		}
	}
	public virtual Environment copy()
	{
		Environment e = new Environment();
		System.Collections.IEnumerator k = this.Keys.GetEnumerator();
		while (k.hasMoreElements())
		{
			object key = k.nextElement();
			e[key] = this[key];
		}
		return e;
	}
	public virtual void update(Environment local)
	{
		System.Collections.IEnumerator kl = local.Keys.GetEnumerator();
		while (kl.hasMoreElements())
		{
			object key = kl.nextElement();
			if (this[key] != null)
			{
				this[key] = local[key];
			}
		}
	}
	public override string ToString()
	{
		System.Collections.IEnumerator k = this.Keys.GetEnumerator();
		string s = "";
		while (k.hasMoreElements())
		{
			object key = k.nextElement();
			s += (key + ": ");
			s += (getValue((string)key) + "\n");
		}
		k = globals.Keys.GetEnumerator();
		s += "Globals:\n";
		while (k.hasMoreElements())
		{
			object key = k.nextElement();
			s += (key + ": ");
			s += (getValue((string)key) + "\n");
		}
		return s;
	}
	public virtual void putValue(string @var, object x)
	{
		if (x.Equals("null"))
		{
			this.Remove(@var);
		}
		else
		{
			if (x is Lambda)
			{
				globals[@var] = x;
			}
			else
			{
				this[@var] = x;
			}
		}
	}
	public virtual object getValue(string @var)
	{
		if (@var.StartsWith(" ", StringComparison.Ordinal))
		{
			return @var;
		}
		object r = this[@var];
		if (r != null)
		{
			return r;
		}
		r = globals[@var];
		if (r != null)
		{
			return r;
		}
		try
		{
			string fname = "Lambda" + @var.ToUpper();
			Type c = Type.GetType(fname);
			Lambda f = (Lambda)c.newInstance();
			putValue(@var, f);
			r = f;
		}
		catch (Exception)
		{
		}
		return r;
	}
	public virtual Zahl getnum(string @var)
	{
		@var = @var;
		object r = this[@var];
		if (r == null)
		{
			r = globals[@var];
		}
		if (r is Zahl)
		{
			return (Zahl)r;
		}
		return null;
	}
}
internal interface NumFmt
{
	string ToString(double x);
}
internal class NumFmtVar : NumFmt
{
	internal double @base;
	internal int ibase;
	internal int nsign;
	internal double mantisse_min, mantisse_max;
	public NumFmtVar(int ibase, int nsign)
	{
		this.@base = (double)ibase;
		this.ibase = ibase;
		this.nsign = nsign;
		mantisse_min = 1.0;
		while (nsign-- > 0)
		{
			mantisse_min *= @base;
		}
		mantisse_max = mantisse_min * @base;
	}
	public virtual string ToString(double x)
	{
		if (x < 0.0)
		{
			return "-" + ToString(-x);
		}
		if (x == 0.0)
		{
			return "0";
		}
		int exp = nsign - 1;
		while (x < mantisse_min)
		{
			exp--;
			x *= @base;
		} while (x >= mantisse_min)
		{
			exp++;
			x /= @base;
		}
		long xl = (long) JMath.round(x);
		string r = "";
		int nc = nsign;
		while (xl != 0L)
		{
			nc--;
			int digit = digit(xl % ibase);
			if (!(r.Equals("") && digit == '0'))
			{
				r = (char)digit + r;
			}
			xl = xl / ibase;
		}
		exp -= nc;
		if (exp > nsign - 1 || exp < -1)
		{
			if (r.Length == 1)
			{
				r = r + "0";
			}
			return sub(r,0,1) + "." + sub(r,1,r.Length) + "E" + exp;
		}
		if (exp == -1)
		{
			return "0." + r;
		}
		else
		{
			return sub(r,0,exp + 1) + (r.Length > exp + 1?"." + sub(r,exp + 1,r.Length):"");
		}
	}
	private int digit(long x)
	{
		if (x < 10)
		{
			return '0' + (int)x;
		}
		return 'A' + (int)x - 10;
	}
	string sub(string s, int a, int b)
	{
		if (s.length() >= b)
		{
			return s.Substring(a, b - a);
		}
		string r = "";
		while (a < b)
		{
			if (a < s.length())
			{
				r += s.charAt(a);
			}
			else
			{
				r += "0";
			}
			a++;
		}
		return r;
	}
}
internal class NumFmtJava : NumFmt
{
	private readonly NumFmtVar outerInstance;

	public NumFmtJava(NumFmtVar outerInstance)
	{
		this.outerInstance = outerInstance;
	}

	public virtual string ToString(double x)
	{
		return "" + x;
	}
}
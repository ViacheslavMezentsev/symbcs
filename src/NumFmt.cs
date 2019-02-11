using System.Globalization;

internal interface INumFmt
{
	string ToString( double x );
}

public class NumFmtVar : INumFmt
{
	internal double dbase;
	internal int ibase;
	internal int nsign;
	internal double mantisse_min, mantisse_max;

	public NumFmtVar( int ibase, int nsign )
	{
		this.dbase = (double) ibase;
		this.ibase = ibase;
		this.nsign = nsign;

		mantisse_min = 1.0;

		while ( nsign-- > 0 )
		{
			mantisse_min *= dbase;
		}

		mantisse_max = mantisse_min * dbase;
	}

	public virtual string ToString( double x )
	{
		if ( x < 0.0 )
		{
			return "-" + ToString( -x );
		}

		if ( x == 0.0 )
		{
			return "0";
		}

		int exp = nsign - 1;

		while ( x < mantisse_min )
		{
			exp--;
			x *= dbase;
		} 
        
        while ( x >= mantisse_min )
		{
			exp++;
			x /= dbase;
		}

		long xl = ( long ) JMath.round(x);

		string r = "";
		int nc = nsign;

		while ( xl != 0L )
		{
			nc--;

			int _digit = digit( xl % ibase );

		    if ( !( r.Equals( "" ) && _digit == '0' ) )
			{
			    r = ( char ) _digit + r;
			}

			xl = xl / ibase;
		}

		exp -= nc;

	    if ( exp > nsign - 1 || exp < -1 )
		{
		    if ( r.Length == 1 )
			{
				r = r + "0";
			}

		    return sub( r, 0, 1 ) + "." + sub( r, 1, r.Length ) + "E" + exp;
		}

	    if ( exp == -1 )
		{
			return "0." + r;
		}
		else
		{
		    return sub( r, 0, exp + 1 ) + ( r.Length > exp + 1 ? "." + sub( r, exp + 1, r.Length ) : "" );
		}
	}

	private int digit(long x)
	{
		if ( x < 10 )
		{
			return '0' + ( int ) x;
		}

		return 'A' + ( int ) x - 10;
	}

    string sub( string s, int a, int b )
	{
		if ( s.Length >= b )
		{
		    return s.Substring( a, b - a );
		}

		var r = "";

	    while ( a < b )
		{
		    if ( a < s.Length )
			{
				r += s[a];
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

public class NumFmtJava : INumFmt
{
	public NumFmtJava()
	{
	}

	public virtual string ToString( double x )
	{
	    var cultureInfo = new CultureInfo( "" ) { NumberFormat = { NumberDecimalSeparator = "." } };

        return string.Format( cultureInfo, "{0:0.################E0}", x );
	}
}

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

public abstract class Parser : Constants
{
	internal static readonly int? ONE = 1, TWO = 2, THREE = 3;
	internal ParserState pst;

    internal Parser( Environment env )
	{
	}

    internal abstract void translate( string s );
    internal abstract bool ready();
    internal abstract List get();
    public abstract List compile( Stream instream, PrintStream ps );
    public abstract List compile( string s );
    public abstract string prompt();
    internal abstract List compile_expr( List expr );
    internal abstract List compile_statement( List expr );
    internal abstract List compile_lval( List expr );
    internal abstract List compile_list( List expr );
    internal abstract List compile_func( List expr );
    internal abstract bool commandq( object expr );

    internal virtual List compile_command_args( List expr )
    {
        var s = new List();

        for ( int n = expr.Count - 1; n >= 0; n-- )
        {
            var x = expr[n];

            if ( x is Algebraic )
            {
                s.Add(x);
            }
            else if ( symbolq(x) )
            {
                s.Add( "$" + x );
            }
            else if ( stringq(x) )
            {
                s.Add( "$" + ( ( string ) x ).Substring(1) );
            }
            else if ( x is ArrayList )
            {
                s.AddRange( compile_command_args( ( List ) x ) );
            }
        }

        return s;
	}

	internal virtual void reset()
	{
	    pst = new ParserState( null, 0 );
	}

    internal virtual Rule[] compile_rules( string[][] s )
	{
	    var r = new Rule[ s.Length ];

	    for ( int i = 0; i < s.Length; i++ )
		{
			var r1 = new Rule();

			reset();

			translate(s[i][0]);

			r1.rule_in = pst.tokens;

			reset();

			translate(s[i][1]);

			r1.rule_out = pst.tokens;

			r[i] = r1;
		}

		return r;
	}

    internal virtual List compile_command( List expr )
	{
		if (expr == null || expr.Count == 0 || !(commandq(expr[0])))
		{
			return null;
		}

	    var s = compile_command_args( expr.take( 1, expr.Count ) );

	    s.Add( s.Count );

		try
		{
			var command = ( string ) expr[0];

			var c = Type.GetType( "Lambda" + command.ToUpper() );

			s.Add( ( Lambda ) Activator.CreateInstance(c) );

			return s;
		}
		catch
		{
			return null;
		}
	}

	internal ArrayList nonsymbols = new ArrayList();

    internal virtual bool symbolq( object expr )
	{
	    return expr is string && ( ( string ) expr ).Length > 0 && ( ( string ) expr )[0] != ' ' && !nonsymbols.Contains( expr );
	}

    internal virtual bool stringq( object expr )
	{
	    return expr is string && ( ( string ) expr ).Length > 0 && ( ( string ) expr )[0] == ' ';
	}

    internal static bool oneof( char c, string s )
	{
		return s.IndexOf(c) != -1;
	}

    internal static bool oneof( object c, string s )
	{
	    return c is string && ( ( string ) c ).Length > 0 && oneof( ( ( string ) c )[0], s );
	}

    internal bool oneof( object c, object[] d )
	{
	    return d.Contains(c);
	}

    internal static bool whitespace( char c )
    {
        return oneof( c, " \t\n\r" );
    }

    internal static void skipWhitespace( StringBuilder s )
	{
		int i = 0;

	    while ( i < s.Length && whitespace( s[i] ) )
		{
			i++;
		}

	    s.Remove( 0, i );
	}

	internal static int nextIndexOf(object x, int idx, List list)
	{
        int n = list.Count;

        while ( idx < n )
        {
            if ( x.Equals( list[ idx ] ) )
            {
                return idx;
            }

            idx++;
        }

        return -1;
	}

    internal static Zahl readNumber( StringBuilder s )
	{
        int kmax = 0;

        while ( kmax < s.Length && oneof( s[ kmax ], "0123456789.eE+-" ) )
        {
            kmax++;
        }

        var sub = s.ToString().Substring( 0, kmax );

        var cultureInfo = new CultureInfo( "" ) { NumberFormat = { NumberDecimalSeparator = "." } };

        for ( int k = kmax; k > 0; k-- )
        {
            try
            {
                var ts = sub.Substring( 0, k );

                var x = double.Parse( ts, cultureInfo );

                if ( ts.EndsWith( ".", StringComparison.Ordinal ) && s.Length > k && ( s[k] == '^' || s[k] == '/' ) )
                {
                    continue;
                }

                bool imag = false;

                if ( s.Length > k && ( s[k] == 'i' || s[k] == 'j' ) )
                {
                    imag = true;
                    k++;
                }

                s.Remove( 0, k );

                if ( Math.Abs(x) > 1e15 )
                {
                    try
                    {
                        var bi = new BigInteger( ts, 10 );

                        return imag ? ( Zahl ) ( new Exakt( bi ) ).mult( Zahl.IONE ) : new Exakt( bi );
                    }
                    catch ( Exception )
                    {
                    }
                }

                return imag ? new Unexakt( 0, x ) : new Unexakt( x );
            }
            catch ( Exception )
            {
            }
        }

        throw new ParseException( "Internal Error." );
	}

    internal static string cutstring( StringBuilder sb, char a, char b )
	{
        sb.Remove( 0, 1 );

        var s = sb.ToString();

        int cnt = 1, i;

        for ( i = 0; i < s.Length; i++ )
		{
			char c = s[i];

            if ( a != b && c == a )
            {
                cnt++;
            }
            else if ( c == b )
            {
                cnt--;
            }

            if ( cnt == 0 )
            {
                break;
            }
        }

        if ( cnt != 0 )
        {
            throw new ParseException( "Unclosed " + a );
        }

        s = s.Substring( 0, i );
        sb.Remove( 0, i + 1 );

        return s;
	}

	internal static string[] listsep = { ",", ";" };
	internal static string[] stringops = { "=", "," };

    internal bool stringopq( object x )
	{
	    return oneof( x, stringops );
	}

    internal static bool number( char c )
	{
	    return oneof( c, "0123456789" );
	}

	internal static string readLine( Stream stream )
	{
		var sb = new StringBuilder();

		int b;

		while ( ( b = stream.ReadByte() ) !=  -1 )
		{
		    var c = ( char ) b;

			sb.Append(c);

			if (c == '\n' || c == '\r')
			{
				return sb.ToString();
			}
		}

		if (sb.Length > 0)
		{
			return sb.ToString();
		}

		return null;
	}
}

internal class ParserState
{
	internal object sub;
	internal object prev;
	internal List tokens;
	internal int inList;

    internal ParserState( object sub, int inList )
	{
		this.sub = sub;
		this.prev = null;
	    this.tokens = new List();
		this.inList = inList;
	}
}

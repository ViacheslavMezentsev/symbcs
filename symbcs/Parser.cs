using System;
using System.Collections;
using System.IO;
using System.Text;

public abstract class Parser : Constants
{
	internal static readonly int? ONE = new int?(1), TWO = new int?(2), THREE = new int?(3);
	internal ParserState pst;
	internal Parser(Environment env)
	{
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: abstract void translate(String s) throws ParseException;
	internal abstract void translate(string s);
	internal abstract bool ready();
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: abstract List get() throws ParseException;
	internal abstract List get();
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract List compile(InputStream is, PrintStream ps) throws ParseException, IOException;
	public abstract List compile( Stream instream, PrintStream ps);
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public abstract List compile(String s) throws ParseException;
	public abstract List compile(string s);
	public abstract string prompt();
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: abstract List compile_expr(List expr) throws ParseException;
	internal abstract List compile_expr(List expr);
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: abstract List compile_statement(List expr) throws ParseException;
	internal abstract List compile_statement(List expr);
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: abstract List compile_lval(List expr) throws ParseException;
	internal abstract List compile_lval(List expr);
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: abstract List compile_list(List expr) throws ParseException;
	internal abstract List compile_list(List expr);
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: abstract List compile_func(List expr) throws ParseException;
	internal abstract List compile_func(List expr);
	internal abstract bool commandq(object expr);
	internal virtual List compile_command_args(List expr)
	{
		List s = Comp.vec2list(new ArrayList());
		for (int n = expr.Count - 1; n >= 0; n--)
		{
			object x = expr[n];
			if (x is Algebraic)
			{
				s.Add(x);
			}
			else if (symbolq(x))
			{
				s.Add("$" + x);
			}
			else if (stringq(x))
			{
				s.Add("$" + ((string)x).Substring(1));
			}
			else if (x is ArrayList)
			{
				s.AddRange(compile_command_args((List)x));
			}
		}
		return s;
	}
	internal virtual void reset()
	{
		pst = new ParserState(null, 0);
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Rule[] compile_rules(String[][] s) throws ParseException
	internal virtual Rule[] compile_rules(string[][] s)
	{
		Rule[] r = new Rule[s.Length];
		for (int i = 0; i < s.Length; i++)
		{
			Rule r1 = new Rule();
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
	internal virtual List compile_command(List expr)
	{
		if (expr == null || expr.Count == 0 || !(commandq(expr[0])))
		{
			return null;
		}

	    var s = compile_command_args( expr.subList( 1, expr.Count ) );

	    s.Add( new int?( s.Count ) );

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
	internal virtual bool symbolq(object expr)
	{
		return expr is string && ((string)expr).Length > 0 && ((string)expr)[0] != ' ' && !nonsymbols.Contains(expr);
	}
	internal virtual bool stringq(object expr)
	{
		return expr is string && ((string)expr).Length > 0 && ((string)expr)[0] == ' ';
	}
	internal static bool oneof(char c, string s)
	{
		return s.IndexOf(c) != -1;
	}
	internal static bool oneof(object c, string s)
	{
		return c is string && ((string)c).Length > 0 && oneof(((string)c)[0], s);
	}
	internal bool oneof(object c, object[] d)
	{
		for (int i = 0; i < d.Length; i++)
		{
			if (d[i].Equals(c))
			{
				return true;
			}
		}
			return false;
	}
	internal static bool whitespace(char c)
	{
		return oneof(c," \t\n\r");
	}
	internal static void skipWhitespace(StringBuilder s)
	{
		int i = 0;
		while (i < s.Length && whitespace(s[i]))
		{
			i++;
		}
		s.Remove(0, i);
	}
	internal static int nextIndexOf(object x, int idx, List list)
	{
		int n = list.Count;
		while (idx < n)
		{
			if (x.Equals(list[idx]))
			{
				return idx;
			}
			idx++;
		}
		return -1;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static Zahl readNumber(StringBuffer s) throws ParseException
	internal static Zahl readNumber(StringBuilder s)
	{
		int kmax = 0;
		while (kmax < s.Length && oneof(s[kmax],"0123456789.eE+-"))
		{
			kmax++;
		}
		//char[] substring = new char[kmax];
        //
		//s.getChars(0,kmax,substring,0);
        //
		//string sub = new string(substring);

        string sub = s.ToString().Substring( 0, kmax );

		for (int k = kmax; k > 0; k--)
		{
			try
			{
				string ts = sub.Substring(0,k);
				double x = Convert.ToDouble(ts);
				if (ts.EndsWith(".", StringComparison.Ordinal) && s.Length > k && (s[k] == '^' || s[k] == '/'))
				{
					continue;
				}
				bool imag = false;
				if (s.Length > k && (s[k] == 'i' || s[k] == 'j'))
				{
					imag = true;
					k++;
				}
				s.Remove(0, k);
				if (Math.Abs(x) > 1e15)
				{
					try
					{
						BigInteger bi = new BigInteger(ts,10);
						return imag ? (Zahl)(new Exakt(bi)).mult(Zahl.IONE): new Exakt(bi);
					}
					catch (Exception)
					{
					}
				}
				return imag ? new Unexakt(0,x) : new Unexakt(x);
			}
			catch (Exception)
			{
			}
		}
		throw new ParseException("Internal Error.");
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static String cutstring(StringBuffer sb, char a, char b) throws ParseException
	internal static string cutstring(StringBuilder sb, char a, char b)
	{
		sb.Remove(0, 1);
		string s = sb.ToString();
		int cnt = 1, i ;
		for (i = 0; i < s.Length; i++)
		{
			char c = s[i];
			if (a != b && c == a)
			{
				cnt++;
			}
			else if (c == b)
			{
				cnt--;
			}
			if (cnt == 0)
			{
				break;
			}
		}
		if (cnt != 0)
		{
			throw new ParseException("Unclosed " + a);
		}
		s = s.Substring(0,i);
		sb.Remove(0, i + 1);
		return s;
	}
	internal static string[] listsep = new string[] {",", ";"};
	internal static string[] stringops = new string[] {"=", ","};
	internal bool stringopq(object x)
	{
		return oneof(x, stringops);
	}
	internal static bool number(char c)
	{
		return oneof(c,"0123456789");
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static String readLine(InputStream is) throws IOException
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
	internal ParserState(object sub, int inList)
	{
		this.sub = sub;
		this.prev = null;
		this.tokens = Comp.vec2list(new ArrayList());
		this.inList = inList;
	}
}
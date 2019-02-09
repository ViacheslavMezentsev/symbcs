using System;
using System.Collections;
using System.IO;
using System.Text;

internal class MaximaParser : Parser
{
	internal Lambda CRV = new CreateVector();
	internal Lambda REF = new REFX();
	internal int IN_PARENT = 1;
	internal int IN_BRACK = 2;
	internal int IN_BLOCK = 4;
	internal Rule[] rules;
	internal string[][] rules_in = new string[][]
	{
		new string[] {"for u step v thru w do ( X )", "X w v u 4 XFOR"},
		new string[] {"while u do ( X )", "X u 2 WHILE"},
		new string[] {"if u then ( X ) else ( Y )", "Y X u 3 BRANCH"},
		new string[] {"if u then ( X )", "X u 2 BRANCH"}
	};
	internal string[] commands = new string[] {"format", "hold", "clear", "addpath"};
	public MaximaParser(Environment env) : base(env)
	{
		env.addPath(".");

		Environment.globals.Add("pi", Zahl.PI);
        Environment.globals.Add( "i", Zahl.IONE );
        Environment.globals.Add( "j", Zahl.IONE );
        Environment.globals.Add( "eps", new Unexakt( 2.220446049250313E-16 ) );
        Environment.globals.Add( "ratepsilon", new Unexakt( 2.0e-8 ) );
        Environment.globals.Add( "algepsilon", new Unexakt( 1.0e-8 ) );
	    Environment.globals.Add( "rombergit", new Unexakt( 11 ) );
	    Environment.globals.Add( "rombergtol", new Unexakt( 1.0e-4 ) );

		pst = new ParserState(null, 0);
		Operator.OPS = new Operator[]
		{
		    new Operator("PPR", "++", 1, Fields.RIGHT_LEFT, Fields.UNARY | Fields.LVALUE), 
            new Operator("MMR", "--", 1, Fields.RIGHT_LEFT, Fields.UNARY | Fields.LVALUE), 
            new Operator("PPL", "++", 1, Fields.LEFT_RIGHT, Fields.UNARY | Fields.LVALUE), 
            new Operator("MML", "--", 1, Fields.LEFT_RIGHT, Fields.UNARY | Fields.LVALUE), 
            new Operator("MPW", "^^", 1, Fields.LEFT_RIGHT, Fields.BINARY), 
            new Operator("POW", "**", 1, Fields.LEFT_RIGHT, Fields.BINARY), 
            new Operator("FCN", ":=",10, Fields.RIGHT_LEFT, Fields.BINARY | Fields.LVALUE | Fields.LIST), 
            new Operator("POW", "^", 1, Fields.LEFT_RIGHT, Fields.BINARY), 
            new Operator("EQU", "==", 6, Fields.LEFT_RIGHT, Fields.BINARY), 
            new Operator("NEQ", "!=", 6, Fields.LEFT_RIGHT, Fields.BINARY), 
            new Operator("GEQ", ">=", 6, Fields.LEFT_RIGHT, Fields.BINARY), 
            new Operator("LEQ", "<=", 6, Fields.LEFT_RIGHT, Fields.BINARY), 
            new Operator("DIV", "/", 3, Fields.LEFT_RIGHT, Fields.BINARY), 
            new Operator("SUB", "=", 5, Fields.LEFT_RIGHT, Fields.BINARY), 
            new Operator("GRE", ">", 6, Fields.LEFT_RIGHT, Fields.BINARY), 
            new Operator("LES", "<", 6, Fields.LEFT_RIGHT, Fields.BINARY), 
            new Operator("OR", "|", 9, Fields.LEFT_RIGHT, Fields.BINARY), 
            new Operator("NOT", "~", 8, Fields.LEFT_RIGHT, Fields.UNARY), 
            new Operator("AND", "&", 7, Fields.LEFT_RIGHT, Fields.BINARY), 
            new Operator("ASS", ":", 10, Fields.RIGHT_LEFT, Fields.BINARY | Fields.LVALUE), 
            new Operator("ADD", "+", 4, Fields.LEFT_RIGHT, Fields.UNARY | Fields.BINARY), 
            new Operator("SUB", "-", 4, Fields.LEFT_RIGHT, Fields.UNARY | Fields.BINARY), 
            new Operator("MMU", ".", 3, Fields.LEFT_RIGHT, Fields.BINARY), 
            new Operator("MUL", "*", 3, Fields.LEFT_RIGHT, Fields.BINARY), 
            new Operator("MDR", "/", 3, Fields.LEFT_RIGHT, Fields.BINARY), 
            new Operator("MDL", "\\", 3, Fields.LEFT_RIGHT, Fields.BINARY), 
            new Operator("ADJ", "'", 1, Fields.RIGHT_LEFT, Fields.UNARY), 
            new Operator("FCT", "!", 1, Fields.RIGHT_LEFT, Fields.UNARY)
		};
		for (int i = 0; i < Operator.OPS.Length; i++)
		{
			nonsymbols.Add(Operator.OPS[i].symbol);
		}
		for (int i = 0; i < listsep.Length; i++)
		{
			nonsymbols.Add(listsep[i]);
		}
		for (int i = 0; i < commands.Length; i++)
		{
			nonsymbols.Add(commands[i]);
		}
		for (int i = 0; i < keywords.Length; i++)
		{
			nonsymbols.Add(keywords[i]);
		}
		try
		{
			rules = compile_rules(rules_in);
		}
		catch (ParseException)
		{
			Console.WriteLine("Failed to compile rules.");
		}
		Lambda.pr = this;
	}
	internal int prompt_Renamed = 1;
	public override string prompt()
	{
		return "(c" + prompt_Renamed++ +") ";
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public List compile(InputStream is, PrintStream ps) throws ParseException, IOException
	public override List compile( Stream istream, PrintStream ps)
	{
		string s , sp = null;
		reset();
		while ((s = readLine(istream)) != null)
		{
			sp = s;
			translate(s);
			if (ready())
			{
				break;
			}
			else
			{
				if (ps != null)
				{
					ps.print("> ");
				}
			}
		}
		if (sp == null)
		{
			return null;
		}
		if (s == null && pst.inList == IN_BLOCK)
		{
			List v = pst.tokens;
			pst = (ParserState)pst.sub;
			pst.tokens.Add(v);
		}
		return get();
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public List compile(String s) throws ParseException
	public override List compile(string s)
	{
		reset();
		translate(s);
		return get();
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: List get() throws ParseException
	internal override List get()
	{
		List r = pst.tokens;
		List pgm = compile_statement(r);
		if (pgm != null)
		{
			return pgm;
		}
		throw new ParseException("Compilation failed.");
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void translate(String s) throws ParseException
	internal override void translate(string s)
	{
		if (s == null)
		{
			return;
		}
		StringBuilder sb = new StringBuilder(s);
		object t;
		while ((t = nextToken(sb)) != null)
		{
			pst.tokens.Add(t);
			pst.prev = t;
		}
	}
	internal static string FOR = "for", WHILE = "while", IF = "if", THEN = "then", ELSE = "else", BREAK = "break", RETURN = "return", CONTINUE = "continue", EXIT = "exit", STEP = "step", THRU = "thru", DO = "do";
	private string[] keywords = new string[] {FOR, WHILE, IF, THEN, ELSE, BREAK, RETURN, CONTINUE, EXIT, STEP, THRU, DO};
	private string sepright = ")]+-*/^!,;:=.<>'\\";
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Object nextToken(StringBuffer s) throws ParseException
	public virtual object nextToken(StringBuilder s)
	{
		skipWhitespace(s);
		if (s.Length < 1)
		{
			return null;
		}
		char c0 = s[0];
		switch (c0)
		{
			case '"':
				return ' ' + cutstring(s,'"','"');
			case '(':
				pst = new ParserState(pst, IN_PARENT);
				return nextToken(s.Remove(0, 1));
			case ')':
				if (pst.inList == IN_BRACK)
				{
					throw new ParseException("Wrong parenthesis.");
				}
				while (pst.inList == IN_BLOCK)
				{
					List v = pst.tokens;
					pst = (ParserState)pst.sub;
					pst.tokens.Add(v);
				}
				if (pst.inList != IN_PARENT)
				{
					throw new ParseException("Wrong parenthesis.");
				}
				List t = pst.tokens;
				pst = (ParserState)pst.sub;
				s.Remove(0, 1);
				return t;
			case '[':
				pst = new ParserState(pst, IN_BRACK);
				return nextToken(s.Remove(0, 1));
			case ']':
				if (pst.inList != IN_BRACK)
				{
					throw new ParseException("Wrong brackets.");
				}
				t = pst.tokens;
				while (t.Count > 0 && ";".Equals(t[t.Count - 1]))
				{
                    // TODO: Check this
					t.RemoveAt(t.Count - 1);
				}
				t.Insert(0, "[");
				pst = (ParserState)pst.sub;
				s.Remove(0, 1);
				return t;
			case '%':
		case '#':
			s.Remove(0, s.Length);
			return null;
		case '\'':
			if (pst.prev == null || stringopq(pst.prev))
			{
				return ' ' + cutstring(s,'\'','\'');
			}
			else
			{
				return readString(s);
			}
			case ',':
				s.Remove(0, 1);
				return "" + c0;
			case ';':
				closeBlocks();
				s.Remove(0, 1);
				return "" + c0;
			case '0':
		case '1':
	case '2':
case '3':
case '4':
case '5':
case '6':
case '7':
case '8':
case '9':
	return readNumber(s);
case '.':
	if (s.Length > 1 && number(s[1]))
	{
		return readNumber(s);
	}
	else
	{
		return readString(s);
	}
	default :
		return readString(s);
		}
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void closeBlocks() throws ParseException
	internal virtual void closeBlocks()
	{
		while (pst.inList != 0)
		{
			if (pst.inList == IN_BRACK || pst.inList == IN_PARENT)
			{
				throw new ParseException("Unclosed brackets or parenthesis.");
			}
			if (pst.inList == IN_BLOCK)
			{
				List v = pst.tokens;
				pst = (ParserState)pst.sub;
				pst.tokens.Add(v);
			}
		}
	}
	internal override bool ready()
	{
		return pst.tokens.Count != 0 && ";".Equals(pst.tokens[pst.tokens.Count - 1]);
	}
	private string separator = "()[]\n\t\r +-*/^!,;:=.<>'\\&|";
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Object readString(StringBuffer s)throws ParseException
	internal virtual object readString(StringBuilder s)
	{
		int len = s.Length > 1?2:s.Length;
		//char[] substring = new char[len];
		//s.getChars(0,len,substring,0);
		//string st = new string(substring);

	    string st = s.ToString().Substring( 0, len );

		Operator op = Operator.get(st);
		if (op != null)
		{
			s.Remove(0, op.symbol.Length);
			return op.symbol;
		}
		int k = 1;
		while (k < s.Length && !oneof(s[k], separator))
		{
			k++;
		}
		//substring = new char[k];
		//s.getChars(0,k,substring,0);
		//string t = new string(substring);

        string t = s.ToString().Substring( 0, k );

		s.Remove(0, k);
		if (t.Equals(IF) || t.Equals(FOR) || t.Equals(WHILE))
		{
			if (pst.inList == IN_BRACK)
			{
				throw new ParseException("Block starts within vector.");
			}
			pst.tokens.Add(t);
			pst = new ParserState(pst, IN_BLOCK);
			return nextToken(s);
		}
		if (t.Equals(STEP) | t.Equals(THRU))
		{
			if (pst.inList != IN_BLOCK)
			{
				throw new ParseException("Orphaned " + t);
			}
			List v = pst.tokens;
			((ParserState)pst.sub).tokens.Add(v);
			pst = new ParserState(pst.sub, IN_BLOCK);
			return ELSE;
		}
		if (t.Equals(THEN) || t.Equals(DO))
		{
			if (pst.inList != IN_BLOCK)
			{
				throw new ParseException("Orphaned " + t);
			}
			List v = pst.tokens;
			((ParserState)pst.sub).tokens.Add(v);
			pst = (ParserState)pst.sub;
			return t;
		}
		return t;
	}
	internal virtual bool expressionq(object expr)
	{
		return expr != null && !operatorq(expr);
	}
	internal virtual bool operatorq(object expr)
	{
		return Operator.get(expr) != null;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: List compile_unary(Operator op, List expr)throws ParseException
	internal virtual List compile_unary(Operator op, List expr)
	{
		List arg_in = (op.left_right() ? expr.take(1, expr.Count) : expr.take(0, expr.Count - 1));
		List arg = (op.lvalue() ? compile_lval(arg_in) : compile_expr(arg_in));
		if (arg == null)
		{
			return null;
		}
		arg.Add(ONE);
		arg.Add(op.Lambda);
		return arg;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: List compile_ternary(Operator op, List expr, int k)throws ParseException
	internal virtual List compile_ternary(Operator op, List expr, int k)
	{
		int n = expr.Count;
		for (int k0 = k + 2; k0 < n - 1; k0++)
		{
			if (op.symbol.Equals(expr[k0]))
			{
				List left_in = expr.take(0, k);
				List left = compile_expr(left_in);
				if (left == null)
				{
					continue;
				}
				List mid_in = expr.take(k + 1, k0);
				List mid = compile_expr(mid_in);
				if (mid == null)
				{
					continue;
				}
				List right_in = expr.take(k0 + 1, expr.Count);
				List right = compile_expr(right_in);
				if (right == null)
				{
					continue;
				}
				left.AddRange(mid);
				left.AddRange(right);
				left.Add(THREE);
				left.Add(op.Lambda);
				return left;
			}
		}
		return null;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: List compile_binary(Operator op, List expr, int k)throws ParseException
	internal virtual List compile_binary(Operator op, List expr, int k)
	{
		List left_in = expr.take(0, k);
		List left = (op.lvalue() ? compile_lval(left_in) : compile_expr(left_in));
		if (left == null)
		{
			return null;
		}
		;
		List right_in = expr.take(k + 1, expr.Count);
		List right = compile_expr(right_in);
		if (right == null)
		{
			return null;
		}
		int? nargs = TWO;
		if (op.lvalue())
		{
			nargs = ONE;
		}
		if (op.list())
		{
			left.Add(right);
		}
		else
		{
			left.AddRange(right);
		}
		left.Add(nargs);
		left.Add(op.Lambda);
		return left;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: List translate_op(List expr) throws ParseException
	internal virtual List translate_op(List expr)
	{
		List s;
		int n = expr.Count;
		for (int pred = 10; pred >= 0; pred--)
		{
			for (int i = 0; i < n; i++)
			{
				int k = i;
				if (pred != 5)
				{
					k = n - i - 1;
				}
                Operator op = Operator.get( expr[ k ], k == 0 ? Fields.START : ( k == n - 1 ? Fields.END : Fields.MID ) );
				if (op == null || op.precedence != pred)
				{
					continue;
				}
				if (op.unary() && ((k == 0 && op.left_right()) || (k == n - 1 && !op.left_right())))
				{
					s = compile_unary(op, expr);
					if (s != null)
					{
						return s;
					}
					else
					{
						continue;
					}
				}
				if (k > 0 && k < n - 3 && op.ternary())
				{
					s = compile_ternary(op, expr, k);
					if (s != null)
					{
						return s;
					}
				}
				if (k > 0 && k < n - 1 && op.binary())
				{
					s = compile_binary(op, expr, k);
					if (s != null)
					{
						return s;
					}
				}
			}
		}
		return null;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: List compile_vektor(List expr) throws ParseException
	internal virtual List compile_vektor(List expr)
	{
		if (expr == null || expr.Count == 0 || !"[".Equals(expr[0]))
		{
			return null;
		}
		expr = expr.take(1, expr.Count);
		List r = new List();
		int nrow = 1;
		List x = expr;
		List xs = compile_list(x);
		if (xs == null)
		{
			return null;
		}
		xs.AddRange(r);
		r = xs;
		r.Add(new int?(nrow));
		r.Add(CRV);
		return r;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: List compile_list(List expr) throws ParseException
	internal override List compile_list(List expr)
	{
		if (expr == null)
		{
			return null;
		}
		List r = new List();
		if (expr.Count == 0)
		{
			r.Add(new int?(0));
			return r;
		}
		int i , ip = 0, n = 1;
		while ((i = nextIndexOf(",",ip,expr)) != -1)
		{
			List x = expr.take(ip, i);
			List xs = compile_expr(x);
			if (xs == null)
			{
				return null;
			}
			xs.AddRange(r);
			r = xs;
			n++;
			ip = i + 1;
		}
		List x1 = expr.take(ip, expr.Count);
		List xs1 = compile_expr(x1);
		if (xs1 == null)
		{
			return null;
		}
		xs1.AddRange(r);
		r = xs1;
		r.Add(new int?(n));
		return r;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: List compile_lval(List expr) throws ParseException
	internal override List compile_lval(List expr)
	{
		if (expr == null || expr.Count == 0)
		{
			return null;
		}
		List r = compile_lval1(expr);
		if (r != null)
		{
			return r;
		}
		if (expr.Count == 1)
		{
			if (expr[0] is List)
			{
				return compile_lval((List)expr[0]);
			}
			else
			{
				return null;
			}
		}
		if (!"[".Equals(expr[0]))
		{
			return null;
		}
		expr = expr.take(1, expr.Count);
		r = new List();
		int i , n = 1;
		while ((i = expr.IndexOf(",")) != -1)
		{
			List x = expr.take(0, i);
			List xs = compile_lval1(x);
			if (xs == null)
			{
				return null;
			}
			xs.AddRange(r);
			r = xs;
			expr = expr.take(i + 1, expr.Count);
			n++;
		}
		List xs1 = compile_lval1(expr);
		if (xs1 == null)
		{
			return null;
		}
		xs1.AddRange(r);
		r = xs1;
		r.Insert(0, new int?(n));
		return r;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: List compile_lval1(List expr) throws ParseException
	internal virtual List compile_lval1(List expr)
	{
		if (expr == null)
		{
			return null;
		}
		switch (expr.Count)
		{
			case 1:
				if (expr[0] is List)
				{
					return compile_lval1((List)expr[0]);
				}
				if (symbolq(expr[0]))
				{
					List s = new List();
					s.Add("$" + expr[0]);
					return s;
				}
				return null;
			case 2:
				if (!symbolq(expr[0]) || !(expr[1] is List))
				{
					return null;
				}
				List @ref = compile_index((List)expr[1]);
				if (@ref == null)
				{
					@ref = compile_list((List)expr[1]);
				}
				if (@ref == null)
				{
					return null;
				}
				@ref.Add("$" + expr[0]);
				return @ref;
			default:
				return null;
		}
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: List compile_index(List expr) throws ParseException
	internal virtual List compile_index(List expr)
	{
		if (expr.Count < 1 || !"[".Equals(expr[0]))
		{
			return null;
		}
		return compile_list(expr.take(1,expr.Count));
	}
	internal override bool commandq(object x)
	{
		return oneof(x, commands);
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: List compile_statement(List expr_in) throws ParseException
	internal override List compile_statement(List expr_in)
	{
		if (expr_in == null)
		{
			return null;
		}
		if (expr_in.Count == 0)
		{
			return new List();
		}
		List expr = expr_in.clone();
		object first = expr[0];
		for (int i = 0; i < rules.Length; i++)
		{
			Rule r = rules[i];
			if (r.rule_in[0].Equals(first) && expr.Count >= r.rule_in.Count)
			{
				Compiler c = new Compiler(r.rule_in, r.rule_out, this);
				List expr_sub = expr.take(0, r.rule_in.Count);
				List s = c.compile(expr_sub);
				if (s != null)
				{
					expr.clear( 0, expr_sub.Count );
					if (expr.Count == 0)
					{
						return s;
					}
					List t = compile_statement(expr);
					if (t == null)
					{
						return null;
					}
					s.AddRange(t);
					return s;
				}
			}
		}
		if (commandq(first))
		{
			return compile_command(expr);
		}
		string lend = null;
		int ic = expr.IndexOf(",");
		int @is = expr.IndexOf(";");
		if (ic >= 0 && (ic < @is || @is == -1))
		{
			lend = "#,";
		}
		else if (@is >= 0 && (@is < ic || ic == -1))
		{
			lend = "#;";
			ic = @is;
		}
		if (ic == 0)
		{
			expr.clear(0,1);
			return compile_statement(expr);
		}
		if (lend != null)
		{
			List expr_sub = expr.take(0, ic);
			List s = compile_expr(expr_sub);
			if (s != null)
			{
				s.Add(lend);
				expr.clear(0,ic + 1);
				if (expr.Count == 0)
				{
					return s;
				}
				List t = compile_statement(expr);
				if (t == null)
				{
					return null;
				}
				s.AddRange(t);
				return s;
			}
		}
		else
		{
			return compile_expr(expr);
		}
		return null;
	}
	internal virtual string compile_keyword(object x)
	{
		if (x.Equals(BREAK))
		{
			return "#brk";
		}
		else if (x.Equals(CONTINUE))
		{
			return "#cont";
		}
		else if (x.Equals(EXIT))
		{
			return "#exit";
		}
		else if (x.Equals(RETURN))
		{
			return "#ret";
		}
		return null;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: List compile_func(List expr) throws ParseException
	internal override List compile_func(List expr)
	{
		if (expr.Count == 2)
		{
			object op = expr[0];
			object ref_in = expr[1];
			if (symbolq(op) && ref_in is List)
			{
				List @ref = compile_list((List)ref_in);
				if (@ref != null)
				{
					@ref.Add(op);
					return @ref;
				}
			}
		}
		return null;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: List compile_expr(List expr) throws ParseException
	internal override List compile_expr(List expr)
	{
		if (expr == null || expr.Count == 0)
		{
			return null;
		}
		if (expr.Count == 1)
		{
			object x = expr[0];
			if (x is Algebraic)
			{
				List s = new List();
				s.Add(x);
				return s;
			}
			if (x is string)
			{
				object y = compile_keyword(x);
				if (y != null)
				{
					List s = new List();
					s.Add(y);
					return s;
				}
				if (stringq(x) || symbolq(x))
				{
					List s = new List();
					s.Add(x);
					return s;
				}
				return null;
			}
			if (x is List)
			{
				List xs = compile_vektor((List)x);
				if (xs != null)
				{
					return xs;
				}
				return compile_expr((List)x);
			}
		}
		if (expr.Count == 2)
		{
			object op = expr[0];
			object ref_in = expr[1];
			if ("block".Equals(op))
			{
				if (ref_in is List)
				{
					List list = compile_statement((List)ref_in);
					if (list != null)
					{
						List s = new List();
						s.Add(list);
						s.Add(ONE);
						s.Add("BLOCK");
						return s;
					}
				}
				return null;
			}
			if (symbolq(op) && (ref_in is List))
			{
				List list1 = compile_list((List)ref_in);
				if (list1 != null)
				{
					list1.Add(op);
					return list1;
				}
			}
		}
		List res = translate_op(expr);
		if (res != null)
		{
			return res;
		}
		List left_in = expr.take(0,expr.Count - 1);
		List left = compile_expr(left_in);
		if (left == null)
		{
			return null;
		}
		object obj = expr[expr.Count - 1];
		if (!(obj is List))
		{
			return null;
		}
		List list2 = compile_index((List)obj);
		if (list2 != null)
		{
			list2.AddRange(left);
			list2.Add(TWO);
			list2.Add(REF);
			return list2;
		}
		return null;
	}
}
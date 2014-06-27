using System.Collections;

public class Compiler
{
	internal Parser p;
	internal static string[] expr_vars = new string[] {"u","v","w","z"};
	internal static string[] stmnt_vars = new string[] {"X","Y"};
	internal static string[] lval_vars = new string[] {"y"};
	internal static string[] func_vars = new string[] {"f"};
	internal static string[] list_vars = new string[] {"x"};
	internal List rule_in, rule_out;
	internal Hashtable vars;
	internal virtual bool variableq(object x)
	{
		return Parser.oneof(x, expr_vars) || Parser.oneof(x, stmnt_vars) || Parser.oneof(x, lval_vars) || Parser.oneof(x, func_vars) || Parser.oneof(x, list_vars);
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Object match(Object v, List expr) throws ParseException
	internal virtual object match(object v, List expr)
	{
		object r = null;
		if (p.oneof(v, expr_vars))
		{
			r = p.compile_expr(expr);
		}
		else if (p.oneof(v, stmnt_vars))
		{
			r = p.compile_statement(expr);
		}
		else if (p.oneof(v, lval_vars))
		{
			r = p.compile_lval(expr);
		}
		else if (p.oneof(v, func_vars))
		{
			r = p.compile_func(expr);
		}
		else if (Parser.oneof(v, list_vars))
		{
			r = p.compile_list(expr);
		}
		return r;
	}
	internal virtual List change()
	{
		List r = Comp.vec2list(new ArrayList());
		for (int i = 0; i < rule_out.Count; i++)
		{
			object x = rule_out[i];
			if (variableq(x))
			{
				r.Add(vars[x]);
			}
			else if (x is Zahl)
			{
				int xi = ((Zahl)x).intval();
				r.Add(new int?(xi));
			}
			else
			{
				r.Add(x);
			}
		}
		return r;
	}
	internal virtual string ToString(Hashtable h)
	{
		string s = "";
		System.Collections.IEnumerator k = vars.Keys.GetEnumerator();
		while (k.hasMoreElements())
		{
			object key = k.nextElement();
			object val = h[key];
			s = s + "key:" + key + "   val:" + val + "\n";
		}
		return s;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: List compile(List expr) throws ParseException
	internal virtual List compile(List expr)
	{
		if (expr.Count != rule_in.Count)
		{
			return null;
		}
		if (matcher(rule_in, expr))
		{
			return change();
		}
		else
		{
			return null;
		}
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: boolean matcher(List rule, List expr) throws ParseException
	internal virtual bool matcher(List rule, List expr)
	{
		if (rule.Count == 0)
		{
			return expr.Count == 0;
		}
		if (rule.Count > expr.Count)
		{
			return false;
		}
		object x = rule[0];
		if (variableq(x))
		{
			int start = expr.Count + 1 - rule.Count;
			for (int i = start; i >= 1; i--)
			{
				object xv = match(x, expr.subList(0, i));
				if (xv != null && matcher(rule.subList(1,rule.Count), expr.subList(i,expr.Count)))
				{
					vars[x] = xv;
					return true;
				}
			}
			return false;
		}
		object y = expr[0];
		if (x is List)
		{
			return (y is List) && matcher((List)x, (List)y) && matcher(rule.subList(1,rule.Count), expr.subList(1,expr.Count));
		}
		if (x.Equals(y))
		{
			return matcher(rule.subList(1,rule.Count), expr.subList(1,expr.Count));
		}
		return false;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Compiler(List rule_in, List rule_out, Parser p) throws ParseException
	public Compiler(List rule_in, List rule_out, Parser p)
	{
		vars = new Hashtable();
		this.rule_in = rule_in;
		this.rule_out = rule_out;
		this.p = p;
	}
}
internal class Rule
{
	internal List rule_in;
	internal List rule_out;
}
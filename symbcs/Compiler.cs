using System.Collections;

public class Compiler
{
	internal Parser p;

    internal static string[] expr_vars = { "u", "v", "w", "z" };
    internal static string[] stmnt_vars = { "X", "Y" };
    internal static string[] lval_vars = { "y" };
    internal static string[] func_vars = { "f" };
    internal static string[] list_vars = { "x" };

	internal List rule_in, rule_out;
	internal Hashtable vars;

    internal virtual bool variableq( object x )
    {
        return p.oneof( x, expr_vars ) || p.oneof( x, stmnt_vars ) || p.oneof( x, lval_vars ) || p.oneof( x, func_vars ) || p.oneof( x, list_vars );
    }

    internal virtual object match( object v, List expr )
    {
        object r = null;

        if ( p.oneof( v, expr_vars ) )
        {
            r = p.compile_expr( expr );
        }
        else if ( p.oneof( v, stmnt_vars ) )
        {
            r = p.compile_statement( expr );
        }
        else if ( p.oneof( v, lval_vars ) )
        {
            r = p.compile_lval( expr );
        }
        else if ( p.oneof( v, func_vars ) )
        {
            r = p.compile_func( expr );
        }
        else if ( p.oneof( v, list_vars ) )
        {
            r = p.compile_list( expr );
        }
        return r;
    }

    internal virtual List change()
    {
        var r = Comp.vec2list( new ArrayList() );

        for ( int i = 0; i < rule_out.Count; i++ )
        {
            object x = rule_out[i];

            if ( variableq(x) )
            {
                r.Add( vars[x] );
            }
            else if ( x is Zahl )
            {
                int xi = ( ( Zahl ) x ).intval();

                r.Add( new int?( xi ) );
            }
            else
            {
                r.Add(x);
            }
        }

        return r;
    }

    internal virtual string ToString( Hashtable h )
    {
        string s = "";
        var k = vars.Keys.GetEnumerator();

        while ( k.MoveNext() )
        {
            object key = k.Current;
            object val = h[ key ];

            s = s + "key:" + key + "   val:" + val + "\n";
        }

        return s;
    }

    internal virtual List compile( List expr )
    {
        if ( expr.Count != rule_in.Count )
        {
            return null;
        }
        if ( matcher( rule_in, expr ) )
        {
            return change();
        }
        else
        {
            return null;
        }
    }

    internal virtual bool matcher( List rule, List expr )
    {
        if ( rule.Count == 0 )
        {
            return expr.Count == 0;
        }

        if ( rule.Count > expr.Count )
        {
            return false;
        }

        object x = rule[0];

        if ( variableq( x ) )
        {
            int start = expr.Count + 1 - rule.Count;

            for ( int i = start; i >= 1; i-- )
            {
                object xv = match( x, expr.subList( 0, i ) );

                if ( xv != null && matcher( rule.subList( 1, rule.Count ), expr.subList( i, expr.Count ) ) )
                {
                    vars[x] = xv;

                    return true;
                }
            }

            return false;
        }

        object y = expr[0];

        if ( x is List )
        {
            return ( y is List ) && matcher( ( List ) x, ( List ) y ) && matcher( rule.subList( 1, rule.Count ), expr.subList( 1, expr.Count ) );
        }

        if ( x.Equals(y) )
        {
            return matcher( rule.subList( 1, rule.Count ), expr.subList( 1, expr.Count ) );
        }

        return false;
    }

    public Compiler( List rule_in, List rule_out, Parser p )
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

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

    public Compiler( List rule_in, List rule_out, Parser p )
    {
        vars = new Hashtable();

        this.rule_in = rule_in;
        this.rule_out = rule_out;
        this.p = p;
    }

    internal virtual bool variableq( object x )
    {
        return p.oneof( x, expr_vars ) || p.oneof( x, stmnt_vars ) || p.oneof( x, lval_vars ) || p.oneof( x, func_vars ) || p.oneof( x, list_vars );
    }

    internal virtual object match( object v, List expr )
    {
        object result = null;

        if ( p.oneof( v, expr_vars ) )
        {
            result = p.compile_expr( expr );
        }
        else if ( p.oneof( v, stmnt_vars ) )
        {
            result = p.compile_statement( expr );
        }
        else if ( p.oneof( v, lval_vars ) )
        {
            result = p.compile_lval( expr );
        }
        else if ( p.oneof( v, func_vars ) )
        {
            result = p.compile_func( expr );
        }
        else if ( p.oneof( v, list_vars ) )
        {
            result = p.compile_list( expr );
        }

        return result;
    }

    internal virtual List change()
    {
        var r = new List();

        foreach ( var x in rule_out )
        {
            if ( variableq(x) )
            {
                r.Add( vars[x] );
            }
            else if ( x is Zahl )
            {
                int xi = ( ( Zahl ) x ).intval();

                r.Add( xi );
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
        var s = "";
        var k = vars.Keys.GetEnumerator();

        while ( k.MoveNext() )
        {
            var key = k.Current;
            var val = h[ key ];

            s = string.Format( "{0}key:{1}   val:{2}\n", s, key, val );
        }

        return s;
    }

    internal virtual List compile( List expr )
    {
        if ( expr.Count != rule_in.Count )
        {
            return null;
        }

        return matcher( rule_in, expr ) ? change() : null;
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

        var x = rule[0];

        if ( variableq(x) )
        {
            int start = expr.Count + 1 - rule.Count;

            for ( int i = start; i >= 1; i-- )
            {
                var xv = match( x, expr.take( 0, i ) );

                if ( xv == null || !matcher( rule.take( 1, rule.Count ), expr.take( i, expr.Count ) ) ) continue;

                vars[x] = xv;

                return true;
            }

            return false;
        }

        var y = expr[0];

        if ( x is List )
        {
            return ( y is List ) && matcher( ( List ) x, ( List ) y ) && matcher( rule.take( 1, rule.Count ), expr.take( 1, expr.Count ) );
        }

        if ( x.Equals(y) )
        {
            return matcher( rule.take( 1, rule.Count ), expr.take( 1, expr.Count ) );
        }

        return false;
    }
}

internal class Rule
{
	internal List rule_in;
	internal List rule_out;
}

using System.IO;
using System.Linq;
using System.Text;

using Tiny.Science.Symbolic;

namespace Tiny.Science.Engine
{
    public class MaximaParser : Parser
    {
        private string[] keywords = { FOR, WHILE, IF, THEN, ELSE, BREAK, RETURN, CONTINUE, EXIT, STEP, THRU, DO };
        private string separator = "()[]\n\t\r +-*/^!,;:=.<>'\\&|";
        private string sepright = ")]+-*/^!,;:=.<>'\\";

        internal int IN_PARENT = 1;
        internal int IN_BRACK = 2;
        internal int IN_BLOCK = 4;
        internal int prompt_Renamed = 1;

        internal Lambda CRV = new CreateVector();
        internal Lambda REF = new REFX();
        internal Rule[] rules;

        internal static string FOR = "for", WHILE = "while", IF = "if", THEN = "then", ELSE = "else", BREAK = "break", RETURN = "return", CONTINUE = "continue", EXIT = "exit", STEP = "step", THRU = "thru", DO = "do";
        internal string[] commands = { "format", "hold", "clear", "addpath" };

        internal string[][] rules_in =
        {
            new[] { "for u step v thru w do ( X )", "X w v u 4 XFOR" },
            new[] { "while u do ( X )", "X u 2 WHILE" },
            new[] { "if u then ( X ) else ( Y )", "Y X u 3 BRANCH" },
            new[] { "if u then ( X )", "X u 2 BRANCH" }
        };

        public MaximaParser( Store env ) : base( env )
        {
            env.AddPath( "." );

            Store.Globals.Add( "pi", Symbol.PI );
            Store.Globals.Add( "i", Symbol.IONE );
            Store.Globals.Add( "j", Symbol.IONE );
            Store.Globals.Add( "eps", new Number( 2.220446049250313E-16 ) );
            Store.Globals.Add( "ratepsilon", new Number( 2.0e-8 ) );
            Store.Globals.Add( "algepsilon", new Number( 1.0e-8 ) );
            Store.Globals.Add( "rombergit", new Number( 11 ) );
            Store.Globals.Add( "rombergtol", new Number( 1.0e-4 ) );

            State = new ParserState( null, 0 );

            Operator.OPS = new Operator[]
            {
                new Operator( "PPR", "++", 1, Flags.RightLeft, Flags.Unary | Flags.LValue),
                new Operator( "MMR", "--", 1, Flags.RightLeft, Flags.Unary | Flags.LValue),
                new Operator( "PPL", "++", 1, Flags.LeftRight, Flags.Unary | Flags.LValue),
                new Operator( "MML", "--", 1, Flags.LeftRight, Flags.Unary | Flags.LValue),
                new Operator( "MPW", "^^", 1, Flags.LeftRight, Flags.Binary),
                new Operator( "POW", "**", 1, Flags.LeftRight, Flags.Binary),
                new Operator( "FCN", ":=",10, Flags.RightLeft, Flags.Binary | Flags.LValue | Flags.List),
                new Operator( "POW", "^", 1, Flags.LeftRight, Flags.Binary),
                new Operator( "EQU", "==", 6, Flags.LeftRight, Flags.Binary),
                new Operator( "NEQ", "!=", 6, Flags.LeftRight, Flags.Binary),
                new Operator( "GEQ", ">=", 6, Flags.LeftRight, Flags.Binary),
                new Operator( "LEQ", "<=", 6, Flags.LeftRight, Flags.Binary),
                new Operator( "DIV", "/", 3, Flags.LeftRight, Flags.Binary),
                new Operator( "SUB", "=", 5, Flags.LeftRight, Flags.Binary),
                new Operator( "GRE", ">", 6, Flags.LeftRight, Flags.Binary),
                new Operator( "LES", "<", 6, Flags.LeftRight, Flags.Binary),
                new Operator( "OR", "|", 9, Flags.LeftRight, Flags.Binary),
                new Operator( "NOT", "~", 8, Flags.LeftRight, Flags.Unary),
                new Operator( "AND", "&", 7, Flags.LeftRight, Flags.Binary),
                new Operator( "ASS", ":", 10, Flags.RightLeft, Flags.Binary | Flags.LValue),
                new Operator( "ADD", "+", 4, Flags.LeftRight, Flags.Unary | Flags.Binary),
                new Operator( "SUB", "-", 4, Flags.LeftRight, Flags.Unary | Flags.Binary),
                new Operator( "MMU", ".", 3, Flags.LeftRight, Flags.Binary),
                new Operator( "MUL", "*", 3, Flags.LeftRight, Flags.Binary),
                new Operator( "MDR", "/", 3, Flags.LeftRight, Flags.Binary),
                new Operator( "MDL", "\\", 3, Flags.LeftRight, Flags.Binary),
                new Operator( "ADJ", "'", 1, Flags.RightLeft, Flags.Unary),
                new Operator( "FCT", "!", 1, Flags.RightLeft, Flags.Unary)
            };

            nonsymbols.AddRange( Operator.OPS.Select( t => t.symbol ).ToList() );

            nonsymbols.AddRange( listsep );
            nonsymbols.AddRange( commands );
            nonsymbols.AddRange( keywords );

            try
            {
                rules = compile_rules( rules_in );
            }
            catch ( ParseException pe )
            {
                Session.Proc.println( "Failed to compile rules." );
            }

            Session.Parser = this;
        }


        public override string prompt()
        {
            return "(c" + prompt_Renamed++ + ") ";
        }

        public override List compile( Stream istream )
        {
            string s, sp = null;

            reset();

            while ( ( s = ReadLine( istream ) ) != null )
            {
                sp = s;

                translate(s);

                if ( ready() )
                {
                    break;
                }

                Session.Proc.print( "> " );
            }

            if ( sp == null )
            {
                return null;
            }

            if ( s == null && State.InList == IN_BLOCK )
            {
                var v = State.Tokens;

                State = ( ParserState ) State.Sub;

                State.Tokens.Add(v);
            }

            return get();
        }

        public override List compile( string s )
        {
            reset();
            translate(s);

            return get();
        }

        public override List get()
        {
            var r = State.Tokens;

            var pgm = compile_statement(r);

            if ( pgm != null )
            {
                return pgm;
            }

            throw new ParseException( "Compilation failed." );
        }

        public override void translate( string s )
        {
            if ( s == null )
            {
                return;
            }

            var sb = new StringBuilder(s);

            object t;

            while ( ( t = nextToken( sb ) ) != null )
            {
                State.Tokens.Add(t);
                State.Prev = t;
            }
        }


        public virtual object nextToken( StringBuilder s )
        {
            skipWhitespace(s);

            if ( s.Length < 1 )
            {
                return null;
            }

            char c0 = s[0];

            switch ( c0 )
            {
                case '"':
                    return ' ' + cutstring( s, '"', '"' );

                case '(':
                    State = new ParserState( State, IN_PARENT );

                    return nextToken( s.Remove( 0, 1 ) );

                case ')':

                    if ( State.InList == IN_BRACK )
                    {
                        throw new ParseException( "Wrong parenthesis." );
                    }

                    while ( State.InList == IN_BLOCK )
                    {
                        var v = State.Tokens;

                        State = ( ParserState ) State.Sub;

                        State.Tokens.Add(v);
                    }

                    if ( State.InList != IN_PARENT )
                    {
                        throw new ParseException( "Wrong parenthesis." );
                    }

                    var t = State.Tokens;

                    State = ( ParserState ) State.Sub;
                    s.Remove( 0, 1 );

                    return t;

                case '[':
                    State = new ParserState( State, IN_BRACK );

                    return nextToken( s.Remove( 0, 1 ) );

                case ']':
                    if ( State.InList != IN_BRACK )
                    {
                        throw new ParseException( "Wrong brackets." );
                    }

                    t = State.Tokens;

                    while ( t.Count > 0 && ";".Equals( t[ t.Count - 1 ] ) )
                    {
                        // TODO: Check this
                        t.RemoveAt( t.Count - 1 );
                    }

                    t.Insert( 0, "[" );
                    State = ( ParserState ) State.Sub;
                    s.Remove( 0, 1 );

                    return t;

                case '%':
                case '#':
                    s.Remove( 0, s.Length );
                    return null;

                case '\'':
                    if ( State.Prev == null || stringopq( State.Prev ) )
                    {
                        return ' ' + cutstring( s, '\'', '\'' );
                    }
                    else
                    {
                        return readString(s);
                    }

                case ',':
                    s.Remove( 0, 1 );
                    return "" + c0;

                case ';':
                    closeBlocks();
                    s.Remove( 0, 1 );
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
                    if ( s.Length > 1 && number( s[1] ) )
                    {
                        return readNumber(s);
                    }
                    else
                    {
                        return readString(s);
                    }
                default:
                    return readString(s);
            }
        }

        internal virtual void closeBlocks()
        {
            while ( State.InList != 0 )
            {
                if ( State.InList == IN_BRACK || State.InList == IN_PARENT )
                {
                    throw new ParseException( "Unclosed brackets or parenthesis." );
                }

                if ( State.InList == IN_BLOCK )
                {
                    var v = State.Tokens;

                    State = ( ParserState ) State.Sub;
                    State.Tokens.Add(v);
                }
            }
        }

        public override bool ready()
        {
            return State.Tokens.Count != 0 && ";".Equals( State.Tokens[ State.Tokens.Count - 1 ] );
        }
        

        internal virtual object readString( StringBuilder s )
        {
            var len = s.Length > 1 ? 2 : s.Length;

            var st = s.ToString().Substring( 0, len );

            var op = Operator.get( st );

            if ( op != null )
            {
                s.Remove( 0, op.symbol.Length );
                return op.symbol;
            }

            int k = 1;

            while ( k < s.Length && !oneof( s[k], separator ) )
            {
                k++;
            }

            var t = s.ToString().Substring( 0, k );

            s.Remove( 0, k );

            if ( t.Equals( IF ) || t.Equals( FOR ) || t.Equals( WHILE ) )
            {
                if ( State.InList == IN_BRACK )
                {
                    throw new ParseException( "Block starts within vector." );
                }

                State.Tokens.Add( t );
                State = new ParserState( State, IN_BLOCK );

                return nextToken(s);
            }

            if ( t.Equals( STEP ) | t.Equals( THRU ) )
            {
                if ( State.InList != IN_BLOCK )
                {
                    throw new ParseException( "Orphaned " + t );
                }

                var v = State.Tokens;

                ( ( ParserState ) State.Sub ).Tokens.Add(v);

                State = new ParserState( State.Sub, IN_BLOCK );

                return ELSE;
            }

            if ( t.Equals( THEN ) || t.Equals( DO ) )
            {
                if ( State.InList != IN_BLOCK )
                {
                    throw new ParseException( "Orphaned " + t );
                }

                var v = State.Tokens;

                ( ( ParserState ) State.Sub ).Tokens.Add(v);

                State = ( ParserState ) State.Sub;

                return t;
            }

            return t;
        }

        internal virtual bool expressionq( object expr )
        {
            return expr != null && !operatorq( expr );
        }

        internal virtual bool operatorq( object expr )
        {
            return Operator.get( expr ) != null;
        }

        internal virtual List compile_unary( Operator op, List expr )
        {
            var arg_in = ( op.left_right() ? expr.Take( 1, expr.Count ) : expr.Take( 0, expr.Count - 1 ) );

            var arg = ( op.lvalue() ? compile_lval( arg_in ) : compile_expr( arg_in ) );

            if ( arg == null )
            {
                return null;
            }

            arg.Add( ONE );
            arg.Add( op.Lambda );

            return arg;
        }

        internal virtual List compile_ternary( Operator op, List expr, int k )
        {
            int n = expr.Count;

            for ( int k0 = k + 2; k0 < n - 1; k0++ )
            {
                if ( op.symbol.Equals( expr[ k0 ] ) )
                {
                    var left_in = expr.Take( 0, k );
                    var left = compile_expr( left_in );

                    if ( left == null )
                    {
                        continue;
                    }

                    var mid_in = expr.Take( k + 1, k0 );
                    var mid = compile_expr( mid_in );

                    if ( mid == null )
                    {
                        continue;
                    }

                    var right_in = expr.Take( k0 + 1, expr.Count );
                    var right = compile_expr( right_in );

                    if ( right == null )
                    {
                        continue;
                    }

                    left.AddRange( mid );
                    left.AddRange( right );
                    left.Add( THREE );
                    left.Add( op.Lambda );

                    return left;
                }
            }

            return null;
        }

        internal virtual List compile_binary( Operator op, List expr, int k )
        {
            var left_in = expr.Take( 0, k );
            var left = ( op.lvalue() ? compile_lval( left_in ) : compile_expr( left_in ) );

            if ( left == null )
            {
                return null;
            }

            var right_in = expr.Take( k + 1, expr.Count );
            var right = compile_expr( right_in );

            if ( right == null )
            {
                return null;
            }

            var nargs = TWO;

            if ( op.lvalue() )
            {
                nargs = ONE;
            }

            if ( op.list() )
            {
                left.Add( right );
            }
            else
            {
                left.AddRange( right );
            }

            left.Add( nargs );
            left.Add( op.Lambda );

            return left;
        }

        internal virtual List translate_op( List expr )
        {
            List s;

            int n = expr.Count;

            for ( int pred = 10; pred >= 0; pred-- )
            {
                for ( int i = 0; i < n; i++ )
                {
                    int k = i;

                    if ( pred != 5 )
                    {
                        k = n - i - 1;
                    }

                    var op = Operator.get( expr[ k ], k == 0 ? Flags.Start : ( k == n - 1 ? Flags.End : Flags.Mid ) );

                    if ( op == null || op.precedence != pred )
                    {
                        continue;
                    }

                    if ( op.unary() && ( ( k == 0 && op.left_right() ) || ( k == n - 1 && !op.left_right() ) ) )
                    {
                        s = compile_unary( op, expr );

                        if ( s != null )
                        {
                            return s;
                        }

                        continue;
                    }

                    if ( k > 0 && k < n - 3 && op.ternary() )
                    {
                        s = compile_ternary( op, expr, k );

                        if ( s != null )
                        {
                            return s;
                        }
                    }

                    if ( k > 0 && k < n - 1 && op.binary() )
                    {
                        s = compile_binary( op, expr, k );
                        if ( s != null )
                        {
                            return s;
                        }
                    }
                }
            }

            return null;
        }

        internal virtual List compile_vektor( List expr )
        {
            if ( expr == null || expr.Count == 0 || !"[".Equals( expr[ 0 ] ) )
            {
                return null;
            }

            expr = expr.Take( 1, expr.Count );

            var res = new List();

            var nrow = 1;

            var x = expr;
            var xs = compile_list(x);

            if ( xs == null )
            {
                return null;
            }

            xs.AddRange( res );

            res = xs;
            res.Add( nrow );
            res.Add( CRV );

            return res;
        }

        public override List compile_list( List expr )
        {
            if ( expr == null )
            {
                return null;
            }

            var res = new List();

            if ( expr.Count == 0 )
            {
                res.Add(0);
                return res;
            }

            int i, ip = 0, n = 1;

            while ( ( i = nextIndexOf( ",", ip, expr ) ) != -1 )
            {
                var x = expr.Take( ip, i );
                var xs = compile_expr(x);

                if ( xs == null )
                {
                    return null;
                }

                xs.AddRange( res );
                res = xs;
                n++;
                ip = i + 1;
            }

            var x1 = expr.Take( ip, expr.Count );
            var xs1 = compile_expr( x1 );

            if ( xs1 == null )
            {
                return null;
            }

            xs1.AddRange( res );

            res = xs1;
            res.Add(n);

            return res;
        }

        public override List compile_lval( List expr )
        {
            if ( expr == null || expr.Count == 0 )
            {
                return null;
            }

            var res = compile_lval1( expr );

            if ( res != null )
            {
                return res;
            }

            if ( expr.Count == 1 )
            {
                if ( expr[0] is List )
                {
                    return compile_lval( ( List ) expr[0] );
                }

                return null;
            }

            if ( !expr[0].Equals( "[" ) )
            {
                return null;
            }

            expr = expr.Take( 1, expr.Count );
            res = new List();
            int i, n = 1;

            while ( ( i = expr.IndexOf( "," ) ) != -1 )
            {
                var x = expr.Take( 0, i );
                var xs = compile_lval1(x);

                if ( xs == null )
                {
                    return null;
                }

                xs.AddRange(res);
                res = xs;
                expr = expr.Take( i + 1, expr.Count );
                n++;
            }

            var xs1 = compile_lval1( expr );

            if ( xs1 == null )
            {
                return null;
            }

            xs1.AddRange( res );
            res = xs1;
            res.Insert( 0, n );

            return res;
        }

        internal virtual List compile_lval1( List expr )
        {
            if ( expr == null )
            {
                return null;
            }

            switch ( expr.Count )
            {
                case 1:
                    var arg0 = expr[0];

                    if ( arg0 is List )
                    {
                        return compile_lval1( ( List ) arg0 );
                    }

                    if ( symbolq( arg0 ) )
                    {
                        var s = new List { "$" + arg0 };

                        return s;
                    }

                    return null;

                case 2:
                    if ( !symbolq( expr[0] ) || !( expr[1] is List ) )
                    {
                        return null;
                    }

                    var res = compile_index( ( List ) expr[1] );

                    if ( res == null )
                    {
                        res = compile_list( ( List ) expr[1] );
                    }

                    if ( res == null )
                    {
                        return null;
                    }

                    res.Add( "$" + expr[0] );

                    return res;

                default:
                    return null;
            }
        }

        internal virtual List compile_index( List expr )
        {
            if ( expr.Count < 1 || !expr[0].Equals( "[" ) )
            {
                return null;
            }

            return compile_list( expr.Take( 1, expr.Count ) );
        }

        public override bool commandq( object x )
        {
            return oneof( x, commands );
        }

        public override List compile_statement( List expr_in )
        {
            if ( expr_in == null )
            {
                return null;
            }

            if ( expr_in.Count == 0 )
            {
                return new List();
            }

            var expr = expr_in.ToList();

            var first = expr[0];

            foreach ( var rule in rules )
            {
                if ( rule.Input[0].Equals( first ) && expr.Count >= rule.Input.Count )
                {
                    var c = new Compiler( rule.Input, rule.Comp, this );

                    var expr_sub = expr.Take( 0, rule.Input.Count );

                    var s = c.compile( expr_sub );

                    if ( s != null )
                    {
                        expr.Remove( 0, expr_sub.Count );

                        if ( expr.Count == 0 )
                        {
                            return s;
                        }

                        var t = compile_statement( expr );

                        if ( t == null )
                        {
                            return null;
                        }
                        s.AddRange(t);

                        return s;
                    }
                }
            }

            if ( commandq( first ) )
            {
                return compile_command( expr );
            }

            string lend = null;

            int ic = expr.IndexOf( "," );
            int isc = expr.IndexOf( ";" );

            if ( ic >= 0 && ( ic < isc || isc == -1 ) )
            {
                lend = "#,";
            }
            else if ( isc >= 0 && ( isc < ic || ic == -1 ) )
            {
                lend = "#;";
                ic = isc;
            }

            if ( ic == 0 )
            {
                expr.Remove( 0, 1 );

                return compile_statement( expr );
            }

            if ( lend != null )
            {
                var expr_sub = expr.Take( 0, ic );
                var s = compile_expr( expr_sub );

                if ( s != null )
                {
                    s.Add( lend );
                    expr.Remove( 0, ic + 1 );

                    if ( expr.Count == 0 )
                    {
                        return s;
                    }

                    var t = compile_statement( expr );

                    if ( t == null )
                    {
                        return null;
                    }

                    s.AddRange(t);

                    return s;
                }
            }
            else
            {
                return compile_expr( expr );
            }

            return null;
        }

        internal virtual string compile_keyword( object x )
        {
            if ( x.Equals( BREAK ) )
            {
                return "#brk";
            }

            if ( x.Equals( CONTINUE ) )
            {
                return "#cont";
            }

            if ( x.Equals( EXIT ) )
            {
                return "#exit";
            }

            if ( x.Equals( RETURN ) )
            {
                return "#ret";
            }

            return null;
        }

        public override List compile_func( List expr )
        {
            if ( expr.Count == 2 )
            {
                var op = expr[0];
                var ref_in = expr[1];

                if ( symbolq( op ) && ref_in is List )
                {
                    var res = compile_list( ( List ) ref_in );

                    if ( res != null )
                    {
                        res.Add( op );

                        return res;
                    }
                }
            }

            return null;
        }

        public override List compile_expr( List expr )
        {
            if ( expr == null || expr.Count == 0 )
            {
                return null;
            }

            if ( expr.Count == 1 )
            {
                var x = expr[0];

                if ( x is Algebraic )
                {
                    var s = new List {x};

                    return s;
                }

                if ( x is string )
                {
                    object y = compile_keyword(x);

                    if ( y != null )
                    {
                        var s = new List {y};

                        return s;
                    }

                    if ( stringq(x) || symbolq(x) )
                    {
                        var s = new List {x};

                        return s;
                    }

                    return null;
                }

                if ( x is List )
                {
                    var xs = compile_vektor( ( List ) x );

                    if ( xs != null )
                    {
                        return xs;
                    }

                    return compile_expr( ( List ) x );
                }
            }

            if ( expr.Count == 2 )
            {
                var op = expr[0];
                var ref_in = expr[1];

                if ( op.Equals( "block" ) )
                {
                    if ( ref_in is List )
                    {
                        var list = compile_statement( ( List ) ref_in );

                        if ( list != null )
                        {
                            var s = new List { list, ONE, "BLOCK" };

                            return s;
                        }
                    }

                    return null;
                }

                if ( symbolq( op ) && ( ref_in is List ) )
                {
                    var list1 = compile_list( ( List ) ref_in );

                    if ( list1 != null )
                    {
                        list1.Add( op );

                        return list1;
                    }
                }
            }

            var res = translate_op( expr );

            if ( res != null )
            {
                return res;
            }

            var left_in = expr.Take( 0, expr.Count - 1 );
            var left = compile_expr( left_in );

            if ( left == null )
            {
                return null;
            }

            var obj = expr[ expr.Count - 1 ];

            if ( !( obj is List ) )
            {
                return null;
            }

            var list2 = compile_index( ( List ) obj );

            if ( list2 != null )
            {
                list2.AddRange( left );
                list2.Add( TWO );
                list2.Add( REF );

                return list2;
            }

            return null;
        }
    }
}

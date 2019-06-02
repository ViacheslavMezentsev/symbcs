using System.IO;
using System.Linq;
using System.Text;

using Tiny.Science.Symbolic;

namespace Tiny.Science.Engine
{
    public class MatlabParser : Parser
    {
        internal int IN_PARENT = 1;
        internal int IN_BRACK = 2;
        internal int IN_BLOCK = 4;

        internal Lambda CRV = new CreateVector();
        internal Lambda REF = new REFM();
        internal Rule[] rules;

        internal string[][] rules_in = 
        {
            new[] { "function  y = f  X end", "X f y 3 @FUNC" },
            new[] { "if u X else Y end", "Y X u 3 @BRANCH" },
            new[] { "if u X end", "X u 2 @BRANCH" },
            new[] { "for u X end", "X u 2 @FOR" },
            new[] { "while u X end", "X u 2 @WHILE" }
        };

        internal string[] commands = { "format", "hold", "syms", "clear" };

        public MatlabParser( Store env ) : base( env )
        {
            env.AddPath( "." );
            env.AddPath( "m" );

            Store.Globals.Add( "pi", Symbol.PI );
            Store.Globals.Add( "i", Symbol.IONE );
            Store.Globals.Add( "j", Symbol.IONE );
            Store.Globals.Add( "eps", new Number( 2.220446049250313E-16 ) );
            Store.Globals.Add( "ratepsilon", new Number( 2.0e-8 ) );
            Store.Globals.Add( "algepsilon", new Number( 1.0e-8 ) );
            Store.Globals.Add( "rombergit", new Number( 11 ) );
            Store.Globals.Add( "rombergtol", new Number( 1.0e-4 ) );
            Store.Globals.Add( "sum", "$sum2" );

            State = new ParserState( null, 0 );

            Operator.OPS = new[]
            {
                new Operator( "PPR", "++", 1, Flags.RightLeft, Flags.Unary | Flags.LValue ),
                new Operator( "MMR", "--", 1, Flags.RightLeft, Flags.Unary | Flags.LValue ),
                new Operator( "PPL", "++", 1, Flags.LeftRight, Flags.Unary | Flags.LValue ),
                new Operator( "MML", "--", 1, Flags.LeftRight, Flags.Unary | Flags.LValue ),
                new Operator( "ADE", "+=",10, Flags.RightLeft, Flags.Binary | Flags.LValue ),
                new Operator( "SUE", "-=",10, Flags.RightLeft, Flags.Binary | Flags.LValue ),
                new Operator( "MUE", "*=",10, Flags.RightLeft, Flags.Binary | Flags.LValue ),
                new Operator( "DIE", "/=",10, Flags.RightLeft, Flags.Binary | Flags.LValue ),
                new Operator( "MUL", ".*", 3, Flags.LeftRight, Flags.Binary ),
                new Operator( "DIV", "./", 3, Flags.LeftRight, Flags.Binary ),
                new Operator( "POW", ".^", 1, Flags.LeftRight, Flags.Binary ),
                new Operator( "EQU", "==", 6, Flags.LeftRight, Flags.Binary ),
                new Operator( "NEQ", "~=", 6, Flags.LeftRight, Flags.Binary ),
                new Operator( "GEQ", ">=", 6, Flags.LeftRight, Flags.Binary ),
                new Operator( "LEQ", "<=", 6, Flags.LeftRight, Flags.Binary ),
                new Operator( "GRE", ">", 6, Flags.LeftRight, Flags.Binary ),
                new Operator( "LES", "<", 6, Flags.LeftRight, Flags.Binary ),
                new Operator( "OR", "|", 9, Flags.LeftRight, Flags.Binary ),
                new Operator( "NOT", "!", 8, Flags.LeftRight, Flags.Unary ),
                new Operator( "AND", "&", 7, Flags.LeftRight, Flags.Binary ),
                new Operator( "GRE", ">", 6, Flags.LeftRight, Flags.Binary ),
                new Operator( "GRE", ">", 6, Flags.LeftRight, Flags.Binary ),
                new Operator( "ASS", "=", 10, Flags.RightLeft, Flags.Binary | Flags.LValue ),
                new Operator( "CR1", ":", 5, Flags.LeftRight, Flags.Binary | Flags.Ternary ),
                new Operator( "ADD", "+", 4, Flags.LeftRight, Flags.Unary | Flags.Binary ),
                new Operator( "SUB", "-", 4, Flags.LeftRight, Flags.Unary | Flags.Binary ),
                new Operator( "MMU", "*", 3, Flags.LeftRight, Flags.Binary ),
                new Operator( "MDR", "/", 3, Flags.LeftRight, Flags.Binary ),
                new Operator( "MDL", "\\", 3, Flags.LeftRight, Flags.Binary ),
                new Operator( "MPW", "^", 1, Flags.LeftRight, Flags.Binary ),
                new Operator( "ADJ", "'", 1, Flags.RightLeft, Flags.Unary )
            };

            nonsymbols.AddRange( Operator.OPS.Select( x => x.symbol ).ToArray() );

            nonsymbols.AddRange( listsep );
            nonsymbols.AddRange( commands );
            nonsymbols.AddRange( keywords );

            try
            {
                rules = compile_rules( rules_in );
            }
            catch ( ParseException )
            {
            }

            Session.Parser = this;
        }

        public override string prompt()
        {
            return ">> ";
        }

        public override List compile( Stream instream )
        {
            string s, sp = null;

            reset();

            while ( ( s = ReadLine( instream ) ) != null )
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

        internal static string FUNCTION = "function", FOR = "for", WHILE = "while", IF = "if", ELSE = "else", END = "end", BREAK = "break", RETURN = "return", CONTINUE = "continue", EXIT = "exit";

        private string[] keywords = { FUNCTION, FOR, WHILE, IF, ELSE, END, BREAK, RETURN, CONTINUE, EXIT };

        private string sepright = ")]*/^!,;:=.<>'\\";

        internal virtual bool refq( object expr )
        {
            return expr is string && ( ( string ) expr ).Length > 0 && ( ( string ) expr )[0] == '@';
        }

        public override bool commandq( object x )
        {
            return oneof( x, commands );
        }

        internal virtual bool operatorq( object expr )
        {
            return Operator.get( expr ) != null;
        }

        public virtual object nextToken( StringBuilder s )
        {
            if ( State.InList == IN_BRACK && State.Prev != null && !oneof( State.Prev, listsep ) )
            {
                int k = 0;

                for ( ; k < s.Length && whitespace( s[k] ); k++ )
                {
                }

                if ( k == s.Length )
                {
                    s.Remove( 0, k );

                    return ";";
                }

                if ( k > 0 && !oneof( s[k], sepright ) )
                {
                    s.Remove( 0, k );

                    return ",";
                }
            }

            if ( State.InList == IN_BLOCK && State.Prev != null && !oneof( State.Prev, listsep ) )
            {
                int k = 0;

                for ( ; k < s.Length && whitespace( s[k] ); k++ )
                {
                }

                if ( k == s.Length )
                {
                    s.Remove( 0, k );

                    return ",";
                }
            }

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
                    if ( symbolq( State.Prev ) )
                    {
                        State.Prev = "@" + State.Prev;
                        // TODO: Check this
                        State.Tokens.RemoveAt( State.Tokens.Count - 1 );
                        State.Tokens.Add( State.Prev );
                    }

                    State = new ParserState( State, IN_PARENT );

                    return nextToken( s.Remove( 0, 1 ) );

                case ')':
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
                        return readString( s );
                    }

                case ';':
                case ',':
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

        public override bool ready()
        {
            return State.Sub == null;
        }

        private string separator = "()[]\n\t\r +-*/^!,;:=.<>'\\&|";

        internal virtual object readString( StringBuilder s )
        {
            var len = s.Length > 1 ? 2 : s.Length;
            //char[] substring = new char[len];
            //s.getChars(0,len,substring,0);

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

            //substring = new char[k];
            //s.getChars(0,k,substring,0);
            var t = s.ToString().Substring( 0, k );

            s.Remove( 0, k );

            if ( t.Equals( IF ) || t.Equals( FOR ) || t.Equals( WHILE ) || t.Equals( FUNCTION ) )
            {
                if ( State.InList == IN_PARENT || State.InList == IN_BRACK )
                {
                    throw new ParseException( "Block starts within list." );
                }

                State.Tokens.Add(t);
                State = new ParserState( State, IN_BLOCK );

                return nextToken(s);
            }

            if ( t.Equals( ELSE ) )
            {
                if ( State.InList != IN_BLOCK )
                {
                    throw new ParseException( "Orphaned else." );
                }

                var v = State.Tokens;

                ( ( ParserState ) State.Sub ).Tokens.Add(v);

                State = new ParserState( State.Sub, IN_BLOCK );

                return ELSE;
            }

            if ( t.Equals( END ) )
            {
                if ( State.InList != IN_BLOCK )
                {
                    throw new ParseException( "Orphaned end." );
                }

                var v = State.Tokens;

                State = ( ParserState ) State.Sub;

                return v;
            }

            return t;
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
            var n = expr.Count;

            for ( var k0 = k - 2; k0 > 0; k0-- )
            {
                if ( op.symbol.Equals( expr[ k0 ] ) )
                {
                    var left_in = expr.Take( 0, k0 );
                    var left = compile_expr( left_in );

                    if ( left == null )
                    {
                        continue;
                    }

                    var mid_in = expr.Take( k0 + 1, k );
                    var mid = compile_expr( mid_in );

                    if ( mid == null )
                    {
                        continue;
                    }

                    var right_in = expr.Take( k + 1, expr.Count );
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

            int? nargs = TWO;

            if ( op.lvalue() )
            {
                var left_narg = left[0];

                if ( left_narg is int? )
                {
                    nargs = ( int? ) left_narg;
                    right.Insert( right.Count - 1, "#" + nargs );
                    left.RemoveAt( 0 );
                }
                else
                {
                    nargs = ONE;
                }
            }

            left.AddRange( right );
            left.Add( nargs );
            left.Add( op.Lambda );

            return left;
        }

        internal virtual List translate_op( List expr )
        {
            List s;
            var n = expr.Count;

            for ( var pred = 10; pred >= 0; pred-- )
            {
                for ( var i = 0; i < n; i++ )
                {
                    var k = i;

                    if ( pred != 6 )
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

                    if ( k > 2 && k < n - 1 && op.ternary() )
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
            if ( expr == null || expr.Count == 0 || !expr[0].Equals( "[" ) )
            {
                return null;
            }

            expr = expr.Take( 1, expr.Count );

            var r = new List();

            int i = 0, ip = 0, nrow = 1;

            while ( ( i = nextIndexOf( ";", ip, expr ) ) != -1 )
            {
                var x = expr.Take( ip, i );
                var xs = compile_list(x);

                if ( xs == null )
                {
                    return null;
                }

                xs.AddRange(r);
                r = xs;
                nrow++;
                ip = i + 1;
            }

            var x1 = expr.Take( ip, expr.Count );
            var xs1 = compile_list( x1 );

            if ( xs1 == null )
            {
                return null;
            }

            xs1.AddRange(r);

            r = xs1;
            r.Add( nrow );
            r.Add( CRV );

            return r;
        }

        public override List compile_list( List expr )
        {
            if ( expr == null )
            {
                return null;
            }

            var r = new List();

            if ( expr.Count == 0 )
            {
                r.Add( 0 );
                return r;
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

                xs.AddRange(r);

                r = xs;
                n++;
                ip = i + 1;
            }

            var x1 = expr.Take( ip, expr.Count );
            var xs1 = compile_expr( x1 );

            if ( xs1 == null )
            {
                return null;
            }

            xs1.AddRange(r);

            r = xs1;
            r.Add(n);

            return r;
        }

        public override List compile_lval( List expr )
        {
            if ( expr == null || expr.Count == 0 )
            {
                return null;
            }

            var r = compile_lval1( expr );

            if ( r != null )
            {
                return r;
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
            r = new List();

            int i, n = 1;

            while ( ( i = expr.IndexOf( "," ) ) != -1 )
            {
                var x = expr.Take( 0, i );
                var xs = compile_lval1(x);

                if ( xs == null )
                {
                    return null;
                }

                xs.AddRange(r);
                r = xs;
                expr = expr.Take( i + 1, expr.Count );
                n++;
            }

            var xs1 = compile_lval1( expr );

            if ( xs1 == null )
            {
                return null;
            }

            xs1.AddRange(r);
            r = xs1;
            r.Insert( 0, n );

            return r;
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
                    var x = expr[0];

                    if ( x is List )
                    {
                        return compile_lval1( ( List ) x );
                    }

                    if ( symbolq(x) && !refq(x) )
                    {
                        var s = new List { "$" + x };

                        return s;
                    }

                    return null;

                case 2:
                    x = expr[0];

                    if ( !symbolq( x ) || !refq( x ) || !( expr[1] is List ) )
                    {
                        return null;
                    }

                    var res = compile_index( ( List ) expr[1] );

                    if ( res == null )
                    {
                        return null;
                    }

                    res.Add( "$" + ( ( string ) x ).Substring(1) );

                    return res;

                default:
                    return null;
            }
        }

        internal virtual List compile_index( List expr )
        {
            if ( expr == null || expr.Count == 0 )
            {
                return null;
            }

            if ( expr.Count == 1 && ":".Equals( expr[0] ) )
            {
                var s = new List { ":", ONE };

                return s;
            }

            var r = compile_expr( expr );

            if ( r != null )
            {
                r.Add( ONE );
                return r;
            }

            var c = expr.IndexOf( "," );

            if ( c == -1 )
            {
                return null;
            }

            var left_in = expr.Take( 0, c );
            var right_in = expr.Take( c + 1, expr.Count );

            if ( left_in != null && left_in.Count == 1 && left_in[0].Equals( ":" ) )
            {
                if ( right_in != null && right_in.Count == 1 && right_in[0].Equals( ":" ) )
                {
                    var s = new List { ":", ":", TWO };

                    return s;
                }

                var right = compile_expr( right_in );

                if ( right == null )
                {
                    return null;
                }

                right.Add( ":" );
                right.Add( TWO );

                return right;
            }

            var left = compile_expr( left_in );

            if ( left == null )
            {
                return null;
            }

            if ( right_in != null && right_in.Count == 1 && ":".Equals( right_in[0] ) )
            {
                left.Insert( 0, ":" );
                left.Add( TWO );

                return left;
            }
            else
            {
                var right = compile_expr( right_in );

                if ( right == null )
                {
                    return null;
                }

                left.AddRange( right );
                left.Add( TWO );

                return left;
            }
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

            var ic = expr.IndexOf( "," );
            var isc = expr.IndexOf( ";" );

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
            else if ( x.Equals( CONTINUE ) )
            {
                return "#cont";
            }
            else if ( x.Equals( EXIT ) )
            {
                return "#exit";
            }
            else if ( x.Equals( RETURN ) )
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

                if ( symbolq( op ) && refq( op ) && ref_in is List )
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

                    if ( stringq(x) || ( symbolq( x ) && !refq(x) ) )
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

            var res = compile_func( expr );

            if ( res != null )
            {
                return res;
            }

            res = translate_op( expr );

            if ( res != null )
            {
                return res;
            }

            var ref_in = expr[ expr.Count - 1 ];

            if ( !( ref_in is List ) )
            {
                return null;
            }

            res = compile_index( ( List ) ref_in );

            if ( res == null )
            {
                return null;
            }

            var left_in = expr.Take( 0, expr.Count - 1 );

            if ( left_in.Count == 1 && symbolq( left_in[0] ) && refq( left_in[0] ) )
            {
                res.AddRange( left_in );

                return res;
            }

            var left = compile_expr( left_in );

            if ( left != null )
            {
                res.AddRange( left );
                res.Add( TWO );
                res.Add( REF );

                return res;
            }

            return null;
        }
    }
}

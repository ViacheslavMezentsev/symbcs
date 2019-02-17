using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using Tiny.Science.Symbolic;
using BigInteger = Tiny.Science.Numeric.BigInteger;
using Math = Tiny.Science.Numeric.Math;

namespace Tiny.Science.Engine
{
    public abstract class Parser
    {

        #region Internal fields

        protected internal static readonly int? ONE = 1;
        protected internal static readonly int? TWO = 2;
        protected internal static readonly int? THREE = 3;
        protected internal static string[] listsep = { ",", ";" };

        internal static string[] stringops = { "=", "," };

        public ParserState State;
        public List nonsymbols = new List();

        #endregion

        #region Constructors

        protected Parser( Store env )
        {
        }

        #endregion

        #region Internal methods

        #region Abstract

        public abstract void translate( string s );
        public abstract bool ready();
        public abstract List get();
        public abstract List compile( Stream instream );
        public abstract List compile( string s );
        public abstract string prompt();
        public abstract List compile_expr( List expr );
        public abstract List compile_statement( List expr );
        public abstract List compile_lval( List expr );
        public abstract List compile_list( List expr );
        public abstract List compile_func( List expr );
        public abstract bool commandq( object expr );

        #endregion

        #region Virtual

        internal virtual List compile_command_args( List expr )
        {
            var s = new List();

            for ( int n = expr.Count - 1; n >= 0; n-- )
            {
                var x = expr[ n ];

                if ( x is Algebraic )
                {
                    s.Add( x );
                }
                else if ( symbolq( x ) )
                {
                    s.Add( "$" + x );
                }
                else if ( stringq( x ) )
                {
                    s.Add( "$" + ( ( string ) x ).Substring( 1 ) );
                }
                else if ( x is List )
                {
                    s.AddRange( compile_command_args( ( List ) x ) );
                }
            }

            return s;
        }

        protected internal virtual void reset()
        {
            State = new ParserState( null, 0 );
        }

        protected internal virtual Rule[] compile_rules( string[][] s )
        {
            var rules = new Rule[ s.Length ];

            for ( int i = 0; i < s.Length; i++ )
            {
                var rule = new Rule();

                reset();

                translate( s[ i ][ 0 ] );

                rule.Input = State.Tokens;

                reset();

                translate( s[ i ][ 1 ] );

                rule.Comp = State.Tokens;

                rules[ i ] = rule;
            }

            return rules;
        }

        protected internal virtual List compile_command( List expr )
        {
            if ( expr == null || expr.Count == 0 || !commandq( expr[ 0 ] ) )
            {
                return null;
            }

            var list = compile_command_args( expr.Take( 1, expr.Count ) );

            list.Add( list.Count );

            var command = ( string ) expr[ 0 ];

            var type = Type.GetType( "Tiny.Science.Symbolic.Lambda" + command.ToUpper() );

            if ( type == null ) return null;

            try
            {
                list.Add( ( Lambda ) Activator.CreateInstance( type ) );

                return list;
            }
            catch
            {
                return null;
            }
        }

        protected internal virtual bool symbolq( object expr )
        {
            return expr is string && ( ( string ) expr ).Length > 0 && ( ( string ) expr )[ 0 ] != ' ' && !nonsymbols.Contains( expr );
        }

        protected internal virtual bool stringq( object expr )
        {
            return expr is string && ( ( string ) expr ).Length > 0 && ( ( string ) expr )[ 0 ] == ' ';
        }

        #endregion

        #region Static

        protected static bool oneof( char c, string s )
        {
            return s.IndexOf( c ) != -1;
        }

        protected static bool oneof( object c, string s )
        {
            return c is string && ( ( string ) c ).Length > 0 && oneof( ( ( string ) c )[ 0 ], s );
        }

        protected internal static bool whitespace( char c )
        {
            return oneof( c, " \t\n\r" );
        }

        protected internal static void skipWhitespace( StringBuilder s )
        {
            int i = 0;

            while ( i < s.Length && whitespace( s[ i ] ) )
            {
                i++;
            }

            s.Remove( 0, i );
        }

        protected internal static int nextIndexOf( object x, int idx, List list )
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

        protected internal static Symbol readNumber( StringBuilder s )
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

                    if ( ts.EndsWith( ".", StringComparison.Ordinal ) && s.Length > k && ( s[ k ] == '^' || s[ k ] == '/' ) )
                    {
                        continue;
                    }

                    bool imag = false;

                    if ( s.Length > k && ( s[ k ] == 'i' || s[ k ] == 'j' ) )
                    {
                        imag = true;
                        k++;
                    }

                    s.Remove( 0, k );

                    if ( Math.abs( x ) > 1e15 )
                    {
                        try
                        {
                            var bi = new BigInteger( ts, 10 );

                            return imag ? ( Symbol ) ( new Number( bi ) * Symbol.IONE ) : new Number( bi );
                        }
                        catch ( Exception )
                        {
                        }
                    }

                    return imag ? new Complex( 0, x ) : new Complex( x );
                }
                catch ( Exception )
                {
                }
            }

            throw new ParseException( "Internal Error." );
        }

        protected internal static string cutstring( StringBuilder sb, char a, char b )
        {
            sb.Remove( 0, 1 );

            var s = sb.ToString();

            int cnt = 1, i;

            for ( i = 0; i < s.Length; i++ )
            {
                char c = s[ i ];

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

        protected internal static bool number( char c )
        {
            return oneof( c, "0123456789" );
        }

        protected internal static string ReadLine( Stream stream )
        {
            var sb = new StringBuilder();

            int b;

            while ( ( b = stream.ReadByte() ) != -1 )
            {
                var c = ( char ) b;

                sb.Append( c );

                if ( c == '\n' || c == '\r' )
                {
                    return sb.ToString();
                }
            }

            return sb.Length > 0 ? sb.ToString() : null;
        }

        #endregion

        #region Others

        protected internal bool oneof( object c, object[] d )
        {
            return d.Contains( c );
        }

        protected internal bool stringopq( object x )
        {
            return oneof( x, stringops );
        }

        #endregion

        #endregion

    }

    public class ParserState
    {
        public object Sub;
        public object Prev;
        public List Tokens;
        public int InList;

        public ParserState( object sub, int inList )
        {
            Sub = sub;
            Prev = null;
            Tokens = new List();
            InList = inList;
        }
    }
}

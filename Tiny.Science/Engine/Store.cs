using System;
using System.Collections;

using Tiny.Science.Symbolic;

namespace Tiny.Science.Engine
{
    public class Store : Hashtable
    {
        public static ArrayList Paths = new ArrayList();
        public static Hashtable Globals = new Hashtable();

        public virtual void AddPath( string path )
        {
            if ( !Paths.Contains( path ) )
            {
                Paths.Add( path );
            }
        }

        public virtual Store Clone()
        {
            var store = new Store();

            var keys = Keys.GetEnumerator();

            while ( keys.MoveNext() )
            {
                var key = keys.Current;

                store[ key ] = this[ key ];
            }

            return store;
        }

        public virtual void Update( Store local )
        {
            var kl = local.Keys.GetEnumerator();

            while ( kl.MoveNext() )
            {
                var key = kl.Current;

                if ( this[ key ] != null )
                {
                    this[ key ] = local[ key ];
                }
            }
        }

        public override string ToString()
        {
            var keys = Keys.GetEnumerator();

            var s = "";

            while ( keys.MoveNext() )
            {
                var key = keys.Current;

                s += key + ": ";

                s += GetValue( ( string ) key ) + "\n";
            }

            keys = Globals.Keys.GetEnumerator();

            s += "Globals:\n";

            while ( keys.MoveNext() )
            {
                var key = keys.Current;

                s += key + ": ";
                s += GetValue( ( string ) key ) + "\n";
            }

            return s;
        }

        public virtual void PutValue( string name, object x )
        {
            if ( x.Equals( "null" ) )
            {
                Remove( name );
            }
            else
            {
                if ( x is Lambda )
                {
                    Globals[ name ] = x;
                }
                else
                {
                    this[ name ] = x;
                }
            }
        }

        public virtual object GetValue( string name )
        {
            if ( name.StartsWith( " " ) )
            {
                return name;
            }

            var x = this[ name ];

            if ( x != null )
            {
                return x;
            }

            x = Globals[ name ];

            if ( x != null )
            {
                return x;
            }

            try
            {
                var fname = "Tiny.Science.Symbolic.Lambda" + name.ToUpper();

                var type = Type.GetType( fname );

                var lambda = ( Lambda ) Activator.CreateInstance( type );

                PutValue( name, lambda );

                x = lambda;
            }
            catch { }

            return x;
        }

        public virtual Symbol GetNum( string name )
        {
            var x = this[ name ] ?? Globals[ name ];

            return x is Symbol ? ( Symbol ) x : null;
        }
    }
}

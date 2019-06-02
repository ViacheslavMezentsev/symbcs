using System.Collections;

namespace Tiny.Science.Symbolic
{
    public sealed class List : ArrayList
    {
        public List()
        {
        }

        public List( IList list )
        {
            AddRange( list );
        }

        public List Take( int i, int k )
        {
            var list = new List();

            for ( var j = i; j < k; j++ )
            {
                list.Add( this[j] );
            }

            return list;
        }

        public void Remove( int from, int to )
        {
            RemoveRange( from, to - from );
        }
    }
}

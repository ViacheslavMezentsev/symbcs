using System.Collections;

namespace Tiny.Science.Symbolic
{
    public static class Extensions
    {
        public static List ToList( this IList list )
        {
            return new List( list );
        }
    }
}

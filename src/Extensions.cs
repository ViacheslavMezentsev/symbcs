using System.Collections;

public static class Extensions
{
    internal static List ToList( this ArrayList list )
    {
        return new List( list );
    }
}

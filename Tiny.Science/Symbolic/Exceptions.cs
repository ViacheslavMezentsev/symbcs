using System;

namespace Tiny.Science.Symbolic
{
    public class ParseException : Exception
    {
        public ParseException( string s ) : base(s)
        {
        }
    }

    internal class SymbolicException : Exception
    {
        public SymbolicException( string s ) : base(s)
        {
        }
    }
}

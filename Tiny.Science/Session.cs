using System.IO;

using Tiny.Science.Engine;
using Tiny.Science.Symbolic;

namespace Tiny.Science
{
    public static class Session
    {
        public static Processor Proc;

        public static Parser Parser;

        public static Store Store;

        public static INumFmt Fmt = new NumFmtVar( 10, 5 );

        public static Stream GetFileInputStream( string fname )
        {
            return new FileStream( fname, FileMode.Open );
        }

        public static Stream GetFileOutputStream( string fname, bool append )
        {
            return new FileStream( fname, append ? FileMode.Append : FileMode.Create );
        }
    }
}

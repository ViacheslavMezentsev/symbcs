using System.IO;

namespace Tiny.Symbolic
{
    public static class Globals
    {
        public static Processor Proc;
        public static Parser Parser;
        public static Store Store;

        internal static INumFmt fmt = new NumFmtVar( 10, 5 );

        internal static Stream GetFileInputStream( string fname )
        {
            return new FileStream( fname, FileMode.Open );
        }

        internal static Stream GetFileOutputStream( string fname, bool append )
        {
            return new FileStream( fname, append ? FileMode.Append : FileMode.Create );
        }
    }
}

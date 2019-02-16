using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace TinyCalc
{
    public class NesymcaMax : Nesymca
    {
        public NesymcaMax() : base( "Maxima" )
        {
        }
    }

    public class NesymcaOct : Nesymca
    {
        public NesymcaOct() : base( "Octave" )
        {
        }
    }

    public class NesymcaMat : Nesymca
    {
        public NesymcaMat() : base( "Matlab" )
        {
        }
    }

    public static class Program
    {
        public static void Main()
        {
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;

            Console.Clear();

            var assembly = Assembly.GetExecutingAssembly();

            var attribute = assembly.GetCustomAttributes( false ).OfType<AssemblyTitleAttribute>().FirstOrDefault();

            var title = attribute != null && attribute.Title != "" ? attribute.Title : Path.GetFileNameWithoutExtension( assembly.CodeBase );

            var version = assembly.GetName().Version;

            var lines = new[]
            {
                new { Text = "OS: " + Environment.OSVersion },
                new { Text = ".Net: " + Environment.Version },
                new { Text = $"{title}, version {version}" }
            };

            foreach ( var line in lines ) Console.WriteLine( line.Text );

            Console.WriteLine();

            var nesymca = new NesymcaOct();
            //var nesymca = new NesymcaMax();
            //var nesymca = new NesymcaMat();

            var thread = new Thread( nesymca.Run );

            thread.Start();
        }
    }
}

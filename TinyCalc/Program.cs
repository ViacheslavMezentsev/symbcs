using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

using Tiny.Science.Engine;

namespace TinyCalc
{
    public static class Program
    {
        public static void Main()
        {
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

            var calc = new Calculator( "Octave" );
            //var calc = new Calculator( "Maxima" );
            //var calc = new Calculator( "Matlab" );

            var thread = new Thread( calc.Run );

            thread.Start();
        }
    }
}

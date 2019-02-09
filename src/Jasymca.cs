using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

public class PrintStream
{
    public void print( string text )
    {
        Console.Write(text);
    }

    public void println( string text )
    {
        Console.WriteLine( text );
    }
}

public class Jasymca
{

    #region Internal fields

    internal static string ui = "Octave";

    internal static PrintStream ps = new PrintStream();

    internal static INumFmt fmt = new NumFmtVar( 10, 5 );

    internal static string welcome =
        "Jasymca	- Java Symbolic Calculator\n" +
        "version 2.1\n" +
        "Copyright (C) 2006, 2009 - Helmut Dersch\n" +
        "der@hs-furtwangen.de\n\n";

    #endregion

    #region Public fields

    public static Environment Env;
    public static Processor Proc;
    public static Parser Pars;

    #endregion

    #region Constructors

    public Jasymca() : this( "Octave" )
    {
    }

    public Jasymca( string ui )
    {
        setup_ui( ui, true );

        welcome += "Executing in " + ui + "-Mode.\n";
        welcome += "Welcome and have fun!\n";
    }

    #endregion

    #region Internal methods

    internal static Stream GetFileInputStream( string fname )
    {
        return new FileStream( fname, FileMode.Open );
    }

    internal static Stream GetFileOutputStream( string fname, bool append )
    {
        return new FileStream( fname, append ? FileMode.Append : FileMode.Create );
    }

    #endregion

    #region Public methods
    
    public virtual void interrupt()
    {
        if ( Proc != null )
        {
            Proc.set_interrupt( true );
        }
    }

    public static void Main()
    {
        Console.BackgroundColor = ConsoleColor.White;
        Console.ForegroundColor = ConsoleColor.Black;

        Console.Clear();

        var jasymca = new JasymcaOct();
        //var jasymca = new JasymcaMax();

        var assembly = Assembly.GetExecutingAssembly();

        var version = assembly.GetName().Version;
        var build = version.Build;
        var revision = version.Revision;

        var bdate = new DateTime( 2000, 1, 1 ).AddDays( build ).AddSeconds( 2 * revision );

        Console.WriteLine( "OS: " + System.Environment.OSVersion );
        Console.WriteLine( ".Net: " + System.Environment.Version );

        Console.WriteLine( @"{0}: {1} ({2} {3})", AssemblyTitle, version, bdate.ToLongDateString(), bdate.ToLongTimeString());
        Console.WriteLine();

        Console.Write( welcome );

        var thread = new Thread( Run );

        thread.Start();        
    }

    public static void Test( string[] args )
    {
        double[] ar = { 0.0, 1.0, 1.0, 1.0 };
        double[] ai = { 0.0, 0.0, 1.0, 0.0 };

        bool[] err = { true, true, true, true };

        Pzeros.aberth( ar, ai, err );

        for ( int i = 0; i < ar.Length - 1; i++ )
        {
            Console.WriteLine( @"{0}: {1}+i*{2}  {3}", i, ar[i], ai[i], err[i]);
        }
    }


	public static void setup_ui( string ui, bool clear_env )
	{
		if ( clear_env )
		{
			Env = new Environment();
		}

		if ( ui != null )
		{
		    Jasymca.ui = ui;
		}

        if ( Jasymca.ui.Equals( "Maxima" ) )
		{
            Proc = new XProcessor( Env ) { PrintStream = ps };
			Pars = new MaximaParser( Env );
		}

        else if ( Jasymca.ui.Equals( "Octave" ) )
		{
            Proc = new Processor( Env ) { PrintStream = ps };
			Pars = new OctaveParser( Env );
		}

		else
		{
            Console.WriteLine( @"Mode {0} not available.", Jasymca.ui );
		    
			System.Environment.Exit(0);
		}
	}

    public static void Run()
	{
		while ( true )
		{
			ps.print( Pars.prompt() );

			try
			{
				Proc.set_interrupt( false );

                var istream = new MemoryStream( Encoding.UTF8.GetBytes( Console.ReadLine() ) );

			    istream.Seek( 0, SeekOrigin.Begin );

                var code = Pars.compile( istream, ps );

				if ( code == null )
				{
					ps.println("");

					continue;
				}

                if ( Proc.process_list( code, false ) == Processor.EXIT )
				{
					Console.WriteLine( "\nGoodbye." );

				    Console.ResetColor();

					return;
				}

				Proc.printStack();
			}
			catch ( Exception ex )
			{
			    Console.WriteLine( "\n" + ex.Message );

				Proc.clearStack();
			}
		}	    
	}

    #endregion

    #region Properties

    public static string AssemblyDirectory
    {
        get
        {
            var filePath = new Uri( Assembly.GetExecutingAssembly().CodeBase ).LocalPath;

            return Path.GetDirectoryName( filePath );
        }
    }


    /// <summary>
    /// Gets the assembly title.
    /// </summary>
    /// <value>The assembly title.</value>
    public static string AssemblyTitle
    {
        get
        {
            // Get all Title attributes on this assembly
            var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes( typeof( AssemblyTitleAttribute ), false );

            // If there is at least one Title attribute
            if ( attributes.Length > 0 )
            {
                // Select the first one
                var titleAttribute = ( AssemblyTitleAttribute ) attributes[0];

                // If it is not an empty string, return it
                if ( titleAttribute.Title != "" ) return titleAttribute.Title;
            }

            // If there was no Title attribute, or if the Title attribute was the empty string, return the .exe name
            return Path.GetFileNameWithoutExtension( Assembly.GetExecutingAssembly().CodeBase );
        }
    }

    #endregion

}

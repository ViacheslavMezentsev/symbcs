using System;
using System.IO;
using System.Text;
using System.Threading;

public class PrintStream
{
    public void print(string text)
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
	internal static Stream getFileInputStream( string fname )
	{
        return new FileStream( fname, FileMode.Open );
	}

	internal static Stream getFileOutputStream( string fname, bool append )
	{
        return new FileStream( fname, append ? FileMode.Create | FileMode.Append : FileMode.Create );
	}

	internal static string JasymcaRC = "vfs/Jasymca.";

	public static Environment env;
	public static Processor proc;
	public static Parser pars;

	internal static string ui = "Octave";
    internal static PrintStream ps = new PrintStream();

	internal static INumFmt fmt = new NumFmtVar(10, 5);

    internal static string welcome =
        "Jasymca	- Java Symbolic Calculator\n" +
        "version 2.1\n" +
        "Copyright (C) 2006, 2009 - Helmut Dersch\n" +
        "der@hs-furtwangen.de\n\n";

	public Jasymca() : this( "Octave" ) 
    {
	}

	public Jasymca( string ui )
	{
		setup_ui( ui, true );

		welcome += "Executing in " + ui + "-Mode.\n";
		welcome += "Welcome and have fun!\n";
	}

    public virtual void interrupt()
    {
        if ( proc != null )
        {
            proc.set_interrupt( true );
        }
    }

    public static void Main()
    {
        Console.BackgroundColor = ConsoleColor.White;
        Console.ForegroundColor = ConsoleColor.Black;

        Console.Clear();

        var jasymca = new JasymcaOct();
        //var jasymca = new JasymcaMax();

        //try
        //{
        //    var fname = JasymcaRC + ui + ".rc";
        //
        //    var file = getFileInputStream( fname );
        //
        //    LambdaLOADFILE.readFile( file );
        //}
        //catch { }

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
            Console.WriteLine( i + ": " + ar[i] + "+i*" + ai[i] + "  " + err[i] );
        }
    }


	public static void setup_ui( string ui, bool clear_env )
	{
		if ( clear_env )
		{
			env = new Environment();
		}

		if ( ui != null )
		{
		    Jasymca.ui = ui;
		}

        if ( Jasymca.ui.Equals( "Maxima" ) )
		{
            proc = new XProcessor( env ) { PrintStream = ps };
			pars = new MaximaParser( env );
		}

        else if ( Jasymca.ui.Equals( "Octave" ) )
		{
            proc = new Processor( env ) { PrintStream = ps };
			pars = new OctaveParser( env );
		}

		else
		{
            Console.WriteLine( "Mode " + Jasymca.ui + " not available." );
		    
			System.Environment.Exit(0);
		}
	}


    public static void Run()
	{
		while ( true )
		{
			ps.print( pars.prompt() );

			try
			{
				proc.set_interrupt( false );

                var istream = new MemoryStream( Encoding.UTF8.GetBytes( Console.ReadLine() ) );

			    istream.Seek( 0, SeekOrigin.Begin );

                var code = pars.compile( istream, ps );

				if ( code == null )
				{
					ps.println("");

					continue;
				}

                if ( proc.process_list( code, false ) == Processor.EXIT )
				{
					Console.WriteLine( "\nGoodbye." );

				    Console.ResetColor();

					return;
				}

				proc.printStack();
			}
			catch ( Exception ex )
			{
			    Console.WriteLine( "\n" + ex.Message );

				proc.clearStack();
			}
		}	    
	}
}

using System;
using System.IO;
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

	public static void Main()
	{
        try
        {
            var fname = JasymcaRC + ui + ".rc";

            var file = getFileInputStream( fname );

            LambdaLOADFILE.readFile( file );
        }
        catch { }

        Console.Write( welcome );

        proc.PrintStream = ps;

        var myThread = new Thread( Run );

        myThread.Start();
	}

	public static Environment env;
	public static Processor proc;
	public static Parser pars;

	internal static string ui = "Octave";
    internal PrintStream pstream;
    internal InputStream istream;

	public virtual void interrupt()
	{
		if ( proc != null )
		{
			proc.set_interrupt( true );
		}
	}

	internal static string welcome = "Jasymca	- Java Symbolic Calculator\n" + "version 2.1\n" + "Copyright (C) 2006, 2009 - Helmut Dersch\n" + "der@hs-furtwangen.de\n\n";
	internal static INumFmt fmt = new NumFmtVar(10, 5);

	internal Thread evalLoop;

	public Jasymca() : this( "Octave" ) 
    {
	}

	public Jasymca( string ui )
	{
		setup_ui( ui, true );

		welcome += "Executing in " + ui + "-Mode.\n";
		welcome += "Welcome and have fun!\n";
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
			proc = new XProcessor( env );
			pars = new MaximaParser( env );
		}

        else if ( Jasymca.ui.Equals( "Octave" ) )
		{
			proc = new Processor( env );
			pars = new OctaveParser( env );
		}

		else
		{
            Console.WriteLine( "Mode " + Jasymca.ui + " not available." );
			//Environment.Exit(0);
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

                var code = pars.compile( istream, pstream );

				if ( code == null )
				{
					ps.println("");
					continue;
				}

				if ( proc.process_list( code, false ) == proc.EXIT )
				{
					Console.WriteLine( "\nGoodbye." );

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

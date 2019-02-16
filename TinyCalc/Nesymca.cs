using System;
using System.IO;
using System.Text;

using Tiny.Symbolic;
using Tiny.Maxima;
using Tiny.Matlab;
using Tiny.Octave;

public class Nesymca : IPrintable
{

    #region Private fields

    private Store _store;
    private Processor _proc;
    private Parser _parser;

    private string ui = "Octave";

    private string welcome =
        "Jasymca	- Java Symbolic Calculator\n" +
        "version 2.1\n" +
        "Copyright (C) 2006, 2009 - Helmut Dersch\n" +
        "der@hs-furtwangen.de\n\n";

    #endregion

    #region Constructors

    public Nesymca() : this( "Octave" )
    {
    }

    public Nesymca( string ui )
    {
        setup_ui( ui, true );

        welcome += "Executing in " + ui + "-Mode.\n";
        welcome += "Welcome and have fun!\n";

        print( welcome );
    }

    #endregion

    #region Private methods

    private void println( string format, params object[] list )
    {
        print( format + Environment.NewLine, list );
    }

    private void println()
    {
        println( "" );
    }

    private void Test( string[] args )
    {
        double[] ar = { 0.0, 1.0, 1.0, 1.0 };
        double[] ai = { 0.0, 0.0, 1.0, 0.0 };

        bool[] err = { true, true, true, true };

        Pzeros.aberth( ar, ai, err );

        for ( int i = 0; i < ar.Length - 1; i++ )
        {
            println( @"{0}: {1}+i*{2}  {3}", i, ar[ i ], ai[ i ], err[ i ] );
        }
    }

	private void setup_ui( string ui, bool clear_env )
	{
		if ( clear_env )
		{
			_store = new Store();
		}

		if ( ui != null )
		{
		    this.ui = ui;
		}

        if ( this.ui.Equals( "Maxima" ) )
        {
            _proc = new XProcessor( _store );
			_parser = new MaximaParser( _store );
		}

        else if ( this.ui.Equals( "Matlab" ) )
        {
            _proc = new Processor( _store );
            _parser = new MatlabParser( _store );
        }

        else if ( this.ui.Equals( "Octave" ) )
        {
            _proc = new Processor( _store );
            _parser = new OctaveParser( _store );
		}

		else
		{
		    println( @"Mode {0} not available.", ui );
		    
			Environment.Exit(0);
		}
	}

    #endregion

    #region Public methods

    public void print( string format, params object[] list )
    {
        Console.Write( format, list );
    }

    public virtual void Interrupt()
    {
        _proc?.SetInterrupt( true );
    }

    public void Run()
    {
        while ( true )
        {
            print( _parser.prompt() );

            try
            {
                _proc.SetInterrupt( false );

                var istream = new MemoryStream( Encoding.UTF8.GetBytes( Console.ReadLine() ) );

                istream.Seek( 0, SeekOrigin.Begin );

                var code = _parser.compile( istream );

                if ( code == null )
                {
                    _proc.println( "" );

                    continue;
                }

                if ( _proc.process_list( code, false ) == Processor.EXIT )
                {
                    _proc.println( "\nGoodbye." );

                    Console.ResetColor();

                    return;
                }

                _proc.printStack();
            }
            catch ( Exception ex )
            {
                _proc.println( "\n" + ex.Message );

                _proc.clearStack();
            }
        }
    }

    #endregion

}

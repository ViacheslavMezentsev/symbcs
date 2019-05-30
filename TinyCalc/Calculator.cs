using System;
using System.IO;
using System.Text;

using Tiny.Science;
using Tiny.Science.Engine;

namespace TinyCalc
{
    public class Printable : IPrintable
    {
        public void print( string format, params object[] list )
        {
            Console.Write( format, list );
        }
    }

    public class Calculator
    {

        #region Private fields

        private string ui = "Octave";

        private string welcome =
            "Jasymca	- Java Symbolic Calculator\n" +
            "version 2.1\n" +
            "Copyright (C) 2006, 2009 - Helmut Dersch\n" +
            "der@hs-furtwangen.de\n\n";

        #endregion

        #region Constructors

        public Calculator( string ui )
        {
            setup_ui( ui, true );

            welcome += "Executing in " + ui + "-Mode.\n";
            welcome += "Welcome and have fun!\n";

            print( welcome );
        }

        public Calculator() : this( "Octave" )
        {
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
                Session.Store = new Store();
            }

            if ( ui != null )
            {
                this.ui = ui;
            }

            if ( this.ui.Equals( "Maxima" ) )
            {
                Session.Proc = new XProcessor( Session.Store );
                Session.Proc.AttachOutput( new Printable() );

                Session.Parser = new MaximaParser( Session.Store );
            }

            else if ( this.ui.Equals( "Matlab" ) )
            {
                Session.Proc = new Processor( Session.Store );
                Session.Proc.AttachOutput( new Printable() );

                Session.Parser = new MatlabParser( Session.Store );
            }

            else if ( this.ui.Equals( "Octave" ) )
            {
                Session.Proc = new Processor( Session.Store );
                Session.Proc.AttachOutput( new Printable() );

                Session.Parser = new OctaveParser( Session.Store );
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
            Session.Proc?.SetInterrupt( true );
        }

        public void Run()
        {
            while ( true )
            {
                print( Session.Parser.prompt() );

                try
                {
                    Session.Proc.SetInterrupt( false );

                    var istream = new MemoryStream( Encoding.UTF8.GetBytes( Console.ReadLine() ) );

                    istream.Seek( 0, SeekOrigin.Begin );

                    var code = Session.Parser.compile( istream );

                    if ( code == null )
                    {
                        Session.Proc.println( "" );

                        continue;
                    }

                    if ( Session.Proc.ProcessList( code, false ) == Processor.EXIT )
                    {
                        Session.Proc.println( "\nGoodbye." );

                        Console.ResetColor();

                        return;
                    }

                    Session.Proc.PrintStack();
                }
                catch ( Exception ex )
                {
                    Session.Proc.println( "\n" + ex.Message );

                    Session.Proc.ClearStack();
                }
            }
        }

        #endregion

    }
}

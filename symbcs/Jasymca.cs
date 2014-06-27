using System;
using System.Threading;

using kjava.io;
using kjava.util;
using jsystem;
using jsystem.JConsole;
public class Jasymca : javax.microedition.midlet.MIDlet, Runnable
{
	internal static InputStream getFileInputStream(string fname)
	{
		return new kjava.io.FileInputStream(fname);
	}
	internal static OutputStream getFileOutputStream(string fname, bool append)
	{
		return new kjava.io.FileOutputStream(fname, append);
	}
	internal static string JasymcaRC = "vfs/Jasymca.";
	public virtual void startApp()
	{
		JSystem.init(Display.getDisplay(this));
		FileHandler[] fh = new FileHandler[] {new TextEdit()};
		JSystem.browser = new FileBrowser(JSystem.display, JSystem.console, fh);
		JSystem.console.Title = "Jasymca";
		JSystem.showConsole();
		JSystem.@out = JSystem.console.stdout;
		JSystem.err = JSystem.console.stdout;
		JSystem.@in = JSystem.console.stdin;
		start(JSystem.@in, JSystem.@out);
	}
	public virtual void destroyApp(bool a)
	{
	}
	public virtual void pauseApp()
	{
	}
	public Environment env;
	public Processor proc = null;
	public Parser pars;
	internal string ui = "Octave";
	internal PrintStream ps;
	internal InputStream @is;
	public virtual void interrupt()
	{
		if (proc != null)
		{
			proc.set_interrupt(true);
		}
	}
	internal string welcome = "Jasymca	- Java Symbolic Calculator\n" + "version 2.1\n" + "Copyright (C) 2006, 2009 - Helmut Dersch\n" + "der@hs-furtwangen.de\n\n";
	internal static NumFmt fmt = new NumFmtVar(10, 5);
	internal Thread evalLoop = null;
	public Jasymca() : this("Octave")
	{
	}
	public Jasymca(string ui)
	{
		setup_ui(ui, true);
		welcome += "Executing in " + ui + "-Mode.\n";
		welcome += "Welcome and have fun!\n";
	}
	public virtual void setup_ui(string ui, bool clear_env)
	{
		if (clear_env)
		{
			env = new Environment();
		}
		if (ui != null)
		{
			this.ui = ui;
		}
		if (this.ui.Equals("Maxima"))
		{
			proc = new XProcessor(env);
			pars = new MaximaParser(env);
		}
		else if (this.ui.Equals("Octave"))
		{
			proc = new Processor(env);
			pars = new OctaveParser(env);
		}
		else
		{
			Console.WriteLine("Mode " + this.ui + " not available.");
			Environment.Exit(0);
		}
	}
	public virtual void start(InputStream @is, PrintStream ps)
	{
		this.@is = @is;
		this.ps = ps;
		try
		{
			string fname = JasymcaRC + ui + ".rc";
			InputStream file = getFileInputStream(fname);
			LambdaLOADFILE.readFile(file);
		}
		catch (Exception)
		{
		}
		ps.print(welcome);
		proc.PrintStream = ps;
		evalLoop = new Thread(this);
		evalLoop.Start();
	}
	public virtual void run()
	{
		while (true)
		{
			ps.print(pars.prompt());
			try
			{
				proc.set_interrupt(false);
				List code = pars.compile(@is, ps);
				if (code == null)
				{
					ps.println("");
					continue;
				}
				if (proc.process_list(code, false) == proc.EXIT)
				{
					ps.println("\nGoodbye.");
					return;
				}
				proc.printStack();
			}
			catch (Exception e)
			{
				ps.println("\n" + e);
				proc.clearStack();
			}
		}
	}
}
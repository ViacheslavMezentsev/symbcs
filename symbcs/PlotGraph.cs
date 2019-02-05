using System;
using System.Collections;
using System.Threading;

internal class LambdaPLOT : Lambda
{
	internal static PlotGraph pg = null;
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
	public override int lambda(Stack st)
	{
		return plotArgs(PlotGraph.LINEAR, st);
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: int plotArgs(int plotmode, Stack st)throws ParseException, JasymcaException
	internal virtual int plotArgs(int plotmode, Stack st)
	{
		if (pg == null)
		{
			pg = new PlotGraph(plotmode);
		}
		else
		{
			pg.setmode(plotmode);
		}
		int narg = getNarg(st);
		object[] pargs = new object[narg];
		for (int i = 0; i < narg; i++)
		{
			object x = st.Pop();
			if (x is Vektor)
			{
				x = (new ExpandConstants()).f_exakt((Vektor)x);
				pargs[i] = ((Vektor)x).Double;
			}
			else
			{
				pargs[i] = x;
			}
		}
		pg.addLine(pargs);
		pg.show();
		return 0;
	}
}
internal class LambdaERRORBAR : LambdaPLOT
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
	public override int lambda(Stack st)
	{
		return plotArgs(PlotGraph.LINEAR, st);
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: int plotArgs(int plotmode, Stack st)throws ParseException, JasymcaException
	internal override int plotArgs(int plotmode, Stack st)
	{
		if (LambdaPLOT.pg == null)
		{
			pg = new PlotGraph(plotmode);
		}
		else
		{
			pg.setmode(plotmode);
		}
		int narg = getNarg(st);
		object[] pargs = new object[narg];
		for (int i = 0; i < narg; i++)
		{
			object x = st.Pop();
			if (x is Vektor)
			{
				x = (new ExpandConstants()).f_exakt((Vektor)x);
				pargs[i] = ((Vektor)x).Double;
			}
			else
			{
				pargs[i] = x;
			}
		}
		pg.addLineErrorbars(pargs);
		pg.show();
		return 0;
	}
}
internal class LambdaLOGLOG : LambdaPLOT
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack args) throws ParseException, JasymcaException
	public override int lambda(Stack args)
	{
		return plotArgs(PlotGraph.LOGLOG, args);
	}
}
internal class LambdaSEMILOGX : LambdaPLOT
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack args) throws ParseException, JasymcaException
	public override int lambda(Stack args)
	{
		return plotArgs(PlotGraph.LOGLIN, args);
	}
}
internal class LambdaSEMILOGY : LambdaPLOT
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack args) throws ParseException, JasymcaException
	public override int lambda(Stack args)
	{
		return plotArgs(PlotGraph.LINLOG, args);
	}
}
internal class LambdaTITLE : Lambda
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		object arg = st.Pop();
		if (!(arg is string))
		{
			throw new JasymcaException("Argument must be string.");
		}
		if (LambdaPLOT.pg != null)
		{
			LambdaPLOT.pg.Tlabel = (string)arg;
		}
		return 0;
	}
}
public class PlotGraph : Frame, Runnable
{
	internal ArrayList PlotLines = new ArrayList();
	internal const int LINEAR = 0;
	internal const int LOGLIN = 1;
	internal const int LINLOG = 2;
	internal const int LOGLOG = 3;
	internal int plotmode = LINEAR;
	public virtual void setmode(int mode)
	{
		if (mode != plotmode)
		{
			reset();
		}
		plotmode = mode;
	}
	internal int width, height;
	internal int xleft, ytop, xright, ybottom;
	internal int yXlabel;
	internal int yTlabel;
	internal string Xlabel_Renamed = null;
	internal string Ylabel_Renamed = null;
	internal string Tlabel_Renamed = null;
	internal virtual void setparam()
	{
		width = Width;
		height = Height;
		xleft = Ylabel_Renamed == null ? 30 : 50;
		xright = 25;
		ytop = Tlabel_Renamed == null ? 20 : 40;
		ybottom = Xlabel_Renamed == null ? 20 : 40;
	}
	public virtual string Xlabel
	{
		set
		{
			Xlabel_Renamed = value;
			repaint();
		}
	}
	public virtual string Ylabel
	{
		set
		{
			Ylabel_Renamed = value;
			repaint();
		}
	}
	public virtual string Tlabel
	{
		set
		{
			Tlabel_Renamed = value;
			repaint();
		}
	}
	internal INumFmt fmt = new NumFmtVar(10, 4);
	internal int xp = -1, yp;
	internal int dx = 0, dy = 0;
	internal double a0, a1;
	internal bool movePointer = false;
	internal Thread moveP = null;
	internal const int PMODE_POINT = 0;
	internal const int PMODE_LINE = 1;
	internal const int PMODE_LINE_POINT = 2;
	internal int pointerMode = PMODE_POINT;
	internal double minx = double.PositiveInfinity;
	internal double maxx = double.NegativeInfinity;
	internal int ntx = 10, nty = 10;
	internal double miny = double.PositiveInfinity;
	internal double maxy = double.NegativeInfinity;
	internal Image ShowLine = null;
	public virtual void run()
	{
		while (movePointer)
		{
			try
			{
				if (xp < 0)
				{
					movePointer = false;
					return;
				}
				int xneu = xp + dx;
				if (xneu >= getScreenX(maxx))
				{
					xneu = getScreenX(minx);
				}
				if (xneu < getScreenX(minx))
				{
					xneu = getScreenX(maxx);
				}
				int yneu = yp + dy;
				if (yneu >= getScreenY(miny))
				{
					yneu = getScreenY(maxy);
				}
				if (yneu < getScreenY(maxy))
				{
					yneu = getScreenY(miny);
				}
				xp = xneu;
				yp = yneu;
				repaint();
				Thread.Sleep(100);
			}
			catch (Exception)
			{
			}
		}
	}
	public PlotGraph(int mode)
	{
		plotmode = mode;
		setSize(300,200);
		setparam();
		try
		{
			ShowLine = Image.createImage("/icons/fav.png");
		}
		catch (Exception)
		{
			Console.WriteLine("Could not load Images.");
		}
	}
	public PlotGraph() : this(LINEAR)
	{
	}
	internal virtual void drawStraightLine(javax.microedition.lcdui.Graphics g)
	{
		double xl = minx;
		double yl = a1 * xl + a0;
		if (yl > maxy)
		{
			yl = maxy;
			xl = (yl - a0) / a1;
		}
		else if (yl < miny)
		{
			yl = miny;
			xl = (yl - a0) / a1;
		}
		double xr = maxx;
		double yr = a1 * xr + a0;
		if (yr > maxy)
		{
			yr = maxy;
			xr = (yr - a0) / a1;
		}
		else if (yr < miny)
		{
			yr = miny;
			xr = (yr - a0) / a1;
		}
		if (xp >= 0 && pointerMode == PMODE_LINE)
		{
			double xm = getXCoordinate(xp);
			double ym = getYCoordinate(yp);
			int Xr = getScreenX(xr), Yr = getScreenY(yr), Xl = getScreenX(xl), Yl = getScreenY(yl);
			if ((xp - Xr) * (xp - Xr) + (yp - Yr) * (yp - Yr) < (xp - Xl) * (xp - Xl) + (yp - Yl) * (yp - Yl))
			{
				a1 = (ym - yl) / (xm - xl);
				a0 = yl - a1 * xl;
				xr = maxx;
				yr = a1 * xr + a0;
				if (yr > maxy)
				{
					yr = maxy;
					xr = (yr - a0) / a1;
				}
				else if (yr < miny)
				{
					yr = miny;
					xr = (yr - a0) / a1;
				}
			}
			else
			{
				a1 = (yr - ym) / (xr - xm);
				a0 = yr - a1 * xr;
				xl = minx;
				yl = a1 * xl + a0;
				if (yl > maxy)
				{
					yl = maxy;
					xl = (yl - a0) / a1;
				}
				else if (yl < miny)
				{
					yl = miny;
					xl = (yl - a0) / a1;
				}
			}
		}
		g.Color = GREEN;
		g.drawLine(getScreenX(xl),getScreenY(yl),getScreenX(xr),getScreenY(yr));
		g.Color = BLACK;
		if (pointerMode == PMODE_LINE)
		{
			drawMessage(g, xp, yp, new string[] {"a1=" + fmt.ToString(a1), "a0=" + fmt.ToString(a0)});
		}
	}
	internal virtual void drawMessage(javax.microedition.lcdui.Graphics g, int xp, int yp, string[] msg)
	{
		double[] sw = new double[msg.Length];
		for (int i = 0; i < sw.Length; i++)
		{
			sw[i] = g.Font.stringWidth(msg[i]);
		}
		int fh = g.Font.Height;
		int mw = (int)max(sw) + 10, mh = msg.Length * fh + 6;
		int xw = xp < width / 2 ? width - xright - mw : xleft;
		int yw = yp < height / 2 ? height - ybottom - mh : ytop;
		g.Color = WHITE;
		g.fillRect(xw,yw,mw,mh);
		g.Color = BLACK;
		for (int i = 0; i < msg.Length; i++)
		{
			g.drawString(msg[i],xw + 5,yw + (i + 1) * fh + 3,g.BOTTOM | g.LEFT);
		}
	}
	internal virtual void drawPointer(javax.microedition.lcdui.Graphics g)
	{
		g.Color = RED;
		fillCircle(g, xp, yp, 3);
		g.Color = WHITE;
		g.fillRect(xp, yp, 1,1);
		double X = getXCoordinate(xp);
		if (plotmode == LOGLIN || plotmode == LOGLOG)
		{
			X = JMath.pow(10.0,X);
		}
		double Y = getYCoordinate(yp);
		if (plotmode == LINLOG || plotmode == LOGLOG)
		{
			Y = JMath.pow(10.0,Y);
		}
		drawMessage(g, xp, yp, new string[] {"X=" + fmt.ToString(X), "Y=" + fmt.ToString(Y)});
	}
	public virtual double getXCoordinate(int x)
	{
		double a = (maxx - minx) / (double)(width - xleft - xright);
		double b = minx - a * xleft;
		return a * x + b;
	}
	public virtual double getYCoordinate(int y)
	{
		double a = (miny - maxy) / (double)(height - ytop - ybottom);
		double b = maxy - a * ytop;
		return a * y + b;
	}
	public virtual int getScreenX(double x)
	{
		double a = (width - xleft - xright) / (maxx - minx);
		double b = xleft - a * minx;
		return (int)(a * x + b + 0.5);
	}
	public virtual int getScreenY(double y)
	{
		double a = (height - ytop - ybottom) / (miny - maxy);
		double b = ytop - a * maxy;
		return (int)(a * y + b + 0.5);
	}
	internal const int BLACK = 0x000000;
	internal const int RED = 0xff0000;
	internal const int GREEN = 0x00ff00;
	internal const int BLUE = 0x0000ff;
	internal const int CYAN = 0x00ffff;
	internal const int MAGENTA = 0xff00ff;
	internal const int YELLOW = 0xffff00;
	internal const int WHITE = 0xffffff;
	internal const int LIGHTGRAY = 0xc0c0c0;
	internal class PlotLine
	{
		private readonly PlotGraph outerInstance;

		public PlotLine(PlotGraph outerInstance)
		{
			this.outerInstance = outerInstance;
		}

		internal double lineMinx, lineMaxx, lineMiny, lineMaxy;
		internal char marker = ' ';
		internal double[] x;
		internal double[] y;
		internal double[] eu = null;
		internal double[] el = null;
		internal int color = BLUE;
		internal virtual void setPoints(double[] xp, double[] yp)
		{
			x = xp;
			y = yp;
			lineMaxx = outerInstance.max(x);
			lineMinx = outerInstance.min(x);
			lineMaxy = outerInstance.max(y);
			lineMiny = outerInstance.min(y);
		}
		internal virtual void paint(javax.microedition.lcdui.Graphics g)
		{
			int old = g.Color;
			g.Color = color;
			if (marker == ' ')
			{
				for (int i = 0;i < x.Length - 1;i++)
				{
					g.drawLine(outerInstance.getScreenX(x[i]), outerInstance.getScreenY(y[i]), outerInstance.getScreenX(x[i + 1]), outerInstance.getScreenY(y[i + 1]));
				}
			}
			else
			{
				for (int i = 0;i < x.Length - 1;i++)
				{
					drawSymbol(g,outerInstance.getScreenX(x[i]), outerInstance.getScreenY(y[i]),marker);
				}
			}
			if (eu != null)
			{
				for (int i = 0;i < x.Length - 1;i++)
				{
					g.drawLine(outerInstance.getScreenX(x[i]), outerInstance.getScreenY(y[i] - el[i]), outerInstance.getScreenX(x[i]), outerInstance.getScreenY(y[i] + eu[i]));
					g.drawLine(outerInstance.getScreenX(x[i]) - 3, outerInstance.getScreenY(y[i] - el[i]), outerInstance.getScreenX(x[i]) + 3, outerInstance.getScreenY(y[i] - el[i]));
					g.drawLine(outerInstance.getScreenX(x[i]) - 3, outerInstance.getScreenY(y[i] + eu[i]), outerInstance.getScreenX(x[i]) + 3, outerInstance.getScreenY(y[i] + eu[i]));
				}
			}
			g.Color = old;
		}
		internal virtual void drawSymbol(javax.microedition.lcdui.Graphics g, int x, int y, char marker)
		{
			switch (marker)
			{
				case '+':
					g.drawLine(x,y - 3,x,y + 3);
					g.drawLine(x - 3, y,x + 3,y);
					break;
				case 'o':
					g.drawLine(x - 1,y - 3,x + 1,y - 3);
					g.drawLine(x + 1, y - 3,x + 3,y - 1);
					g.drawLine(x + 3, y - 1,x + 3,y + 1);
					g.drawLine(x + 3,y + 1,x + 1,y + 3);
					g.drawLine(x + 1,y + 3,x - 1,y + 3);
					g.drawLine(x - 1,y + 3,x - 3,y + 1);
					g.drawLine(x - 3,y + 1,x - 3,y - 1);
					g.drawLine(x - 3,y - 1,x - 1,y - 3);
					break;
				case 'x':
					g.drawLine(x - 3, y - 3,x + 3,y + 3);
					g.drawLine(x + 3, y - 3,x - 3,y + 3);
					break;
				case '*':
			default:
				g.drawLine(x,y - 3,x,y + 3);
				g.drawLine(x - 3, y,x + 3,y);
				g.drawLine(x - 2, y - 2,x + 2,y + 2);
				g.drawLine(x + 2, y - 2,x - 2,y + 2);
				break;
			}
		}
		public virtual string LineAttributes
		{
			set
			{
				for (int i = 0;i < value.Length;i++)
				{
					switch (value[i])
					{
						case 'r':
							color = RED;
							break;
						case 'g':
							color = GREEN;
							break;
						case 'b':
							color = BLUE;
							break;
						case 'y':
							color = YELLOW;
							break;
						case 'm':
							color = MAGENTA;
							break;
						case 'c':
							color = CYAN;
							break;
						case 'w':
							color = WHITE;
							break;
						case 'k':
							color = BLACK;
							break;
						default:
							marker = value[i];
							break;
					}
				}
			}
		}
	}
	internal bool Hold_b = false;
	private void reset()
	{
		PlotLines.Clear();
		minx = double.PositiveInfinity;
		maxx = double.NegativeInfinity;
		miny = double.PositiveInfinity;
		maxy = double.NegativeInfinity;
		Xlabel_Renamed = null;
		Ylabel_Renamed = null;
		Tlabel_Renamed = null;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void log10(double[] x) throws JasymcaException
	internal virtual void log10(double[] x)
	{
		for (int i = 0; i < x.Length; i++)
		{
			if (x[i] <= 0.0)
			{
				throw new JasymcaException("Log from negative number.");
			}
			x[i] = JMath.log(x[i]) / JMath.log(10.0);
		}
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void addLine(Object[] params) throws JasymcaException
	internal virtual void addLine(object[] @params)
	{
		if (!Hold_b)
		{
			reset();
		}
		for (int i = 0;i < @params.Length;)
		{
			PlotLine line = new PlotLine(this);
			if (i < @params.Length - 1 && !(@params[i + 1] is string))
			{
				double[] x = (double[]) @params[i];
				if (plotmode == LOGLIN || plotmode == LOGLOG)
				{
					log10(x);
				}
				line.lineMaxx = Math.Max(max(x),maxx);
				line.lineMinx = Math.Min(min(x),minx);
				double[] y = (double[]) @params[i + 1];
				if (plotmode == LINLOG || plotmode == LOGLOG)
				{
					log10(y);
				}
				line.lineMaxy = Math.Max(max(y),maxy);
				line.lineMiny = Math.Min(min(y),miny);
				if (x.Length != y.Length)
				{
					throw new JasymcaException("X and Y must be same length");
				}
				line.setPoints(x,y);
				i += 2;
			}
			else
			{
				double[] x = (double[]) @params[i];
				if (plotmode == LINLOG || plotmode == LOGLOG)
				{
					log10(x);
				}
				maxx = x.Length;
				minx = 1;
				maxy = max(x);
				miny = min(x);
				line.x = new double[x.Length];
				for (int ind = 0;ind < x.Length;ind++)
				{
					line.x[ind] = ind + 1;
				}
				line.setPoints(line.x,x);
				i++;
			}
			maxx = Math.Max(line.lineMaxx,maxx);
			minx = Math.Min(line.lineMinx,minx);
			maxy = Math.Max(line.lineMaxy,maxy);
			miny = Math.Min(line.lineMiny,miny);
			if (i < @params.Length && (@params[i] is string))
			{
				line.LineAttributes = @params[i].ToString();
				i++;
			}
			PlotLines.Add(line);
		}
		setMinMax();
		repaint();
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void addLineErrorbars(Object[] params) throws JasymcaException
	internal virtual void addLineErrorbars(object[] @params)
	{
		if (!Hold_b)
		{
			reset();
		}
		if (@params.Length < 3)
		{
			throw new JasymcaException("At least 3 arguments required.");
		}
		PlotLine line = new PlotLine(this);
		double[] x = (double[]) @params[0];
		line.lineMaxx = Math.Max(max(x),maxx);
		line.lineMinx = Math.Min(min(x),minx);
		double[] y = (double[]) @params[1];
		line.lineMaxy = Math.Max(max(y),maxy);
		line.lineMiny = Math.Min(min(y),miny);
		if (x.Length != y.Length)
		{
			throw new JasymcaException("X and Y must be same length");
		}
		line.setPoints(x,y);
		double[] el = (double[]) @params[2]; double[] eu = el;
		if (el.Length != y.Length)
		{
			throw new JasymcaException("Errors and Y must be same length");
		}
		int i = 3;
		if (@params.Length > 3 && !(@params[3] is string))
		{
			eu = (double[]) @params[3];
			if (eu.Length != y.Length)
			{
				throw new JasymcaException("Errors and Y must be same length");
			}
			i++;
		}
		line.eu = eu;
		line.el = el;
		maxx = Math.Max(line.lineMaxx,maxx);
		minx = Math.Min(line.lineMinx,minx);
		maxy = Math.Max(line.lineMaxy,maxy);
		miny = Math.Min(line.lineMiny,miny);
		if (i < @params.Length && (@params[i] is string))
		{
			line.LineAttributes = @params[i].ToString();
			i++;
		}
		PlotLines.Add(line);
		setMinMax();
		repaint();
	}
	internal virtual void drawGraph(javax.microedition.lcdui.Graphics g)
	{
		int old = g.Color;
		g.Color = LIGHTGRAY;
		g.fillRect(0,0,width,height);
		for (int i = 0;i < PlotLines.Count;i++)
		{
			PlotLine line = (PlotLine)PlotLines[i];
			line.paint(g);
		}
		g.Color = old;
		drawAxis(g);
	}
	public virtual void paint(javax.microedition.lcdui.Graphics g)
	{
		setparam();
		if (width < 100 || height < 100)
		{
			return;
		}
		drawGraph(g);
		if (pointerMode != PMODE_POINT)
		{
			drawStraightLine(g);
		}
		if (pointerMode != PMODE_LINE && xp >= 0)
		{
			drawPointer(g);
		}
		g.drawImage(ShowLine,width - xright + 3,ytop,g.LEFT | g.TOP);
	}
	internal virtual bool inShowLine(int x, int y)
	{
		if (ShowLine != null)
		{
			if (x > width - xright + 3 && x < width - xright + 3 + ShowLine.Width && y > ytop && y < ytop + ShowLine.Height)
			{
				return true;
			}
		}
		return false;
	}
	public virtual void fillCircle(javax.microedition.lcdui.Graphics g, int x, int y, int r)
	{
		int r2 = r * r;
		for (int yd = y - r; yd < y + r; yd++)
		{
			if (yd >= 0 && yd < height)
			{
				int y2 = (yd - y) * (yd - y);
				for (int xd = x - r; xd < x + r; xd++)
				{
					if (xd >= 0 && xd < width)
					{
						if (y2 + (xd - x) * (xd - x) <= r2)
						{
							g.fillRect(xd,yd,1,1);
						}
					}
				}
			}
		}
	}
	internal virtual double max(double[] x)
	{
		double max = x[0];
		for (int i = 1;i < x.Length;i++)
		{
			if (x[i] > max)
			{
				max = x[i];
			}
		}
		return max;
	}
	internal virtual double min(double[] x)
	{
		double min = x[0];
		for (int i = 1;i < x.Length;i++)
		{
			if (x[i] < min)
			{
				min = x[i];
			}
		}
		return min;
	}
	internal virtual void drawAxis(javax.microedition.lcdui.Graphics g)
	{
		g.Color = BLACK;
		int x2 = width - xright, y2 = height - ybottom;
		g.drawLine(xleft,ytop,x2,ytop);
		g.drawLine(xleft,ytop,xleft,y2);
		g.drawLine(x2,ytop,x2,y2);
		g.drawLine(xleft,y2,x2,y2);
		if (minx < 0 && 0 < maxx)
		{
			g.StrokeStyle = g.DOTTED;
			g.drawLine(getScreenX(0.0),getScreenY(miny),getScreenX(0.0), getScreenY(maxy));
			g.StrokeStyle = g.SOLID;
		}
		if (miny < 0 && 0 < maxy)
		{
			g.StrokeStyle = g.DOTTED;
			g.drawLine(getScreenX(minx),getScreenY(0.0),getScreenX(maxx), getScreenY(0.0));
			g.StrokeStyle = g.SOLID;
		}
		drawOrnaments(g);
	}
	internal static int decExp(double x)
	{
		return (int)(JMath.log(x) / JMath.log(10.0));
	}
	internal virtual double largestp10(double x)
	{
		double p = 1.0;
		while (p < x)
		{
			p *= 10.0;
		}
		while (p > x)
		{
			p /= 10.0;
		}
		return p;
	}
	internal virtual void setMinMax()
	{
		if (maxx == minx)
		{
			maxx++;
		}
		double div = largestp10(maxx - minx) / 10;
		int ntx1 = (int)Math.Ceiling(maxx / div);
		int ntx2 = (int)Math.Floor(minx / div);
		ntx = ntx1 - ntx2;
		maxx = ntx1 * div;
		minx = ntx2 * div;
		if (maxy == miny)
		{
			maxy++;
		}
		div = largestp10(maxy - miny) / 10;
		int nty1 = (int)Math.Ceiling(maxy / div);
		int nty2 = (int)Math.Floor(miny / div);
		nty = nty1 - nty2;
		maxy = nty1 * div;
		miny = nty2 * div;
		a1 = 0.0;
		a0 = (maxy + miny) / 2.0;
	}
	internal virtual void drawOrnaments(javax.microedition.lcdui.Graphics g)
	{
		int lenMajor = height / 40, i ;
		double x = 0, y = 0;
		int axis_w = width - xleft - xright;
		int axis_h = height - ytop - ybottom;
		string label;
		int lenlabel = g.Font.stringWidth(" 00.00 ");
		int maxnumtics = axis_w / lenlabel;
		double dX = (maxx - minx) / ntx;
		double startx = minx;
		while ((maxx - minx) / dX > maxnumtics)
		{
			dX *= 2;
			startx = (int)(startx / dX + 0.5) * dX;
			if ((maxx - minx) / dX > maxnumtics)
			{
				dX *= 2.5;
				startx = (int)(startx / dX + 0.5) * dX;
			}
		}
		if (plotmode == LOGLIN || plotmode == LOGLOG)
		{
			while (Math.Abs((startx / dX) - JMath.round(startx / dX)) > 0.01)
			{
				startx += dX;
			}
			if (dX < 1.0)
			{
				dX = 1.0;
			}
			else if (dX == 2.5)
			{
				dX = 2.0;
			}
			startx = dX * JMath.floor(startx / dX);
		}
		int exponent = Math.Max(decExp(maxx),decExp(minx)) - 1;
		if (Math.Abs(exponent) < 2)
		{
			exponent = 0;
		}
		double scf = JMath.pow(10.0,(double)exponent);
		for (i = 0,x = startx;x <= maxx; i++, x += dX)
		{
			int xworld = getScreenX(x);
			if (xworld < xleft)
			{
				continue;
			}
			g.drawLine(xworld,ytop,xworld,ytop + lenMajor);
			g.drawLine(xworld,height - ybottom - lenMajor,xworld,height - ybottom);
			if ((plotmode == LOGLIN || plotmode == LOGLOG) && (dX < 1.5))
			{
				for (int k = 2; k <= 9; k++)
				{
					int xk = getScreenX(x + JMath.log((double)k) / JMath.log(10.0));
					if (xk > xleft && xk < xleft + width)
					{
						g.drawLine(xk,ytop,xk,ytop + lenMajor / 2);
						g.drawLine(xk,height - ybottom - lenMajor / 2,xk,height - ybottom);
					}
				}
			}
			if (plotmode == LOGLIN || plotmode == LOGLOG)
			{
				label = "10^";
			}
			else
			{
				label = "";
			}
			if (x + dX > maxx && exponent != 0)
			{
				label = "E" + exponent;
			}
			else
			{
				label += fmt.ToString(x / scf);
			}
			centerText(g,label,height - ybottom + 1.5 * g.Font.Height,xworld);
		}
		if (Xlabel_Renamed != null)
		{
			centerText(g,Xlabel_Renamed,height - ybottom + 3 * g.Font.Height, xleft + (width - xleft - xright) / 2);
		}
		if (Tlabel_Renamed != null)
		{
			centerText(g,Tlabel_Renamed, ytop - g.Font.Height / 2, xleft + (width - xleft - xright) / 2);
		}
		if (Ylabel_Renamed != null)
		{
			centerTextV(g,Ylabel_Renamed, g.Font.Height, ytop + (height - ytop - ybottom) / 2);
		}
		int hilabel = g.Font.Height;
		maxnumtics = (int)((axis_h) / (1.5 * hilabel));
		double dY = (maxy - miny) / nty;
		double starty = miny;
		while ((maxy - miny) / dY > maxnumtics)
		{
			dY *= 2;
			starty = (int)(starty / dY + 0.5) * dY;
			if ((maxy - miny) / dY > maxnumtics)
			{
				dY *= 2.5;
				starty = (int)(starty / dY + 0.5) * dY;
			}
		}
		if (plotmode == LINLOG || plotmode == LOGLOG)
		{
			if (dY < 1.0)
			{
				dY = 1.0;
			}
			else if (dY == 2.5)
			{
				dY = 2.0;
			}
			starty = dY * JMath.floor(starty / dY);
		}
		exponent = Math.Max(decExp(maxy),decExp(miny)) - 1;
		if (Math.Abs(exponent) < 2)
		{
			exponent = 0;
		}
		scf = JMath.pow(10.0,(double)exponent);
		for (i = 0,y = starty;y <= maxy; i++, y += dY)
		{
			int yworld = getScreenY(y);
			if (yworld > height - ybottom)
			{
				continue;
			}
			g.drawLine(xleft,yworld,xleft + lenMajor,yworld);
			g.drawLine(xleft + axis_w,yworld,xleft + axis_w - lenMajor,yworld);
			if ((plotmode == LINLOG || plotmode == LOGLOG) && dY < 1.5)
			{
				for (int k = 2; k <= 9; k++)
				{
					int yk = getScreenY(y + JMath.log((double)k) / JMath.log(10.0));
					if (yk > ytop && yk < ytop + height)
					{
						g.drawLine(xleft,yk,xleft + lenMajor / 2,yk);
						g.drawLine(xleft + axis_w,yk,xleft + axis_w - lenMajor / 2,yk);
					}
				}
			}
			if (plotmode == LINLOG || plotmode == LOGLOG)
			{
				label = "10^";
			}
			else
			{
				label = "";
			}
			if (y + dY > maxy && exponent != 0)
			{
				label = "E" + exponent;
			}
			else
			{
				label += fmt.ToString(y / scf);
			}
			g.drawString(label,xleft - g.Font.stringWidth(label) - 3,yworld + hilabel / 2 - 3,g.BOTTOM | g.LEFT);
		}
	}
	public static void centerText(javax.microedition.lcdui.Graphics g, string msg, double posY, double centerPos)
	{
		int sw = g.Font.stringWidth(msg);
		double posX = (centerPos) - sw / 2;
		double dy = 4.0, dx = 0.0;
		g.drawString(msg,(int)JMath.round(posX - dx),(int)JMath.round(posY - dy),g.BOTTOM | g.LEFT);
	}
	public static void centerTextV(javax.microedition.lcdui.Graphics g, string msg, double posX, double centerPos)
	{
		int sw = g.Font.stringWidth(msg);
		double posY = (centerPos) + sw / 2;
		double dy = 0.0, dx = 0.0;
		Font oldFont = g.Font;
		g.drawString(msg,(int)posX,(int)posY,g.BOTTOM | g.LEFT);
		g.Font = oldFont;
	}
	internal virtual void startMovePointer()
	{
		if (moveP != null && moveP.IsAlive)
		{
			return;
		}
		movePointer = true;
		moveP = new Thread(this);
		moveP.Start();
	}
	public virtual bool handleEvent(Event e)
	{
		switch (e.id)
		{
			case Event.WINDOW_DESTROY:
				JSystem.desktop.removeFrame();
				return true;
			case Event.MOUSE_DOWN:
				if (inShowLine(e.x,e.y))
				{
					xp = -1;
					if (pointerMode == PMODE_POINT)
					{
						pointerMode = PMODE_LINE;
					}
					else if (pointerMode == PMODE_LINE)
					{
						pointerMode = PMODE_LINE_POINT;
					}
					else
					{
						pointerMode = PMODE_POINT;
					}
				}
				else
				{
					xp = e.x;
					yp = e.y;
				}
				repaint();
				return true;
			case Event.MOUSE_DRAG:
				if (xp != -1)
				{
					xp = e.x;
					yp = e.y;
					repaint();
				}
				return true;
			case Event.MOUSE_UP:
				xp = -1;
				repaint();
				return true;
			case Event.KEY_ACTION:
				switch (e.key)
				{
					case Event.ENTER:
						pointerMode = PMODE_POINT;
						if (xp < 0)
						{
							xp = width / 2;
							yp = height / 2;
						}
						else
						{
							movePointer = false;
							xp = -1;
						}
						repaint();
						return true;
					case Event.RIGHT:
						dx = 1;
						startMovePointer();
						return true;
					case Event.LEFT:
						dx = -1;
						startMovePointer();
						return true;
					case Event.DOWN:
						dy = 1;
						startMovePointer();
						return true;
					case Event.UP:
						dy = -1;
						startMovePointer();
						return true;
				}
				break;
			case Event.KEY_ACTION_RELEASE:
				switch (e.key)
				{
					case Event.RIGHT:
				case Event.LEFT:
			case Event.DOWN:
		case Event.UP:
			movePointer = false;
			dx = dy = 0;
			return true;
				}
				break;
		}
		return true;
	}
	public virtual Event getClickEvent(int x, int y)
	{
		if (x >= 0 && x < width && y >= 0 && y < height)
		{
			Event e = new Event();
			e.id = Event.MOUSE_DOWN;
			e.x = x;
			e.y = y;
			return e;
		}
		else
		{
			return null;
		}
	}
	public virtual Event getMoveEvent(int x, int y)
	{
		if (x >= 0 && x < width && y >= 0 && y < height)
		{
			Event e = new Event();
			e.id = Event.MOUSE_DRAG;
			e.x = x;
			e.y = y;
			return e;
		}
		else
		{
			return null;
		}
	}
	public virtual Event getReleaseEvent(int x, int y)
	{
		if (x >= 0 && x < width && y >= 0 && y < height)
		{
			Event e = new Event();
			e.id = Event.MOUSE_UP;
			e.x = x;
			e.y = y;
			return e;
		}
		else
		{
			return null;
		}
	}
}
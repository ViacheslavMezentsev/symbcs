using System;
using System.Collections;

public class Exakt : Zahl
{
	internal BigInteger[] real; internal BigInteger[] imag = null;

	public Exakt(BigInteger[] real) : this(real, null)
	{
	}

	public Exakt(long nom, long den)
	{
		real = new BigInteger[2];

		real[0] = BigInteger.valueOf(nom);
		real[1] = BigInteger.valueOf(den);
	}

	public Exakt(BigInteger r)
	{
		real = new BigInteger[2];

		real[0] = r;
		real[1] = BigInteger.ONE;
	}

	public Exakt(BigInteger[] real, BigInteger[] imag)
	{
		this.real = reducev(real);

		if (imag != null && !imag[0].Equals(BigInteger.ZERO))
		{
			this.imag = reducev(imag);
		}
	}

	public Exakt(double x) : this(x,0.0)
	{
	}

	public Exakt(double x, double y)
	{
		real = reducev(double2rat(x));

		if (y != 0.0)
		{
			imag = reducev(double2rat(y));
		}
	}

	internal virtual BigInteger double2big(double x)
	{
		int exp = 0;

		while (x > 1e15)
		{
			x /= 10.0;
			exp++;
		}

		BigInteger y = BigInteger.valueOf((long)(JMath.round(x)));

		if (exp > 0)
		{
			BigInteger ten = BigInteger.valueOf(10L);

			y = y.multiply(ten.pow(exp));
		}

		return y;
	}

	private BigInteger[] double2rat(double x)
	{
		BigInteger[] br;

		if (x == 0)
		{
			br = new BigInteger[2];

			br[0] = BigInteger.ZERO;
			br[1] = BigInteger.ONE;

			return br;
		}

		if (x < 0.0)
		{
			br = double2rat(-x);
			br[0] = br[0].negate();

			return br;
		}

		double eps = 1.0e-8;
		Zahl a = Lambda.pc.env.getnum("ratepsilon");

		if (a != null)
		{
			double epstry = a.unexakt().real;

			if (epstry > 0)
			{
				eps = epstry;
			}
		}

		if (x < 1 / eps)
		{
			double[] y = cfs(x, eps);

			br = new BigInteger[2];

			br[0] = double2big(y[0]);
			br[1] = double2big(y[1]);

			return br;
		}

		br = new BigInteger[2];

		br[0] = double2big(x);
		br[1] = BigInteger.ONE;

		return br;
	}

	private double[] cfs(double x, double tol)
	{
		ArrayList a = new ArrayList();

	    double[] y = new double[2];

		tol = Math.Abs(x * tol);

		double aa = Math.Floor(x);

		a.Add(new double?(aa));

		double ra = x;

		cfsd(a,y);

		while (Math.Abs(x - y[0] / y[1]) > tol)
		{
			ra = 1.0 / (ra - aa);
			aa = Math.Floor(ra);
			a.Add(new double?(aa));
			cfsd(a,y);
		}

		return y;
	}

	private void cfsd(ArrayList a, double[] y)
	{
		int i = a.Count - 1;

		double N = (double)((double?)a[i]), Z = 1.0, N1 ;

		i--;

		while (i >= 0)
		{
			N1 = (double)((double?)a[i]) * N + Z;
			Z = N;
			N = N1;
			i--;
		}

		y[0] = N;
		y[1] = Z;
	}

	internal virtual Exakt cfs(double tol1)
	{
		ArrayList a = new ArrayList();

		Exakt error, y, ra, tol;
		BigInteger aa;

		tol = (Exakt)mult(new Exakt(tol1));
		aa = real[0].divide(real[1]);
		a.Add(aa);
		y = new Exakt(cfs(a));
		error = (Exakt)((Exakt)(sub(y))).abs();
		ra = this;

		while (tol.smaller(error))
		{
			ra = (Exakt)Zahl.ONE.div(ra.sub(new Exakt(aa)));
			aa = ra.real[0].divide(ra.real[1]);
			a.Add(aa);
			y = new Exakt(cfs(a));
			error = (Exakt)((Exakt)sub(y)).abs();
		}

		return y;
	}

	private BigInteger[] cfs(ArrayList a)
	{
		int i = a.Count - 1;

		BigInteger N = (BigInteger)a[i], Z = BigInteger.ONE, N1 ;

		i--;

		while (i >= 0)
		{
			N1 = ((BigInteger)a[i]).multiply(N).add(Z);
			Z = N;
			N = N1;
			i--;
		}

		BigInteger[] r = new BigInteger[] {N,Z};

		return r;
	}

	private BigInteger[] reducev(BigInteger[] y)
	{
		BigInteger[] x = new BigInteger[2];

		x[0] = y[0];
		x[1] = y[1];

		BigInteger gcd = x[0].gcd(x[1]);

		if (!gcd.Equals(BigInteger.ONE))
		{
			x[0] = x[0].divide(gcd);
			x[1] = x[1].divide(gcd);
		}

		if (x[1].compareTo(BigInteger.ZERO) < 0)
		{
			x[0] = x[0].negate();
			x[1] = x[1].negate();
		}

		return x;
	}

	public override Algebraic realpart()
	{
		return new Exakt(real);
	}

	public override Algebraic imagpart()
	{
		if (imag != null)
		{
			return new Exakt(imag);
		}

		return new Exakt(BigInteger.ZERO);
	}

	public override bool exaktq()
	{
		return true;
	}

	private double floatValue(BigInteger[] x)
	{
		var q = x[0].divideAndRemainder( x[1] );

		return q[0].doubleValue() + q[1].doubleValue() / x[1].doubleValue();
	}

	public virtual Unexakt tofloat()
	{
		if (imag == null)
		{
			return new Unexakt(floatValue(real));
		}
		else
		{
			return new Unexakt(floatValue(real), floatValue(imag));
		}
	}

	private BigInteger[] add(BigInteger[] x, BigInteger[] y)
	{
		if (x == null)
		{
			return y;
		}

		if (y == null)
		{
			return x;
		}

		BigInteger[] r = new BigInteger[2];

		r[0] = x[0].multiply(y[1]).add(y[0].multiply(x[1]));
		r[1] = x[1].multiply(y[1]);

		return r;
	}

	private BigInteger[] sub(BigInteger[] x, BigInteger[] y)
	{
		if (y == null)
		{
			return x;
		}

		BigInteger[] r = new BigInteger[2];

		r[0] = y[0].negate();
		r[1] = y[1];

		return add(x,r);
	}

	private BigInteger[] mult(BigInteger[] x, BigInteger[] y)
	{
		if (x == null || y == null)
		{
			return null;
		}

		BigInteger[] r = new BigInteger[2];

		r[0] = x[0].multiply(y[0]);
		r[1] = x[1].multiply(y[1]);

		return r;
	}

	private BigInteger[] div(BigInteger[] x, BigInteger[] y)
	{
		if (x == null)
		{
			return null;
		}

		if (y == null)
		{
			throw new JasymcaException("Division by Zero.");
		}

		BigInteger[] r = new BigInteger[2];

		r[0] = x[0].multiply(y[1]);
		r[1] = x[1].multiply(y[0]);

		return r;
	}

	private bool Equals(BigInteger[] x, BigInteger[] y)
	{
		if (x == null && y == null)
		{
			return true;
		}

		if (x == null || y == null)
		{
			return false;
		}

		return x[0].Equals(y[0]) && x[1].Equals(y[1]);
	}

	public override Algebraic add(Algebraic x)
	{
		if (!(x is Zahl))
		{
			return x.add(this);
		}

		Exakt X = ((Zahl)x).exakt();

		return new Exakt(add(real, X.real), add(imag, X.imag));
	}

	public override Algebraic mult(Algebraic x)
	{
		if (!(x is Zahl))
		{
			return x.mult(this);
		}

		Exakt X = ((Zahl)x).exakt();

		return new Exakt(sub(mult(real,X.real), mult(imag,X.imag)), add(mult(imag,X.real), mult(real,X.imag)));
	}

	public override Algebraic div(Algebraic x)
	{
		if (!(x is Zahl))
		{
			return base.div(x);
		}

		Exakt X = ((Zahl)x).exakt();

		BigInteger[] N = add(mult(X.real,X.real),mult(X.imag,X.imag));

		if (N == null || N[0].Equals(BigInteger.ZERO))

		{
			throw new JasymcaException("Division by Zero.");
		}

		return new Exakt(div(add(mult(real,X.real), mult(imag,X.imag)), N), div(sub(mult(imag,X.real), mult(real,X.imag)), N));
	}

	private BigInteger lsm(BigInteger x, BigInteger y)
	{
		return x.multiply(y).divide(x.gcd(y));
	}

	public override Algebraic[] div(Algebraic q1, Algebraic[] result)
	{
		if (result == null)
		{
			result = new Algebraic[2];
		}

		if (!(q1 is Zahl))
		{
			result[0] = Zahl.ZERO;
			result[1] = this;

			return result;
		}

		Exakt q = ((Zahl)q1).exakt();

		if (!komplexq() && q.komplexq())
		{
			result[0] = Zahl.ZERO;
			result[1] = this;
			return result;
		}

		if (komplexq() && !q.komplexq())
		{
			result[0] = div(q);
			result[1] = Zahl.ZERO;

			return result;
		}

		if (komplexq() && q.komplexq())
		{
			result[0] = imagpart().div(q.imagpart());
			result[1] = sub(result[0].mult(q));

			return result;
		}

		if (integerq() && q.integerq())
		{
			BigInteger[] d = real[0].divideAndRemainder(q.real[0]);

			result[0] = new Exakt(d[0]);
			result[1] = new Exakt(d[1]);

			return result;
		}

		result[0] = div(q);
		result[1] = Zahl.ZERO;

		return result;
	}

	private string b2string(BigInteger[] x)
	{
		if (x[1].Equals(BigInteger.ONE))
		{
			return x[0].ToString();
		}

		return x[0].ToString() + "/" + x[1].ToString();
	}

	public override string ToString()
	{
		if (imag == null || imag[0].Equals(BigInteger.ZERO))
		{
			return "" + b2string(real);
		}

		if (real[0].Equals(BigInteger.ZERO))
		{
			return b2string(imag) + "*i";
		}

		return "(" + b2string(real) + (imag[0].compareTo(BigInteger.ZERO) > 0?"+":"") + b2string(imag) + "*i)";
	}

	public override bool integerq()
	{
		return real[1].Equals(BigInteger.ONE) && imag == null;
	}

	public override bool smaller(Zahl x)
	{
		return unexakt().smaller(x);
	}

	public override bool komplexq()
	{
		return imag != null && !imag[0].Equals(BigInteger.ZERO);
	}

	public override bool imagq()
	{
		return imag != null && !imag[0].Equals(BigInteger.ZERO) && real[0].Equals(BigInteger.ZERO);
	}

	public override bool Equals(object x)
	{
		if (x is Exakt)
		{
			return Equals(real,((Exakt)x).real) && Equals(imag,((Exakt)x).imag);
		}

		return tofloat().Equals(x);
	}

	public override double norm()
	{
		return tofloat().norm();
	}

	public override Algebraic rat()
	{
		return this;
	}

	public override Zahl abs()
	{
		if (komplexq())
		{
			return tofloat().abs();
		}

		BigInteger[] r = new BigInteger[2];

		r[0] = real[0].compareTo(BigInteger.ZERO) < 0?real[0].negate():real[0];
		r[1] = real[1];

		return new Exakt(r);
	}

	public virtual Exakt gcd(Exakt x)
	{
		if (Equals(Zahl.ZERO))
		{
			return x;
		}
		else if (x.Equals(Zahl.ZERO))
		{
			return this;
		}
		if (komplexq() && x.komplexq())
		{
			Exakt r = ((Exakt)realpart()).gcd((Exakt)x.realpart());
			Exakt i = ((Exakt)imagpart()).gcd((Exakt)x.imagpart());

			if (r.Equals(Zahl.ZERO))
			{
				return (Exakt)i.mult(Zahl.IONE);
			}

			if (realpart().div(r).Equals(imagpart().div(i)))
			{
				return (Exakt)r.add(i.mult(Zahl.IONE));
			}
			else
			{
				return Zahl.ONE.exakt();
			}
		}
		else if (komplexq() || x.komplexq())
		{
			return Zahl.ONE.exakt();
		}
		else
		{
			return (Exakt)(new Exakt(real[0].multiply(x.real[1]).gcd(real[1].multiply(x.real[0])))).div(new Exakt(real[1].multiply(x.real[1])));
		}
	}

	public override int intval()
	{
		return real[0].intValue();
	}
}

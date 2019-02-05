using System;
using System.Collections;

public class LambdaPRIMES : Lambda
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack st) throws ParseException,JasymcaException
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		Algebraic x = getAlgebraic(st);
		if (!(x is Zahl) && !((Zahl)x).integerq())
		{
			throw new JasymcaException("Expected integer argument.");
		}
		if (((Zahl)x).smaller(Zahl.ZERO))
		{
			x = x.mult(Zahl.MINUS);
		}
		Algebraic res = null;
		if (x is Unexakt)
		{
			long xl = (long)((Zahl)x).unexakt().real;
			res = teiler(xl);
		}
		else
		{
			BigInteger xb = ((Exakt)x).real[0];
			if (xb.compareTo(BigInteger.valueOf(long.MaxValue)) <= 0)
			{
				long xl = (long)xb.longValue();
				res = teiler(xl);
			}
			else
			{
				res = teiler(xb);
			}
		}
		if (res != null)
		{
			st.Push(res);
		}
		return 0;
	}
	internal static readonly int[] mod = new int[] {1, 7, 11, 13, 17, 19, 23, 29};
	internal static readonly int[] moddif = new int[] {1, 6, 4, 2, 4, 2, 4, 6};
	internal static long kleinsterTeiler(long X, long start)
	{
		long stop = (long)JMath.ceil(Math.Sqrt((double)X));
		if (start > stop)
		{
			return X;
		}
		long b = start / 30L;
		b *= 30L;
		long m = start % 30L;
		int i = 0;
		while (m > mod[i])
		{
			i++;
		}
		while (start <= stop)
		{
			if (pc.check_interrupt())
			{
				return -1L;
			}
			if (X % start == 0)
			{
				return start;
			}
			i++;
			if (i >= mod.Length)
			{
				i = 0;
				b += 30L;
				start = b;
			}
			start += moddif[i];
		}
		return X;
	}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static Vektor teiler(long X) throws JasymcaException
	internal static Vektor teiler(long X)
	{
		ArrayList teiler = new ArrayList();
		while (X % 2L == 0)
		{
			teiler.Add(Zahl.TWO);
			X /= 2L;
		} while (X % 3L == 0)
		{
			teiler.Add(Zahl.THREE);
			X /= 3L;
		} while (X % 5L == 0)
		{
			teiler.Add(new Unexakt(5.0));
			X /= 5L;
		}
		long f = 7L;
		while (X != 1L)
		{
			f = kleinsterTeiler(X, f);
			if (f < 0)
			{
				return null;
			}
			teiler.Add(new Exakt(f,1L));
			X /= f;
		}
		return Vektor.create(teiler);
	}
	static BigInteger kleinsterTeiler(BigInteger X, BigInteger start)
	{
		sbyte[] stop_in = new sbyte[X.bitLength() / 2 + 1];
		stop_in[0] = (sbyte)1;
		for (int n = 1; n < stop_in.Length; n++)
		{
			stop_in[n] = (sbyte)0;
		}
		BigInteger stop = new BigInteger(stop_in);
		if (start.compareTo(stop) > 0)
		{
			return X;
		}
		BigInteger b30 = BigInteger.valueOf(30L);
		BigInteger b = start.divide(b30);
		b = b.multiply(b30);
		int m = (int)start.mod(b30).intValue();
		int i = 0;
		while (m > mod[i])
		{
			i++;
		}
		while (start.compareTo(stop) <= 0)
		{
			if (pc.check_interrupt())
			{
				return null;
			}
			if (X.mod(start).Equals(BigInteger.ZERO))
			{
				return start;
			}
			i++;
			if (i >= mod.Length)
			{
				i = 0;
				b = b.add(b30);
				start = b;
			}
			start = start.add(BigInteger.valueOf((long)moddif[i]));
		}
		return X;
	}
	static Vektor teiler(BigInteger X) 
	{
		var teiler = new ArrayList();
		var b2 = BigInteger.valueOf(2L);

		while (X.mod(b2).Equals(BigInteger.ZERO))
		{
			teiler.Add(Zahl.TWO);

			X = X.divide(b2);
		}

		var b3 = BigInteger.valueOf(3L);

		while (X.mod(b3).Equals(BigInteger.ZERO))
		{
			teiler.Add(Zahl.THREE);

			X = X.divide(b3);
		}

		var b5 = BigInteger.valueOf(5L);

		while (X.mod(b5).Equals(BigInteger.ZERO))
		{
			teiler.Add(new Unexakt(5.0));

			X = X.divide(b5);
		}

		var f = BigInteger.valueOf(7L);

		while (!X.Equals(BigInteger.ONE))
		{
			f = kleinsterTeiler(X, f);

			if (f == null)
			{
				return null;
			}

			teiler.Add(new Exakt(f));

			X = X.divide(f);
		}

		return Vektor.create(teiler);

	    //throws JasymcaException();
	}
}

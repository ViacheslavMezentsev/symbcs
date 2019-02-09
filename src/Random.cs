using System;

public class Random
{
	private bool haveNextNextGaussian;
	private double nextNextGaussian;
	private long seed;
	private const long serialVersionUID = 3905348978240129619L;
	public Random() : this(DateTimeHelperClass.CurrentUnixTimeMillis())
	{
	}
	public Random(long seed)
	{
		Seed = seed;
	}
	public virtual long Seed
	{
		set
		{
			lock (this)
			{
				this.seed = (value ^ 0x5DEECE66DL) & ((1L << 48) - 1);
				haveNextNextGaussian = false;
			}
		}
	}
	protected internal virtual int next(int bits)
	{
		lock (this)
		{
			seed = (seed * 0x5DEECE66DL + 0xBL) & ((1L << 48) - 1);
			return (int)((long)((ulong)seed >> (48 - bits)));
		}
	}
	public virtual void nextBytes(sbyte[] bytes)
	{
		int random;
		int max = bytes.Length & ~0x3;
		for (int i = 0; i < max; i += 4)
		{
			random = next(32);
			bytes[i] = (sbyte) random;
			bytes[i + 1] = (sbyte)(random >> 8);
			bytes[i + 2] = (sbyte)(random >> 16);
			bytes[i + 3] = (sbyte)(random >> 24);
		}
		if (max < bytes.Length)
		{
			random = next(32);
			for (int j = max; j < bytes.Length; j++)
			{
				bytes[j] = (sbyte) random;
				random >>= 8;
			}
		}
	}
	public virtual int nextInt()
	{
		return next(32);
	}
	public virtual int nextInt(int n)
	{
		if (n <= 0)
		{
			throw new System.ArgumentException("n must be positive");
		}
		if ((n & -n) == n)
		{
			return (int)((n * (long) next(31)) >> 31);
		}
		int bits, val;
		do
		{
			bits = next(31);
			val = bits % n;
		} while (bits - val + (n - 1) < 0);
		return val;
	}
	public virtual long nextLong()
	{
		return ((long) next(32) << 32) + next(32);
	}
	public virtual bool nextBoolean()
	{
		return next(1) != 0;
	}
	public virtual float nextFloat()
	{
		return next(24) / (float)(1 << 24);
	}
	public virtual double nextDouble()
	{
		return (((long) next(26) << 27) + next(27)) / (double)(1L << 53);
	}
	public virtual double nextGaussian()
	{
		lock (this)
		{
			if (haveNextNextGaussian)
			{
				haveNextNextGaussian = false;
				return nextNextGaussian;
			}
			double v1, v2, s;
			do
			{
				v1 = 2 * nextDouble() - 1;
				v2 = 2 * nextDouble() - 1;
				s = v1 * v1 + v2 * v2;
			} while (s >= 1);
			double norm = Math.Sqrt(-2 * JMath.log(s) / s);
			nextNextGaussian = v2 * norm;
			haveNextNextGaussian = true;
			return v1 * norm;
		}
	}
}
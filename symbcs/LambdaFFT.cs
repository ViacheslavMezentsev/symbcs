using System;
using System.Collections;

internal class LambdaFFT : Lambda
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		Vektor x = getVektor(st);
		x = (Vektor)(new ExpandConstants()).f_exakt(x);
		double[] re = ((Vektor)x.realpart()).Double;
		double[] im = ((Vektor)x.imagpart()).Double;
		int n = re.Length;
		double power = JMath.log(n) / JMath.log(2.0);
		if (power != JMath.round(power))
		{
			double[] outRe = new double[n];
			double[] outIm = new double[n];
			dft(re,im,outRe,outIm);
			re = outRe;
			im = outIm;
		}
		else
		{
			ifft_1d(re,im,-1);
		}
		Unexakt[] a = new Unexakt[n];
		for (int i = 0; i < n; i++)
		{
			a[i] = new Unexakt(re[i],im[i]);
		}
		st.Push(new Vektor(a));
		return 0;
	}
	internal static void dft(double[] re, double[] im, double[] outRe, double[] outIm)
	{
		int N = re.Length;
		for (int k = 0;k < N;k++)
		{
			outRe[k] = outIm[k] = 0.0;
			for (int n = 0;n < N;n++)
			{
				double ang = -2.0 * Math.PI * k * n / N;
				double eim = Math.Sin(ang);
				double ere = Math.Cos(ang);
				outRe[k] += re[n] * ere - im[n] * eim;
				outIm[k] += re[n] * eim + im[n] * ere;
			}
		}
	}
	internal static void idft(double[] re, double[] im, double[] outRe, double[] outIm)
	{
		int N = re.Length;
		for (int k = 0;k < N;k++)
		{
			outRe[k] = outIm[k] = 0.0;
			for (int n = 0;n < N;n++)
			{
				double ang = 2.0 * Math.PI * k * n / N;
				double eim = Math.Sin(ang);
				double ere = Math.Cos(ang);
				outRe[k] += re[n] * ere - im[n] * eim;
				outIm[k] += re[n] * eim + im[n] * ere;
			}
			outRe[k] /= N;
			outIm[k] /= N;
		}
	}
	internal static void ifft_1d(double[] re, double[] im, int sign)
	{
		double u_r, u_i, w_r, w_i, t_r, t_i;
		int ln, nv2, k, l, le, le1, j, ip, i, n;
		n = re.Length;
		ln = (int)(JMath.log((double)n) / JMath.log(2) + 0.5);
		nv2 = n / 2;
		j = 1;
		for (i = 1; i < n; i++)
		{
			if (i < j)
			{
				t_r = re[i - 1];
				t_i = im[i - 1];
				re[i - 1] = re[j - 1];
				im[i - 1] = im[j - 1];
				re[j - 1] = t_r;
				im[j - 1] = t_i;
			}
			k = nv2;
			while (k < j)
			{
				j = j - k;
				k = k / 2;
			}
			j = j + k;
		}
		for (l = 1; l <= ln; l++)
		{
			le = (int)(JMath.exp((double)l * JMath.log(2)) + 0.5);
			le1 = le / 2;
			u_r = 1.0;
			u_i = 0.0;
			w_r = Math.Cos(Math.PI / (double)le1);
			w_i = sign * Math.Sin(Math.PI / (double)le1);
			for (j = 1; j <= le1; j++)
			{
				for (i = j; i <= n; i += le)
				{
					ip = i + le1;
					t_r = re[ip - 1] * u_r - u_i * im[ip - 1];
					t_i = im[ip - 1] * u_r + u_i * re[ip - 1];
					re[ip - 1] = re[i - 1] - t_r;
					im[ip - 1] = im[i - 1] - t_i;
					re[i - 1] = re[i - 1] + t_r;
					im[i - 1] = im[i - 1] + t_i;
				}
				t_r = u_r * w_r - w_i * u_i;
				u_i = w_r * u_i + w_i * u_r;
				u_r = t_r;
			}
		}
		if (sign > 0)
		{
			for (i = 0; i < n; i++)
			{
			re[i] /= n;
			im[i] /= n;
			}
		}
		return;
	}
}
internal class LambdaIFFT : Lambda
{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int lambda(Stack st) throws ParseException, JasymcaException
	public override int lambda(Stack st)
	{
		int narg = getNarg(st);
		Vektor x = getVektor(st);
		x = (Vektor)(new ExpandConstants()).f_exakt(x);
		double[] re = ((Vektor)x.realpart()).Double;
		double[] im = ((Vektor)x.imagpart()).Double;
		int n = re.Length;
		double power = JMath.log(n) / JMath.log(2.0);
		if (power != JMath.round(power))
		{
			double[] outRe = new double[n];
			double[] outIm = new double[n];
			LambdaFFT.idft(re,im,outRe,outIm);
			re = outRe;
			im = outIm;
		}
		else
		{
			LambdaFFT.ifft_1d(re,im,1);
		}
		Unexakt[] a = new Unexakt[n];
		for (int i = 0; i < n; i++)
		{
			a[i] = new Unexakt(re[i],im[i]);
		}
		st.Push(new Vektor(a));
		return 0;
	}
}
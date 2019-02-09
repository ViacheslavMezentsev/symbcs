public class MPN
{
	public static int add_1(int[] dest, int[] x, int size, int y)
	{
		long carry = (long) y & 0xffffffffL;
		for (int i = 0; i < size; i++)
		{
			carry += ((long) x[i] & 0xffffffffL);
			dest[i] = (int) carry;
			carry >>= 32;
		}
		return (int) carry;
	}
	public static int add_n(int[] dest, int[] x, int[] y, int len)
	{
		long carry = 0;
		for (int i = 0; i < len; i++)
		{
			carry += ((long) x[i] & 0xffffffffL) + ((long) y[i] & 0xffffffffL);
			dest[i] = (int) carry;
			carry = (long)((ulong)carry >> 32);
		}
		return (int) carry;
	}
	public static int sub_n(int[] dest, int[] X, int[] Y, int size)
	{
		int cy = 0;
		for (int i = 0; i < size; i++)
		{
			int y = Y[i];
			int x = X[i];
			y += cy;
			cy = (y ^ 0x80000000) < (cy ^ 0x80000000) ? 1 : 0;
			y = x - y;
			cy += (y ^ 0x80000000) > (x ^ 0x80000000) ? 1 : 0;
			dest[i] = y;
		}
		return cy;
	}
	public static int mul_1(int[] dest, int[] x, int len, int y)
	{
		long yword = (long) y & 0xffffffffL;
		long carry = 0;
		for (int j = 0; j < len; j++)
		{
			carry += ((long) x[j] & 0xffffffffL) * yword;
			dest[j] = (int) carry;
			carry = (long)((ulong)carry >> 32);
		}
		return (int) carry;
	}
	public static void mul(int[] dest, int[] x, int xlen, int[] y, int ylen)
	{
		dest[xlen] = MPN.mul_1(dest, x, xlen, y[0]);
		for (int i = 1; i < ylen; i++)
		{
			long yword = (long) y[i] & 0xffffffffL;
			long carry = 0;
			for (int j = 0; j < xlen; j++)
			{
				carry += ((long) x[j] & 0xffffffffL) * yword + ((long) dest[i + j] & 0xffffffffL);
				dest[i + j] = (int) carry;
				carry = (long)((ulong)carry >> 32);
			}
			dest[i + xlen] = (int) carry;
		}
	}
	public static long udiv_qrnnd(long N, int D)
	{
		long q, r;
		long a1 = (long)((ulong)N >> 32);
		long a0 = N & 0xffffffffL;
		if (D >= 0)
		{
			if (a1 < ((D - a1 - ((long)((ulong)a0 >> 31))) & 0xffffffffL))
			{
				q = N / D;
				r = N % D;
			}
			else
			{
				long c = N - ((long) D << 31);
				q = c / D;
				r = c % D;
				q += 1 << 31;
			}
		}
		else
		{
			long b1 = (int)((uint)D >> 1);
			long c = (long)((ulong)N >> 1);
			if (a1 < b1 || (a1 >> 1) < b1)
			{
				if (a1 < b1)
				{
					q = c / b1;
					r = c % b1;
				}
				else
				{
					c = ~(c - (b1 << 32));
					q = c / b1;
					r = c % b1;
					q = (~q) & 0xffffffffL;
					r = (b1 - 1) - r;
				}
				r = 2 * r + (a0 & 1);
				if ((D & 1) != 0)
				{
					if (r >= q)
					{
						r = r - q;
					}
					else if (q - r <= ((long) D & 0xffffffffL))
					{
						r = r - q + D;
						q -= 1;
					}
					else
					{
						r = r - q + D + D;
						q -= 2;
					}
				}
			}
			else
			{
				if (a0 >= ((long)(-D) & 0xffffffffL))
				{
					q = -1;
					r = a0 + D;
				}
				else
				{
					q = -2;
					r = a0 + D + D;
				}
			}
		}
		return (r << 32) | (q & 0xFFFFFFFFL);
	}
	public static int divmod_1(int[] quotient, int[] dividend, int len, int divisor)
	{
		int i = len - 1;
		long r = dividend[i];
		if ((r & 0xffffffffL) >= ((long)divisor & 0xffffffffL))
		{
			r = 0;
		}
		else
		{
			quotient[i--] = 0;
			r <<= 32;
		}
		for (; i >= 0; i--)
		{
			int n0 = dividend[i];
			r = (r & ~0xffffffffL) | (n0 & 0xffffffffL);
			r = udiv_qrnnd(r, divisor);
			quotient[i] = (int) r;
		}
		return (int)(r >> 32);
	}
	public static int submul_1(int[] dest, int offset, int[] x, int len, int y)
	{
		long yl = (long) y & 0xffffffffL;
		int carry = 0;
		int j = 0;
		do
		{
			long prod = ((long) x[j] & 0xffffffffL) * yl;
			int prod_low = (int) prod;
			int prod_high = (int)(prod >> 32);
			prod_low += carry;
			carry = ((prod_low ^ 0x80000000) < (carry ^ 0x80000000) ? 1 : 0) + prod_high;
			int x_j = dest[offset + j];
			prod_low = x_j - prod_low;
			if ((prod_low ^ 0x80000000) > (x_j ^ 0x80000000))
			{
				carry++;
			}
			dest[offset + j] = prod_low;
		} while (++j < len);
		return carry;
	}
	public static void divide(int[] zds, int nx, int[] y, int ny)
	{
		int j = nx;
		do
		{
			int qhat;
			if (zds[j] == y[ny - 1])
			{
				qhat = -1;
			}
			else
			{
				long w = (((long)(zds[j])) << 32) + ((long)zds[j - 1] & 0xffffffffL);
				qhat = (int) udiv_qrnnd(w, y[ny - 1]);
			}
			if (qhat != 0)
			{
				int borrow = submul_1(zds, j - ny, y, ny, qhat);
				int save = zds[j];
				long num = ((long)save & 0xffffffffL) - ((long)borrow & 0xffffffffL);
				while (num != 0)
				{
					qhat--;
					long carry = 0;
					for (int i = 0; i < ny; i++)
					{
						carry += ((long) zds[j - ny + i] & 0xffffffffL) + ((long) y[i] & 0xffffffffL);
						zds[j - ny + i] = (int) carry;
						carry = (long)((ulong)carry >> 32);
					}
					zds[j] += (int)carry;
					num = carry - 1;
				}
			}
			zds[j] = qhat;
		} while (--j >= ny);
	}
	public static int chars_per_word(int radix)
	{
		if (radix < 10)
		{
			if (radix < 8)
			{
				if (radix <= 2)
				{
					return 32;
				}
				else if (radix == 3)
				{
					return 20;
				}
				else if (radix == 4)
				{
					return 16;
				}
				else
				{
					return 18 - radix;
				}
			}
			else
			{
				return 10;
			}
		}
		else if (radix < 12)
		{
			return 9;
		}
		else if (radix <= 16)
		{
			return 8;
		}
		else if (radix <= 23)
		{
			return 7;
		}
		else if (radix <= 40)
		{
			return 6;
		}
		else if (radix <= 256)
		{
			return 4;
		}
		else
		{
			return 1;
		}
	}
	public static int count_leading_zeros(int i)
	{
		if (i == 0)
		{
			return 32;
		}
		int count = 0;
		for (int k = 16; k > 0; k = k >> 1)
		{
			int j = (int)((uint)i >> k);
			if (j == 0)
			{
				count += k;
			}
			else
			{
				i = j;
			}
		}
		return count;
	}
	public static int set_str(int[] dest, sbyte[] str, int str_len, int @base)
	{
		int size = 0;
		if ((@base & (@base - 1)) == 0)
		{
			int next_bitpos = 0;
			int bits_per_indigit = 0;
			for (int i = @base; (i >>= 1) != 0;)
			{
				bits_per_indigit++;
			}
			int res_digit = 0;
			for (int i = str_len; --i >= 0;)
			{
				int inp_digit = str[i];
				res_digit |= inp_digit << next_bitpos;
				next_bitpos += bits_per_indigit;
				if (next_bitpos >= 32)
				{
					dest[size++] = res_digit;
					next_bitpos -= 32;
					res_digit = inp_digit >> (bits_per_indigit - next_bitpos);
				}
			}
			if (res_digit != 0)
			{
				dest[size++] = res_digit;
			}
		}
		else
		{
			int indigits_per_limb = MPN.chars_per_word(@base);
			int str_pos = 0;
			while (str_pos < str_len)
			{
				int chunk = str_len - str_pos;
				if (chunk > indigits_per_limb)
				{
					chunk = indigits_per_limb;
				}
				int res_digit = str[str_pos++];
				int big_base = @base;
				while (--chunk > 0)
				{
					res_digit = res_digit * @base + str[str_pos++];
					big_base *= @base;
				}
				int cy_limb;
				if (size == 0)
				{
					cy_limb = res_digit;
				}
				else
				{
					cy_limb = MPN.mul_1(dest, dest, size, big_base);
					cy_limb += MPN.add_1(dest, dest, size, res_digit);
				}
				if (cy_limb != 0)
				{
					dest[size++] = cy_limb;
				}
			}
		}
		return size;
	}
	public static int cmp(int[] x, int[] y, int size)
	{
		while (--size >= 0)
		{
			int x_word = x[size];
			int y_word = y[size];
			if (x_word != y_word)
			{
				return (x_word ^ 0x80000000) > (y_word ^ 0x80000000) ? 1 : -1;
			}
		}
		return 0;
	}
	public static int cmp(int[] x, int xlen, int[] y, int ylen)
	{
		return xlen > ylen ? 1 : xlen < ylen ? - 1 : cmp(x, y, xlen);
	}
	public static int rshift(int[] dest, int[] x, int x_start, int len, int count)
	{
		int count_2 = 32 - count;
		int low_word = x[x_start];
		int retval = low_word << count_2;
		int i = 1;
		for (; i < len; i++)
		{
			int high_word = x[x_start + i];
			dest[i - 1] = ((int)((uint)low_word >> count)) | (high_word << count_2);
			low_word = high_word;
		}
		dest[i - 1] = (int)((uint)low_word >> count);
		return retval;
	}
	public static void rshift0(int[] dest, int[] x, int x_start, int len, int count)
	{
		if (count > 0)
		{
			rshift(dest, x, x_start, len, count);
		}
		else
		{
			for (int i = 0; i < len; i++)
			{
				dest[i] = x[i + x_start];
			}
		}
	}
	public static long rshift_long(int[] x, int len, int count)
	{
		int wordno = count >> 5;
		count &= 31;
		int sign = x[len - 1] < 0 ? - 1 : 0;
		int w0 = wordno >= len ? sign : x[wordno];
		wordno++;
		int w1 = wordno >= len ? sign : x[wordno];
		if (count != 0)
		{
			wordno++;
			int w2 = wordno >= len ? sign : x[wordno];
			w0 = ((int)((uint)w0 >> count)) | (w1 << (32 - count));
			w1 = ((int)((uint)w1 >> count)) | (w2 << (32 - count));
		}
		return ((long)w1 << 32) | ((long)w0 & 0xffffffffL);
	}
	public static int lshift(int[] dest, int d_offset, int[] x, int len, int count)
	{
		int count_2 = 32 - count;
		int i = len - 1;
		int high_word = x[i];
		int retval = (int)((uint)high_word >> count_2);
		d_offset++;
		while (--i >= 0)
		{
			int low_word = x[i];
			dest[d_offset + i] = (high_word << count) | ((int)((uint)low_word >> count_2));
			high_word = low_word;
		}
		dest[d_offset + i] = high_word << count;
		return retval;
	}
	public static int findLowestBit(int word)
	{
		int i = 0;
		while ((word & 0xF) == 0)
		{
			word >>= 4;
			i += 4;
		}
		if ((word & 3) == 0)
		{
			word >>= 2;
			i += 2;
		}
		if ((word & 1) == 0)
		{
			i += 1;
		}
		return i;
	}
	public static int findLowestBit(int[] words)
	{
		for (int i = 0; ; i++)
		{
			if (words[i] != 0)
			{
				return 32 * i + findLowestBit(words[i]);
			}
		}
	}
	public static int gcd(int[] x, int[] y, int len)
	{
		int i, word;
		for (i = 0; ; i++)
		{
			word = x[i] | y[i];
			if (word != 0)
			{
				break;
			}
		}
		int initShiftWords = i;
		int initShiftBits = findLowestBit(word);
		len -= initShiftWords;
		MPN.rshift0(x, x, initShiftWords, len, initShiftBits);
		MPN.rshift0(y, y, initShiftWords, len, initShiftBits);
		int[] odd_arg;
		int[] other_arg;
		if ((x[0] & 1) != 0)
		{
			odd_arg = x;
			other_arg = y;
		}
		else
		{
			odd_arg = y;
			other_arg = x;
		}
		for (;;)
		{
			for (i = 0; other_arg[i] == 0;)
			{
				i++;
			}
			if (i > 0)
			{
				int j;
				for (j = 0; j < len - i; j++)
				{
					other_arg[j] = other_arg[j + i];
				}
				for (; j < len; j++)
				{
					other_arg[j] = 0;
				}
			}
			i = findLowestBit(other_arg[0]);
			if (i > 0)
			{
				MPN.rshift(other_arg, other_arg, 0, len, i);
			}
			i = MPN.cmp(odd_arg, other_arg, len);
			if (i == 0)
			{
				break;
			}
			if (i > 0)
			{
				MPN.sub_n(odd_arg, odd_arg, other_arg, len);
				int[] tmp = odd_arg;
				odd_arg = other_arg;
				other_arg = tmp;
			}
			else
			{
				MPN.sub_n(other_arg, other_arg, odd_arg, len);
			} while (odd_arg[len - 1] == 0 && other_arg[len - 1] == 0) len--;
		}
		if (initShiftWords + initShiftBits > 0)
		{
			if (initShiftBits > 0)
			{
				int sh_out = MPN.lshift(x, initShiftWords, x, len, initShiftBits);
				if (sh_out != 0)
				{
					x[(len++) + initShiftWords] = sh_out;
				}
			}
			else
			{
				for (i = len; --i >= 0;)
				{
					x[i + initShiftWords] = x[i];
				}
			}
			for (i = initShiftWords; --i >= 0;)
			{
				x[i] = 0;
			}
			len += initShiftWords;
		}
		return len;
	}
	public static int intLength(int i)
	{
		return 32 - count_leading_zeros(i < 0 ?~i : i);
	}
	public static int intLength(int[] words, int len)
	{
		len--;
		return intLength(words[len]) + 32 * len;
	}
}
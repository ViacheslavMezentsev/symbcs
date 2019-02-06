using System;
using System.Collections;

public class Matrix : Algebraic
{
	private Algebraic[][] a;
	public Matrix(Algebraic[][] a)
	{
		this.a = a;
	}
	public Matrix(Algebraic x, int nrow, int ncol)
	{
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: this.a = new Algebraic[nrow][ncol];
		this.a = RectangularArrays.ReturnRectangularAlgebraicArray(nrow, ncol);
		for (int i = 0; i < nrow; i++)
		{
			for (int k = 0; k < ncol; k++)
			{
				a[i][k] = x;
			}
		}
	}
	public Matrix(int nrow, int ncol) : this(Zahl.ZERO, nrow, ncol)
	{
	}
	public Matrix(double[][] b, int nr, int nc)
	{
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: a = new Algebraic[nr][nc];
		a = RectangularArrays.ReturnRectangularAlgebraicArray(nr, nc);
		nr = Math.Min(nr,b.Length);
		nc = Math.Min(nc,b[0].Length);
		for (int i = 0; i < nr; i++)
		{
			for (int k = 0; k < nc; k++)
			{
			a[i][k] = new Unexakt(b[i][k]);
			}
		}
	}
	public Matrix(double[][] b) : this(b, b.Length, b[0].Length)
	{
	}
	public Matrix(Algebraic x)
	{
		if (x == null)
		{
			this.a = new Algebraic[][]
			{
				new Algebraic[] {Zahl.ZERO}
			};
		}
		else if (x is Vektor)
		{
			this.a = new Algebraic[][] {((Vektor)x).get()};
		}
		else if (x is Matrix)
		{
			this.a = ((Matrix)x).a;
		}
		else
		{
			this.a = new Algebraic[][]
			{
				new Algebraic[] {x}
			};
		}
	}
	public virtual Algebraic get(int i, int k)
	{
		if (i < 0 || i >= a.Length || k < 0 || k >= a[0].Length)
		{
			throw new JasymcaException("Index out of bounds.");
		}
		return a[i][k];
	}
	public virtual void set(int i, int k, Algebraic x)
	{
		if (i < 0 || i >= a.Length || k < 0 || k >= a[0].Length)
		{
			throw new JasymcaException("Index out of bounds.");
		}
		a[i][k] = x;
	}
	public virtual int nrow()
	{
		return a.Length;
	}
	public virtual int ncol()
	{
		return a[0].Length;
	}
	public virtual double[][] getDouble(int nr, int nc)
	{
		if (nr == 0)
		{
			nr = a.Length;
		}
		if (nc == 0)
		{
			nc = a[0].Length;
		}
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: double[][] b = new double[nr][nc];
		double[][] b = RectangularArrays.ReturnRectangularDoubleArray(nr, nc);
		nr = Math.Min(nr,a.Length);
		nc = Math.Min(nc,a[0].Length);
		for (int i = 0; i < nr; i++)
		{
			for (int k = 0; k < nc; k++)
			{
				Algebraic x = a[i][k];
				if (!(x is Unexakt) || x.komplexq())
				{
					throw new JasymcaException("Not a real, double Matrix");
				}
				b[i][k] = ((Unexakt)x).real;
			}
		}
		return b;
	}
	public virtual double[][] Double
	{
		get
		{
			return getDouble(0,0);
		}
	}
	public virtual Algebraic col(int k)
	{
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: Algebraic[][] c = new Algebraic[a.Length][1];
		Algebraic[][] c = RectangularArrays.ReturnRectangularAlgebraicArray(a.Length, 1);
		for (int i = 0; i < a.Length; i++)
		{
			c[i][0] = a[i][k - 1];
		}
		return (new Matrix(c)).reduce();
	}
	public virtual Algebraic row(int k)
	{
		Algebraic[] c = new Algebraic[a[0].Length];
		for (int i = 0; i < a[0].Length; i++)
		{
			c[i] = a[k - 1][i];
		}
		return (new Vektor(c)).reduce();
	}
	public virtual void insert(Matrix x, Index idx)
	{
		if (idx.row_max > nrow() || idx.col_max > ncol())
		{
			Matrix e = new Matrix(Math.Max(idx.row_max,nrow()), Math.Max(idx.col_max,ncol()));
			for (int i = 0; i < nrow(); i++)
			{
				for (int k = 0; k < ncol(); k++)
				{
					e.a[i][k] = a[i][k];
				}
			}
				a = e.a;
		}
		if (x.nrow() == 1 && x.ncol() == 1)
		{
			for (int i = 0; i < idx.row.Length; i++)
			{
				for (int k = 0; k < idx.col.Length; k++)
				{
					a[idx.row[i] - 1][idx.col[k] - 1] = x.a[0][0];
				}
			}
				return;
		}
		if (x.nrow() == idx.row.Length && x.ncol() == idx.col.Length)
		{
			for (int i = 0; i < idx.row.Length; i++)
			{
				for (int k = 0; k < idx.col.Length; k++)
				{
					a[idx.row[i] - 1][idx.col[k] - 1] = x.a[i][k];
				}
			}
				return;
		}
		throw new JasymcaException("Wrong index dimension.");
	}
	public virtual Matrix extract(Index idx)
	{
		if (idx.row_max > nrow() || idx.col_max > ncol())
		{
			throw new JasymcaException("Index out of range.");
		}
		Matrix x = new Matrix(idx.row.Length, idx.col.Length);
		for (int i = 0; i < idx.row.Length; i++)
		{
			for (int k = 0; k < idx.col.Length; k++)
			{
				x.a[i][k] = a[idx.row[i] - 1][idx.col[k] - 1];
			}
		}
			return x;
	}
	public static Matrix column(Vektor x)
	{
		return (new Matrix(x)).transpose();
	}
	public static Matrix row(Vektor x)
	{
		return new Matrix(x);
	}
	public override Algebraic cc()
	{
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: Algebraic[][] b = new Algebraic[a.Length][a[0].Length];
		Algebraic[][] b = RectangularArrays.ReturnRectangularAlgebraicArray(a.Length, a[0].Length);
		for (int i = 0; i < a.Length; i++)
		{
			for (int k = 0; k < a[0].Length; k++)
			{
				b[i][k] = a[i][k].cc();
			}
		}
			return new Matrix(b);
	}
	public override Algebraic add(Algebraic x)
	{
		if (x.scalarq())
		{
			x = x.promote(this);
		}
		if (x is Matrix && equalsized((Matrix)x))
		{
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: Algebraic[][] b = new Algebraic[a.Length][a[0].Length];
			Algebraic[][] b = RectangularArrays.ReturnRectangularAlgebraicArray(a.Length, a[0].Length);
			for (int i = 0; i < a.Length; i++)
			{
				for (int k = 0; k < a[0].Length; k++)
				{
					b[i][k] = a[i][k].add(((Matrix)x).a[i][k]);
				}
			}
				return new Matrix(b);
		}
		throw new JasymcaException("Wrong arguments for add:" + this + "," + x);
	}
	public override bool scalarq()
	{
		return false;
	}
	public virtual bool equalsized(Matrix x)
	{
		return nrow() == x.nrow() && ncol() == x.ncol();
	}
	public override Algebraic mult(Algebraic x)
	{
		if (x.scalarq())
		{
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: Algebraic[][] b = new Algebraic[a.Length][a[0].Length];
			Algebraic[][] b = RectangularArrays.ReturnRectangularAlgebraicArray(a.Length, a[0].Length);
			for (int i = 0; i < a.Length; i++)
			{
				for (int k = 0; k < a[0].Length; k++)
				{
				b[i][k] = a[i][k].mult(x);
				}
			}
			return new Matrix(b);
		}
		Matrix xm = new Matrix(x);
		if (ncol() != xm.nrow())
		{
			throw new JasymcaException("Matrix dimensions wrong.");
		}
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: Algebraic[][] b = new Algebraic[a.Length][xm.a[0].Length];
		Algebraic[][] b1 = RectangularArrays.ReturnRectangularAlgebraicArray(a.Length, xm.a[0].Length);
		for (int i = 0; i < a.Length; i++)
		{
			for (int k = 0; k < xm.a[0].Length; k++)
			{
			b1[i][k] = a[i][0].mult(xm.a[0][k]);
			for (int l = 1; l < xm.a.Length; l++)
			{
				b1[i][k] = b1[i][k].add(a[i][l].mult(xm.a[l][k]));
			}
			}
		}
		return new Matrix(b1);
	}
	public override Algebraic div(Algebraic x)
	{
		if (x.scalarq())
		{
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: Algebraic[][] b = new Algebraic[a.Length][a[0].Length];
			Algebraic[][] b = RectangularArrays.ReturnRectangularAlgebraicArray(a.Length, a[0].Length);
			for (int i = 0; i < a.Length; i++)
			{
				for (int k = 0; k < a[0].Length; k++)
				{
				b[i][k] = a[i][k].div(x);
				}
			}
			return new Matrix(b);
		}
		return mult((new Matrix(x)).pseudoinverse());
	}
	public static Matrix eye(int nr, int nc)
	{
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: Algebraic[][] b = new Algebraic[nr][nc];
		Algebraic[][] b = RectangularArrays.ReturnRectangularAlgebraicArray(nr, nc);
		for (int i = 0; i < nr; i++)
		{
			for (int k = 0; k < nc; k++)
			{
				b[i][k] = (i == k ? Zahl.ONE : Zahl.ZERO);
			}
		}
			return new Matrix(b);
	}
	public virtual Algebraic mpow(int n)
	{
		if (n == 0)
		{
			return Matrix.eye(a.Length, a[0].Length);
		}
		if (n == 1)
		{
			return this;
		}
		if (n > 1)
		{
			return pow_n(n);
		}
		;
		return (new Matrix(mpow(-n))).invert();
	}
	public override Algebraic reduce()
	{
		if (a.Length == 1)
		{
			return (new Vektor(a[0])).reduce();
		}
		else
		{
			return this;
		}
	}
	public override Algebraic deriv(Variable @var)
	{
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: Algebraic[][] b = new Algebraic[nrow()][ncol()];
		Algebraic[][] b = RectangularArrays.ReturnRectangularAlgebraicArray(nrow(), ncol());
		for (int i = 0; i < a.Length; i++)
		{
			for (int k = 0; k < a[0].Length; k++)
			{
				b[i][k] = a[i][k].deriv(@var);
			}
		}
			return new Matrix(b);
	}
	public override Algebraic integrate(Variable @var)
	{
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: Algebraic[][] b = new Algebraic[nrow()][ncol()];
		Algebraic[][] b = RectangularArrays.ReturnRectangularAlgebraicArray(nrow(), ncol());
		for (int i = 0; i < a.Length; i++)
		{
			for (int k = 0; k < a[0].Length; k++)
			{
				b[i][k] = a[i][k].integrate(@var);
			}
		}
			return new Matrix(b);
	}
	public override double norm()
	{
		double n = 0.0;
		for (int i = 0; i < a.Length; i++)
		{
			for (int k = 0; k < a[0].Length; k++)
			{
				n += a[i][k].norm();
			}
		}
			return n;
	}
	public override bool constantq()
	{
		for (int i = 0; i < a.Length; i++)
		{
			for (int k = 0; k < a[0].Length; k++)
			{
				if (!a[i][k].constantq())
				{
					return false;
				}
			}
		}
				return true;
	}
	public override bool Equals(object x)
	{
		if (!(x is Matrix) || !equalsized((Matrix)x))
		{
			return false;
		}
		for (int i = 0; i < a.Length; i++)
		{
			for (int k = 0; k < a[0].Length; k++)
			{
				if (!a[i][k].Equals(((Matrix)x).a[i][k]))
				{
					return false;
				}
			}
		}
				return true;
	}
	public override Algebraic map_lambda(LambdaAlgebraic f, Algebraic arg2)
	{
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: Algebraic[][] b = new Algebraic[a.Length][a[0].Length];
		Algebraic[][] b = RectangularArrays.ReturnRectangularAlgebraicArray(a.Length, a[0].Length);
		if (arg2 is Matrix && equalsized((Matrix)arg2))
		{
			for (int i = 0; i < a.Length; i++)
			{
				for (int k = 0; k < a[0].Length; k++)
				{
					Algebraic c = ((Matrix)arg2).get(i,k);
					object r = a[i][k].map_lambda(f, c);
					if (r is Algebraic)
					{
						b[i][k] = (Algebraic)r;
					}
					else
					{
						throw new JasymcaException("Cannot evaluate function to algebraic.");
					}
				}
			}
		}
		else
		{
			for (int i = 0; i < a.Length; i++)
			{
				for (int k = 0; k < a[0].Length; k++)
				{
					object r = a[i][k].map_lambda(f, arg2);
					if (r is Algebraic)
					{
						b[i][k] = (Algebraic)r;
					}
					else
					{
						throw new JasymcaException("Cannot evaluate function to algebraic.");
					}
				}
			}
		}
		return new Matrix(b);
	}
	public override Algebraic value(Variable @var, Algebraic x)
	{
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: Algebraic[][] b = new Algebraic[a.Length][a[0].Length];
		Algebraic[][] b = RectangularArrays.ReturnRectangularAlgebraicArray(a.Length, a[0].Length);
		for (int i = 0; i < a.Length; i++)
		{
			for (int k = 0; k < a[0].Length; k++)
			{
				b[i][k] = a[i][k].value(@var,x);
			}
		}
			return new Matrix(b);
	}
	public override string ToString()
	{
		int max = 0;
		string r = "";
		for (int i = 0; i < a.Length; i++)
		{
			for (int k = 0; k < a[0].Length; k++)
			{
				int l = StringFmt.compact(a[i][k].ToString()).Length;
				if (l > max)
				{
					max = l;
				}
			}
		}
		max += 2;
		for (int i = 0; i < a.Length; i++)
		{
			r += "\n  ";
			for (int k = 0; k < a[0].Length; k++)
			{
				string c = StringFmt.compact(a[i][k].ToString());
				r += c;
				for (int m = 0; m < max - c.Length; m++)
				{
					r += " ";
				}
			}
		}
		return r;
	}
	public override void print(PrintStream p)
	{
		int max = 0;
		for (int i = 0; i < a.Length; i++)
		{
			for (int k = 0; k < a[0].Length; k++)
			{
				int l = StringFmt.compact(a[i][k].ToString()).Length;
				if (l > max)
				{
					max = l;
				}
			}
		}
		max += 2;
		for (int i = 0; i < a.Length; i++)
		{
			p.print("\n  ");
			for (int k = 0; k < a[0].Length; k++)
			{
				string r = StringFmt.compact(a[i][k].ToString());
				p.print(r);
				for (int m = 0; m < max - r.Length; m++)
				{
					p.print(" ");
				}
			}
		}
	}
	public override Algebraic map(LambdaAlgebraic f)
	{
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: Algebraic[][] cn = new Algebraic[a.Length][a[0].Length];
		Algebraic[][] cn = RectangularArrays.ReturnRectangularAlgebraicArray(a.Length, a[0].Length);
		for (int i = 0; i < a.Length; i++)
		{
			for (int k = 0; k < a[0].Length; k++)
			{
				cn[i][k] = f.f_exakt(a[i][k]);
			}
		}
			return new Matrix(cn);
	}
	public virtual Matrix transpose()
	{
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: Algebraic[][] b = new Algebraic[a[0].Length][a.Length];
		Algebraic[][] b = RectangularArrays.ReturnRectangularAlgebraicArray(a[0].Length, a.Length);
		for (int i = 0; i < a.Length; i++)
		{
			for (int k = 0; k < a[0].Length; k++)
			{
				b[k][i] = a[i][k];
			}
		}
			return new Matrix(b);
	}
	public virtual Matrix adjunkt()
	{
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: Algebraic[][] b = new Algebraic[a[0].Length][a.Length];
		Algebraic[][] b = RectangularArrays.ReturnRectangularAlgebraicArray(a[0].Length, a.Length);
		for (int i = 0; i < a.Length; i++)
		{
			for (int k = 0; k < a[0].Length; k++)
			{
				b[k][i] = a[i][k].cc();
			}
		}
			return new Matrix(b);
	}
	public virtual Matrix invert()
	{
		Algebraic _det = det();
		if (_det.Equals(Zahl.ZERO))
		{
			throw new JasymcaException("Matrix not invertible.");
		}
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: Algebraic[][] b = new Algebraic[a.Length][a.Length];
		Algebraic[][] b = RectangularArrays.ReturnRectangularAlgebraicArray(a.Length, a.Length);
		if (a.Length == 1)
		{
			b[0][0] = Zahl.ONE.div(_det);
		}
		else
		{
			for (int i = 0; i < a.Length; i++)
			{
				for (int k = 0; k < a[0].Length; k++)
				{
					b[i][k] = unterdet(k,i).div(_det);
				}
			}
		}
		return new Matrix(b);
	}
	public virtual Algebraic min()
	{
		Algebraic[] r = new Algebraic[ncol()];
		for (int i = 0; i < ncol(); i++)
		{
			Algebraic min = a[0][i];
			if (!(min is Zahl))
			{
				throw new JasymcaException("MIN requires constant arguments.");
			}
			for (int k = 1; k < nrow(); k++)
			{
				Algebraic x = a[k][i];
				if (!(x is Zahl))
				{
					throw new JasymcaException("MIN requires constant arguments.");
				}
				if (((Zahl)x).smaller((Zahl)min))
				{
					min = x;
				}
			}
			r[i] = min;
		}
		return (new Vektor(r)).reduce();
	}
	public virtual Algebraic max()
	{
		Algebraic[] r = new Algebraic[ncol()];
		for (int i = 0; i < ncol(); i++)
		{
			Algebraic max = a[0][i];
			if (!(max is Zahl))
			{
				throw new JasymcaException("MAX requires constant arguments.");
			}
			for (int k = 1; k < nrow(); k++)
			{
				Algebraic x = a[k][i];
				if (!(x is Zahl))
				{
					throw new JasymcaException("MAX requires constant arguments.");
				}
				if (((Zahl)max).smaller((Zahl)x))
				{
					max = x;
				}
			}
			r[i] = max;
		}
		return (new Vektor(r)).reduce();
	}
	public virtual Algebraic find()
	{
		ArrayList v = new ArrayList();
		for (int i = 0; i < nrow(); i++)
		{
			for (int k = 0; k < ncol(); k++)
			{
				if (!Zahl.ZERO.Equals(a[i][k]))
				{
					v.Add(new Unexakt(i * nrow() + k + 1.0));
				}
			}
		}
		Vektor vx = Vektor.create(v);
		if (nrow() == 1)
		{
			return vx;
		}
		return column(vx);
	}
	public virtual Polynomial charpoly(Variable x)
	{
		Polynomial p = new Polynomial(x);
		Matrix m = (Matrix)(sub(Matrix.eye(a.Length,a[0].Length).mult(p)));
		p = (Polynomial)(m.det2());
		p = (Polynomial)p.rat();
		return p;
	}
	public virtual Vektor eigenvalues()
	{
		Variable x = SimpleVariable.top;
		Polynomial p = charpoly(x);
		Algebraic[] ps = p.square_free_dec(p.v);
		Vektor r;
		ArrayList v = new ArrayList();
		for (int i = 0; i < ps.Length; i++)
		{
			if (ps[i] is Polynomial)
			{
				r = ((Polynomial)ps[i]).monic().roots();
				for (int k = 0; r != null && k < r.length() ; k++)
				{
					for (int j = 0; j <= i; j++)
					{
						v.Add(r.get(k));
					}
				}
			}
		}
		return Vektor.create(v);
	}
	public virtual Algebraic det()
	{
		if (a.Length != a[0].Length)
		{
			return Zahl.ZERO;
		}
		switch (a.Length)
		{
			case 1:
				return a[0][0];
			case 2:
				return a[0][0].mult(a[1][1]).sub(a[0][1].mult(a[1][0]));
			case 3:
				return a[0][0].mult(a[1][1]).mult(a[2][2]).add(a[0][1].mult(a[1][2]).mult(a[2][0])).add(a[0][2].mult(a[1][0]).mult(a[2][1])).sub(a[0][2].mult(a[1][1]).mult(a[2][0])).sub(a[0][0].mult(a[1][2]).mult(a[2][1])).sub(a[0][1].mult(a[1][0]).mult(a[2][2]));
			default:
				Matrix c = copy();
				int perm = c.rank_decompose(null,null);
				Algebraic r = c.get(0,0);
				for (int i = 1; i < c.nrow(); i++)
				{
					r = r.mult(c.get(i,i));
				}
				return (perm % 2 == 0 ? r : r.mult(Zahl.MINUS));
		}
	}
	internal virtual Algebraic det2()
	{
		if (a.Length != a[0].Length)
		{
			return Zahl.ZERO;
		}
		if (a.Length < 4)
		{
			return det();
		}
		Algebraic d = unterdet(0,0).mult(a[0][0]);
		for (int i = 1; i < a.Length; i++)
		{
			d = d.add(unterdet(i,0).mult(a[i][0]));
		}
		return d;
	}
	public virtual Algebraic unterdet(int i, int k)
	{
		if (i < 0 || i>a.Length || k < 0 || k>a[0].Length)
		{
			throw new JasymcaException("Operation not possible.");
		}
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: Algebraic[][] b = new Algebraic[a.Length-1][a[0].Length-1];
		Algebraic[][] b = RectangularArrays.ReturnRectangularAlgebraicArray(a.Length - 1, a[0].Length - 1);
		int i1, i2, k1, k2;
		for (i1 = 0,i2 = 0; i1 < a.Length - 1; i1++,i2++)
		{
			if (i2 == i)
			{
				i2++;
			}
			for (k1 = 0,k2 = 0; k1 < a[0].Length - 1; k1++,k2++)
			{
				if (k2 == k)
				{
					k2++;
				}
				b[i1][k1] = this.a[i2][k2];
			}
		}
		Algebraic u = (new Matrix(b)).det2();
		if ((i + k) % 2 == 0)
		{
			return u;
		}
		return u.mult(Zahl.MINUS);
	}
	internal virtual int pivot(int k)
	{
		if (k >= ncol())
		{
			return k;
		}
		int _pivot = k;
		double maxa = a[k][k].norm();
		for (int i = k + 1; i < nrow(); i++)
		{
			double dummy = a[i][k].norm();
			if (dummy > maxa)
			{
				maxa = dummy;
				_pivot = i;
			}
		}
		if (maxa == 0.0)
		{
			int kn = pivot(k + 1);
			if (kn == k + 1)
			{
				return k;
			}
			else
			{
				return kn;
			}
		}
		if (_pivot != k)
		{
			for (int j = k;j < ncol();j++)
			{
				Algebraic dummy = a[_pivot][j];
				a[_pivot][j] = a[k][j];
				a[k][j] = dummy;
			}
		}
		return _pivot;
	}
	private bool row_zero(int k)
	{
		if (k >= nrow())
		{
			return true;
		}
		for (int i = 0; i < ncol(); i++)
		{
			if (a[k][i] != Zahl.ZERO)
			{
				return false;
			}
		}
		return true;
	}
	public override bool exaktq()
	{
		bool exakt = true;
		for (int i = 0; i < a.Length; i++)
		{
			for (int k = 0; k < a[0].Length; k++)
			{
				exakt = exakt && a[i][k].exaktq();
			}
		}
			return exakt;
	}
	private void remove_row(int i)
	{
		if (i >= nrow())
		{
			return;
		}
		Algebraic[][] b = new Algebraic[nrow() - 1][];
		for (int k = 0; k < i; k++)
		{
			b[k] = a[k];
		}
		for (int k = i + 1; k < nrow(); k++)
		{
			b[k - 1] = a[k];
		}
		a = b;
	}
	internal virtual void remove_col(int i)
	{
		if (i >= ncol())
		{
			return;
		}
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: Algebraic[][] b = new Algebraic[nrow()][ncol()-1];
		Algebraic[][] b = RectangularArrays.ReturnRectangularAlgebraicArray(nrow(), ncol() - 1);
		for (int j = 0; j < nrow(); j++)
		{
			for (int k = 0; k < i; k++)
			{
				b[j][k] = a[j][k];
			}
			for (int k = i + 1; k < ncol(); k++)
			{
				b[j][k - 1] = a[j][k];
			}
		}
		a = b;
	}
	internal static Matrix elementary(int n, int i, int k, Algebraic m)
	{
		Matrix t = eye(n,n);
		t.a[i][k] = m;
		return t;
	}
	internal static Matrix elementary(int n, int i, int k)
	{
		Matrix t = eye(n,n);
		t.a[k][k] = t.a[i][i] = Zahl.ZERO;
		t.a[i][k] = t.a[k][i] = Zahl.ONE;
		return t;
	}
	public virtual int rank_decompose(Matrix B, Matrix P)
	{
		int m = nrow(), n = ncol(), perm = 0;
		Matrix C = eye(m,m);
		Matrix D = eye(m,m);
		for (int k = 0; k < m - 1; k++)
		{
			int _pivot = pivot(k);
			if (_pivot != k)
			{
				Matrix E = elementary(m,k,_pivot);
				C = (Matrix)C.mult(E);
				D = (Matrix)D.mult(E);
				perm++;
			}
			int p = k;
			for (p = k; p < n; p++)
			{
				if (!a[k][p].Equals(Zahl.ZERO))
				{
					break;
				}
			}
			if (p < n)
			{
				for (int i = k + 1; i < m; i++)
				{
					if (!a[i][p].Equals(Zahl.ZERO))
					{
						Algebraic f = a[i][p].div(a[k][p]);
						a[i][p] = Zahl.ZERO;
						for (int j = p + 1; j < n; j++)
						{
							a[i][j] = a[i][j].sub(f.mult(a[k][j]));
						}
						C = (Matrix)C.mult(elementary(m,i,k,f));
					}
				}
			}
		}
		int nm = Math.Max(n,m);
		for (int i = nm - 1; i >= 0; i--)
		{
			if (row_zero(i))
			{
				remove_row(i);
				C.remove_col(i);
			}
		}
		if (B != null)
		{
			B.a = C.a;
		}
		if (P != null)
		{
			P.a = D.a;
		}
		return perm;
	}
	public virtual Matrix copy()
	{
		int nr = nrow(), nc = ncol();
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: Algebraic[][] b = new Algebraic[nr][nc];
		Algebraic[][] b = RectangularArrays.ReturnRectangularAlgebraicArray(nr, nc);
		for (int i = 0; i < nr; i++)
		{
			for (int k = 0; k < nc; k++)
			{
				b[i][k] = a[i][k];
			}
		}
			return new Matrix(b);
	}
	public virtual Matrix pseudoinverse()
	{
		if (!det().Equals(Zahl.ZERO))
		{
			return invert();
		}
		Matrix c = copy();
		Matrix b = new Matrix(1,1);
		c.rank_decompose(b, null);
		int rank = c.nrow();
		if (rank == nrow())
		{
			Matrix ad = adjunkt();
			return (Matrix) ad.mult(((Matrix)(mult(ad))).invert());
		}
		else if (rank == ncol())
		{
			Matrix ad = adjunkt();
			return (Matrix)((Matrix)(ad.mult(this))).invert().mult(ad);
		}
		Matrix ca = c.adjunkt();
		Matrix ba = b.adjunkt();
		return (Matrix) ca.mult(((Matrix)c.mult(ca)).invert()).mult(((Matrix)ba.mult(b)).invert()).mult(ba);
	}
}
using System;
using System.Collections;

public class Exponential : Polynomial
{
	public Variable expvar;
	public Algebraic exp_b;

    public Exponential( Algebraic a, Algebraic c, Variable x, Algebraic b )
	{
		this.Coeffs = new Algebraic[2];

		this[0] = c;
		this[1] = a;

		var z = new Algebraic[2];

		z[0] = Symbolic.ZERO;
		z[1] = b;

		var la = Lambda.pc.env.getValue( "exp" );

	    if ( !( la is LambdaEXP ) )
		{
			la = new LambdaEXP();
		}

	    this._v = new FunctionVariable( "exp", new Polynomial( x, z ), ( LambdaAlgebraic ) la );

		this.expvar = x;
		this.exp_b = b;
	}

	public Exponential(Polynomial x) : base(x._v, x.Coeffs)
	{
		this.expvar = ((Polynomial)((FunctionVariable)this._v).Var)._v;
		this.exp_b = ((Polynomial)((FunctionVariable)this._v).Var)[1];
	}

	public static Algebraic poly2exp(Algebraic x)
	{
		if (x is Exponential)
		{
			return x;
		}
		if (x is Polynomial && ((Polynomial)x).Degree() == 1 && ((Polynomial)x)._v is FunctionVariable && ((FunctionVariable)(((Polynomial)x)._v)).Name.Equals("exp"))
		{
			Algebraic arg = ((FunctionVariable)(((Polynomial)x)._v)).Var;
			if (arg is Polynomial && ((Polynomial)arg).Degree() == 1 && ((Polynomial)arg)[0].Equals(Symbolic.ZERO))
			{
				return new Exponential((Polynomial)x);
			}
		}
		return x;
	}

	public override Algebraic Conj()
	{
		return new Exponential(this[1].Conj(), this[0].Conj(), expvar, exp_b.Conj());
	}

	internal static bool containsexp(Algebraic x)
	{
		if (x is Symbolic)
		{
			return false;
		}
		if (x is Exponential)
		{
			return true;
		}
		if (x is Polynomial)
		{
			for (int i = 0; i < ((Polynomial)x).Coeffs.Length; i++)
			{
				if (containsexp(((Polynomial)x)[i]))
				{
					return true;
				}
			}
				if (((Polynomial)x)._v is FunctionVariable)
				{
					return containsexp(((FunctionVariable)((Polynomial)x)._v).Var);
				}
				return false;
		}
		if (x is Rational)
		{
			return containsexp(((Rational)x).nom) || containsexp(((Rational)x).den);
		}
		if (x is Vector)
		{
			for (int i = 0; i < ((Vector)x).Length(); i++)
			{
				if (containsexp(((Vector)x)[i]))
				{
					return true;
				}
			}
				return false;
		}
		throw new JasymcaException("containsexp not suitable for x");
	}

    public override bool Equals(object x)
    {
        return base.Equals(x);
    }

    protected override Algebraic Add( Algebraic x )
	{
		if ( x is Symbolic )
		{
			return new Exponential( this[1], x + this[0], expvar, exp_b );
		}

		if ( x is Exponential )
		{
		    if ( _v.Equals( ( ( Exponential ) x )._v ) )
			{
				return poly2exp( this + x );
			}

		    if ( _v.Smaller( ( ( Exponential ) x )._v ) )
			{
				return x + this;
			}
			return new Exponential( this[1], x + this[0], expvar, exp_b );
		}
		return poly2exp( this + x );
	}

    protected override Algebraic Mul(Algebraic x)
	{
	    if ( x.Equals( Symbolic.ZERO ) )
		{
			return x;
		}

		if (x is Symbolic)
		{
			return new Exponential( this[1] * x, this[0] * x, expvar, exp_b );
		}

	    if ( x is Exponential && expvar.Equals( ( ( Exponential ) x ).expvar ) )
		{
			var xp = (Exponential)x;

			Algebraic r = Symbolic.ZERO;

			var nex = exp_b + xp.exp_b;

			if ( Equals(nex, Symbolic.ZERO) )
			{
				r = this[1] * xp[1];
			}
			else
			{
				r = new Exponential( this[1] * xp[1], Symbolic.ZERO, expvar, nex );
			}

			r = r + this[0] * xp;

			r = r + this * xp[0];

			r = r.Reduce();

			return r;
		}

		return poly2exp( this * x );
	}

	public override Algebraic Reduce()
	{
        if ( Equals(this[1].Reduce(), Symbolic.ZERO) )
        {
            return this[ 0 ].Reduce();
        }

        if ( Equals(exp_b, Symbolic.ZERO) )
        {
            return ( this[0] + this[1] ).Reduce();
        }

        return this;
	}

    protected override Algebraic Div(Algebraic x)
	{
	    if ( x is Symbolic )
		{
			return new Exponential( ( Polynomial ) ( this / x ) );
		}

		return this / x;
	}

	public override Algebraic Map( LambdaAlgebraic f )
	{
		return poly2exp( base.Map(f) );
	}

	public static Symbolic exp_gcd(ArrayList v, Variable x)
	{
		var gcd = Symbolic.ZERO;

		int k = 0;

		foreach (var t in v)
		{
		    var a = (Algebraic)t;

		    Algebraic c;

		    if ( Poly.Degree( a, x ) == 1 && ( c = Poly.Coefficient( a, x, 1 ) ) is Symbolic )
		    {
		        k++;
		        gcd = gcd.gcd( ( Symbolic ) c );
		    }
		}

	    return k > 0 ? gcd : Symbolic.ONE;
	}

	public static Algebraic reduce_exp(Algebraic p)
	{
		var a = new[] {p};

		a = reduce_exp(a);

		return a[0];
	}

	public static Algebraic[] reduce_exp(Algebraic[] p)
	{
		var v = new ArrayList();
		var vars = new ArrayList();

		var g = new GetExpVars2(v);

		foreach (var t in p)
		{
		    g.SymEval(t);
		}

		foreach (var t in v)
		{
		    var a = ( Algebraic ) t;

		    Variable x = null;

		    if ( a is Polynomial )
		    {
		        x = ( ( Polynomial ) a )._v;
		    }
		    else
		    {
		        continue;
		    }

		    if ( vars.Contains(x) )
		    {
		        continue;
		    }
		    else
		    {
		        vars.Add(x);
		    }

		    var gcd = exp_gcd(v, x);

		    if ( gcd != Symbolic.ZERO && gcd != Symbolic.ONE )
		    {
		        var sb = new SubstExp( gcd, x );

		        for ( int k = 0; k < p.Length; k++ )
		        {
		            p[k] = sb.SymEval( p[k] );
		        }
		    }
		}

		return p;
	}
}

internal class SubstExp : LambdaAlgebraic
{
	internal Symbolic gcd;
	internal Variable @var;
	internal Variable t = new SimpleVariable("t_exponential");

	public SubstExp(Symbolic gcd, Variable @var)
	{
		this.gcd = gcd;
		this.@var = @var;
	}

	public SubstExp(Variable @var, Algebraic expr)
	{
		this.@var = @var;
		ArrayList v = new ArrayList();
		(new GetExpVars2(v)).SymEval(expr);
		this.gcd = Exponential.exp_gcd(v, @var);
		if (gcd.Equals(Symbolic.ZERO))
		{
			t = @var;
		}
	}

	public virtual Algebraic ratsubst(Algebraic expr)
	{
		if (gcd.Equals(Symbolic.ZERO))
		{
			return expr;
		}
		if (!expr.Depends(@var))
		{
			return expr;
		}
		if (expr is Rational)
		{
		    return ratsubst( ( ( Rational ) expr ).nom ) / ratsubst( ( ( Rational ) expr ).den );
		}

		if (expr is Polynomial && ((Polynomial)expr)._v is FunctionVariable 
            && ((FunctionVariable)((Polynomial)expr)._v).Name.Equals("exp") 
            && ((FunctionVariable)((Polynomial)expr)._v).Var is Polynomial 
            && ((Polynomial)((FunctionVariable)((Polynomial)expr)._v).Var)._v.Equals(@var) 
            && ((Polynomial)((FunctionVariable)((Polynomial)expr)._v).Var).Degree() == 1 
            && ((Polynomial)((FunctionVariable)((Polynomial)expr)._v).Var)[0].Equals(Symbolic.ZERO))
		{
			Polynomial pexpr = (Polynomial)expr;
			int degree = pexpr.Degree();
			Algebraic[] a = new Algebraic[degree+1];
			for (int i = 0; i <= degree; i++)
			{
				Algebraic cf = pexpr[i];
				if (cf.Depends(@var))
				{
					throw new JasymcaException("Rationalize failed: 2");
				}
				a[i] = cf;
			}
			return new Polynomial(t, a);
		}
		throw new JasymcaException("Could not rationalize " + expr);
	}

	public virtual Algebraic rational(Algebraic expr)
	{
		return ( ratsubst(expr) / gcd / new Polynomial(t) ).Reduce();
	}

	public virtual Algebraic rat_reverse(Algebraic expr)
	{
		if (gcd.Equals(Symbolic.ZERO))
		{
			return expr;
		}
		Symbolic gc = gcd;
		Algebraic s = new Exponential(Symbolic.ONE, Symbolic.ZERO, @var, Symbolic.ONE * gc );
		return expr.Value(t, s);
	}

	internal override Algebraic SymEval(Algebraic f)
	{
		if (gcd.Equals(Symbolic.ZERO))
		{
			return f;
		}
		if (f is Polynomial)
		{
			Polynomial p = (Polynomial)f;
			if (p._v is FunctionVariable && ((FunctionVariable)p._v).Name.Equals("exp") && Poly.Degree(((FunctionVariable)p._v).Var,@var) == 1)
			{
				Algebraic arg = ((FunctionVariable)p._v).Var;

				Algebraic[] new_coef = new Algebraic[2];

				new_coef[1] = gcd.ToComplex();
				new_coef[0] = Symbolic.ZERO;

				Algebraic new_arg = new Polynomial(@var, new_coef);

				Algebraic subst = FunctionVariable.Create("exp", new_arg);
				Algebraic exp = Poly.Coefficient(arg,@var,1) / gcd;

				if (!(exp is Symbolic) && !((Symbolic)exp).IsInteger())
				{
					throw new JasymcaException("Not integer exponent in exponential simplification.");
				}

				subst = subst ^ ( ( Symbolic ) exp ).ToInt();

				subst = subst * FunctionVariable.Create("exp", Poly.Coefficient(arg,@var,0));

				int n = p.Coeffs.Length;

				Algebraic r = SymEval(p[n - 1]);

				for (int i = n - 2; i >= 0; i--)
				{
					r = r * subst + SymEval( p[i] );
				}

				return r;
			}
		}

		return f.Map(this);
	}
}

internal class NormExp : LambdaAlgebraic
{
	internal override Algebraic SymEval(Algebraic f)
	{
		if (f is Rational)
		{
		    var r = ( Rational ) f;

			var nom = SymEval( r.nom );
			var den = SymEval( r.den );

		    if ( den is Symbolic )
			{
				return SymEval( nom / den );
			}

		    if ( den is Exponential && ( ( Polynomial ) den )[0].Equals( Symbolic.ZERO ) && ( ( Polynomial ) den )[1] is Symbolic )
			{
				if ( nom is Symbolic || nom is Polynomial )
				{
				    var denx = ( Exponential ) den;

					var den_inv = new Exponential( Symbolic.ONE / denx[1], Symbolic.ZERO, denx.expvar, -denx.exp_b );

					return nom * den_inv;
				}
			}

			f = nom / den;

			return f;
		}

		if (f is Exponential)
		{
			return f.Map(this);
		}

		if (!(f is Polynomial))
		{
			return f.Map(this);
		}

		var fp = (Polynomial)f;

		if (!(fp._v is FunctionVariable) || !((FunctionVariable)fp._v).Name.Equals("exp"))
		{
			return f.Map(this);
		}

		var arg = ((FunctionVariable)fp._v).Var.Reduce();

		if (arg is Symbolic)
		{
			return fp.value(FunctionVariable.Create("exp",arg)).Map(this);
		}

		if (!(arg is Polynomial) || ((Polynomial)arg).Degree() != 1)
		{
			return f.Map(this);
		}

		Algebraic z = Symbolic.ZERO;

		var a = ((Polynomial)arg)[1];

		for (int i = 1; i < fp.Coeffs.Length; i++)
		{
			var b = ( ( Polynomial ) arg )[0];

		    Symbolic I = new Complex( ( double ) i );

			Algebraic ebi = Symbolic.ONE;

		    while ( b is Polynomial && ( ( Polynomial ) b ).Degree() == 1 )
			{
                var p = ( Polynomial ) b;

				var f1 = FunctionVariable.Create( "exp", new Polynomial( p._v ) * p[1] * I );

			    f1 = Exponential.poly2exp( f1 );

				ebi = ebi * f1;

				b = ( ( Polynomial ) b )[0];
			}

		    ebi = ebi * FunctionVariable.Create( "exp", b * I );

			var cf = SymEval( fp[i] * ebi );

		    var f2 = FunctionVariable.Create( "exp", new Polynomial( ( ( Polynomial ) arg )._v ) * a * I );

		    f2 = Exponential.poly2exp( f2 );

			z = z + cf * f2;
		}

	    if ( fp.Coeffs.Length > 0 )
		{
			z = z + SymEval( fp[0] );
		}

		return Exponential.poly2exp(z);
	}
}

internal class CollectExp : LambdaAlgebraic
{
	internal ArrayList v;

	public CollectExp(Algebraic f)
	{
		v = new ArrayList();

		(new GetExpVars(v)).SymEval(f);
	}

	internal override Algebraic SymEval(Algebraic x1)
	{
		if (v.Count == 0)
		{
			return x1;
		}

		if (!(x1 is Exponential))
		{
			return x1.Map(this);
		}

		Exponential e = (Exponential)x1;

		int exp = 1;

		Algebraic exp_b = e.exp_b;

		if (exp_b is Symbolic && ((Symbolic)exp_b) < Symbolic.ZERO )
		{
			exp *= -1;
			exp_b = -exp_b;
		}

		Variable x = e.expvar;

		for (int i = 0; i < v.Count; i++)
		{
			Polynomial y = (Polynomial)v[i];

			if (y._v.Equals(x))
			{
				Algebraic rat = exp_b / y[1];

				if (rat is Symbolic && !((Symbolic)rat).IsComplex())
				{
					int _cfs = cfs( ( ( Symbolic ) rat ).ToComplex().Re );

					if (_cfs != 0 && _cfs != 1)
					{
						exp *= _cfs;
						exp_b = exp_b / new Complex((double)_cfs);
					}
				}
			}
		}

        var p = ( new Polynomial( x ) ) * exp_b;

        p = FunctionVariable.Create( "exp", p ).Pow( exp );

        return p * SymEval( e[1] ) + SymEval( e[0] );
    }

	internal virtual int cfs(double x)
	{
		if (x < 0)
		{
			return cfs(-x);
		}

		int a0 = ( int ) Math.Floor(x);

		if ( x == ( double ) a0 )
		{
			return a0;
		}

		int a1 = ( int ) Math.Floor( 1.0 / ( x - a0 ) );

		int z = a0 * a1 + 1;

	    if ( Math.Abs( ( double ) z / ( double ) a1 - x ) < 1.0e-6 )
		{
			return z;
		}

		return 0;
	}
}

internal class GetExpVars : LambdaAlgebraic
{
	internal ArrayList v;

	public GetExpVars(ArrayList v)
	{
		this.v = v;
	}

	internal override Algebraic SymEval(Algebraic f)
	{
		if (f is Exponential)
		{
			Algebraic x = new Polynomial(((Exponential)f).expvar);

		    x = x * ( ( Exponential ) f ).exp_b;

			v.Add(x);

            // TODO: Check this
            SymEval( ( ( Exponential ) f )[1] );
            SymEval( ( ( Exponential ) f )[0] );

			return Symbolic.ONE;
		}

		return f.Map(this);
	}
}

internal class GetExpVars2 : LambdaAlgebraic
{
	internal ArrayList v;

	public GetExpVars2(ArrayList v)
	{
		this.v = v;
	}

	internal override Algebraic SymEval(Algebraic f)
	{
		if (f is Polynomial)
		{
			Polynomial p = (Polynomial)f;
			if (p._v is FunctionVariable && ((FunctionVariable)p._v).Name.Equals("exp"))
			{
				v.Add(((FunctionVariable)p._v).Var);
			}
			for (int i = 0; i < p.Coeffs.Length; i++)
			{
				SymEval(p[i]);
			}
			return Symbolic.ONE;
		}
		return f.Map(this);
	}
}

internal class DeExp : LambdaAlgebraic
{
	internal override Algebraic SymEval(Algebraic f)
	{
		if (f is Exponential)
		{
			Exponential x = (Exponential)f;
			Algebraic[] cn = new Algebraic[2];
			cn[0] = SymEval(x[0]);
			cn[1] = SymEval(x[1]);
			return new Polynomial(x._v, cn);
		}

		return f.Map(this);
	}
}

internal class LambdaEXP : LambdaAlgebraic
{
	public LambdaEXP()
	{
		diffrule = "exp(x)";
		intrule = "exp(x)";
	}

	internal override Symbolic PreEval(Symbolic x)
	{
		Complex z = x.ToComplex();

		double r = JMath.exp(z.Re);

		if (z.Im != 0.0)
		{
			return new Complex(r * Math.Cos(z.Im), r * Math.Sin(z.Im));
		}

		return new Complex(r);
	}

	internal override Algebraic SymEval(Algebraic x)
	{
		if (x.Equals(Symbolic.ZERO))
		{
			return Symbolic.ONE;
		}

		if (x is Polynomial 
            && ((Polynomial)x).Degree() == 1 
            && ((Polynomial)x)[0].Equals(Symbolic.ZERO))
		{
			Polynomial xp = (Polynomial)x;

			if (xp._v is SimpleVariable && ((SimpleVariable)xp._v).name.Equals("pi"))
			{
				Algebraic q = xp[1] / Symbolic.IONE;

				if (q is Symbolic)
				{
					return fzexakt((Symbolic)q);
				}
			}
			if (xp[1] is Symbolic 
                && xp._v is FunctionVariable 
                && ((FunctionVariable)xp._v).Name.Equals("log"))
			{
				if (((Symbolic)xp[1]).IsInteger())
				{
					int n = ((Symbolic)xp[1]).ToInt();

					return ((FunctionVariable)xp._v).Var.Pow(n);
				}
			}
		}
		return null;
	}

	internal virtual Algebraic fzexakt(Symbolic x)
	{
		if ( x < Symbolic.ZERO )
		{
			var r = fzexakt( ( Symbolic ) ( -x ) );

			if (r != null)
			{
				return r.Conj();
			}
			return r;
		}

		if ( x.IsInteger() )
		{
			if (x.ToInt() % 2 == 0)
			{
				return Symbolic.ONE;
			}
			else
			{
				return Symbolic.MINUS;
			}
		}

		var qs = x + new Complex( 0.5 );

		if (((Symbolic)qs).IsInteger())
		{
			if (((Symbolic)qs).ToInt() % 2 == 0)
			{
				return Symbolic.IMINUS;
			}
			else
			{
				return Symbolic.IONE;
			}
		}

		qs = x * new Complex(4);

		if (((Symbolic)qs).IsInteger())
		{
			Algebraic sq2 = FunctionVariable.Create("sqrt",new Complex(0.5));
			switch (((Symbolic)qs).ToInt() % 8)
			{
				case 1:
					return ( Symbolic.ONE + Symbolic.IONE ) / Symbolic.SQRT2;
				case 3:
					return ( Symbolic.MINUS + Symbolic.IONE ) / Symbolic.SQRT2;
				case 5:
					return ( Symbolic.MINUS + Symbolic.IMINUS ) / Symbolic.SQRT2;
				case 7:
					return ( Symbolic.ONE + Symbolic.IMINUS ) / Symbolic.SQRT2;
			}
		}

		qs = x * new Complex(6);

		if (((Symbolic)qs).IsInteger())
		{
			switch (((Symbolic)qs).ToInt() % 12)
			{
				case 1:
					return ( Symbolic.SQRT3 + Symbolic.IONE ) / Symbolic.TWO;
				case 2:
					return ( Symbolic.ONE + Symbolic.SQRT3 ) * Symbolic.IONE / Symbolic.TWO;
				case 4:
					return ( Symbolic.SQRT3 * Symbolic.IONE + Symbolic.MINUS ) / Symbolic.TWO;
				case 5:
					return ( Symbolic.IONE - Symbolic.SQRT3 ) / Symbolic.TWO;
				case 7:
					return ( Symbolic.IMINUS - Symbolic.SQRT3 ) /  Symbolic.TWO;
				case 8:
					return ( Symbolic.SQRT3 * Symbolic.IMINUS - Symbolic.ONE ) / Symbolic.TWO;
				case 10:
					return ( Symbolic.SQRT3 * Symbolic.IMINUS + Symbolic.ONE ) / Symbolic.TWO;
				case 11:
					return ( Symbolic.IMINUS + Symbolic.SQRT3 ) / Symbolic.TWO;
			}
		}
		return null;
	}
}

internal class LambdaLOG : LambdaAlgebraic
{
	public LambdaLOG()
	{
		diffrule = "1/x";
		intrule = "x*log(x)-x";
	}

	internal override Symbolic PreEval(Symbolic x)
	{
		Complex z = x.ToComplex();
		if (z.Re < 0 || z.Im != 0.0)
		{
			return new Complex(JMath.log(z.Re * z.Re + z.Im * z.Im) / 2, JMath.atan2(z.Im,z.Re));
		}
		return new Complex(JMath.log(z.Re));
	}

	internal override Algebraic SymEval(Algebraic x)
	{
		if (x.Equals(Symbolic.ONE))
		{
			return Symbolic.ZERO;
		}
		if (x.Equals(Symbolic.MINUS))
		{
			return Symbolic.PI * Symbolic.IONE;
		}
		if (x is Polynomial 
            && ((Polynomial)x).Degree() == 1 
            && ((Polynomial)x)[0].Equals(Symbolic.ZERO) 
            && ((Polynomial)x)._v is FunctionVariable 
            && ((FunctionVariable)((Polynomial)x)._v).Name.Equals("exp"))
		{
			return ((FunctionVariable)((Polynomial)x)._v).Var + FunctionVariable.Create("log",((Polynomial)x)[1]);
		}
		return null;
	}
}
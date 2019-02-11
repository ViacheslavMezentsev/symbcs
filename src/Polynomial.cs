using System;
using System.Collections;
using System.Linq;

public class Polynomial : Algebraic
{
	public Algebraic[] Coeffs;
	public Variable _v;

    public Algebraic this[ int n ]
    {
        get { return Coeffs[n]; }
        set { Coeffs[n] = value; }
    }

    public Polynomial()
	{
	}

    public Polynomial( Variable v, Algebraic[] c )
	{
		_v = v;
		Coeffs = Poly.Reduce(c);
	}

    public Polynomial( Variable v, Vector vec )
	{
		_v = v;
		Coeffs = new Algebraic[ vec.Length() ];

		for (int i = 0; i < Coeffs.Length; i++)
		{
			Coeffs[i] = vec[ Coeffs.Length - 1 - i ];
		}

		Coeffs = Poly.Reduce(Coeffs);
	}

    public Polynomial( Variable @var )
	{
	    Coeffs = new Algebraic[] { Symbolic.ZERO, Symbolic.ONE };

		this._v = @var;
	}

	public virtual Variable Var
	{
		get { return _v; }
	}

	public virtual Vector Coeff()
	{
		var c = Poly.Clone(Coeffs);

		return new Vector(c);
	}

	public virtual Algebraic coefficient(Variable v, int n)
	{
		if (v.Equals(_v))
		{
			return coefficient(n);
		}

		Algebraic c = Symbolic.ZERO;

		for (int i = 0; i < Coeffs.Length; i++)
		{
			var ci = Coeffs[i];

		    if ( ci is Polynomial )
			{
				c = c + ( ( Polynomial ) ci ).coefficient( v, n ) * ( new Polynomial( _v ) ^ i ) ;
			}
		    else if ( n == 0 )
			{
				c = c + ci * ( new Polynomial( _v ) ^ i );
			}
		}

		return c;
	}

	public virtual Algebraic coefficient( int i )
	{
	    return i >= 0 && i < Coeffs.Length ? Coeffs[i] : Symbolic.ZERO;
	}

	public override bool IsRat( Variable v )
	{
		if ( v is FunctionVariable && ( ( FunctionVariable ) this._v ).Var.Depends(v) )
		{
			return false;
		}

	    return Coeffs.All( t => t.IsRat(v) );
	}

	public virtual int Degree()
	{
		return Coeffs.Length - 1;
	}

	public virtual int Degree( Variable v )
	{
		if ( this._v == v )
		{
			return Coeffs.Length - 1;
		}

		int degree = 0;

		foreach ( var t in Coeffs )
		{
		    int d = Poly.Degree( t, v );

		    if ( d > degree )
		    {
		        degree = d;
		    }
		}

		return degree;
	}

    protected override Algebraic Add( Algebraic a )
	{
		if ( a is Rational )
		{
			return a + this;
		}

	    if ( a is Polynomial )
		{
            var p = ( Polynomial ) a;

		    if ( _v.Equals( p._v ) )
			{
			    int len = Math.Max( Coeffs.Length, p.Coeffs.Length );

			    var csum = new Algebraic[ len ];

				for (int i = 0; i < len; i++)
				{
				    csum[i] = coefficient(i) + p.coefficient(i);
				}

			    return ( new Polynomial( _v, csum ) ).Reduce();
			}
		    else if ( _v.Smaller( p._v ) )
			{
				return a + this;
			}
		}

	    var _csum = Poly.Clone( Coeffs );

		_csum[0] = Coeffs[0] + a;

	    return ( new Polynomial( _v, _csum ) ).Reduce();
	}

    protected override Algebraic Mul(Algebraic p)
	{
		if (p is Rational)
		{
			return p * this;
		}
		if (p is Polynomial)
		{
			if (_v.Equals(((Polynomial)p)._v))
			{
				int len = Coeffs.Length + ((Polynomial)p).Coeffs.Length - 1;

				var cprod = new Algebraic[len];

				for (int i = 0; i < len; i++)
				{
					cprod[i] = Symbolic.ZERO;
				}

				for (int i = 0; i < Coeffs.Length; i++)
				{
					for (int k = 0; k < ((Polynomial)p).Coeffs.Length; k++)
					{
						cprod[i + k] = cprod[i + k] + Coeffs[i] * ( ( Polynomial ) p )[k];
					}
				}

				return (new Polynomial(_v, cprod)).Reduce();
			}
			else if (_v.Smaller(((Polynomial)p)._v))
			{
				return p * this;
			}
		}

	    var _cprod = new Algebraic[ Coeffs.Length ];

		for ( int i = 0; i < Coeffs.Length; i++ )
		{
			_cprod[i] = Coeffs[i] * p;
		}

		return (new Polynomial(_v, _cprod)).Reduce();
	}

    protected override Algebraic Div( Algebraic q )
	{
		if (q is Symbolic)
		{
		    var c = new Algebraic[ Coeffs.Length ];

			for (int i = 0; i < Coeffs.Length; i++)
			{
				c[i] = Coeffs[i] / q;
			}

			return new Polynomial(_v, c);
		}

		return base.Div(q);
	}

	public override Algebraic Reduce()
	{
		if ( Coeffs.Length == 0 )
		{
			return Symbolic.ZERO;
		}

		if ( Coeffs.Length == 1 )
		{
			return Coeffs[0].Reduce();
		}

		return this;
	}

	public override string ToString()
	{
		var x = new ArrayList();

		for ( int i = Coeffs.Length - 1; i > 0;i-- )
		{
            if ( Equals( Coeffs[i], Symbolic.ZERO ) ) continue;

			var s = "";

			if ( Equals(Coeffs[i], Symbolic.MINUS) )
			{
				s += "-";
			}
			else if ( !Equals(Coeffs[i], Symbolic.ONE) )
			{
				s += Coeffs[i] + "*";
			}

			s += _v.ToString();

			if ( i > 1 )
			{
				s += "^" + i;
			}

			x.Add(s);
		}

		if ( !Equals(Coeffs[0], Symbolic.ZERO) )
		{
			x.Add( Coeffs[0].ToString() );
		}

		var _s = "";

		if ( x.Count > 1 )
		{
			_s += "(";
		}

		for ( int i = 0; i < x.Count; i++ )
		{
			_s += ( string ) x[i];

		    if ( i >= x.Count - 1 || ( ( string ) x[ i + 1 ] )[0] == '-' ) continue;

		    _s += "+";
		}

		if ( x.Count > 1 )
		{
			_s += ")";
		}

		return _s;
	}

	public override bool Equals(object x)
	{
		if ( !( x is Polynomial ) )
		{
			return false;
		}

	    if ( !( _v.Equals( ( ( Polynomial ) x )._v ) ) || Coeffs.Length != ( ( Polynomial ) x ).Coeffs.Length )
		{
			return false;
		}

	    return !Coeffs.Where( ( t, i ) => !t.Equals( ( ( Polynomial ) x )[i] ) ).Any();
	}

	public override Algebraic Derive(Variable v)
	{
		Algebraic r1 = Symbolic.ZERO, r2 = Symbolic.ZERO;

		var x = new Polynomial(this._v);

		for (int i = Coeffs.Length - 1; i > 1; i--)
		{
			r1 = ( r1 + Coeffs[i] * new Complex(i) ) * x;
		}

		if (Coeffs.Length > 1)
		{
			r1 = r1 + Coeffs[1];
		}

		for (int i = Coeffs.Length - 1; i > 0; i--)
		{
			r2 = ( r2 + Coeffs[i].Derive(v) ) * x;
		}

		if (Coeffs.Length > 0)
		{
			r2 = r2 + Coeffs[0].Derive(v);
		}

		return ( r1 * _v.Derive(v) + r2 ).Reduce();
	}

	public override bool Depends(Variable v)
	{
		if ( Coeffs.Length == 0 )
		{
			return false;
		}

		if ( _v == v )
		{
			return true;
		}

		if ( _v is FunctionVariable && ( ( FunctionVariable ) _v ).Var.Depends(v) )
		{
			return true;
		}

	    return Coeffs.Any( t => t.Depends(v) );
	}

	internal static bool loopPartial = false;

	public override Algebraic Integrate( Variable v )
	{
		Algebraic tmp = Symbolic.ZERO;

		for (int i = 1; i < Coeffs.Length; i++)
		{
		    if ( !Coeffs[ i ].Depends(v) )
			{
			    if ( v.Equals( _v ) )
				{
					tmp = tmp + Coeffs[i] * ( new Polynomial(v) ^ ( i + 1 ) ) / new Complex( i + 1 );
				}
			    else if ( _v is FunctionVariable && ( ( FunctionVariable ) _v ).Var.Depends(v) )
				{
					if (i == 1)
					{
						tmp = tmp + ( ( FunctionVariable ) _v ).Integrate(v) * Coeffs[1];
					}
					else
					{
						throw new JasymcaException("Integral not supported.");
					}
				}
					else
					{
						tmp = tmp + Coeffs[i] * ( new Polynomial(v) * new Polynomial( _v ) ^ i );
					}
			}
					else if (v.Equals(this._v))
					{
						throw new JasymcaException("Integral not supported.");
					}
					else if (this._v is FunctionVariable && ((FunctionVariable)this._v).Var.Depends(v))
					{
						if (i == 1 && Coeffs[i] is Polynomial && ((Polynomial)Coeffs[i])._v.Equals(v))
						{
						    Debug( "Trying to isolate inner derivative " + this );

							try
							{
								var f = ( FunctionVariable ) _v;

                                var w = f.Var;

                                var q = Coeffs[ i ] / w.Derive(v);

								if ( Equals(q.Derive(v), Symbolic.ZERO) )
								{
									var sv = new SimpleVariable( "v" );

                                    var p = FunctionVariable.Create( f.Name, new Polynomial( sv ) );

                                    var r = p.Integrate( sv ).Value( sv, w ) * q;

									tmp = tmp + r;

									continue;
								}
							}
							catch ( JasymcaException )
							{
							}

						    Debug( "Failed." );

							if ( ( ( Polynomial ) Coeffs[i] ).Coeffs.Any( t => t.Depends(v) ) )
							{
							    throw new JasymcaException("Function not supported by this method");
							}

						    if ( loopPartial )
							{
								loopPartial = false;

							    Debug( "Partial Integration Loop detected." );

								throw new JasymcaException("Partial Integration Loop: " + this);
							}

						    Debug( "Trying partial integration: x^n*f(x) , n-times diff " + this );

							try
							{
								loopPartial = true;

								var c = Coeffs[i];

							    var fv = ( ( FunctionVariable ) _v ).Integrate(v);

								var r = fv * c;

								while ( ( c = c.Derive(v) ) !=  Symbolic.ZERO )
								{
								    r = r - fv.Integrate(v) * c;
								}

								loopPartial = false;
								tmp = tmp + r;

								continue;
							}
							catch ( JasymcaException )
							{
								loopPartial = false;
							}

						    Debug( "Failed." );
						    Debug( "Trying partial integration: x^n*f(x) , 1-times int " + this );

							try
							{
								loopPartial = true;

								var p1 = Coeffs[i].Integrate(v);

								Algebraic f = new Polynomial( ( FunctionVariable ) _v );

								var r = p1 * f - ( p1 * f.Derive(v) ).Integrate(v);

								loopPartial = false;

								tmp = tmp + r;

								continue;
							}
							catch ( JasymcaException )
							{
								loopPartial = false;
							}

						    Debug( "Failed" );

						    throw new JasymcaException( "Function not supported by this method" );
						}
						else
						{
						    throw new JasymcaException( "Integral not supported." );
						}
					}
					else
					{
						tmp = tmp + Coeffs[i].Integrate(v) * new Polynomial( _v )^i;
					}
		}

		if ( Coeffs.Length > 0 )
		{
			tmp = tmp + Coeffs[0].Integrate(v);
		}

		return tmp;
	}

	public override Algebraic Conj()
	{
	    var xn = new Polynomial( _v.Conj() );

		Algebraic r = Symbolic.ZERO;

		for (int i = Coeffs.Length - 1; i > 0; i--)
		{
			r = ( r + Coeffs[i].Conj() ) * xn;
		}

		if ( Coeffs.Length > 0 )
		{
			r = r + Coeffs[0].Conj();
		}

		return r;
	}

	public override Algebraic Value( Variable v, Algebraic a )
	{
		Algebraic r = Symbolic.ZERO;

		var b = _v.Value( v, a );

		for ( int i = Coeffs.Length - 1; i > 0; i-- )
		{
			r = ( r + Coeffs[i].Value( v, a ) ) * b;
		}

		if ( Coeffs.Length > 0 )
		{
			r = r + Coeffs[0].Value( v, a );
		}

		return r;
	}

	public virtual Algebraic value( Algebraic x )
	{
		return Value( _v, x );
	}

	public override bool IsNumber()
	{
		var exakt = Coeffs[0].IsNumber();

		for (int i = 1; i < Coeffs.Length; i++)
		{
			exakt = exakt && Coeffs[i].IsNumber();
		}

		return exakt;
	}

	public override double Norm()
	{
	    return Coeffs.Sum( t => t.Norm() );
	}

	public override Algebraic Map( LambdaAlgebraic f )
	{
		var x = _v is SimpleVariable ? new Polynomial(_v): FunctionVariable.Create(((FunctionVariable)_v).Name, f.SymEval(((FunctionVariable)_v).Var));

		Algebraic r = Symbolic.ZERO;

		for ( int i = Coeffs.Length - 1; i > 0; i-- )
		{
			r = ( r + f.SymEval( Coeffs[i] ) ) * x;
		}

		if ( Coeffs.Length > 0 )
		{
			r = r + f.SymEval( Coeffs[0] );
		}

		return r;
	}

	public virtual Polynomial Monic()
	{
		var cm = Coeffs.Last();

		if ( Equals(cm, Symbolic.ONE) )
		{
			return this;
		}

		if ( Equals(cm, Symbolic.ZERO) || cm.Depends( _v ) )
		{
		    throw new JasymcaException( "Ill conditioned polynomial: main coefficient Zero or not number" );
		}

		var b = new Algebraic[ Coeffs.Length ];

		b[ Coeffs.Length - 1 ] = Symbolic.ONE;

		for ( int i = 0; i < Coeffs.Length - 1; i++ )
		{
			b[i] = Coeffs[i] / cm;
		}

		return new Polynomial( _v, b );
	}

	public virtual Algebraic[] square_free_dec(Variable v)
	{
		if ( !IsRat(v) )
		{
			return null;
		}

		var dp = Derive(v);

		var gcd_pdp = Poly.poly_gcd(this,dp);

		var q = Poly.polydiv(this,gcd_pdp);

		var p1 = Poly.polydiv(q, Poly.poly_gcd(q,gcd_pdp));

		if (gcd_pdp is Polynomial && gcd_pdp.Depends(v) && ((Polynomial)gcd_pdp).IsRat(v))
		{
			var sq = ((Polynomial)gcd_pdp).square_free_dec(v);

			var result = new Algebraic[sq.Length + 1];

			result[0] = p1;

			for (int i = 0; i < sq.Length;i++)
			{
				result[i + 1] = sq[i];
			}

			return result;
		}
		else
		{
			var result = new[] {p1};

			return result;
		}
	}

	public virtual Symbolic gcd_coeff()
	{
		Symbolic gcd;
		if (Coeffs[0] is Symbolic)
		{
			gcd = (Symbolic)Coeffs[0];
		}
		else if (Coeffs[0] is Polynomial)
		{
			gcd = ((Polynomial)Coeffs[0]).gcd_coeff();
		}
		else
		{
			throw new JasymcaException("Cannot calculate gcd from " + this);
		}
		for (int i = 1; i < Coeffs.Length; i++)
		{
			if (Coeffs[i] is Symbolic)
			{
				gcd = gcd.gcd((Symbolic)Coeffs[i]);
			}
			else if (Coeffs[i] is Polynomial)
			{
				gcd = gcd.gcd(((Polynomial)Coeffs[i]).gcd_coeff());
			}
			else
			{
				throw new JasymcaException("Cannot calculate gcd from " + this);
			}
		}
		return gcd;
	}

	public virtual Vector solve(Variable @var)
	{
		if (!@var.Equals(this._v))
		{
			return ((Polynomial)Value(@var, Poly.top)).solve(SimpleVariable.top);
		}
		var factors = square_free_dec(@var);
		var s = new ArrayList();
		int n = factors == null?0:factors.Length;
		for (int i = 0; i < n; i++)
		{
			if (factors[i] is Polynomial)
			{
				Vector sol = null;
				var equ = factors[i];
				try
				{
					sol = ((Polynomial)equ).solvepoly();
				}
				catch (JasymcaException)
				{
					sol = ((Polynomial)equ).Monic().roots();
				}
				for (int k = 0; k < sol.Length(); k++)
				{
					s.Add(sol[k]);
				}
			}
		}

		var cn = new Algebraic[s.Count];

		for (int i = 0; i < cn.Length; i++)
		{
			cn[i] = (Algebraic)s[i];
		}

		return new Vector(cn);
	}

	public virtual Vector solvepoly()
	{
		var s = new ArrayList();

		switch ( Degree() )
		{
			case 0:
				break;

			case 1:
				s.Add( -Coeffs[0] / Coeffs[1] );
				break;

			case 2:
				var p = Coeffs[1] / Coeffs[2];
				var q = Coeffs[0] / Coeffs[2];

				p = -p / Symbolic.TWO;

				q = p * p - q;

				if ( Equals(q, Symbolic.ZERO) )
				{
					s.Add(p);
					break;
				}

			    q = FunctionVariable.Create( "sqrt", q );

				s.Add( p + q );
				s.Add( p - q );
				break;

			default:
				int gcd = -1;

				for (int i = 1; i < Coeffs.Length; i++)
				{
					if ( Coeffs[i] !=  Symbolic.ZERO )
					{
					    gcd = gcd < 0 ? i : Poly.gcd( i, gcd );
					}
				}

				int deg = Degree() / gcd;

				if (deg < 3)
				{
					var cn = new Algebraic[deg + 1];

					for (int i = 0; i < cn.Length; i++)
					{
						cn[i] = Coeffs[i * gcd];
					}

					var pr = new Polynomial(_v, cn);

					var sn = pr.solvepoly();

					if (gcd == 2)
					{
						cn = new Algebraic[sn.Length() * 2];

						for (int i = 0; i < sn.Length(); i++)
						{
							cn[2 * i] = FunctionVariable.Create("sqrt", sn[i]);

							cn[2 * i + 1] = -cn[2 * i];
						}
					}
					else
					{
						cn = new Algebraic[sn.Length()];

						Symbolic wx = new Complex( 1.0 / gcd );

						for (int i = 0; i < sn.Length(); i++)
						{
							var exp = FunctionVariable.Create("log",sn[i]);

							cn[i] = FunctionVariable.Create("exp", exp * wx );
						}
					}
					return new Vector(cn);
				}
				throw new JasymcaException("Can't solve expression " + this);
		}

		return Vector.Create(s);
	}

	public virtual Vector roots()
	{
		if ( Coeffs.Length == 2 )
		{
			return new Vector( new[] { -Coeffs[0] / Coeffs[1] } );
		}
		else if ( Coeffs.Length == 3 )
		{
			return new Vector( Poly.pqsolve( Coeffs[1] / Coeffs[2], Coeffs[0] / Coeffs[2] ) );
		}

        var ar = new double[ Coeffs.Length ];
        var ai = new double[ Coeffs.Length ];

		var err = new bool[Coeffs.Length];
		var komplex = false;

		for (int i = 0; i < Coeffs.Length; i++)
		{
			var cf = Coeffs[i];

			if (!(cf is Symbolic))
			{
			    throw new JasymcaException( "Roots requires constant coefficients." );
			}

			ar[i] = ((Symbolic)cf).ToComplex().Re;
			ai[i] = ((Symbolic)cf).ToComplex().Im;

			if (ai[i] != 0.0)
			{
				komplex = true;
			}
		}

		if (komplex)
		{
			Pzeros.aberth(ar, ai, err);
		}
		else
		{
			Pzeros.bairstow(ar, ai, err);

			bool ok = true;

			for (int i = 0; i < err.Length - 1; i++)
			{
				if (err[i])
				{
					ok = false;
				}
			}

			if (!ok)
			{
				for (int i = 0; i < Coeffs.Length; i++)
				{
					Algebraic cf = Coeffs[i];

					ar[i] = ((Symbolic)cf).ToComplex().Re;
					ai[i] = ((Symbolic)cf).ToComplex().Im;
				}

				Pzeros.aberth(ar, ai, err);
			}
		}

		var r = new Algebraic[Coeffs.Length - 1];

		for (int i = 0; i < r.Length; i++)
		{
			if (!err[i])
			{
				var x0 = new Complex(ar[i],ai[i]);

				r[i] = x0;
			}
			else
			{
			    throw new JasymcaException( "Could not calculate root " + i );
			}
		}

		return new Vector(r);
	}
}

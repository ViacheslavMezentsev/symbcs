using System;
using System.Collections;
using System.Linq;

namespace Tiny.Science.Symbolic
{
    public class Polynomial : Algebraic
    {
        internal static bool loopPartial;

        public Algebraic[] Coeffs { get; set; }
        public Variable Var { get; set; }

        public Algebraic this[ int n ]
        {
            get => GetCoeff(n);
            set => Coeffs[n] = value;
        }

        public Polynomial()
        {
            Coeffs= new Algebraic[0];
        }

        public Polynomial( Variable v, Algebraic[] c )
        {
            Var = v;
            Coeffs = Poly.Reduce(c);
        }

        public Polynomial( Variable v, Vector vec )
        {
            Var = v;

            Coeffs = Poly.Reduce( vec.Reverse().ToArray() );
        }

        public Polynomial( Variable v )
        {
            Coeffs = new Algebraic[] { Symbol.ZERO, Symbol.ONE };

            Var = v;
        }

        public virtual Vector Coeff()
        {
            var c = Poly.Clone( Coeffs );

            return new Vector(c);
        }

        public virtual Algebraic GetCoeff( Variable v, int n )
        {
            if ( v.Equals( Var ) )
            {
                return GetCoeff(n);
            }

            Algebraic c = Symbol.ZERO;

            for ( var i = 0; i < Coeffs.Length; i++ )
            {
                var ci = Coeffs[i];

                if ( ci is Polynomial )
                {
                    c = c + ( ( Polynomial ) ci ).GetCoeff( v, n ) * ( new Polynomial( Var ) ^ i );
                }
                else if ( n == 0 )
                {
                    c = c + ci * ( new Polynomial( Var ) ^ i );
                }
            }

            return c;
        }

        private Algebraic GetCoeff( int n )
        {
            return n >= 0 && n < Coeffs.Length ? Coeffs[n] : Symbol.ZERO;
        }

        public override bool IsRat( Variable v )
        {
            if ( v is FunctionVariable && ( ( FunctionVariable ) Var ).Var.Depends(v) )
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
            if ( Var == v )
            {
                return Coeffs.Length - 1;
            }

            var degree = 0;

            foreach ( var t in Coeffs )
            {
                var d = Poly.Degree( t, v );

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

                if ( Var.Equals( p.Var ) )
                {
                    var len = Math.Max( Coeffs.Length, p.Coeffs.Length );

                    var csum = new Algebraic[ len ];

                    for ( var i = 0; i < len; i++ )
                    {
                        csum[i] = GetCoeff(i) + p.GetCoeff(i);
                    }

                    return ( new Polynomial( Var, csum ) ).Reduce();
                }

                if ( Var.Smaller( p.Var ) )
                {
                    return a + this;
                }
            }

            var _csum = Poly.Clone( Coeffs );

            _csum[0] = Coeffs[0] + a;

            return ( new Polynomial( Var, _csum ) ).Reduce();
        }

        protected override Algebraic Mul( Algebraic p )
        {
            if ( p is Rational )
            {
                return p * this;
            }

            if ( p is Polynomial )
            {
                var poly = ( Polynomial ) p;

                if ( Var.Equals( poly.Var ) )
                {
                    var len = Coeffs.Length + poly.Coeffs.Length - 1;

                    var cprod = new Algebraic[ len ];

                    for ( var i = 0; i < len; i++ )
                    {
                        cprod[i] = Symbol.ZERO;
                    }

                    for ( var n = 0; n < Coeffs.Length; n++ )
                    {
                        for ( var k = 0; k < poly.Coeffs.Length; k++ )
                        {
                            cprod[ n + k ] = cprod[ n + k ] + Coeffs[n] * poly[k];
                        }
                    }

                    return ( new Polynomial( Var, cprod ) ).Reduce();
                }

                if ( Var.Smaller( poly.Var ) )
                {
                    return p * this;
                }
            }

            return ( new Polynomial( Var, Coeffs.Select( c => c * p ).ToArray() ) ).Reduce();
        }

        protected override Algebraic Div( Algebraic q )
        {
            if ( q is Symbol )
            {
                return new Polynomial( Var, Coeffs.Select( c => c / q ).ToArray() );
            }

            return base.Div(q);
        }

        public override Algebraic Reduce()
        {
            if ( Coeffs.Length == 0 )
            {
                return Symbol.ZERO;
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

            for ( var i = Coeffs.Length - 1; i > 0; i-- )
            {
                if ( Equals( Coeffs[i], Symbol.ZERO ) ) continue;

                var s = "";

                if ( Equals( Coeffs[i], Symbol.MINUS ) )
                {
                    s += "-";
                }
                else if ( !Equals( Coeffs[i], Symbol.ONE ) )
                {
                    s += Coeffs[i] + "*";
                }

                s += Var.ToString();

                if ( i > 1 )
                {
                    s += "^" + i;
                }

                x.Add( s );
            }

            if ( !Equals( Coeffs[0], Symbol.ZERO ) )
            {
                x.Add( Coeffs[0].ToString() );
            }

            var _s = "";

            if ( x.Count > 1 )
            {
                _s += "(";
            }

            for ( var i = 0; i < x.Count; i++ )
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

        public override bool Equals( object x )
        {
            if ( !( x is Polynomial ) )
            {
                return false;
            }

            var poly = ( Polynomial ) x;

            if ( !( Var.Equals( poly.Var ) ) || Coeffs.Length != poly.Coeffs.Length )
            {
                return false;
            }

            return !Coeffs.Where( ( t, n ) => !t.Equals( poly[n] ) ).Any();
        }

        public override Algebraic Derive( Variable v )
        {
            Algebraic r1 = Symbol.ZERO, r2 = Symbol.ZERO;

            var x = new Polynomial( Var );

            for ( var n = Coeffs.Length - 1; n > 1; n-- )
            {
                r1 = ( r1 + Coeffs[n] * new Complex(n) ) * x;
            }

            if ( Coeffs.Length > 1 )
            {
                r1 = r1 + Coeffs[1];
            }

            for ( var n = Coeffs.Length - 1; n > 0; n-- )
            {
                r2 = ( r2 + Coeffs[n].Derive(v) ) * x;
            }

            if ( Coeffs.Length > 0 )
            {
                r2 = r2 + Coeffs[0].Derive(v);
            }

            return ( r1 * Var.Derive(v) + r2 ).Reduce();
        }

        public override bool Depends( Variable v )
        {
            if ( Coeffs.Length == 0 )
            {
                return false;
            }

            if ( Var == v )
            {
                return true;
            }

            if ( Var is FunctionVariable && ( ( FunctionVariable ) Var ).Var.Depends(v) )
            {
                return true;
            }

            return Coeffs.Any( t => t.Depends(v) );
        }

        

        public override Algebraic Integrate( Variable v )
        {
            Algebraic tmp = Symbol.ZERO;

            for ( var n = 1; n < Coeffs.Length; n++ )
            {
                if ( !Coeffs[n].Depends(v) )
                {
                    if ( v.Equals( Var ) )
                    {
                        tmp = tmp + Coeffs[n] * ( new Polynomial( v ) ^ ( n + 1 ) ) / new Complex( n + 1 );
                    }
                    else if ( Var is FunctionVariable && ( ( FunctionVariable ) Var ).Var.Depends(v) )
                    {
                        if ( n == 1 )
                        {
                            tmp = tmp + ( ( FunctionVariable ) Var ).Integrate(v) * Coeffs[1];
                        }
                        else
                        {
                            throw new SymbolicException( "Integral not supported." );
                        }
                    }
                    else
                    {
                        tmp = tmp + Coeffs[n] * ( new Polynomial(v) * new Polynomial( Var ) ^ n );
                    }
                }

                else if ( v.Equals( Var ) )
                {
                    throw new SymbolicException( "Integral not supported." );
                }

                else if ( Var is FunctionVariable && ( ( FunctionVariable ) Var ).Var.Depends(v) )
                {
                    if ( n == 1 && Coeffs[n] is Polynomial && ( ( Polynomial ) Coeffs[n] ).Var.Equals(v) )
                    {
                        Debug( "Trying to isolate inner derivative " + this );

                        try
                        {
                            var f = ( FunctionVariable ) Var;

                            var w = f.Var;

                            var q = Coeffs[n] / w.Derive(v);

                            if ( Equals( q.Derive(v), Symbol.ZERO ) )
                            {
                                var sv = new SimpleVariable( "v" );

                                var p = FunctionVariable.Create( f.Name, new Polynomial( sv ) );

                                var r = p.Integrate( sv ).Value( sv, w ) * q;

                                tmp = tmp + r;

                                continue;
                            }
                        }
                        catch ( SymbolicException )
                        {
                        }

                        Debug( "Failed." );

                        if ( ( ( Polynomial ) Coeffs[n] ).Coeffs.Any( t => t.Depends(v) ) )
                        {
                            throw new SymbolicException( "Function not supported by this method" );
                        }

                        if ( loopPartial )
                        {
                            loopPartial = false;

                            Debug( "Partial Integration Loop detected." );

                            throw new SymbolicException( "Partial Integration Loop: " + this );
                        }

                        Debug( "Trying partial integration: x^n*f(x), n-times diff " + this );

                        try
                        {
                            loopPartial = true;

                            var c = Coeffs[n];

                            var fv = ( ( FunctionVariable ) Var ).Integrate(v);

                            var r = fv * c;

                            while ( ( c = c.Derive(v) ) != Symbol.ZERO )
                            {
                                r = r - fv.Integrate(v) * c;
                            }

                            loopPartial = false;
                            tmp = tmp + r;

                            continue;
                        }
                        catch ( SymbolicException )
                        {
                            loopPartial = false;
                        }

                        Debug( "Failed." );
                        Debug( "Trying partial integration: x^n*f(x), 1-times int " + this );

                        try
                        {
                            loopPartial = true;

                            var p1 = Coeffs[n].Integrate(v);

                            Algebraic f = new Polynomial( ( FunctionVariable ) Var );

                            var r = p1 * f - ( p1 * f.Derive(v) ).Integrate(v);

                            loopPartial = false;

                            tmp = tmp + r;

                            continue;
                        }
                        catch ( SymbolicException )
                        {
                            loopPartial = false;
                        }

                        Debug( "Failed" );

                        throw new SymbolicException( "Function not supported by this method" );
                    }

                    throw new SymbolicException( "Integral not supported." );
                }
                else
                {
                    tmp = tmp + Coeffs[n].Integrate(v) * new Polynomial( Var ) ^ n;
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
            var xn = new Polynomial( Var.Conj() );

            Algebraic r = Symbol.ZERO;

            for ( var n = Coeffs.Length - 1; n > 0; n-- )
            {
                r = ( r + Coeffs[n].Conj() ) * xn;
            }

            if ( Coeffs.Length > 0 )
            {
                r = r + Coeffs[0].Conj();
            }

            return r;
        }

        public override Algebraic Value( Variable v, Algebraic a )
        {
            Algebraic r = Symbol.ZERO;

            var b = Var.Value( v, a );

            for ( var n = Coeffs.Length - 1; n > 0; n-- )
            {
                r = ( r + Coeffs[n].Value( v, a ) ) * b;
            }

            if ( Coeffs.Length > 0 )
            {
                r = r + Coeffs[0].Value( v, a );
            }

            return r;
        }

        public virtual Algebraic value( Algebraic x )
        {
            return Value( Var, x );
        }

        public override bool IsNumber()
        {
            return Coeffs.All( x => x.IsNumber() );
        }

        public override double Norm()
        {
            return Coeffs.Sum( t => t.Norm() );
        }

        public override Algebraic Map( LambdaAlgebraic f )
        {
            var x = Var is SimpleVariable ? new Polynomial( Var ) : FunctionVariable.Create( ( ( FunctionVariable ) Var ).Name, f.SymEval( ( ( FunctionVariable ) Var ).Var ) );

            Algebraic r = Symbol.ZERO;

            for ( var n = Coeffs.Length - 1; n > 0; n-- )
            {
                r = ( r + f.SymEval( Coeffs[n] ) ) * x;
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

            if ( Equals( cm, Symbol.ONE ) )
            {
                return this;
            }

            if ( Equals( cm, Symbol.ZERO ) || cm.Depends( Var ) )
            {
                throw new SymbolicException( "Ill conditioned polynomial: main coefficient Zero or not number" );
            }

            var b = new Algebraic[ Coeffs.Length ];

            b[ Coeffs.Length - 1 ] = Symbol.ONE;

            for ( var n = 0; n < Coeffs.Length - 1; n++ )
            {
                b[n] = Coeffs[n] / cm;
            }

            return new Polynomial( Var, b );
        }

        public virtual Algebraic[] square_free_dec( Variable v )
        {
            if ( !IsRat(v) )
            {
                return null;
            }

            var dp = Derive(v);

            var gcd_pdp = Poly.poly_gcd( this, dp );

            var q = Poly.polydiv( this, gcd_pdp );

            var p1 = Poly.polydiv( q, Poly.poly_gcd( q, gcd_pdp ) );

            if ( gcd_pdp is Polynomial && gcd_pdp.Depends(v) && ( ( Polynomial ) gcd_pdp ).IsRat(v) )
            {
                var sq = ( ( Polynomial ) gcd_pdp ).square_free_dec(v);

                var result = new Algebraic[ sq.Length + 1 ];

                result[0] = p1;

                for ( var n = 0; n < sq.Length; n++ )
                {
                    result[ n + 1 ] = sq[n];
                }

                return result;
            }
            else
            {
                var result = new[] { p1 };

                return result;
            }
        }

        public virtual Symbol gcd_coeff()
        {
            Symbol gcd;

            if ( Coeffs[0] is Symbol )
            {
                gcd = ( Symbol ) Coeffs[0];
            }
            else if ( Coeffs[0] is Polynomial )
            {
                gcd = ( ( Polynomial ) Coeffs[0] ).gcd_coeff();
            }
            else
            {
                throw new SymbolicException( "Cannot calculate gcd from " + this );
            }

            for ( var n = 1; n < Coeffs.Length; n++ )
            {
                if ( Coeffs[n] is Symbol )
                {
                    gcd = gcd.gcd( ( Symbol ) Coeffs[n] );
                }
                else if ( Coeffs[n] is Polynomial )
                {
                    gcd = gcd.gcd( ( ( Polynomial ) Coeffs[n] ).gcd_coeff() );
                }
                else
                {
                    throw new SymbolicException( "Cannot calculate gcd from " + this );
                }
            }

            return gcd;
        }

        public virtual Vector solve( Variable v )
        {
            if ( !v.Equals( Var ) )
            {
                return ( ( Polynomial ) Value( v, Poly.top ) ).solve( SimpleVariable.top );
            }

            var factors = square_free_dec(v);

            var s = new ArrayList();

            var len = factors?.Length ?? 0;

            for ( var n = 0; n < len; n++ )
            {
                if ( factors[n] is Polynomial )
                {
                    Vector sol;

                    var equ = factors[n];

                    try
                    {
                        sol = ( ( Polynomial ) equ ).solvepoly();
                    }
                    catch ( SymbolicException )
                    {
                        sol = ( ( Polynomial ) equ ).Monic().roots();
                    }

                    for ( var k = 0; k < sol.Length(); k++ )
                    {
                        s.Add( sol[k] );
                    }
                }
            }

            var cn = new Algebraic[ s.Count ];

            for ( var n = 0; n < cn.Length; n++ )
            {
                cn[n] = ( Algebraic ) s[n];
            }

            return new Vector( cn );
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

                    p = -p / Symbol.TWO;

                    q = p * p - q;

                    if ( Equals( q, Symbol.ZERO ) )
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

                    for ( int i = 1; i < Coeffs.Length; i++ )
                    {
                        if ( Coeffs[i] != Symbol.ZERO )
                        {
                            gcd = gcd < 0 ? i : Poly.gcd( i, gcd );
                        }
                    }

                    int deg = Degree() / gcd;

                    if ( deg < 3 )
                    {
                        var cn = new Algebraic[ deg + 1 ];

                        for ( var i = 0; i < cn.Length; i++ )
                        {
                            cn[i] = Coeffs[ i * gcd ];
                        }

                        var pr = new Polynomial( Var, cn );

                        var sn = pr.solvepoly();

                        if ( gcd == 2 )
                        {
                            cn = new Algebraic[ sn.Length() * 2 ];

                            for ( var i = 0; i < sn.Length(); i++ )
                            {
                                cn[ 2 * i ] = FunctionVariable.Create( "sqrt", sn[i] );

                                cn[ 2 * i + 1 ] = -cn[ 2 * i ];
                            }
                        }
                        else
                        {
                            cn = new Algebraic[ sn.Length() ];

                            Symbol wx = new Complex( 1.0 / gcd );

                            for ( var i = 0; i < sn.Length(); i++ )
                            {
                                var exp = FunctionVariable.Create( "log", sn[i] );

                                cn[i] = FunctionVariable.Create( "exp", exp * wx );
                            }
                        }

                        return new Vector( cn );
                    }

                    throw new SymbolicException( "Can't solve expression " + this );
            }

            return Vector.Create(s);
        }

        public virtual Vector roots()
        {
            if ( Coeffs.Length == 2 )
            {
                return new Vector( new[] { -Coeffs[0] / Coeffs[1] } );
            }

            if ( Coeffs.Length == 3 )
            {
                return new Vector( Poly.pqsolve( Coeffs[1] / Coeffs[2], Coeffs[0] / Coeffs[2] ) );
            }

            var ar = new double[ Coeffs.Length ];
            var ai = new double[ Coeffs.Length ];

            var err = new bool[ Coeffs.Length ];
            var komplex = false;

            for ( var n = 0; n < Coeffs.Length; n++ )
            {
                var cf = Coeffs[n];

                if ( !( cf is Symbol ) )
                {
                    throw new SymbolicException( "Roots requires constant coefficients." );
                }

                ar[n] = ( ( Symbol ) cf ).ToComplex().Re;
                ai[n] = ( ( Symbol ) cf ).ToComplex().Im;

                if ( ai[n] != 0.0 )
                {
                    komplex = true;
                }
            }

            if ( komplex )
            {
                Pzeros.aberth( ar, ai, err );
            }
            else
            {
                Pzeros.bairstow( ar, ai, err );

                var ok = true;

                for ( var n = 0; n < err.Length - 1; n++ )
                {
                    if ( err[n] )
                    {
                        ok = false;
                    }
                }

                if ( !ok )
                {
                    for ( var n = 0; n < Coeffs.Length; n++ )
                    {
                        var cf = Coeffs[n];

                        ar[n] = ( ( Symbol ) cf ).ToComplex().Re;
                        ai[n] = ( ( Symbol ) cf ).ToComplex().Im;
                    }

                    Pzeros.aberth( ar, ai, err );
                }
            }

            var r = new Algebraic[ Coeffs.Length - 1 ];

            for ( var n = 0; n < r.Length; n++ )
            {
                if ( !err[n] )
                {
                    var x0 = new Complex( ar[n], ai[n] );

                    r[n] = x0;
                }
                else
                {
                    throw new SymbolicException( "Could not calculate root " + n );
                }
            }

            return new Vector(r);
        }
    }
}

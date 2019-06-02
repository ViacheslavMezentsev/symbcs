namespace Tiny.Science.Symbolic
{
    public class Rational : Algebraic
    {
        internal Algebraic nom;
        internal Polynomial den;

        public Rational( Algebraic nom, Polynomial den )
        {
            var norm = den[ den.Degree() ];

            if ( norm is Symbol )
            {
                this.nom = nom / norm;
                this.den = ( Polynomial ) ( den / norm );
            }
            else
            {
                this.nom = nom;
                this.den = den;
            }
        }

        public override bool IsRat( Variable v )
        {
            return nom.IsRat(v) && den.IsRat(v);
        }

        public override Algebraic Reduce()
        {
            if ( nom is Symbol )
            {
                if ( nom.Equals( Symbol.ZERO ) )
                {
                    return Symbol.ZERO;
                }
                return this;
            }

            var pq = new[] { nom, den };

            pq = Exponential.reduce_exp( pq );

            if ( !nom.Equals( pq[0] ) || !den.Equals( pq[1] ) )
            {
                return ( pq[0] / pq[1] ).Reduce();
            }

            if ( IsNumber() )
            {
                var gcd = Poly.poly_gcd( den, nom );

                if ( !gcd.Equals( Symbol.ONE ) )
                {
                    var n = Poly.polydiv( nom, gcd );
                    var d = Poly.polydiv( den, gcd );

                    if ( d.Equals( Symbol.ONE ) )
                    {
                        return n;
                    }
                    else if ( d is Symbol )
                    {
                        return n / d;
                    }
                    else
                    {
                        return new Rational( n, ( Polynomial ) d );
                    }
                }
            }

            return this;
        }

        public override bool IsNumber()
        {
            return nom.IsNumber() && den.IsNumber();
        }

        protected override Algebraic Add( Algebraic a )
        {
            if ( a is Rational )
            {
                var r = ( Rational ) a;

                return ( ( nom * r.den + r.nom * den ) / ( den * r.den ) ).Reduce();
            }

            return ( ( nom + a * den ) / den ).Reduce();
        }

        protected override Algebraic Mul( Algebraic a )
        {
            if ( a is Rational )
            {
                var r = ( Rational ) a;

                return ( nom * r.nom / ( den * r.den ) ).Reduce();
            }

            return ( a * nom / den ).Reduce();
        }

        protected override Algebraic Div( Algebraic a )
        {
            if ( a is Rational )
            {
                var r = ( Rational ) a;

                return ( nom * r.den / ( den * r.nom ) ).Reduce();
            }
            else
            {
                return ( nom / ( den * a ) ).Reduce();
            }
        }

        public override string ToString()
        {
            return $"({nom}/{den})";
        }

        public override bool Equals( object x )
        {
            if ( x is Rational )
            {
                var r = ( Rational ) x;

                return r.nom == nom && r.den == den;
            }

            return false;
        }

        public override Algebraic Derive( Variable v )
        {
            return ( ( nom.Derive(v) * den - den.Derive(v) * nom ) / ( den * den ) ).Reduce();
        }

        public override Algebraic Integrate( Variable v )
        {
            if ( !den.Depends(v) )
            {
                return nom.Integrate(v) / den;
            }

            var quot = den.Derive(v) / nom;

            if ( quot.Derive(v).Equals( Symbol.ZERO ) )
            {
                return FunctionVariable.Create( "log", den ) / quot;
            }

            var q = new[] { nom, den };

            Poly.polydiv( q, v );

            if ( q[0] != Symbol.ZERO && nom.IsRat(v) && den.IsRat(v) )
            {
                return q[0].Integrate(v) + ( q[1] / den ).Integrate(v);
            }

            if ( IsRat(v) )
            {
                Algebraic r = Symbol.ZERO;

                var h = Horowitz( nom, den, v );

                if ( h[0] is Rational )
                {
                    r = r + h[0];
                }

                if ( h[1] is Rational )
                {
                    r = r + new TrigInverseExpand().SymEval( ( ( Rational ) h[1] ).intrat(v) );
                }

                return r;
            }

            throw new SymbolicException( "Could not integrate Function " + this );
        }

        public override double Norm()
        {
            return nom.Norm() / den.Norm();
        }

        public override Algebraic Conj()
        {
            return nom.Conj() / den.Conj();
        }

        public override bool Depends( Variable v )
        {
            return nom.Depends(v) || den.Depends(v);
        }

        public override Algebraic Value( Variable v, Algebraic x )
        {
            return nom.Value( v, x ) / den.Value( v, x );
        }

        public override Algebraic Map( LambdaAlgebraic f )
        {
            return f.SymEval( nom ) / f.SymEval( den );
        }

        public static Vector Horowitz( Algebraic p, Polynomial q, Variable x )
        {
            if ( Poly.Degree( p, x ) >= Poly.Degree( q, x ) )
            {
                throw new SymbolicException( "Degree of p must be smaller than degree of q" );
            }

            p = p.Rat();

            q = ( Polynomial ) q.Rat();

            var d = Poly.poly_gcd( q, q.Derive(x) );
            var b = Poly.polydiv( q, d );

            var m = b is Polynomial ? ( ( Polynomial ) b ).Degree() : 0;
            var n = d is Polynomial ? ( ( Polynomial ) d ).Degree() : 0;

            var a = new SimpleVariable[m];
            var X = new Polynomial(x);

            Algebraic A = Symbol.ZERO;

            for ( var i = a.Length - 1; i >= 0; i-- )
            {
                a[i] = new SimpleVariable( "a" + i );

                A = A + new Polynomial( a[i] );

                if ( i > 0 )
                {
                    A = A * X;
                }
            }

            var c = new SimpleVariable[n];

            Algebraic C = Symbol.ZERO;

            for ( var i = c.Length - 1; i >= 0; i-- )
            {
                c[i] = new SimpleVariable( "c" + i );

                C = C + new Polynomial( c[i] );

                if ( i > 0 )
                {
                    C = C * X;
                }
            }

            var r = Poly.polydiv( C * b * d.Derive(x), d );

            r = b * C.Derive(x) - r + d * A;

            var aik = Matrix.CreateRectangularArray<Algebraic>( m + n, m + n );

            Algebraic cf;

            var co = new Algebraic[ m + n ];

            for ( var i = 0; i < m + n; i++ )
            {
                co[i] = Poly.Coefficient( p, x, i );

                cf = Poly.Coefficient( r, x, i );

                for ( var k = 0; k < m; k++ )
                {
                    aik[i][k] = cf.Derive( a[k] );
                }

                for ( var k = 0; k < n; k++ )
                {
                    aik[i][ k + m ] = cf.Derive( c[k] );
                }
            }

            var s = LambdaLINSOLVE.Gauss( new Matrix( aik ), new Vector( co ) );

            A = Symbol.ZERO;

            for ( var i = m - 1; i >= 0; i-- )
            {
                A = A + s[i];

                if ( i > 0 )
                {
                    A = A * X;
                }
            }

            C = Symbol.ZERO;

            for ( var i = n - 1; i >= 0; i-- )
            {
                C = C + s[ i + m ];

                if ( i > 0 )
                {
                    C = C * X;
                }
            }

            return new Vector( new[] { C / d, A / b } );
        }

        internal virtual Algebraic intrat( Variable x )
        {
            var de = den.Derive(x);

            if ( de is Symbol )
            {
                return MakeLog( nom / de, x, -den[0] / de );
            }

            var r = nom / de;

            var xi = den.Monic().roots();

            Algebraic rs = Symbol.ZERO;

            for ( var i = 0; i < xi.Length(); i++ )
            {
                var c = r.Value( x, xi[i] );

                rs = rs + MakeLog( c, x, xi[i] );
            }

            return rs;
        }

        internal virtual Algebraic MakeLog( Algebraic c, Variable x, Algebraic a )
        {
            var arg = new Polynomial(x) - a;

            return FunctionVariable.Create( "log", arg ) * c;
        }
    }
}

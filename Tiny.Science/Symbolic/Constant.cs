namespace Tiny.Science.Symbolic
{
    public class Constant : SimpleVariable
    {
        private Complex value;

        public Constant( string name, double value ) : base( name )
        {
            this.value = new Complex( value );
        }

        public Constant( string name, Complex value ) : base( name )
        {
            this.value = value;
        }

        public override bool Smaller( Variable v )
        {
            if ( v is Constant )
            {
                return name.CompareTo( ( ( Constant ) v ).name ) < 0;
            }

            return true;
        }

        internal virtual Algebraic Value => value;
    }

    internal class Root : Constant
    {
        internal Vector poly;
        internal int n;

        public Root( Vector poly, int n ) : base( "Root", 0.0 )
        {
            this.poly = poly;
            this.n = n;
        }

        public override bool Smaller( Variable v )
        {
            if ( !( v is Root ) )
            {
                return base.Smaller(v);
            }

            var root = ( Root ) v;

            if ( !poly.Equals( root.poly ) )
            {
                return poly.Norm() < root.poly.Norm();
            }

            return n < root.n;
        }

        public override bool Equals( object x )
        {
            return x is Root && ( ( Root ) x ).poly.Equals( poly ) && ( ( Root ) x ).n == n;
        }

        public override string ToString()
        {
            return $"Root({new Vector(poly)}, {n})";
        }

        internal override Algebraic Value
        {
            get
            {
                var roots = ( new Polynomial( new SimpleVariable( "x" ), poly ) ).roots();

                return roots[n];
            }
        }
    }

    internal class ExpandConstants : LambdaAlgebraic
    {
        internal override Algebraic SymEval( Algebraic f )
        {
            while ( f is Polynomial && ( ( Polynomial ) f ).Var is Constant )
            {
                var v = ( ( Polynomial ) f ).Var;

                f = f.Value( v, ( ( Constant ) v ).Value );
            }

            return f.Map( this );
        }
    }
}

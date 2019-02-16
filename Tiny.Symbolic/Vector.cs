using System.Collections;
using System.Linq;

namespace Tiny.Symbolic
{
    public class Vector : Algebraic, IEnumerable
    {

        #region Private fields

        private Algebraic[] _items;

        #endregion

        #region Constructors

        private Vector()
        {
        }

        public Vector( Algebraic[] items )
        {
            _items = items;
        }

        public Vector( Algebraic value, int n )
        {
            _items = new Symbol[ n ];

            for ( var k = 0; k < n; k++ )
            {
                _items[ k ] = value;
            }
        }

        public Vector( int n ) : this( Symbol.ZERO, n )
        {
        }

        public Vector( Algebraic item )
        {
            _items = item is Vector ? Poly.Clone( ( Vector ) item ) : new[] { item };
        }

        public Vector( double[] items )
        {
            _items = new Algebraic[ items.Length ];

            for ( int i = 0; i < items.Length; i++ )
            {
                _items[ i ] = new Complex( items[ i ] );
            }
        }

        public Vector( double[] re, double[] im )
        {
            _items = new Algebraic[ re.Length ];

            for ( int k = 0; k < re.Length; k++ )
            {
                _items[ k ] = new Complex( re[ k ], im[ k ] );
            }
        }

        #endregion

        #region Protected methods

        protected override Algebraic Add( Algebraic x )
        {
            return this + x;
        }

        protected override Algebraic Mul( Algebraic a )
        {
            return this * a;
        }

        protected override Algebraic Div( Algebraic a )
        {
            return this / a;
        }

        #endregion

        #region Public methods

        #region Static

        public static Vector Create( ArrayList v )
        {
            if ( v.Count != v.OfType<Algebraic>().Count() )
            {
                throw new SymbolicException( "Error creating Vektor." );
            }

            return new Vector( v.Cast<Algebraic>().ToArray() );
        }

        #endregion

        #region Operators

        public static Algebraic operator +( Vector lhs, Algebraic rhs )
        {
            if ( rhs.IsScalar() )
            {
                rhs = rhs.Promote( lhs );
            }

            if ( rhs is Vector )
            {
                return lhs + ( Vector ) rhs;
            }

            throw new SymbolicException( "Wrong type of operand." );
        }

        public static Algebraic operator +( Vector lhs, Vector rhs )
        {
            if ( rhs.Length() == lhs._items.Length )
            {
                return new Vector( lhs._items.Select( ( x, n ) => x + rhs._items[ n ] ).ToArray() );
            }

            throw new SymbolicException( "Wrong vector dimension." );
        }

        public static Algebraic operator *( Vector lhs, Vector rhs )
        {
            if ( rhs.Length() == lhs._items.Length )
            {
                var r = lhs._items.Select( ( x, n ) => x * rhs._items[ n ] ).ToArray();

                return r.Aggregate( Symbol.ZERO, ( s, x ) => ( Symbol ) ( s + x ) );
            }

            throw new SymbolicException( "Wrong vector dimension." );
        }

        public static Algebraic operator *( Vector lhs, Algebraic rhs )
        {
            if ( rhs.IsScalar() )
            {
                return new Vector( lhs._items.Select( x => x * rhs ).ToArray() );
            }

            if ( rhs is Vector )
            {
                return lhs * ( Vector ) rhs;
            }

            throw new SymbolicException( "Wrong type of operand." );
        }

        public static Algebraic operator /( Vector lhs, Algebraic rhs )
        {
            if ( rhs.IsScalar() )
            {
                return new Vector( lhs._items.Select( x => x / rhs ).ToArray() );
            }

            throw new SymbolicException( "Divide not implemented for vectors" );
        }

        #endregion

        #region Others

        public virtual Algebraic[] ToArray()
        {
            return _items;
        }

        internal virtual List ToList()
        {
            return _items.ToList();
        }

        public virtual double[] Double
        {
            get
            {
                var x = new double[ _items.Length ];

                for ( int i = 0; i < _items.Length; i++ )
                {
                    var c = _items[ i ];

                    if ( !( c is Symbol ) )
                    {
                        throw new SymbolicException( "Vector element not constant:" + c );
                    }

                    x[ i ] = ( ( Symbol ) c ).ToComplex().Re;
                }

                return x;
            }
        }

        public virtual Vector Reverse()
        {
            return new Vector( _items.Reverse().ToArray() );
        }

        public virtual void set( int i, Algebraic x )
        {
            if ( i >= 0 && i < _items.Length )
            {
                _items[ i ] = x;
            }
            else
            {
                throw new SymbolicException( "Index out of bounds." );
            }
        }

        public virtual int Length()
        {
            return _items.Length;
        }

        public override bool IsScalar()
        {
            return false;
        }

        public override bool IsConstant()
        {
            return _items.All( t => t.IsConstant() );
        }

        public override bool IsNumber()
        {
            return _items.Aggregate( _items[ 0 ].IsNumber(), ( r, x ) => r && x.IsNumber() );
        }

        public override Algebraic Reduce()
        {
            return _items.Length == 1 ? _items[ 0 ] : this;
        }

        public override Algebraic Conj()
        {
            return new Vector( _items.Select( x => x.Conj() ).ToArray() );
        }

        public override Algebraic Map( LambdaAlgebraic f )
        {
            return new Vector( _items.Select( f.SymEval ).ToArray() );
        }

        public override Algebraic Map( LambdaAlgebraic f, Algebraic args )
        {

            if ( args is Vector && ( ( Vector ) args ).Length() == _items.Length )
            {
                // TODO: Check this
                var v = _items.Select( ( x, n ) => x.Map( f, ( ( Vector ) args )[ n ] ) ).ToList();

                if ( v.Any( x => !( x is Algebraic ) ) )
                {
                    throw new SymbolicException( "Cannot evaluate function to algebraic." );
                }

                return new Vector( v.Cast<Algebraic>().ToArray() );
            }
            else
            {
                // TODO: Check this
                var v = _items.Select( x => x.Map( f, args ) ).ToList();

                if ( v.Any( x => !( x is Algebraic ) ) )
                {
                    throw new SymbolicException( "Cannot evaluate function to algebraic." );
                }

                return new Vector( v.Cast<Algebraic>().ToArray() );
            }
        }

        public override Algebraic Value( Variable item, Algebraic a )
        {
            return new Vector( _items.Select( x => x.Value( item, a ) ).ToArray() );
        }

        public override Algebraic Derive( Variable item )
        {
            return new Vector( _items.Select( x => x.Derive( item ) ).ToArray() );
        }

        public override Algebraic Integrate( Variable item )
        {
            return new Vector( _items.Select( x => x.Integrate( item ) ).ToArray() );
        }

        public override double Norm()
        {
            return _items.Sum( t => t.Norm() );
        }

        public override bool Equals( object x )
        {
            if ( !( x is Vector ) || ( ( Vector ) x )._items.Length != _items.Length )
            {
                return false;
            }

            return !_items.Where( ( t, i ) => !t.Equals( ( ( Vector ) x )._items[ i ] ) ).Any();
        }

        public IEnumerator GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        public override string ToString()
        {
            return "[ " + string.Join( "  ", _items.Select( x => StringFmt.Compact( x.ToString() ) ).ToArray() ) + " ]";
        }

        public override bool Depends( Variable item )
        {
            return _items.Any( t => t.Depends( item ) );
        }

        #endregion

        #endregion

        #region Properties

        public Algebraic this[ int index ]
        {
            get
            {
                if ( index < 0 || index >= _items.Length )
                {
                    throw new SymbolicException( "Index out of bounds." );
                }

                return _items[ index ];
            }
            set
            {
                _items[ index ] = value;
            }
        }

        #endregion

    }
}

using System;
using System.Collections;
using System.Linq;

namespace Tiny.Science.Symbolic
{
    public class Matrix : Algebraic
    {

        #region Private fields

        private Algebraic[][] a;

        #endregion

        #region Constructors

        public Matrix( Algebraic[][] a )
        {
            this.a = a;
        }

        public Matrix( Algebraic x, int nrow, int ncol )
        {
            a = CreateRectangularArray<Algebraic>( nrow, ncol );

            for ( int i = 0; i < nrow; i++ )
            {
                for ( int k = 0; k < ncol; k++ )
                {
                    a[ i ][ k ] = x;
                }
            }
        }

        public Matrix( int nrow, int ncol ) : this( Symbol.ZERO, nrow, ncol )
        {
        }

        public Matrix( double[][] b, int nr, int nc )
        {
            a = CreateRectangularArray<Algebraic>( nr, nc );

            nr = Math.Min( nr, b.Length );
            nc = Math.Min( nc, b[ 0 ].Length );

            for ( int i = 0; i < nr; i++ )
            {
                for ( int k = 0; k < nc; k++ )
                {
                    a[ i ][ k ] = new Complex( b[ i ][ k ] );
                }
            }
        }

        public Matrix( double[][] b ) : this( b, b.Length, b[ 0 ].Length )
        {
        }

        public Matrix( Algebraic x )
        {
            if ( x == null )
            {
                a = new[] { new Algebraic[] { Symbol.ZERO } };
            }
            else if ( x is Vector )
            {
                a = new[] { ( ( Vector ) x ).ToArray() };
            }
            else if ( x is Matrix )
            {
                a = ( ( Matrix ) x ).a;
            }
            else
            {
                a = new[] { new[] { x } };
            }
        }

        #endregion

        public static T[][] CreateRectangularArray<T>( int rows, int cols )
        {
            T[][] array = null;

            if ( rows > -1 )
            {
                array = new T[ rows ][];

                if ( cols > -1 )
                {
                    for ( int r = 0; r < rows; r++ )
                    {
                        array[ r ] = new T[ cols ];
                    }
                }
            }

            return array;
        }

        public Algebraic this[ int i, int k ]
        {
            get
            {
                if ( i < 0 || i >= a.Length || k < 0 || k >= a[ 0 ].Length )
                {
                    throw new SymbolicException( "Index out of bounds." );
                }

                return a[ i ][ k ];
            }
            set
            {
                if ( i < 0 || i >= a.Length || k < 0 || k >= a[ 0 ].Length )
                {
                    throw new SymbolicException( "Index out of bounds." );
                }

                a[ i ][ k ] = value;
            }
        }

        public virtual int Rows()
        {
            return a.Length;
        }

        public virtual int Cols()
        {
            return a[ 0 ].Length;
        }

        public virtual double[][] GetDouble( int nr, int nc )
        {
            if ( nr == 0 )
            {
                nr = a.Length;
            }

            if ( nc == 0 )
            {
                nc = a[ 0 ].Length;
            }

            var b = CreateRectangularArray<double>( nr, nc );

            nr = Math.Min( nr, a.Length );
            nc = Math.Min( nc, a[ 0 ].Length );

            for ( int i = 0; i < nr; i++ )
            {
                for ( int k = 0; k < nc; k++ )
                {
                    var x = a[ i ][ k ];

                    if ( !( x is Complex ) || x.IsComplex() )
                    {
                        throw new SymbolicException( "Not a real, double Matrix" );
                    }

                    b[ i ][ k ] = ( ( Complex ) x ).Re;
                }
            }

            return b;
        }

        public virtual double[][] Double
        {
            get
            {
                return GetDouble( 0, 0 );
            }
        }

        public virtual Algebraic col( int k )
        {
            var c = CreateRectangularArray<Algebraic>( a.Length, 1 );

            for ( int i = 0; i < a.Length; i++ )
            {
                c[ i ][ 0 ] = a[ i ][ k - 1 ];
            }

            return new Matrix( c ).Reduce();
        }

        public virtual Algebraic row( int k )
        {
            var c = new Algebraic[ a[ 0 ].Length ];

            for ( int i = 0; i < a[ 0 ].Length; i++ )
            {
                c[ i ] = a[ k - 1 ][ i ];
            }

            return new Vector( c ).Reduce();
        }

        public virtual void Insert( Matrix x, Index idx )
        {
            if ( idx.row_max > Rows() || idx.col_max > Cols() )
            {
                var e = new Matrix( Math.Max( idx.row_max, Rows() ), Math.Max( idx.col_max, Cols() ) );

                for ( int i = 0; i < Rows(); i++ )
                {
                    for ( int k = 0; k < Cols(); k++ )
                    {
                        e.a[ i ][ k ] = a[ i ][ k ];
                    }
                }

                a = e.a;
            }

            if ( x.Rows() == 1 && x.Cols() == 1 )
            {
                foreach ( int r in idx.row )
                {
                    foreach ( int c in idx.col )
                    {
                        a[ r - 1 ][ c - 1 ] = x.a[ 0 ][ 0 ];
                    }
                }

                return;
            }

            if ( x.Rows() == idx.row.Length && x.Cols() == idx.col.Length )
            {
                for ( int i = 0; i < idx.row.Length; i++ )
                {
                    for ( int k = 0; k < idx.col.Length; k++ )
                    {
                        a[ idx.row[ i ] - 1 ][ idx.col[ k ] - 1 ] = x.a[ i ][ k ];
                    }
                }

                return;
            }

            throw new SymbolicException( "Wrong index dimension." );
        }

        public virtual Matrix Extract( Index idx )
        {
            if ( idx.row_max > Rows() || idx.col_max > Cols() )
            {
                throw new SymbolicException( "Index out of range." );
            }

            var x = new Matrix( idx.row.Length, idx.col.Length );

            for ( int i = 0; i < idx.row.Length; i++ )
            {
                for ( int k = 0; k < idx.col.Length; k++ )
                {
                    x.a[ i ][ k ] = a[ idx.row[ i ] - 1 ][ idx.col[ k ] - 1 ];
                }
            }
            return x;
        }

        public static Matrix Column( Vector x )
        {
            return new Matrix( x ).transpose();
        }

        public static Matrix row( Vector x )
        {
            return new Matrix( x );
        }

        public override Algebraic Conj()
        {
            var b = CreateRectangularArray<Algebraic>( a.Length, a[ 0 ].Length );

            for ( int i = 0; i < a.Length; i++ )
            {
                for ( int k = 0; k < a[ 0 ].Length; k++ )
                {
                    b[ i ][ k ] = a[ i ][ k ].Conj();
                }
            }

            return new Matrix( b );
        }

        protected override Algebraic Add( Algebraic x )
        {
            if ( x.IsScalar() )
            {
                x = x.Promote( this );
            }

            if ( x is Matrix && Equalsized( ( Matrix ) x ) )
            {
                var b = CreateRectangularArray<Algebraic>( a.Length, a[ 0 ].Length );

                for ( int i = 0; i < a.Length; i++ )
                {
                    for ( int k = 0; k < a[ 0 ].Length; k++ )
                    {
                        b[ i ][ k ] = a[ i ][ k ] + ( ( Matrix ) x ).a[ i ][ k ];
                    }
                }

                return new Matrix( b );
            }

            throw new SymbolicException( "Wrong arguments for add:" + this + "," + x );
        }

        public override bool IsScalar()
        {
            return false;
        }

        public virtual bool Equalsized( Matrix x )
        {
            return Rows() == x.Rows() && Cols() == x.Cols();
        }

        protected override Algebraic Mul( Algebraic x )
        {
            if ( x.IsScalar() )
            {
                var b = CreateRectangularArray<Algebraic>( a.Length, a[ 0 ].Length );

                for ( int i = 0; i < a.Length; i++ )
                {
                    for ( int k = 0; k < a[ 0 ].Length; k++ )
                    {
                        b[ i ][ k ] = a[ i ][ k ] * x;
                    }
                }

                return new Matrix( b );
            }

            var xm = new Matrix( x );

            if ( Cols() != xm.Rows() )
            {
                throw new SymbolicException( "Matrix dimensions wrong." );
            }

            var b1 = CreateRectangularArray<Algebraic>( a.Length, xm.a[ 0 ].Length );

            for ( int i = 0; i < a.Length; i++ )
            {
                for ( int k = 0; k < xm.a[ 0 ].Length; k++ )
                {
                    b1[ i ][ k ] = a[ i ][ 0 ] * xm.a[ 0 ][ k ];

                    for ( int l = 1; l < xm.a.Length; l++ )
                    {
                        b1[ i ][ k ] = b1[ i ][ k ] + a[ i ][ l ] * xm.a[ l ][ k ];
                    }
                }
            }

            return new Matrix( b1 );
        }

        protected override Algebraic Div( Algebraic x )
        {
            if ( x.IsScalar() )
            {
                var b = CreateRectangularArray<Algebraic>( a.Length, a[ 0 ].Length );

                for ( int i = 0; i < a.Length; i++ )
                {
                    for ( int k = 0; k < a[ 0 ].Length; k++ )
                    {
                        b[ i ][ k ] = a[ i ][ k ] / x;
                    }
                }

                return new Matrix( b );
            }

            return Mul( new Matrix( x ).pseudoinverse() );
        }

        public static Matrix Eye( int nr, int nc )
        {
            var b = CreateRectangularArray<Algebraic>( nr, nc );

            for ( int i = 0; i < nr; i++ )
            {
                for ( int k = 0; k < nc; k++ )
                {
                    b[ i ][ k ] = i == k ? Symbol.ONE : Symbol.ZERO;
                }
            }

            return new Matrix( b );
        }

        public virtual Algebraic mpow( int n )
        {
            if ( n == 0 )
            {
                return Eye( a.Length, a[ 0 ].Length );
            }

            if ( n == 1 )
            {
                return this;
            }

            if ( n > 1 )
            {
                return Pow( n );
            }

            return new Matrix( mpow( -n ) ).invert();

        }

        public override Algebraic Reduce()
        {
            return a.Length == 1 ? new Vector( a[ 0 ] ).Reduce() : this;
        }

        public override Algebraic Derive( Variable item )
        {
            var b = CreateRectangularArray<Algebraic>( Rows(), Cols() );

            for ( int i = 0; i < a.Length; i++ )
            {
                for ( int k = 0; k < a[ 0 ].Length; k++ )
                {
                    b[ i ][ k ] = a[ i ][ k ].Derive( item );
                }
            }

            return new Matrix( b );
        }

        public override Algebraic Integrate( Variable item )
        {
            var b = CreateRectangularArray<Algebraic>( Rows(), Cols() );

            for ( int i = 0; i < a.Length; i++ )
            {
                for ( int k = 0; k < a[ 0 ].Length; k++ )
                {
                    b[ i ][ k ] = a[ i ][ k ].Integrate( item );
                }
            }

            return new Matrix( b );
        }

        public override double Norm()
        {
            return a.Sum( t => t.Sum( x => x.Norm() ) );
        }

        public override bool IsConstant()
        {
            foreach ( var t in a )
            {
                return t.Any( x => !x.IsConstant() );
            }

            return true;
        }

        public override bool Equals( object x )
        {
            if ( !( x is Matrix ) || !Equalsized( ( Matrix ) x ) )
            {
                return false;
            }

            for ( int i = 0; i < a.Length; i++ )
            {
                for ( int k = 0; k < a[ 0 ].Length; k++ )
                {
                    if ( !a[ i ][ k ].Equals( ( ( Matrix ) x ).a[ i ][ k ] ) )
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public override Algebraic Map( LambdaAlgebraic f, Algebraic arg )
        {
            var b = CreateRectangularArray<Algebraic>( a.Length, a[ 0 ].Length );

            if ( arg is Matrix && Equalsized( ( Matrix ) arg ) )
            {
                for ( int i = 0; i < a.Length; i++ )
                {
                    for ( int k = 0; k < a[ 0 ].Length; k++ )
                    {
                        var c = ( ( Matrix ) arg )[ i, k ];

                        object r = a[ i ][ k ].Map( f, c );

                        if ( r is Algebraic )
                        {
                            b[ i ][ k ] = ( Algebraic ) r;
                        }
                        else
                        {
                            throw new SymbolicException( "Cannot evaluate function to algebraic." );
                        }
                    }
                }
            }
            else
            {
                for ( int i = 0; i < a.Length; i++ )
                {
                    for ( int k = 0; k < a[ 0 ].Length; k++ )
                    {
                        object r = a[ i ][ k ].Map( f, arg );
                        if ( r is Algebraic )
                        {
                            b[ i ][ k ] = ( Algebraic ) r;
                        }
                        else
                        {
                            throw new SymbolicException( "Cannot evaluate function to algebraic." );
                        }
                    }
                }
            }
            return new Matrix( b );
        }
        public override Algebraic Value( Variable item, Algebraic x )
        {
            var b = CreateRectangularArray<Algebraic>( a.Length, a[ 0 ].Length );

            for ( int i = 0; i < a.Length; i++ )
            {
                for ( int k = 0; k < a[ 0 ].Length; k++ )
                {
                    b[ i ][ k ] = a[ i ][ k ].Value( item, x );
                }
            }

            return new Matrix( b );
        }

        public override string ToString()
        {
            int max = 0;
            string r = "";

            foreach ( var t in a )
            {
                for ( int k = 0; k < a[ 0 ].Length; k++ )
                {
                    int l = StringFmt.Compact( t[ k ].ToString() ).Length;

                    if ( l > max )
                    {
                        max = l;
                    }
                }
            }

            max += 2;

            foreach ( var t in a )
            {
                r += "\n  ";

                for ( int k = 0; k < a[ 0 ].Length; k++ )
                {
                    var c = StringFmt.Compact( t[ k ].ToString() );

                    r += c;

                    for ( int m = 0; m < max - c.Length; m++ )
                    {
                        r += " ";
                    }
                }
            }

            return r;
        }

        public override void Print()
        {
            int max = 0;

            foreach ( var t in a )
            {
                for ( int k = 0; k < a[ 0 ].Length; k++ )
                {
                    int l = StringFmt.Compact( t[ k ].ToString() ).Length;

                    if ( l > max )
                    {
                        max = l;
                    }
                }
            }

            max += 2;

            var p = Session.Proc;

            for ( int i = 0; i < a.Length; i++ )
            {
                p.print( "\n  " );
                for ( int k = 0; k < a[ 0 ].Length; k++ )
                {
                    string r = StringFmt.Compact( a[ i ][ k ].ToString() );
                    p.print( r );
                    for ( int m = 0; m < max - r.Length; m++ )
                    {
                        p.print( " " );
                    }
                }
            }
        }
        public override Algebraic Map( LambdaAlgebraic f )
        {
            var cn = CreateRectangularArray<Algebraic>( a.Length, a[ 0 ].Length );
            for ( int i = 0; i < a.Length; i++ )
            {
                for ( int k = 0; k < a[ 0 ].Length; k++ )
                {
                    cn[ i ][ k ] = f.SymEval( a[ i ][ k ] );
                }
            }
            return new Matrix( cn );
        }
        public virtual Matrix transpose()
        {
            var b = CreateRectangularArray<Algebraic>( a[ 0 ].Length, a.Length );
            for ( int i = 0; i < a.Length; i++ )
            {
                for ( int k = 0; k < a[ 0 ].Length; k++ )
                {
                    b[ k ][ i ] = a[ i ][ k ];
                }
            }
            return new Matrix( b );
        }
        public virtual Matrix adjunkt()
        {
            var b = CreateRectangularArray<Algebraic>( a[ 0 ].Length, a.Length );
            for ( int i = 0; i < a.Length; i++ )
            {
                for ( int k = 0; k < a[ 0 ].Length; k++ )
                {
                    b[ k ][ i ] = a[ i ][ k ].Conj();
                }
            }
            return new Matrix( b );
        }

        public virtual Matrix invert()
        {
            var _det = det();

            if ( _det.Equals( Symbol.ZERO ) )
            {
                throw new SymbolicException( "Matrix not invertible." );
            }

            var b = CreateRectangularArray<Algebraic>( a.Length, a.Length );

            if ( a.Length == 1 )
            {
                b[ 0 ][ 0 ] = Symbol.ONE / _det;
            }
            else
            {
                for ( int i = 0; i < a.Length; i++ )
                {
                    for ( int k = 0; k < a[ 0 ].Length; k++ )
                    {
                        b[ i ][ k ] = unterdet( k, i ) / _det;
                    }
                }
            }

            return new Matrix( b );
        }

        public virtual Algebraic min()
        {
            var r = new Algebraic[ Cols() ];
            for ( int i = 0; i < Cols(); i++ )
            {
                var min = a[ 0 ][ i ];
                if ( !( min is Symbol ) )
                {
                    throw new SymbolicException( "MIN requires constant arguments." );
                }
                for ( int k = 1; k < Rows(); k++ )
                {
                    var x = a[ k ][ i ];
                    if ( !( x is Symbol ) )
                    {
                        throw new SymbolicException( "MIN requires constant arguments." );
                    }
                    if ( ( ( Symbol ) x ).Smaller( ( Symbol ) min ) )
                    {
                        min = x;
                    }
                }
                r[ i ] = min;
            }
            return ( new Vector( r ) ).Reduce();
        }
        public virtual Algebraic max()
        {
            var r = new Algebraic[ Cols() ];
            for ( int i = 0; i < Cols(); i++ )
            {
                var max = a[ 0 ][ i ];
                if ( !( max is Symbol ) )
                {
                    throw new SymbolicException( "MAX requires constant arguments." );
                }
                for ( int k = 1; k < Rows(); k++ )
                {
                    var x = a[ k ][ i ];
                    if ( !( x is Symbol ) )
                    {
                        throw new SymbolicException( "MAX requires constant arguments." );
                    }
                    if ( ( ( Symbol ) max ).Smaller( ( Symbol ) x ) )
                    {
                        max = x;
                    }
                }
                r[ i ] = max;
            }
            return ( new Vector( r ) ).Reduce();
        }

        public virtual Algebraic find()
        {
            var v = new ArrayList();

            for ( int i = 0; i < Rows(); i++ )
            {
                for ( int k = 0; k < Cols(); k++ )
                {
                    if ( !Symbol.ZERO.Equals( a[ i ][ k ] ) )
                    {
                        v.Add( new Complex( i * Rows() + k + 1.0 ) );
                    }
                }
            }

            var vx = Vector.Create( v );

            if ( Rows() == 1 )
            {
                return vx;
            }

            return Column( vx );
        }

        public virtual Polynomial CharPoly( Variable x )
        {
            var p = new Polynomial( x );

            var m = ( Matrix ) ( this - Eye( a.Length, a[ 0 ].Length ) * p );

            p = ( Polynomial ) m.det2();
            p = ( Polynomial ) p.Rat();

            return p;
        }

        public virtual Vector EigenValues()
        {
            Variable x = SimpleVariable.top;

            var p = CharPoly( x );
            var ps = p.square_free_dec( p._v );

            Vector r;

            var v = new ArrayList();

            for ( int i = 0; i < ps.Length; i++ )
            {
                if ( !( ps[ i ] is Polynomial ) )
                    continue;

                r = ( ( Polynomial ) ps[ i ] ).Monic().roots();

                for ( int k = 0; r != null && k < r.Length(); k++ )
                {
                    for ( int j = 0; j <= i; j++ )
                    {
                        v.Add( r[ k ] );
                    }
                }
            }

            return Vector.Create( v );
        }

        public virtual Algebraic det()
        {
            if ( a.Length != a[ 0 ].Length )
            {
                return Symbol.ZERO;
            }

            switch ( a.Length )
            {
                case 1:
                    return a[ 0 ][ 0 ];

                case 2:

                    return a[ 0 ][ 0 ] * a[ 1 ][ 1 ] - a[ 0 ][ 1 ] * a[ 1 ][ 0 ];

                case 3:

                    return a[ 0 ][ 0 ] * a[ 1 ][ 1 ] * a[ 2 ][ 2 ] + a[ 0 ][ 1 ] * a[ 1 ][ 2 ] * a[ 2 ][ 0 ] + a[ 0 ][ 2 ] * a[ 1 ][ 0 ] * a[ 2 ][ 1 ]
                        - ( a[ 0 ][ 2 ] * a[ 1 ][ 1 ] * a[ 2 ][ 0 ] + a[ 0 ][ 0 ] * a[ 1 ][ 2 ] * a[ 2 ][ 1 ] + a[ 0 ][ 1 ] * a[ 1 ][ 0 ] * a[ 2 ][ 2 ] );

                default:
                    var c = copy();

                    int perm = c.rank_decompose( null, null );

                    var r = c[ 0, 0 ];

                    for ( int i = 1; i < c.Rows(); i++ )
                    {
                        r = r * c[ i, i ];
                    }

                    return perm % 2 == 0 ? r : r * Symbol.MINUS;
            }
        }

        internal virtual Algebraic det2()
        {
            if ( a.Length != a[ 0 ].Length )
            {
                return Symbol.ZERO;
            }

            if ( a.Length < 4 )
            {
                return det();
            }

            var d = a.Select( ( x, n ) => unterdet( n, 0 ) * x[ 0 ] ).Aggregate( ( s, x ) => s + x );

            return d;
        }

        public virtual Algebraic unterdet( int i, int k )
        {
            if ( i < 0 || i > a.Length || k < 0 || k > a[ 0 ].Length )
            {
                throw new SymbolicException( "Operation not possible." );
            }
            var b = CreateRectangularArray<Algebraic>( a.Length - 1, a[ 0 ].Length - 1 );
            int i1, i2, k1, k2;
            for ( i1 = 0, i2 = 0; i1 < a.Length - 1; i1++, i2++ )
            {
                if ( i2 == i )
                {
                    i2++;
                }
                for ( k1 = 0, k2 = 0; k1 < a[ 0 ].Length - 1; k1++, k2++ )
                {
                    if ( k2 == k )
                    {
                        k2++;
                    }
                    b[ i1 ][ k1 ] = this.a[ i2 ][ k2 ];
                }
            }

            Algebraic u = ( new Matrix( b ) ).det2();

            if ( ( i + k ) % 2 == 0 )
            {
                return u;
            }

            return -u;
        }

        internal virtual int pivot( int k )
        {
            if ( k >= Cols() )
            {
                return k;
            }
            int _pivot = k;
            var maxa = a[ k ][ k ].Norm();
            for ( int i = k + 1; i < Rows(); i++ )
            {
                var dummy = a[ i ][ k ].Norm();
                if ( dummy > maxa )
                {
                    maxa = dummy;
                    _pivot = i;
                }
            }
            if ( maxa == 0.0 )
            {
                int kn = pivot( k + 1 );
                if ( kn == k + 1 )
                {
                    return k;
                }
                else
                {
                    return kn;
                }
            }
            if ( _pivot != k )
            {
                for ( int j = k; j < Cols(); j++ )
                {
                    var dummy = a[ _pivot ][ j ];
                    a[ _pivot ][ j ] = a[ k ][ j ];
                    a[ k ][ j ] = dummy;
                }
            }
            return _pivot;
        }

        private bool row_zero( int k )
        {
            if ( k >= Rows() )
            {
                return true;
            }

            for ( int i = 0; i < Cols(); i++ )
            {
                if ( a[ k ][ i ] != Symbol.ZERO )
                {
                    return false;
                }
            }

            return true;
        }

        public override bool IsNumber()
        {
            bool exakt = true;

            foreach ( var t in a )
            {
                for ( int k = 0; k < a[ 0 ].Length; k++ )
                {
                    exakt = exakt && t[ k ].IsNumber();
                }
            }

            return exakt;
        }

        private void remove_row( int i )
        {
            if ( i >= Rows() )
            {
                return;
            }

            var b = new Algebraic[ Rows() - 1 ][];

            for ( int k = 0; k < i; k++ )
            {
                b[ k ] = a[ k ];
            }

            for ( int k = i + 1; k < Rows(); k++ )
            {
                b[ k - 1 ] = a[ k ];
            }

            a = b;
        }

        internal virtual void remove_col( int i )
        {
            if ( i >= Cols() )
            {
                return;
            }

            var b = CreateRectangularArray<Algebraic>( Rows(), Cols() - 1 );

            for ( int j = 0; j < Rows(); j++ )
            {
                for ( int k = 0; k < i; k++ )
                {
                    b[ j ][ k ] = a[ j ][ k ];
                }

                for ( int k = i + 1; k < Cols(); k++ )
                {
                    b[ j ][ k - 1 ] = a[ j ][ k ];
                }
            }

            a = b;
        }

        internal static Matrix elementary( int n, int i, int k, Algebraic m )
        {
            var t = Eye( n, n );

            t.a[ i ][ k ] = m;

            return t;
        }

        internal static Matrix elementary( int n, int i, int k )
        {
            var t = Eye( n, n );

            t.a[ k ][ k ] = t.a[ i ][ i ] = Symbol.ZERO;
            t.a[ i ][ k ] = t.a[ k ][ i ] = Symbol.ONE;

            return t;
        }

        public virtual int rank_decompose( Matrix B, Matrix P )
        {
            int m = Rows(), n = Cols(), perm = 0;

            var C = Eye( m, m );
            var D = Eye( m, m );

            for ( int k = 0; k < m - 1; k++ )
            {
                int _pivot = pivot( k );

                if ( _pivot != k )
                {
                    var E = elementary( m, k, _pivot );

                    C = ( Matrix ) ( C * E );
                    D = ( Matrix ) ( D * E );

                    perm++;
                }

                int p = k;

                for ( p = k; p < n; p++ )
                {
                    if ( !a[ k ][ p ].Equals( Symbol.ZERO ) )
                    {
                        break;
                    }
                }

                if ( p < n )
                {
                    for ( int i = k + 1; i < m; i++ )
                    {
                        if ( !a[ i ][ p ].Equals( Symbol.ZERO ) )
                        {
                            var f = a[ i ][ p ] / a[ k ][ p ];

                            a[ i ][ p ] = Symbol.ZERO;

                            for ( int j = p + 1; j < n; j++ )
                            {
                                a[ i ][ j ] = a[ i ][ j ] - f * a[ k ][ j ];
                            }

                            C = ( Matrix ) ( C * elementary( m, i, k, f ) );
                        }
                    }
                }
            }

            int nm = Math.Max( n, m );

            for ( int i = nm - 1; i >= 0; i-- )
            {
                if ( !row_zero( i ) )
                    continue;

                remove_row( i );

                C.remove_col( i );
            }

            if ( B != null )
            {
                B.a = C.a;
            }

            if ( P != null )
            {
                P.a = D.a;
            }

            return perm;
        }

        public virtual Matrix copy()
        {
            int nr = Rows(), nc = Cols();

            var b = CreateRectangularArray<Algebraic>( nr, nc );

            for ( int i = 0; i < nr; i++ )
            {
                for ( int k = 0; k < nc; k++ )
                {
                    b[ i ][ k ] = a[ i ][ k ];
                }
            }

            return new Matrix( b );
        }

        public virtual Matrix pseudoinverse()
        {
            if ( !det().Equals( Symbol.ZERO ) )
            {
                return invert();
            }

            var c = copy();
            var b = new Matrix( 1, 1 );

            c.rank_decompose( b, null );

            int rank = c.Rows();

            if ( rank == Rows() )
            {
                var ad = adjunkt();

                return ( Matrix ) ( ad * ( ( Matrix ) ( this * ad ) ).invert() );
            }
            else if ( rank == Cols() )
            {
                var ad = adjunkt();

                return ( Matrix ) ( ( ( Matrix ) ( ad * this ) ).invert() * ad );
            }

            var ca = c.adjunkt();
            var ba = b.adjunkt();

            return ( Matrix ) ( ca.Mul( ( ( Matrix ) ( c * ca ) ).invert() ) * ( ( ( Matrix ) ( ba * b ) ).invert() ) * ba );
        }
    }
}

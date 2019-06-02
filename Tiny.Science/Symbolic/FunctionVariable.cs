using System;

namespace Tiny.Science.Symbolic
{
    public class FunctionVariable : Variable
    {

        #region Properties

        public string Name { get; }
        public Algebraic Var { get; private set; }
        public LambdaAlgebraic Body { get; }

        #endregion

        public FunctionVariable( string fname, Algebraic fvar, LambdaAlgebraic flambda )
        {
            Name = fname;
            Var = fvar;
            Body = flambda;
        }

        public override Algebraic Derive( Variable v )
        {
            if ( Equals(v) )
            {
                return Symbol.ONE;
            }

            if ( !Var.Depends(v) )
            {
                return Symbol.ZERO;
            }

            if ( Body == null )
            {
                throw new SymbolicException( $"Can not differentiate {Name} : No definition." );
            }

            var diffrule = Body.diffrule;

            if ( diffrule == null )
            {
                throw new SymbolicException( $"Can not differentiate {Name} : No rule available." );
            }

            var y = Lambda.evalx( diffrule, Var );

            return y * Var.Derive(v);
        }

        public virtual Algebraic Integrate( Variable x )
        {
            Var = Var.Reduce();

            if ( Body == null )
            {
                throw new SymbolicException( $"Can not integrate {Name}" );
            }

            return Body.Integrate( Var, x );
        }

        public static Algebraic Create( string f, Algebraic arg )
        {
            arg = arg.Reduce();

            var fl = Session.Store.GetValue(f);

            if ( fl != null && fl is LambdaAlgebraic )
            {
                var r = ( ( LambdaAlgebraic ) fl ).SymEval( arg );

                if ( r != null )
                {
                    return r;
                }

                if ( arg is Complex )
                {
                    return ( ( LambdaAlgebraic ) fl ).PreEval( ( Symbol ) arg );
                }
            }
            else
            {
                fl = null;
            }

            return new Polynomial( new FunctionVariable( f, arg, ( LambdaAlgebraic ) fl ) );
        }

        public override bool Equals( object x )
        {
            return x is FunctionVariable && Name.Equals( ( ( FunctionVariable ) x ).Name ) && Var.Equals( ( ( FunctionVariable ) x ).Var );
        }

        public override Algebraic Value( Variable v, Algebraic x )
        {
            if ( Equals(v) )
            {
                return x;
            }

            x = Var.Value( v, x );

            var r = Body.SymEval(x);

            if ( r != null )
            {
                return r;
            }

            if ( x is Complex )
            {
                return Body.PreEval( ( Symbol ) x );
            }

            return new Polynomial( new FunctionVariable( Name, x, Body ) );
        }

        public override bool Smaller( Variable v )
        {
            if ( v == SimpleVariable.top )
            {
                return true;
            }

            if ( v is SimpleVariable )
            {
                return false;
            }

            if ( !( ( FunctionVariable ) v ).Name.Equals( Name ) )
            {
                return Name.CompareTo( ( ( FunctionVariable ) v ).Name ) < 0;
            }

            if ( Var.Equals( ( ( FunctionVariable ) v ).Var ) )
            {
                return false;
            }

            if ( Var is Polynomial && ( ( FunctionVariable ) v ).Var is Polynomial )
            {
                var a = ( Polynomial ) Var;
                var b = ( Polynomial ) ( ( FunctionVariable ) v ).Var;

                if ( !a.Var.Equals( b.Var ) )
                {
                    return a.Var.Smaller( b.Var );
                }

                if ( a.Degree() != b.Degree() )
                {
                    return a.Degree() < b.Degree();
                }

                for ( var n = a.Coeffs.Length - 1; n >= 0; n-- )
                {
                    if ( a[n].Equals( b[n] ) ) continue;

                    if ( a[n] is Symbol && b[n] is Symbol )
                    {
                        return ( ( Symbol ) a[n] ).Smaller( ( Symbol ) b[n] );
                    }

                    return a[n].Norm() < b[n].Norm();
                }
            }

            return false;
        }

        public override Variable Conj()
        {
            if ( Name.Equals( "exp" ) || Name.Equals( "log" ) || Name.Equals( "sqrt" ) )
            {
                return new FunctionVariable( Name, Var.Conj(), Body );
            }

            throw new SymbolicException( $"Can\'t calculate cc for Function {Name}" );
        }

        public override string ToString()
        {
            var a = Var.ToString();

            if ( a.StartsWith( "(", StringComparison.Ordinal ) && a.EndsWith( ")", StringComparison.Ordinal ) )
            {
                return Name + a;
            }

            return $"{Name}({a})";
        }
    }
}

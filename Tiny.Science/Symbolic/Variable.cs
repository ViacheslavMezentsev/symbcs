namespace Tiny.Science.Symbolic
{
    public abstract class Variable
    {

        #region Abstract

        public abstract Algebraic Derive( Variable x );
        public abstract override bool Equals( object x );
        public abstract bool Smaller( Variable v );
        public abstract Algebraic Value( Variable v, Algebraic a );

        #endregion

        #region Virtual

        public virtual Variable Conj()
        {
            return this;
        }

        #endregion

    }
}

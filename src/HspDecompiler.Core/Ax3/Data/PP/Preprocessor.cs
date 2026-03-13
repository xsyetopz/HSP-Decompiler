namespace HspDecompiler.Core.Ax3.Data.PP
{
    internal abstract class Preprocessor
    {
        protected Preprocessor() { }
        protected Preprocessor(int index)
        {
            this.index = index;
        }
        protected readonly int index;
        public abstract override string ToString();
    }
}

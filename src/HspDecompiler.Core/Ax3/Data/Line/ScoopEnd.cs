namespace HspDecompiler.Core.Ax3.Data.Line
{
    internal sealed class ScoopEnd : LogicalLine
    {
        internal override bool TabDecrement
        {
            get
            {
                return true;
            }
        }
        internal override int TokenOffset
        {
            get { return -1; }
        }

        public override string ToString()
        {
            return "}";
        }
    }
}

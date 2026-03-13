namespace HspDecompiler.Core.Ax3.Data.Primitive
{
    internal sealed class UnknownPrimitive : PrimitiveToken
    {
        private UnknownPrimitive() { }
        internal UnknownPrimitive(PrimitiveTokenDataSet dataSet)
            : base(dataSet)
        {
        }

        public override string ToString()
        {
            return DefaultName;
        }
    }
}

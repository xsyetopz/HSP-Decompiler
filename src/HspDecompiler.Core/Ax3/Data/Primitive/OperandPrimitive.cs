namespace HspDecompiler.Core.Ax3.Data.Primitive
{
    internal abstract class OperandPrimitive : PrimitiveToken
    {
        protected OperandPrimitive() { }
        internal OperandPrimitive(PrimitiveTokenDataSet dataSet)
            : base(dataSet)
        {
        }
    }
}

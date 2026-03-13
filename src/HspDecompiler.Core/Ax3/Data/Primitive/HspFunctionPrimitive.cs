namespace HspDecompiler.Core.Ax3.Data.Primitive
{
    internal class HspFunctionPrimitive : FunctionPrimitive
    {
        protected HspFunctionPrimitive() { }
        internal HspFunctionPrimitive(PrimitiveTokenDataSet dataSet)
            : base(dataSet)
        {
        }
    }

    internal sealed class OnFunctionPrimitive : HspFunctionPrimitive
    {
        private OnFunctionPrimitive() { }
        internal OnFunctionPrimitive(PrimitiveTokenDataSet dataSet)
            : base(dataSet)
        {
        }
    }

    internal sealed class OnEventFunctionPrimitive : HspFunctionPrimitive
    {
        private OnEventFunctionPrimitive() { }
        internal OnEventFunctionPrimitive(PrimitiveTokenDataSet dataSet)
            : base(dataSet)
        {
        }
    }

    internal sealed class McallFunctionPrimitive : HspFunctionPrimitive
    {
        private McallFunctionPrimitive() { }
        internal McallFunctionPrimitive(PrimitiveTokenDataSet dataSet)
            : base(dataSet)
        {
        }
    }
}

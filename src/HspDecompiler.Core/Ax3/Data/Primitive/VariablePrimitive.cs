using System.Text;
using HspDecompiler.Core.Ax3.Data.PP;

namespace HspDecompiler.Core.Ax3.Data.Primitive
{
    internal abstract class VariablePrimitive : OperandPrimitive
    {
        protected VariablePrimitive() { }
        internal VariablePrimitive(PrimitiveTokenDataSet dataSet)
            : base(dataSet)
        {
        }
    }

    internal sealed class GlobalVariablePrimitive : VariablePrimitive
    {
        readonly string varName;

        private GlobalVariablePrimitive() { }
        internal GlobalVariablePrimitive(PrimitiveTokenDataSet dataSet)
            : base(dataSet)
        {
            varName = dataSet.Parent.GetVariableName(Value);
        }

        public override string ToString()
        {
            if (varName != null)
                return varName;
            StringBuilder bld = new StringBuilder("var");
            bld.Append("_");
            bld.Append(Value.ToString());
            return bld.ToString();
        }
    }

    internal sealed class ParameterPrimitive : VariablePrimitive
    {
        private ParameterPrimitive() { }
        internal ParameterPrimitive(PrimitiveTokenDataSet dataSet)
            : base(dataSet)
        {
            param = dataSet.Parent.GetParam(Value);
            if (param != null)
                param.ParamNameIsUsed = true;
        }

        private readonly Param param = null;
        public override string ToString()
        {
            if (param != null)
                return param.ParamName;
            return DefaultName;
        }
    }
}

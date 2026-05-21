using System.Text;
using HspDecompiler.Core.Ax3.Data.PP;

namespace HspDecompiler.Core.Ax3.Data.Primitive;

internal abstract class VariablePrimitive : OperandPrimitive
{
    protected VariablePrimitive() { }

    internal VariablePrimitive(PrimitiveTokenDataSet dataSet)
        : base(dataSet) { }
}

internal sealed class GlobalVariablePrimitive : VariablePrimitive
{
    private readonly string? _varName;

    private GlobalVariablePrimitive() { }

    internal GlobalVariablePrimitive(PrimitiveTokenDataSet dataSet)
        : base(dataSet)
    {
        _varName = dataSet._parent!.GetVariableName(Value);
    }

    public override string ToString()
    {
        if (_varName != null)
        {
            return _varName;
        }

        var bld = new StringBuilder("var");
        bld.Append('_');
        bld.Append(Value);
        return bld.ToString();
    }
}

internal sealed class ParameterPrimitive : VariablePrimitive
{
    private ParameterPrimitive() { }

    internal ParameterPrimitive(PrimitiveTokenDataSet dataSet)
        : base(dataSet)
    {
        _param = dataSet._parent!.GetParam(Value);
        if (_param != null)
        {
            _param.ParamNameIsUsed = true;
        }
    }

    private readonly Param? _param;

    public override string ToString() => _param != null ? _param.ParamName : DefaultName;
}

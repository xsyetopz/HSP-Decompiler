using HspDecompiler.Core.Ax3.Data.PP;

namespace HspDecompiler.Core.Ax3.Data.Primitive;

internal abstract class FunctionPrimitive : PrimitiveToken
{
    protected FunctionPrimitive() { }

    internal FunctionPrimitive(PrimitiveTokenDataSet dataSet)
        : base(dataSet) { }
}

internal sealed class UserFunctionPrimitive : FunctionPrimitive
{
    private UserFunctionPrimitive() { }

    internal UserFunctionPrimitive(PrimitiveTokenDataSet dataSet)
        : base(dataSet)
    {
        _func = dataSet._parent!.GetUserFunction(Value);
    }

    private readonly Function? _func;

    public override string ToString() =>
        _func == null ? DefaultName : _func.FunctionName ?? DefaultName;
}

internal sealed class DllFunctionPrimitive : FunctionPrimitive
{
    private DllFunctionPrimitive() { }

    internal DllFunctionPrimitive(PrimitiveTokenDataSet dataSet)
        : base(dataSet)
    {
        _func = dataSet._parent!.GetDllFunction(Value);
    }

    private readonly Function? _func;

    public override string ToString() =>
        _func == null ? DefaultName : _func.FunctionName ?? DefaultName;
}

internal sealed class PlugInFunctionPrimitive : FunctionPrimitive
{
    private PlugInFunctionPrimitive() { }

    internal PlugInFunctionPrimitive(PrimitiveTokenDataSet dataSet)
        : base(dataSet)
    {
        int pluginIndex = dataSet._dicValue._operatorPriority;
        _cmd = dataSet._parent!.AddCmd(pluginIndex, Value);
    }

    private readonly Cmd? _cmd;

    public override string ToString() => _cmd == null ? DefaultName : _cmd.FunctionName;
}

internal sealed class ComFunctionPrimitive : FunctionPrimitive
{
    private ComFunctionPrimitive() { }

    internal ComFunctionPrimitive(PrimitiveTokenDataSet dataSet)
        : base(dataSet)
    {
        _func = dataSet._parent!.GetDllFunction(Value - 0x1000);
    }

    private readonly Function? _func;

    public override string ToString() =>
        _func == null ? DefaultName : _func.FunctionName ?? DefaultName;
}

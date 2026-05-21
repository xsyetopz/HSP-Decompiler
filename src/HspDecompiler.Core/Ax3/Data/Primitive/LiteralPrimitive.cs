using System.Globalization;
using System.Text;
using HspDecompiler.Core.Ax3.Data.PP;

namespace HspDecompiler.Core.Ax3.Data.Primitive;

internal abstract class LiteralPrimitive : OperandPrimitive
{
    protected LiteralPrimitive() { }

    internal virtual bool IsNegativeNumber => false;
    internal virtual bool IsMinusOne => false;

    internal LiteralPrimitive(PrimitiveTokenDataSet dataSet)
        : base(dataSet) { }
}

internal sealed class LabelPrimitive : LiteralPrimitive
{
    private LabelPrimitive() { }

    internal LabelPrimitive(PrimitiveTokenDataSet dataSet)
        : base(dataSet)
    {
        _label = dataSet._parent!.Labels[Value];
    }

    private readonly Label? _label;

    public override string ToString() => _label == null ? DefaultName : _label.LabelName;

    internal void LabelIsUsed()
    {
        if (_label == null)
        {
            return;
        }

        _label.Visible = true;
    }
}

internal sealed class IntegerPrimitive : LiteralPrimitive
{
    private IntegerPrimitive() { }

    internal IntegerPrimitive(PrimitiveTokenDataSet dataSet)
        : base(dataSet) { }

    internal override bool IsNegativeNumber => (Value < 0);
    internal override bool IsMinusOne => Value == -1;

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}

internal sealed class DoublePrimitive : LiteralPrimitive
{
    private DoublePrimitive() { }

    internal DoublePrimitive(PrimitiveTokenDataSet dataSet, double d)
        : base(dataSet)
    {
        _d = d;
    }

    private readonly double _d;

    internal override bool IsNegativeNumber => (_d < 0.0);

    public override string ToString() =>
        _d.ToString(
            "0.0#########################################################################################################################################################################################################################################################################################################################################################",
            CultureInfo.InvariantCulture
        );
}

internal sealed class StringPrimitive : LiteralPrimitive
{
    private StringPrimitive() { }

    internal StringPrimitive(PrimitiveTokenDataSet dataSet, string str)
        : base(dataSet)
    {
        _str = str;
    }

    private readonly string? _str;

    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append('"');
        builder.Append(_str);
        builder.Append('"');
        return builder.ToString();
    }
}

internal sealed class SymbolPrimitive : LiteralPrimitive
{
    private SymbolPrimitive() { }

    internal SymbolPrimitive(PrimitiveTokenDataSet dataSet)
        : base(dataSet) { }
}

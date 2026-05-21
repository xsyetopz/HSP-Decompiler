using System.Globalization;
using System.Text;

namespace HspDecompiler.Core.Ax3.Data.Primitive;

internal sealed class IfStatementPrimitive : HspFunctionPrimitive
{
    private IfStatementPrimitive() { }

    internal IfStatementPrimitive(PrimitiveTokenDataSet dataSet, int extraValue)
        : base(dataSet)
    {
        _extraValue = extraValue;
    }

    private readonly int _extraValue = -1;
    internal int JumpToOffset
    {
        get
        {
            if (_extraValue == -1)
            {
                return -1;
            }

            int ret = _extraValue + TokenOffset;
            if (HasLongTypeValue)
            {
                ret += 4;
            }
            else
            {
                ret += 3;
            }

            return ret;
        }
    }

    internal override string DefaultName
    {
        get
        {
            var builder = new StringBuilder();
            builder.Append("/*");
            builder.Append(_type.ToString("X02", CultureInfo.InvariantCulture));
            builder.Append(' ');
            builder.Append(_flag.ToString("X02", CultureInfo.InvariantCulture));
            builder.Append(' ');
            if (HasLongTypeValue)
            {
                builder.Append(Value.ToString("X08", CultureInfo.InvariantCulture));
            }
            else
            {
                builder.Append(Value.ToString("X04", CultureInfo.InvariantCulture));
            }

            builder.Append(' ');
            builder.Append(_extraValue.ToString("X04", CultureInfo.InvariantCulture));
            builder.Append("*/");
            return builder.ToString();
        }
    }
}

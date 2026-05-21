using System.Text;
using HspDecompiler.Core.Ax3.Data.Primitive;
using HspDecompiler.Core.Ax3.Data.Token;

namespace HspDecompiler.Core.Ax3.Data.Line;

internal class McallStatement : LogicalLine
{
    private McallStatement() { }

    internal McallStatement(
        McallFunctionPrimitive theToken,
        VariablePrimitive var,
        ExpressionToken exp,
        ArgumentToken? arg
    )
    {
        _token = theToken;
        _var = var;
        _exp = exp;
        _arg = arg;
    }

    private readonly McallFunctionPrimitive? _token;
    private readonly VariablePrimitive? _var;
    private readonly ExpressionToken? _exp;
    private readonly ArgumentToken? _arg;

    internal override int TokenOffset => _token!.TokenOffset;

    private string ToStringFunctionStyle()
    {
        if (_arg == null)
        {
            return _token!.ToString();
        }

        var builder = new StringBuilder();
        builder.Append(_token!.ToString());
        if (_var != null)
        {
            builder.Append(' ');
            builder.Append(_var.ToString());
            if (_exp != null)
            {
                builder.Append(' ');
                builder.Append(',');
                builder.Append(_var.ToString());
            }
        }
        if (_arg != null)
        {
            builder.Append(_arg.ToString());
        }
        return builder.ToString();
    }

    internal string ToString(bool convertMcall)
    {
        if (!convertMcall)
        {
            return ToStringFunctionStyle();
        }

        if (_var == null)
        {
            return ToStringFunctionStyle();
        }

        if (_exp == null)
        {
            return ToStringFunctionStyle();
        }

        if (_arg == null)
        {
            return ToStringFunctionStyle();
        }

        var builder = new StringBuilder();
        builder.Append(_var.ToString());
        builder.Append("->");
        builder.Append(_exp.ToString());
        if (_arg != null)
        {
            builder.Append(_arg.ToString(true));
        }

        return builder.ToString();
    }

    public override string ToString() => ToString(true);

    internal override void CheckLabel()
    {
        _exp?.CheckLabel();

        _arg?.CheckLabel();
    }

    internal override bool CheckRpn()
    {
        bool ret = true;
        if (_exp != null)
        {
            ret &= _exp.CheckRpn();
        }

        if (_arg != null)
        {
            ret &= _arg.CheckRpn();
        }

        return ret;
    }
}

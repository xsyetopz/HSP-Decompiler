using System.Text;
using HspDecompiler.Core.Ax3.Data.Primitive;
using HspDecompiler.Core.Ax3.Data.Token;

namespace HspDecompiler.Core.Ax3.Data.Line;

internal sealed class OnStatement : LogicalLine
{
    private OnStatement() { }

    internal OnStatement(OnFunctionPrimitive theToken, ExpressionToken? exp, FunctionToken? func)
    {
        _token = theToken;
        _exp = exp;
        _func = func;
    }

    private readonly OnFunctionPrimitive? _token;
    private readonly ExpressionToken? _exp;
    private readonly FunctionToken? _func;

    internal override int TokenOffset => _token!.TokenOffset;

    public override string ToString()
    {
        var builder = new StringBuilder();
        if (_token != null)
        {
            builder.Append(_token.ToString());
        }
        if (_exp != null)
        {
            builder.Append(' ');
            builder.Append(_exp.ToString());
        }
        if (_func != null)
        {
            builder.Append(' ');
            builder.Append(_func.ToString());
        }
        return builder.ToString();
    }

    internal override void CheckLabel()
    {
        _exp?.CheckLabel();

        _func?.CheckLabel();
    }

    internal override bool CheckRpn()
    {
        bool ret = true;
        if (_exp != null)
        {
            ret &= _exp.CheckRpn();
        }

        if (_func != null)
        {
            ret &= _func.CheckRpn();
        }

        return ret;
    }
}

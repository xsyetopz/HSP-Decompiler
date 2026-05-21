using System.Text;
using HspDecompiler.Core.Ax3.Data.Primitive;
using HspDecompiler.Core.Ax3.Data.Token;

namespace HspDecompiler.Core.Ax3.Data.Line;

internal sealed class OnEventStatement : LogicalLine
{
    private OnEventStatement() { }

    internal OnEventStatement(OnEventFunctionPrimitive theToken, FunctionToken? func)
    {
        _token = theToken;
        _func = func;
    }

    private readonly OnEventFunctionPrimitive? _token;
    private readonly FunctionToken? _func;

    internal override int TokenOffset => _token!.TokenOffset;

    public override string ToString()
    {
        var builder = new StringBuilder();
        if (_token != null)
        {
            builder.Append(_token.ToString());
        }
        if (_func != null)
        {
            builder.Append(' ');
            builder.Append(_func.ToString());
        }
        return builder.ToString();
    }

    internal override void CheckLabel() => _func?.CheckLabel();

    internal override bool CheckRpn() => _func != null ? _func.CheckRpn() : true;
}

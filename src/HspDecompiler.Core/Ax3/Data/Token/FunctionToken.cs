using System.Text;
using HspDecompiler.Core.Ax3.Data.Primitive;

namespace HspDecompiler.Core.Ax3.Data.Token;

internal sealed class FunctionToken : OperandToken
{
    private FunctionToken() { }

    internal FunctionToken(FunctionPrimitive token)
    {
        _primitive = token;
    }

    internal FunctionToken(FunctionPrimitive token, ArgumentToken theArg)
    {
        _primitive = token;
        _arg = theArg;
    }

    private readonly FunctionPrimitive? _primitive;

    internal FunctionPrimitive Primitive => _primitive!;

    private readonly ArgumentToken? _arg;

    internal override int TokenOffset => _primitive!.TokenOffset;

    public override string ToString()
    {
        if (_arg == null)
        {
            return _primitive!.ToString();
        }

        var builder = new StringBuilder(_primitive!.ToString());
        builder.Append(_arg.ToString());
        return builder.ToString();
    }

    internal override int Priority => 100;

    internal override void CheckLabel() => _arg?.CheckLabel();

    internal override bool CheckRpn() => _arg != null ? _arg.CheckRpn() : true;
}

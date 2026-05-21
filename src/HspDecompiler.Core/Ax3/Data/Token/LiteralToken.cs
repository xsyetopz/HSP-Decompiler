using HspDecompiler.Core.Ax3.Data.Primitive;

namespace HspDecompiler.Core.Ax3.Data.Token;

internal sealed class LiteralToken : OperandToken
{
    private LiteralToken() { }

    internal LiteralToken(LiteralPrimitive token)
    {
        _token = token;
    }

    private readonly LiteralPrimitive? _token;
    internal bool IsNegativeNumber => _token == null ? false : _token.IsNegativeNumber;

    internal bool IsMinusOne => _token!.IsMinusOne;

    internal override int TokenOffset => _token == null ? -1 : _token.TokenOffset;

    public override string ToString() =>
        (_token!.CodeType == HspCodeType.Symbol) && (_token.ToString() == "?")
            ? ""
            : _token.ToString();

    internal override int Priority => IsNegativeNumber ? -1 : 100;

    internal override void CheckLabel()
    {
        var label = _token as LabelPrimitive;
        label?.LabelIsUsed();
    }
}

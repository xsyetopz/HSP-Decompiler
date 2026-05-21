namespace HspDecompiler.Core.Ax3.Data.Token;

internal sealed class UnknownToken : CodeToken
{
    private UnknownToken() { }

    internal UnknownToken(PrimitiveToken token)
    {
        _token = token;
    }

    private readonly PrimitiveToken? _token;
    internal override int TokenOffset => _token!.TokenOffset;

    public override string ToString() => " /*" + _token!.ToString() + "*/";
}

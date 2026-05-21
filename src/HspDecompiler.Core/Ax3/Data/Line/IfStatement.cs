using System.Text;
using HspDecompiler.Core.Ax3.Data.Primitive;
using HspDecompiler.Core.Ax3.Data.Token;

namespace HspDecompiler.Core.Ax3.Data.Line;

internal sealed class IfStatement : LogicalLine
{
    private IfStatement() { }

    internal IfStatement(IfStatementPrimitive token)
    {
        _ifToken = token;
    }

    internal IfStatement(IfStatementPrimitive token, ArgumentToken theArg)
    {
        _ifToken = token;
        _arg = theArg;
    }

    private readonly IfStatementPrimitive? _ifToken;
    private readonly ArgumentToken? _arg;

    internal override int TokenOffset => _ifToken == null ? -1 : _ifToken.TokenOffset;

    internal int JumpToOffset => _ifToken!.JumpToOffset;

    internal bool isIfStatement =>
        (_ifToken!.CodeType & HspCodeType.IfStatement) == HspCodeType.IfStatement;

    internal bool isElseStatement =>
        (_ifToken!.CodeType & HspCodeType.ElseStatement) == HspCodeType.ElseStatement;

    private bool _scopeEndIsDefined;
    internal bool ScopeEndIsDefined
    {
        get => _scopeEndIsDefined;
        set => _scopeEndIsDefined = value;
    }
    internal override bool TabIncrement => _scopeEndIsDefined;

    public override string ToString()
    {
        var builder = new StringBuilder(_ifToken!.ToString());
        if (_arg != null)
        {
            builder.Append(" (");
            builder.Append(_arg.ToString());
            builder.Append(" )");
        }
        builder.Append(" {");
        return builder.ToString();
    }

    internal override void CheckLabel() => _arg?.CheckLabel();

    internal override bool CheckRpn() => _arg != null ? _arg.CheckRpn() : true;
}

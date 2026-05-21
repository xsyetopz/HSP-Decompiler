using System.Collections.Generic;
using System.Text;

namespace HspDecompiler.Core.Ax3.Data.Line;

internal sealed class UnknownLine : LogicalLine
{
    private UnknownLine() { }

    internal UnknownLine(List<PrimitiveToken> primitives)
    {
        _tokens = new PrimitiveToken[primitives.Count];
        primitives.CopyTo(_tokens);
    }

    private readonly PrimitiveToken[]? _tokens;

    internal override int TokenOffset =>
        (_tokens == null) || (_tokens.Length == 0) ? -1 : _tokens[0].TokenOffset;

    public override string ToString()
    {
        if ((_tokens == null) || (_tokens.Length == 0))
        {
            return "//空";
        }

        var builder = new StringBuilder("//");
        foreach (PrimitiveToken token in _tokens)
        {
            builder.Append(' ');
            builder.Append(token.ToString());
        }
        return builder.ToString();
    }
}

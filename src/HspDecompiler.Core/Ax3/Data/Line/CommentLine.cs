using System.Text;

namespace HspDecompiler.Core.Ax3.Data.Line;

internal class CommentLine : LogicalLine
{
    internal CommentLine() { }

    internal CommentLine(string str)
    {
        _comment = str;
    }

    private readonly string? _comment;
    internal override int TokenOffset => -1;

    internal override int TabCount => 0;

    public override string ToString()
    {
        if (_comment == null)
        {
            return string.Empty;
        }

        var strbd = new StringBuilder();
        strbd.Append("//");
        strbd.Append(_comment);
        return strbd.ToString();
    }
}

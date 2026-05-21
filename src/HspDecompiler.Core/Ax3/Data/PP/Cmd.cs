using System.Text;

namespace HspDecompiler.Core.Ax3.Data.PP;

internal sealed class Cmd : Preprocessor
{
    private Cmd() { }

    internal Cmd(int pluginIndex, int methodIndex)
    {
        _pluginIndex = pluginIndex;
        _methodIndex = methodIndex;
    }

    private readonly int _pluginIndex;
    private readonly int _methodIndex;

    internal string FunctionName
    {
        get
        {
            var strbd = new StringBuilder();
            strbd.Append("cmd_");
            strbd.Append(_pluginIndex);
            strbd.Append('_');
            strbd.Append(_methodIndex);
            return strbd.ToString();
        }
    }

    public override string ToString()
    {
        var strbd = new StringBuilder();
        strbd.Append("#cmd ");
        strbd.Append(FunctionName);
        strbd.Append(' ');
        strbd.Append(_methodIndex);
        return strbd.ToString();
    }
}

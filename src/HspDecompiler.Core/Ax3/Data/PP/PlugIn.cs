using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HspDecompiler.Core.Ax3.Data.PP;

internal class PlugIn : Preprocessor
{
    private PlugIn() { }

    private PlugIn(int index)
        : base(index) { }

    private string? _dllName;
    private string? _exportName;
    private int _int3;

    internal static PlugIn FromBinaryReader(BinaryReader reader, AxData parent, int index)
    {
        _ = reader.ReadInt32();
        var ret = new PlugIn(index);
        int dllNameOffset = reader.ReadInt32();
        int exportNameOffset = reader.ReadInt32();
        ret._int3 = reader.ReadInt32();
        ret._dllName = parent.ReadStringLiteral(dllNameOffset);
        ret._exportName = parent.ReadStringLiteral(exportNameOffset);
        return ret;
    }

    private readonly Dictionary<int, Cmd> _cmds = new();
    private int _extendedTypeCount;

    internal int ExtendedTypeCount
    {
        get => _extendedTypeCount;
        set => _extendedTypeCount = value;
    }

    internal Cmd AddCmd(int methodIndex)
    {
        Cmd? cmd = null;
        if (_cmds.TryGetValue(methodIndex, out cmd))
        {
            return cmd;
        }

        cmd = new Cmd(_index, methodIndex);
        _cmds.Add(methodIndex, cmd);
        return cmd;
    }

    internal Dictionary<int, Cmd> GetCmds() => _cmds;

    public override string ToString()
    {
        var strbd = new StringBuilder();
        strbd.Append("#regcmd");
        strbd.Append(' ');
        strbd.Append('"');
        strbd.Append(_exportName);
        strbd.Append('"');
        strbd.Append(',');
        strbd.Append(' ');
        strbd.Append('"');
        strbd.Append(_dllName);
        strbd.Append('"');
        if (_extendedTypeCount != 0)
        {
            strbd.Append(',');
            strbd.Append(' ');
            strbd.Append(_extendedTypeCount);
        }

        return strbd.ToString();
    }
}

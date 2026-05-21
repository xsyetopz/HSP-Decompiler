using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HspDecompiler.Core.Ax3.Data.PP;

internal enum UsedllType
{
    None = 0x00,
    uselib = 0x01,
    usecom = 0x02,
}

internal sealed class Usedll : Preprocessor
{
    private Usedll() { }

    private Usedll(int index)
        : base(index) { }

    private string? _name;
    private string? _clsName;
    private int _type;
    private int _int2;

    internal static Usedll FromBinaryReader(BinaryReader reader, AxData parent, int index)
    {
        var ret = new Usedll(index) { _type = reader.ReadInt32() };
        int nameOffset = reader.ReadInt32();
        ret._int2 = reader.ReadInt32();
        int clsNameOffset = reader.ReadInt32();
        switch (ret.Type)
        {
            case UsedllType.usecom:
                ret._name = parent.ReadIidCodeLiteral(nameOffset);
                ret._clsName = parent.ReadStringLiteral(clsNameOffset);
                break;
            case UsedllType.uselib:
                ret._name = parent.ReadStringLiteral(nameOffset);
                break;
            case UsedllType.None:
                break;
            default:
                break;
        }

        return ret;
    }

    private readonly List<Function> _functions = new();

    internal UsedllType Type =>
        _type switch
        {
            1 => UsedllType.uselib,
            4 => UsedllType.usecom,
            _ => UsedllType.None,
        };

    public override string ToString()
    {
        if (_name == null)
        {
            return @"//#uselib? //dll情報なし";
        }

        var strBld = new StringBuilder();
        switch (Type)
        {
            case UsedllType.uselib:
                strBld.Append(@"#uselib """);
                strBld.Append(_name);
                strBld.Append('"');
                break;
            case UsedllType.usecom:
                strBld.Append(@"#usecom");
                if (_functions.Count != 0)
                {
                    strBld.Append(' ');
                    strBld.Append(_functions[0].FunctionName);
                }
                else
                {
                    strBld.Append(' ');
                    strBld.Append("/*関数なし*/");
                }

                strBld.Append(' ');
                strBld.Append('"');
                strBld.Append(_name);
                strBld.Append('"');
                strBld.Append(' ');
                strBld.Append('"');
                strBld.Append(_clsName);
                strBld.Append('"');
                break;
            case UsedllType.None:
                break;
            default:
                return @"//#uselib? //非対応の形式";
        }
        return strBld.ToString();
    }

    internal void AddFunction(Function ret) => _functions.Add(ret);

    internal List<Function> GetFunctions()
    {
        if ((Type == UsedllType.usecom) && (_functions.Count != 0))
        {
            var ret = new List<Function>(_functions);
            ret.RemoveAt(0);
            return ret;
        }
        return _functions;
    }
}

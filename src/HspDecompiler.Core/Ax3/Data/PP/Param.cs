using System.Globalization;
using System.IO;
using System.Text;

namespace HspDecompiler.Core.Ax3.Data.PP;

internal class Param : Preprocessor
{
    // Fix #38: named constant for the invalid-type comment prefix (HSP source convention)
    private const string InvalidTypeCommentPrefix = "/*不正な型 ";

    private Param() { }

    private Param(int paramIndex)
        : base(paramIndex) { }

    private string _paramTypeName = "NULL";
    private short _deffuncIndex;
    private int _paramStartByte;

    internal static Param FromBinaryReader(BinaryReader reader, AxData parent, int index)
    {
        var ret = new Param(index) { _paramType = reader.ReadUInt16() };
        ret._paramTypeName = !parent.Dictionary!.ParamLookUp(ret._paramType, out string? lookupName)
            ? "NULL"
            : lookupName ?? "NULL";

        ret._deffuncIndex = reader.ReadInt16();
        ret._paramStartByte = reader.ReadInt32();

        return ret;
    }

    private bool _paramNameIsUsed;
    private System.UInt16 _paramType;
    private Function? _module;

    internal Function? Module => _module;
    private bool _isStructParameter;

    internal void SetFunction(AxData parent)
    {
        if (_deffuncIndex < 0)
        {
            return;
        }

        _module = parent.GetUserFunction(_deffuncIndex);
        if (_module == null)
        {
            return;
        }

        if (_module.IsModuleFunction)
        {
            if (IsModuleType)
            {
                _nameFormatter = _module.FunctionName ?? "prm_{0}";
            }
            else
            {
                _isStructParameter = true;
            }
        }
    }

    internal bool ParamNameIsUsed
    {
        get => _paramNameIsUsed;
        set => _paramNameIsUsed = value;
    }

    private string _nameFormatter = "prm_{0}";
    internal string ParamName
    {
        get
        {
            if (_isStructParameter)
            {
                var strbd = new StringBuilder();
                strbd.Append(_module!.FunctionName);
                strbd.Append('_');
                strbd.Append(string.Format(CultureInfo.InvariantCulture, _nameFormatter, _index));
                return strbd.ToString();
            }
            return string.Format(CultureInfo.InvariantCulture, _nameFormatter, _index);
        }
    }

    internal string ToString(bool force_Named, bool remove_type, bool localToVar)
    {
        var strbd = new StringBuilder();
        if (!remove_type)
        {
            if (_paramTypeName == "NULL")
            {
                strbd.Append(InvalidTypeCommentPrefix);
                strbd.Append(_paramType.ToString("X04", CultureInfo.InvariantCulture));
                strbd.Append("*/");
            }
            else if (
                (localToVar) && (_paramTypeName.Equals("local", System.StringComparison.Ordinal))
            )
            {
                strbd.Append("var");
            }
            else
            {
                strbd.Append(_paramTypeName);
            }
        }
        if ((force_Named) || (_paramNameIsUsed) || (IsModuleType))
        {
            if (strbd.Length > 0)
            {
                strbd.Append(' ');
            }

            strbd.Append(string.Format(CultureInfo.InvariantCulture, _nameFormatter, _index));
        }
        return strbd.ToString();
    }

    public override string ToString() => ToString(false, false, false);

    internal bool IsModuleType =>
        _paramTypeName switch
        {
            "modvar" or "modinit" or "modterm" or "struct" => true,
            _ => false,
        };
}

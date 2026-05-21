using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace HspDecompiler.Core.Ax3.Data.PP;

internal enum FunctionType
{
    NULL = 0x00,
    func = 0x01,
    cfunc = 0x02,
    deffunc = 0x03,
    defcfunc = 0x04,
    comfunc = 0x05,
    module = 0x06,
}

internal enum FunctionFlags
{
    NULL = 0,
    onexit = 0x01,
}

internal class Function : Preprocessor
{
    // Fix #32: named constants for dllIndex/functionIndex/labelIndex sentinel values
    private const int DllIndexDeffunc = -1;
    private const int DllIndexDefcfunc = -2;
    private const int DllIndexModule = -3;
    private const int FuncIndexComfunc = -7;
    private const int LabelIndexFunc1 = 2;
    private const int LabelIndexOnExit = 3;
    private const int LabelIndexCfunc = 4;
    private const int LabelIndexFunc2 = 6;

    private Function() { }

    private Function(int index)
        : base(index) { }

    private int _dllIndex;
    private int _functionIndex;
    private List<Param> _functionParams = new();
    private int _strIndex;
    private int _paramSizeSum;
    private int _labelIndex;
    private Int16 _int0;
    private int _flags;

    internal static Function FromBinaryReader(BinaryReader reader, AxData parent, int index)
    {
        var ret = new Function(index)
        {
            _dllIndex = reader.ReadInt16(),
            _functionIndex = reader.ReadInt16(),
        };

        int paramStart = reader.ReadInt32();
        int paramCount = reader.ReadInt32();
        if (paramCount != 0)
        {
            ret._functionParams = parent.FunctionParams.GetRange(paramStart, paramCount);
        }

        ret._strIndex = reader.ReadInt32();
        if (ret._strIndex >= 0)
        {
            ret._defaultName = parent.ReadStringLiteral(ret._strIndex);
        }

        ret._paramSizeSum = reader.ReadInt32();
        ret._labelIndex = reader.ReadInt32();

        ret._int0 = reader.ReadInt16();
        ret._flags = reader.ReadInt16();
        switch (ret.Type)
        {
            case FunctionType.defcfunc:
            case FunctionType.deffunc:
                Label label = parent.GetLabel(ret._labelIndex);
                label?.SetFunction(ret);

                ret._label = label;
                break;

            case FunctionType.func:
            case FunctionType.cfunc:
            case FunctionType.comfunc:
                Usedll? dll = parent.GetUsedll(ret._dllIndex);
                dll?.AddFunction(ret);

                ret._dll = dll;
                break;
            case FunctionType.module:
                parent.Modules.Add(ret);
                break;
            case FunctionType.NULL:
                break;
            default:
                break;
        }
        return ret;
    }

    internal bool IsModuleFunction => Type == FunctionType.module;
    internal bool IsComFunction => Type == FunctionType.comfunc;
    internal bool IsUserFunction =>
        Type switch
        {
            FunctionType.deffunc or FunctionType.defcfunc => true,
            FunctionType.NULL => throw new NotImplementedException(),
            FunctionType.func => throw new NotImplementedException(),
            FunctionType.cfunc => throw new NotImplementedException(),
            FunctionType.comfunc => throw new NotImplementedException(),
            FunctionType.module => throw new NotImplementedException(),
            _ => false,
        };

    internal bool IsDllFunction =>
        Type switch
        {
            FunctionType.func or FunctionType.cfunc => true,
            FunctionType.NULL => throw new NotImplementedException(),
            FunctionType.deffunc => throw new NotImplementedException(),
            FunctionType.defcfunc => throw new NotImplementedException(),
            FunctionType.comfunc => throw new NotImplementedException(),
            FunctionType.module => throw new NotImplementedException(),
            _ => false,
        };

    private string? _defaultName;

    internal string? DefaultName => _defaultName;

    internal Function? ParentModule =>
        _functionParams.Count == 0 ? null
        : !_functionParams[0].IsModuleType ? null
        : _functionParams[0].Module;
    private string? _name;
    private Label? _label;
    private Usedll? _dll;

    // Fix #32: use named sentinel constants instead of magic numbers
    internal FunctionType Type
    {
        get
        {
            if (_dllIndex == DllIndexDeffunc)
            {
                return FunctionType.deffunc;
            }

            if (_dllIndex == DllIndexDefcfunc)
            {
                return FunctionType.defcfunc;
            }

            if (_dllIndex == DllIndexModule)
            {
                return FunctionType.module;
            }

            if (_dllIndex >= 0)
            {
                if (_functionIndex == FuncIndexComfunc)
                {
                    return FunctionType.comfunc;
                }

                if (_labelIndex is LabelIndexFunc1 or LabelIndexFunc2)
                {
                    return FunctionType.func;
                }

                if (_labelIndex == LabelIndexOnExit)
                {
                    return FunctionType.func;
                }

                if (_labelIndex == LabelIndexCfunc)
                {
                    return FunctionType.cfunc;
                }
            }
            return FunctionType.NULL;
        }
    }
    internal FunctionFlags Flags =>
        (_flags == 1) && (_dllIndex == DllIndexDeffunc) ? FunctionFlags.onexit
        : (_dllIndex >= 0) && (_labelIndex == LabelIndexOnExit) ? FunctionFlags.onexit
        : FunctionFlags.NULL;

    internal void SetName(string name) => _name = name;

    internal string? FunctionName
    {
        get
        {
            if (_name != null)
            {
                return _name;
            }

            if (_defaultName == null)
            {
                return Type == FunctionType.comfunc
                    ? "comfunc_" + _index.ToString(CultureInfo.InvariantCulture)
                    : null;
            }
            switch (Type)
            {
                case FunctionType.defcfunc:
                case FunctionType.deffunc:
                case FunctionType.module:
                    return _defaultName;
                case FunctionType.func:
                case FunctionType.cfunc:
                    if (_name != null)
                    {
                        return _name;
                    }

                    return _defaultName;
                case FunctionType.comfunc:
                    return "comfunc_" + _index.ToString(CultureInfo.InvariantCulture);
                case FunctionType.NULL:
                    break;
                default:
                    break;
            }
            return null;
        }
    }

    private string modFunctionToString()
    {
        var strBld = new StringBuilder();
        switch (_defaultName)
        {
            case "__init":
                strBld.Append("#modinit");
                break;
            case "__term":
                strBld.Append("#modterm");
                break;
            default:
                strBld.Append("#modfunc");
                strBld.Append(' ');
                strBld.Append(FunctionName);
                break;
        }
        if (_functionParams.Count > 1)
        {
            for (int i = 1; i < _functionParams.Count; i++)
            {
                if (i != 1)
                {
                    strBld.Append(',');
                }

                strBld.Append(' ');
                strBld.Append(_functionParams[i].ToString());
            }
        }
        return strBld.ToString();
    }

    private string moduleToString(bool useModuleStyle)
    {
        var strBld = new StringBuilder();
        if (useModuleStyle)
        {
            strBld.Append("#module ");
            strBld.Append(FunctionName);
        }
        else
        {
            strBld.Append("#struct ");
            strBld.Append(FunctionName);
        }
        if (_functionParams.Count > 1)
        {
            for (int i = 1; i < _functionParams.Count; i++)
            {
                if (i != 1)
                {
                    strBld.Append(',');
                }

                strBld.Append(' ');
                if (useModuleStyle)
                {
                    strBld.Append(_functionParams[i].ToString(true, true, true));
                }
                else
                {
                    strBld.Append(_functionParams[i].ToString(true, false, true));
                }
            }
        }
        return strBld.ToString();
    }

    // Fix #11: extracted parameter-formatting logic into AppendParams helper
    private void AppendParams(StringBuilder strBld, int paramStart)
    {
        if (_functionParams.Count > paramStart)
        {
            for (int i = paramStart; i < _functionParams.Count; i++)
            {
                if (i != paramStart)
                {
                    strBld.Append(',');
                }

                strBld.Append(' ');
                strBld.Append(_functionParams[i].ToString());
            }
        }
    }

    internal string ToString(bool useModuleStyle)
    {
        var strBld = new StringBuilder();

        int paramStart = 0;
        switch (Type)
        {
            case FunctionType.defcfunc:
                strBld.Append("#defcfunc ");
                strBld.Append(FunctionName);
                break;
            case FunctionType.module:
                return moduleToString(useModuleStyle);
            case FunctionType.deffunc:
                if (useModuleStyle)
                {
                    if ((_functionParams.Count != 0) && (_functionParams[0].IsModuleType))
                    {
                        return modFunctionToString();
                    }
                }

                strBld.Append("#deffunc ");
                strBld.Append(FunctionName);
                if ((Flags & FunctionFlags.onexit) == FunctionFlags.onexit)
                {
                    strBld.Append(" onexit");
                }

                break;
            case FunctionType.func:
                strBld.Append("#func ");
                strBld.Append(FunctionName);
                strBld.Append(' ');
                if ((Flags & FunctionFlags.onexit) == FunctionFlags.onexit)
                {
                    strBld.Append("onexit ");
                }

                strBld.Append('"');
                strBld.Append(_defaultName);
                strBld.Append('"');
                break;
            case FunctionType.cfunc:
                strBld.Append("#cfunc ");
                strBld.Append(FunctionName);
                strBld.Append(@" """);
                strBld.Append(_defaultName);
                strBld.Append('"');
                break;
            case FunctionType.comfunc:
                strBld.Append("#comfunc ");
                strBld.Append(FunctionName);
                strBld.Append(' ');
                strBld.Append(_labelIndex);
                paramStart = 1;
                break;
            case FunctionType.NULL:
                break;
            default:
                return "/*#deffunc?*/";
        }
        AppendParams(strBld, paramStart);
        return strBld.ToString();
    }

    public override string ToString() => ToString(false);
}

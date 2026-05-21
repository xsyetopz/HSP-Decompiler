using System;
using System.Diagnostics;
using System.Globalization;

namespace HspDecompiler.Core.Ax2.Data;

internal class Token
{
    private Token() { }

    private readonly AxData _data = null!;

    private Token(AxData data)
    {
        _data = data;
    }

    private int _fType;
    private int _fValue;
    private int _id;
    private int _size;
    internal bool _isLineend;
    internal int _ifJumpTo = -1;

    internal static Token? GetToken(AxData data, int offset)
    {
        if ((offset < 0) || ((offset + 1) >= data.TokenData.Length))
        {
            return null;
        }

        var ret = new Token(data) { _id = offset / 2, _fValue = data.TokenData[offset] };
        offset++;
        ret._fType = data.TokenData[offset];
        offset++;
        ret._size = 2;
        if ((ret._fType & Ax2TokenType.ExtendedValueBit) != 0)
        {
            ret._fType ^= Ax2TokenType.ExtendedValueBit;
            ret._fValue += Ax2TokenType.ExtendedValueCarry;
        }
        if ((ret.Type == Ax2TokenType.IfElse) && ((ret.Value == 0) || (ret.Value == 1)))
        {
            ret._size += 2;
            ret._ifJumpTo = BitConverter.ToInt16(data.TokenData, offset);
            offset += 2;
            ret._ifJumpTo += ret._id + 2;
        }
        if (((ret._fType & Ax2TokenType.LongValueFlag) != 0))
        {
            ret._fType ^= Ax2TokenType.LongValueFlag;
            ret._size += 4;
            ret._fValue = BitConverter.ToInt32(data.TokenData, offset);
            offset += 4;
        }
        return ret;
    }

    internal int Size => _size;

    #region properties
    private int Type => _fType & Ax2TokenType.TypeMask;

    private int Value => _fValue;

    internal bool NextIsUnenableLabel =>
        (Type == Ax2TokenType.FlowControl)
        && ((_fValue == 0x03) || (_fValue == 0x11) || (_fValue == 0x2b));

    internal bool IsLinehead => (_fType & Ax2TokenType.LineHeadFlag) != 0;

    internal bool IsArg => (_fType & Ax2TokenType.ArgFlag) != 0;

    internal bool TabPlus
    {
        get
        {
            if (Type == Ax2TokenType.FlowControl)
            {
                if (Value == 0x11)
                {
                    return true;
                }
            }
            if (Type == Ax2TokenType.IfElse)
            {
                if (Value is 0 or 1)
                {
                    return true;
                }
            }
            return false;
        }
    }

    internal bool TabMinus
    {
        get
        {
            if (Type == Ax2TokenType.FlowControl)
            {
                if (Value == 0x12)
                {
                    return true;
                }
            }
            return false;
        }
    }

    internal int Id => _id;

    internal int IfJumpId => (Type == Ax2TokenType.IfElse) && (Value == 1) ? _id + 2 : _id;

    internal bool IsEmpty => (_fType == 0) && (_fValue == 0);

    internal int LabelIndex =>
        IsLinehead ? -1
        : Type == Ax2TokenType.Label ? Value
        : -1;
    #endregion

    #region properties for decompile
    internal string GetString()
    {
        string? ret = null;
        switch (Type)
        {
            case Ax2TokenType.Operator:
                switch (Value)
                {
                    case 0x61:
                        return "<=";
                    case 0x62:
                        return ">=";
                    case 0x63:
                        return "<<";
                    case 0x64:
                        return ">>";
                    default:
                        System.Text.Encoding encode = HspDecompiler
                            .Core
                            .Encoding
                            .ShiftJisHelper
                            .Encoding;
                        byte[] bytes = new byte[1];
                        bytes[0] = (byte)Value;
                        return encode.GetString(bytes);
                }
            case Ax2TokenType.Integer:
                return Value.ToString(CultureInfo.InvariantCulture);
            case Ax2TokenType.String:
                ret = _data.GetString(Value);
                return "\"" + Escape(ret) + "\"";
            case Ax2TokenType.Label:
                return "label_" + Value.ToString(CultureInfo.InvariantCulture);
            case Ax2TokenType.Variable:
                return "var_" + Value.ToString(CultureInfo.InvariantCulture);
            case Ax2TokenType.SystemVariable:
                ret = GetStdFunc1Name(Value);
                if (ret != null)
                {
                    return ret;
                }

                break;
            case Ax2TokenType.FlowControl:
                ret = GetStdFunc2Name(Value);
                if (ret != null)
                {
                    return ret;
                }

                break;
            case Ax2TokenType.GuiFunction:
                ret = GetStdFunc3Name(Value);
                if (ret != null)
                {
                    return ret;
                }

                break;
            case Ax2TokenType.UserFunction:
                ret = _data.GetFuncName(Value - 0x10);
                if (ret != null)
                {
                    return "func_" + ret;
                }

                break;
            case Ax2TokenType.IfElse:
                if (Value == 0)
                {
                    return "if";
                }

                if (Value == 1)
                {
                    return "else";
                }

                break;
            case Ax2TokenType.Deffunc:
                ret = _data.GetDeffuncName(Value - 0x10);
                if (ret != null)
                {
                    return ret;
                }

                break;
            case Ax2TokenType.EndProgram:
                if (Value == 0)
                {
                    return "end";
                }

                break;
            default:
                break;
        }
        return Type.ToString("x2", CultureInfo.InvariantCulture)
            + Value.ToString("x2", CultureInfo.InvariantCulture);
    }

    internal bool IsKnown =>
        !GetString()
            .Equals(
                Type.ToString("x2", CultureInfo.InvariantCulture)
                    + Value.ToString("x2", CultureInfo.InvariantCulture),
                StringComparison.Ordinal
            );

    private static readonly char[] s_escapeWord = ['\n', '\r', '\t', '\"', '\\'];

    private static string? Escape(string? str)
    {
        if (str == null)
        {
            return null;
        }

        if (str.Length == 0)
        {
            return str;
        }

        int i;
        if ((i = str.IndexOfAny(s_escapeWord)) >= 0)
        {
            char spliter = str[i];
            string mid;
            switch (spliter)
            {
                case '\n':
                    mid = @"\n";
                    break;
                case '\r':
                    mid = "";
                    break;
                case '\t':
                    mid = @"\t";
                    break;
                case '\"':
                    mid = @"\""";
                    break;
                case '\\':
                    mid = @"\\";
                    break;
                default:
                    Debug.Assert(false);
                    mid = "";
                    break;
            }
            string[] ret = str.Split(s_escapeWord, 2);
            return ret[0] + mid + Escape(ret[1]);
        }
        return str;
    }

    #region HSPfuncname
    private static string? GetStdFunc1Name(int v)
    {
        return v switch
        {
            0x00 => "system",
            0x01 => "hspstat",
            0x02 => "hspver",
            0x03 => "cnt",
            0x04 => "err",
            0x05 => "strsize",
            0x06 => "looplev",
            0x07 => "sublev",
            0x40 => "mousex",
            0x41 => "mousey",
            0x42 => "csrx",
            0x43 => "csry",
            0x44 => "paluse",
            0x45 => "dispx",
            0x46 => "dispy",
            0x47 => "rval",
            0x48 => "gval",
            0x49 => "bval",
            0x4a => "stat",
            0x4b => "winx",
            0x4c => "winy",
            0x4d => "prmx",
            0x4e => "prmy",
            0x4f => "iparam",
            0x50 => "wparam",
            0x51 => "lparam",
            0x60 => "cmdline",
            0x61 => "windir",
            0x62 => "curdir",
            0x63 => "refstr",
            0x64 => "exedir",
            _ => null,
        };
    }

    private static string? GetStdFunc2Name(int v)
    {
        return v switch
        {
            0x00 => "goto",
            0x01 => "gosub",
            0x02 => "return",
            0x03 => "break",
            0x04 => "onexit",
            0x05 => "onkey",
            0x06 => "onclick",
            0x08 => "onerror",
            0x09 => "on",
            0x0a => "exgoto",
            0x10 => "wait",
            0x11 => "repeat",
            0x12 => "loop",
            0x13 => "mes",
            0x14 => "dim",
            0x15 => "sdim",
            0x16 => "alloc",
            0x17 => "bload",
            0x18 => "bsave",
            0x19 => "bcopy",
            0x1a => "stop",
            0x1b => "run",
            0x1c => "rnd",
            0x1d => "str",
            0x1e => "int",
            0x1f => "skiperr",
            0x20 => "dup",
            0x21 => "await",
            0x22 => "poke",
            0x23 => "peek",
            0x24 => "wpoke",
            0x25 => "wpeek",
            0x26 => "strlen",
            0x27 => "getstr",
            0x28 => "exist",
            0x29 => "strmid",
            0x2a => "instr",
            0x2b => "continue",
            0x2c => "mref",
            0x2d => "logmode",
            0x2e => "logmes",
            0x2f => "memcpy",
            0x30 => "memset",
            0x31 => "notesel",
            0x32 => "noteadd",
            0x33 => "noteget",
            0x34 => "notemax",
            0x35 => "notedel",
            0x36 => "noteload",
            0x37 => "notesave",
            0x38 => "memfile",
            _ => null,
        };
    }

    private static string? GetStdFunc3Name(int v)
    {
        return v switch
        {
            0x00 => "button",
            0x10 => "title",
            0x11 => "pos",
            0x13 => "cls",
            0x14 => "font",
            0x15 => "sysfont",
            0x16 => "objsize",
            0x17 => "picload",
            0x18 => "color",
            0x19 => "palcolor",
            0x1a => "palette",
            0x1b => "redraw",
            0x1c => "width",
            0x1d => "gsel",
            0x1e => "gcopy",
            0x1f => "gzoom",
            0x20 => "gmode",
            0x21 => "bmpsave",
            0x22 => "text",
            0x23 => "getkey",
            0x24 => "sndload",
            0x25 => "snd",
            0x26 => "mci",
            0x27 => "input",
            0x28 => "mesbox",
            0x29 => "buffer",
            0x2a => "screen",
            0x2b => "bgscr",
            0x2c => "dialog",
            0x2d => "chgdisp",
            0x2e => "exec",
            0x2f => "mkdir",
            0x30 => "sndoff",
            0x31 => "boxf",
            0x32 => "pget",
            0x33 => "pset",
            0x34 => "palfade",
            0x35 => "getpal",
            0x36 => "gettime",
            0x37 => "palcopy",
            0x38 => "randomize",
            0x39 => "clrobj",
            0x3a => "chkbox",
            0x3b => "line",
            0x3c => "stick",
            0x3d => "ginfo",
            0x3e => "combox",
            0x3f => "chdir",
            0x40 => "objprm",
            0x41 => "objsend",
            0x42 => "objmode",
            0x43 => "sysinfo",
            0x44 => "getpath",
            0x48 => "mouse",
            0x49 => "dirlist",
            0x4a => "delete",
            0x4b => "listbox",
            0x4c => "objsel",
            0x4d => "ll_ret",
            0x4e => "ll_retset",
            0x4f => "ll_getptr",
            0x50 => "ll_peek",
            0x51 => "ll_peek1",
            0x52 => "ll_peek2",
            0x53 => "ll_peek4",
            0x54 => "ll_poke",
            0x55 => "ll_callfunc",
            0x56 => "ll_n",
            0x57 => "ll_poke1",
            0x58 => "ll_poke2",
            0x59 => "ll_poke4",
            0x5a => "ll_libfree",
            0x5b => "ll_callfnv",
            0x5c => "ll_call",
            0x5d => "ll_free",
            0x5e => "ll_s",
            0x5f => "ll_p",
            0x60 => "ll_str",
            0x61 => "ll_dll",
            0x62 => "ll_func",
            0x63 => "ll_type",
            0x64 => "ll_z",
            0x65 => "ll_libload",
            0x66 => "ll_getproc",
            0x67 => "ll_bin",
            _ => null,
        };
    }
    #endregion
    #endregion
}

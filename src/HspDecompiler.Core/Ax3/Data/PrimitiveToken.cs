using System;
using System.Globalization;
using System.Text;
using HspDecompiler.Core.Ax3.Dictionary;
using HspDecompiler.Core.Resources;

namespace HspDecompiler.Core.Ax3.Data;

internal class PrimitiveTokenDataSet
{
    internal AxData? _parent;
    internal int _tokenOffset;
    internal int _type;
    internal int _flag;
    internal int _value;
    internal string? _name;
    internal HspDictionaryValue _dicValue;
}

internal abstract class PrimitiveToken
{
    protected PrimitiveToken() { }

    internal PrimitiveToken(PrimitiveTokenDataSet dataSet)
    {
        _parent = dataSet._parent;
        _codeType = dataSet._dicValue._type;
        _codeExtraFlags = dataSet._dicValue._extra;
        _dicValueName = dataSet._dicValue._name;
        _operatorPriority = dataSet._dicValue._operatorPriority;
        _tokenOffset = dataSet._tokenOffset;
        _type = dataSet._type;
        _flag = dataSet._flag;
        _value = dataSet._value;
        _name = dataSet._name;
    }

    protected string _dicValueName = "null";
    protected readonly int _type;
    protected readonly HspCodeType _codeType;

    protected readonly int _flag;
    protected readonly int _value;

    internal int Value => _value;

    private readonly HspCodeExtraOptions _codeExtraFlags;
    private readonly AxData? _parent;
    private readonly int _operatorPriority;
    private string? _name;
    private readonly int _tokenOffset;

    internal bool HasGhostLabel =>
        !IsLineHead
            ? false
            : (_codeExtraFlags & HspCodeExtraOptions.HasGhostLabel)
                == HspCodeExtraOptions.HasGhostLabel;
    internal HspCodeType CodeType => _codeType;
    internal HspCodeExtraOptions CodeExtraFlags => _codeExtraFlags;
    internal int OperatorPriority =>
        _codeType != HspCodeType.Operator
            ? throw new InvalidOperationException(Strings.OperatorPriorityOnNonOperator)
            : _operatorPriority;
    internal bool HasLongTypeValue =>
        ((_flag & HspTokenFlag.LongTypeValue) == HspTokenFlag.LongTypeValue);
    internal bool IsParamHead => ((_flag & HspTokenFlag.ParamHead) == HspTokenFlag.ParamHead);
    internal bool IsLineHead => ((_flag & HspTokenFlag.LineHead) == HspTokenFlag.LineHead);

    internal string? Name => _name;

    internal int TokenOffset => _tokenOffset;

    internal void SetName()
    {
        switch (_codeType)
        {
            case HspCodeType.Label:
                _name = _dicValueName + _value.ToString(CultureInfo.InvariantCulture);
                return;
            case HspCodeType.IntegerValue:
            case HspCodeType.Param:
            case HspCodeType.Variable:
                _name = _dicValueName + _value.ToString(CultureInfo.InvariantCulture);
                return;
            case HspCodeType.Operator:
                break;
            case HspCodeType.Symbol:
                break;
            case HspCodeType.StringValue:
                break;
            case HspCodeType.DoubleValue:
                break;
            case HspCodeType.HspFunction:
                break;
            case HspCodeType.IfStatement:
                break;
            case HspCodeType.ComFunction:
                break;
            case HspCodeType.PlugInFunction:
                break;
            case HspCodeType.OnEventStatement:
                break;
            case HspCodeType.OnStatement:
                break;
            case HspCodeType.ElseStatement:
                break;
            case HspCodeType.McallStatement:
                break;
            case HspCodeType.UserFunction:
            case HspCodeType.DllFunction:
            case HspCodeType.NONE:
            default:
                break;
        }
    }

    public override string ToString() => _name ?? string.Empty;

    internal virtual string DefaultName
    {
        get
        {
            var builder = new StringBuilder();
            builder.Append("/*");
            builder.Append(_type.ToString("X02", CultureInfo.InvariantCulture));
            builder.Append(' ');
            builder.Append(_flag.ToString("X02", CultureInfo.InvariantCulture));
            builder.Append(' ');
            if (HasLongTypeValue)
            {
                builder.Append(_value.ToString("X08", CultureInfo.InvariantCulture));
            }
            else
            {
                builder.Append(_value.ToString("X04", CultureInfo.InvariantCulture));
            }

            builder.Append("*/");
            return builder.ToString();
        }
    }
}

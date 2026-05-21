using System;
using System.IO;
using HspDecompiler.Core.Ax3.Data.Analyzer;
using HspDecompiler.Core.Ax3.Data.Primitive;
using HspDecompiler.Core.Ax3.Dictionary;

namespace HspDecompiler.Core.Ax3.Data;

internal class LexicalAnalyzer
{
    private LexicalAnalyzer() { }

    internal LexicalAnalyzer(Hsp3Dictionary theDic)
    {
        ArgumentNullException.ThrowIfNull(theDic);

        _dictionary = theDic;
    }

    private readonly Hsp3Dictionary? _dictionary;
    private int _tokenOffset;

    internal TokenCollection Analyze(AxData data)
    {
        if (!data.IsStarted)
        {
            throw new InvalidOperationException();
        }

        var stream = new TokenCollection();
        BinaryReader reader = data.Reader!;
        long sizeOfCode = data.Header!.CodeSize;
        long startOfCode = data.StartOfCode;
        _tokenOffset = 0;
        reader.BaseStream.Seek(startOfCode, SeekOrigin.Begin);
        while (_tokenOffset < sizeOfCode)
        {
            PrimitiveToken? code = ReadPrimitive(reader, data);
            if (code != null)
            {
                stream.Add(code);
            }
        }
        return stream;
    }

    private PrimitiveToken ReadPrimitive(BinaryReader reader, AxData data)
    {
        PrimitiveToken? ret = null;

        int theTokenOffset = _tokenOffset;
        int type = reader.ReadByte();
        int flag = reader.ReadByte();
        int value = 0;
        int extraValue = -1;
        _tokenOffset += 1;
        if ((flag & HspTokenFlag.LongTypeValue) == HspTokenFlag.LongTypeValue)
        {
            value = reader.ReadInt32();
            _tokenOffset += 2;
        }
        else
        {
            value = reader.ReadUInt16();
            _tokenOffset += 1;
        }

        var key = new HspDictionaryKey { _type = type, _value = value };
        HspDictionaryValue dicValue;
        if (_dictionary!.CodeLookUp(key, out dicValue))
        {
            if (
                (dicValue._extra & HspCodeExtraOptions.HasExtraInt16)
                == HspCodeExtraOptions.HasExtraInt16
            )
            {
                if ((flag & HspTokenFlag.LineHead) == HspTokenFlag.LineHead)
                {
                    extraValue = reader.ReadUInt16();
                    _tokenOffset += 1;
                    ret = CreatePrimitive(
                        data,
                        dicValue,
                        theTokenOffset,
                        type,
                        flag,
                        value,
                        extraValue
                    );
                }
                else
                {
                    ret = CreatePrimitive(data, dicValue, theTokenOffset, type, flag, value, -1);
                }
            }
            else
            {
                ret = CreatePrimitive(data, dicValue, theTokenOffset, type, flag, value, -1);
            }
        }
        else
        {
            ret = CreatePrimitive(
                data,
                new HspDictionaryValue(),
                theTokenOffset,
                type,
                flag,
                value,
                -1
            );
        }

        ret!.SetName();

        return ret;
    }

    private static PrimitiveToken CreatePrimitive(
        AxData data,
        HspDictionaryValue dicValue,
        int theTokenOffset,
        int type,
        int flag,
        int value,
        int extraValue
    )
    {
        var dataset = new PrimitiveTokenDataSet
        {
            _parent = data,
            _dicValue = dicValue,
            _tokenOffset = theTokenOffset,
            _type = type,
            _flag = flag,
            _value = value,
            _name = dicValue._name,
        };
        switch (dicValue._type)
        {
            case HspCodeType.Label:
                return new LabelPrimitive(dataset);
            case HspCodeType.IntegerValue:
                return new IntegerPrimitive(dataset);
            case HspCodeType.DoubleValue:
                return new DoublePrimitive(dataset, data.ReadDoubleLiteral(value));
            case HspCodeType.StringValue:
                return new StringPrimitive(dataset, data.ReadStringLiteral(value));
            case HspCodeType.Symbol:
                return new SymbolPrimitive(dataset);
            case HspCodeType.Param:
                return new ParameterPrimitive(dataset);
            case HspCodeType.Variable:
                return new GlobalVariablePrimitive(dataset);
            case HspCodeType.Operator:
                return new OperatorPrimitive(dataset);
            case HspCodeType.IfStatement:
            case HspCodeType.ElseStatement:
                return extraValue >= 0
                    ? new IfStatementPrimitive(dataset, extraValue)
                    : (PrimitiveToken)new HspFunctionPrimitive(dataset);

            case HspCodeType.HspFunction:
                return new HspFunctionPrimitive(dataset);
            case HspCodeType.OnStatement:
                return new OnFunctionPrimitive(dataset);
            case HspCodeType.OnEventStatement:
                return new OnEventFunctionPrimitive(dataset);
            case HspCodeType.McallStatement:
                return new McallFunctionPrimitive(dataset);
            case HspCodeType.UserFunction:
                return new UserFunctionPrimitive(dataset);
            case HspCodeType.DllFunction:
                return new DllFunctionPrimitive(dataset);
            case HspCodeType.PlugInFunction:
                return new PlugInFunctionPrimitive(dataset);
            case HspCodeType.ComFunction:
                return new ComFunctionPrimitive(dataset);
            case HspCodeType.NONE:
            default:
                break;
        }
        return new UnknownPrimitive(dataset);
    }
}

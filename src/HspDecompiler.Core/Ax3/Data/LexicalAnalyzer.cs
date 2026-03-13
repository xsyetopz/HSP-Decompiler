using System;
using System.Collections.Generic;
using System.IO;
using HspDecompiler.Core.Ax3.Data.Analyzer;
using HspDecompiler.Core.Ax3.Data.Primitive;
using HspDecompiler.Core.Ax3.Dictionary;

namespace HspDecompiler.Core.Ax3.Data
{
    class LexicalAnalyzer
    {
        private LexicalAnalyzer() { }
        internal LexicalAnalyzer(Hsp3Dictionary theDic)
        {
            if (theDic == null)
                throw new ArgumentNullException();
            dictionary = theDic;
        }

        Hsp3Dictionary dictionary = null;
        int tokenOffset = 0;

        internal TokenCollection Analyze(AxData data)
        {
            if (!data.IsStarted)
                throw new InvalidOperationException();
            TokenCollection stream = new TokenCollection();
            BinaryReader reader = data.Reader;
            long sizeOfCode = data.Header.CodeSize;
            long startOfCode = data.StartOfCode;
            tokenOffset = 0;
            reader.BaseStream.Seek(startOfCode, SeekOrigin.Begin);
            while (tokenOffset < sizeOfCode)
            {
                PrimitiveToken code = ReadPrimitive(reader, data);
                if (code != null)
                    stream.Add(code);
            }
            return stream;
        }

        private PrimitiveToken ReadPrimitive(BinaryReader reader, AxData data)
        {
            PrimitiveToken ret = null;

            int theTokenOffset = tokenOffset;
            int type = reader.ReadByte();
            int flag = reader.ReadByte();
            int value = 0;
            int extraValue = -1;
            tokenOffset += 1;
            if ((flag & 0x80) == 0x80)
            {
                value = reader.ReadInt32();
                tokenOffset += 2;
            }
            else
            {
                value = reader.ReadUInt16();
                tokenOffset += 1;
            }

            HspDictionaryKey key = new HspDictionaryKey();
            key.Type = type;
            key.Value = value;
            HspDictionaryValue dicValue;
            if (dictionary.CodeLookUp(key, out dicValue))
            {
                if ((dicValue.Extra & HspCodeExtraFlags.HasExtraInt16) == HspCodeExtraFlags.HasExtraInt16)
                {
                    if ((flag & 0x20) == 0x20)
                    {
                        extraValue = reader.ReadUInt16();
                        tokenOffset += 1;
                        ret = createPrimitive(data, dicValue, theTokenOffset, type, flag, value, extraValue);
                    }
                    else
                    {
                        ret = createPrimitive(data, dicValue, theTokenOffset, type, flag, value, -1);
                    }
                }
                else
                {
                    ret = createPrimitive(data, dicValue, theTokenOffset, type, flag, value, -1);
                }
            }
            else
                ret = createPrimitive(data, new HspDictionaryValue(), theTokenOffset, type, flag, value, -1);
            ret.SetName();

            return ret;
        }

        private PrimitiveToken createPrimitive(AxData data, HspDictionaryValue dicValue, int theTokenOffset, int type, int flag, int value, int extraValue)
        {
            PrimitiveTokenDataSet dataset = new PrimitiveTokenDataSet();
            dataset.Parent = data;
            dataset.DicValue = dicValue;
            dataset.TokenOffset = theTokenOffset;
            dataset.Type = type;
            dataset.Flag = flag;
            dataset.Value = value;
            dataset.Name = dicValue.Name;
            switch (dicValue.Type)
            {
                case HspCodeType.Label:
                    return new LabelPrimitive(dataset);
                case HspCodeType.Integer:
                    return new IntegerPrimitive(dataset);
                case HspCodeType.Double:
                    return new DoublePrimitive(dataset, data.ReadDoubleLiteral(value));
                case HspCodeType.String:
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
                    if (extraValue >= 0)
                        return new IfStatementPrimitive(dataset, extraValue);
                    else
                        return new HspFunctionPrimitive(dataset);
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
}

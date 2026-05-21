using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using HspDecompiler.Core.Ax3.Data.Analyzer;
using HspDecompiler.Core.Ax3.Data.PP;
using HspDecompiler.Core.Exceptions;
using HspDecompiler.Core.Resources;

namespace HspDecompiler.Core.Ax3.Data;

internal class AxData
{
    private static readonly CompositeFormat s_userFunctionNameFormat = CompositeFormat.Parse(
        "{0}_{1}"
    );
    private static readonly CompositeFormat s_dllFunctionNameFormat = CompositeFormat.Parse(
        "func_{0}"
    );
    private static readonly CompositeFormat s_comFunctionNameFormat = CompositeFormat.Parse(
        "comfunc_{0}"
    );

    // Fix #30: Named constants for HSP debug info markers
    private const byte DebugMarkerFileName = 252;
    private const byte DebugMarkerVariable = 253;
    private const byte DebugMarkerLineNumber = 254;
    private const byte DebugMarkerEnd = 255;

    private AxHeader? _header;
    private readonly TokenCollection _tokens = new();
    private List<Label> _labels = new();
    private readonly List<Usedll> _dlls = new();
    private readonly List<Function> _functions = new();
    private readonly List<Param> _functionParams = new();
    private readonly List<PlugIn> _plugIns = new();
    private Runtime? _runtime;
    private readonly List<Function> _modules = new();
    private readonly List<string> _variableName = new();

    internal List<Function> Modules => _modules;

    internal List<PlugIn> PlugIns => _plugIns;

    internal Runtime? Runtime => _runtime;

    internal AxHeader? Header => _header;
    internal TokenCollection Tokens => _tokens;
    internal List<Usedll> Usedlls => _dlls;
    internal List<Label> Labels => _labels;
    internal List<Function> Functions => _functions;
    internal List<Param> FunctionParams => _functionParams;

    internal Label GetLabel(int index) => Labels[index];

    internal Function? GetUserFunction(int index) =>
        index < 0 ? null
        : index >= _functions.Count ? null
        : _functions[index];

    internal Function? GetDllFunction(int index) =>
        index < 0 ? null
        : index >= _functions.Count ? null
        : _functions[index];

    internal Usedll? GetUsedll(int index) =>
        index < 0 ? null
        : index >= _dlls.Count ? null
        : _dlls[index];

    internal Param? GetParam(int index) =>
        index < 0 ? null
        : index >= _functionParams.Count ? null
        : _functionParams[index];

    internal string? GetVariableName(int index) =>
        index < 0 ? null
        : index >= _variableName.Count ? null
        : _variableName[index];

    internal Cmd? AddCmd(int pluginIndex, int methodIndex) =>
        pluginIndex < 0 ? null
        : pluginIndex >= _plugIns.Count ? null
        : _plugIns[pluginIndex].AddCmd(methodIndex);

    internal string ReadString(int offset, int max_count)
    {
        long seekOffset = _seekOrigin + offset;
        long nowPosition = _reader!.BaseStream.Position;
        _reader.BaseStream.Seek(seekOffset, SeekOrigin.Begin);
        var chars = new List<char>();
        char token = '\0';
        int count = 0;
        while ((token = _reader.ReadChar()) != '\0')
        {
            switch (token)
            {
                case '\\':
                    chars.Add('\\');
                    chars.Add('\\');
                    break;
                case '\"':
                    chars.Add('\\');
                    chars.Add('\"');
                    break;
                case '\t':
                    chars.Add('\\');
                    chars.Add('t');
                    break;
                case '\n':
                    chars.Add('\\');
                    chars.Add('n');
                    break;
                case '\r':
                    break;
                default:
                    chars.Add(token);
                    break;
            }
            count++;
            if (count >= max_count)
            {
                break;
            }
        }
        char[] arrayChars = new char[chars.Count];
        chars.CopyTo(arrayChars);
        _reader.BaseStream.Seek(nowPosition, SeekOrigin.Begin);
        return new string(arrayChars);
    }

    internal string ReadStringLiteral(int offset) =>
        ReadString((int)(_header!.LiteralStart + offset), (int)(_header.LiteralSize - offset));

    internal double ReadDoubleLiteral(int offset)
    {
        double ret = 0.0;
        long seekOffset = _seekOrigin + _header!.LiteralStart + offset;
        long nowPosition = _reader!.BaseStream.Position;
        _reader.BaseStream.Seek(seekOffset, SeekOrigin.Begin);
        ret = _reader.ReadDouble();
        _reader.BaseStream.Seek(nowPosition, SeekOrigin.Begin);
        return ret;
    }

    internal string ReadIidCodeLiteral(int offset)
    {
        var strbd = new StringBuilder();
        byte[] buf;
        long seekOffset = _seekOrigin + _header!.LiteralStart + offset;
        long nowPosition = _reader!.BaseStream.Position;
        _reader.BaseStream.Seek(seekOffset, SeekOrigin.Begin);
        buf = _reader.ReadBytes(0x10);
        _reader.BaseStream.Seek(nowPosition, SeekOrigin.Begin);
        strbd.Append('{');
        strbd.Append(buf[0x03].ToString("X02", CultureInfo.InvariantCulture));
        strbd.Append(buf[0x02].ToString("X02", CultureInfo.InvariantCulture));
        strbd.Append(buf[0x01].ToString("X02", CultureInfo.InvariantCulture));
        strbd.Append(buf[0x00].ToString("X02", CultureInfo.InvariantCulture));
        strbd.Append('-');
        strbd.Append(buf[0x05].ToString("X02", CultureInfo.InvariantCulture));
        strbd.Append(buf[0x04].ToString("X02", CultureInfo.InvariantCulture));
        strbd.Append('-');
        strbd.Append(buf[0x07].ToString("X02", CultureInfo.InvariantCulture));
        strbd.Append(buf[0x06].ToString("X02", CultureInfo.InvariantCulture));
        strbd.Append('-');
        strbd.Append(buf[0x08].ToString("X02", CultureInfo.InvariantCulture));
        strbd.Append(buf[0x09].ToString("X02", CultureInfo.InvariantCulture));
        strbd.Append('-');
        strbd.Append(buf[0x0A].ToString("X02", CultureInfo.InvariantCulture));
        strbd.Append(buf[0x0B].ToString("X02", CultureInfo.InvariantCulture));
        strbd.Append(buf[0x0C].ToString("X02", CultureInfo.InvariantCulture));
        strbd.Append(buf[0x0D].ToString("X02", CultureInfo.InvariantCulture));
        strbd.Append(buf[0x0E].ToString("X02", CultureInfo.InvariantCulture));
        strbd.Append(buf[0x0F].ToString("X02", CultureInfo.InvariantCulture));
        strbd.Append('}');
        return strbd.ToString();
    }

    internal void LoadStart(BinaryReader theReader, Hsp3Dictionary? theDictionary)
    {
        if (theReader == null)
        {
            throw new ArgumentNullException(nameof(theReader), Strings.ArgumentNullReader);
        }

        if (theDictionary == null)
        {
            throw new ArgumentNullException(Strings.ArgumentNullDictionary);
        }

        _seekOrigin = theReader.BaseStream.Position;
        _reader = theReader;
        _dictionary = theDictionary;
        _isStarted = true;
    }

    internal void LoadEnd()
    {
        _seekOrigin = -1;
        _reader = null;
        _dictionary = null;
        _isStarted = false;
    }

    private long _seekOrigin;
    private BinaryReader? _reader;
    private Hsp3Dictionary? _dictionary;
    private bool _isStarted;

    internal bool IsStarted => _isStarted;

    // Fix #17: removed unused setter
    internal BinaryReader? Reader => _reader;
    internal long StartOfCode => _header!.CodeStart + _seekOrigin;

    // Fix #18: removed unused setter
    internal Hsp3Dictionary? Dictionary => _dictionary;

    internal void ReadHeader()
    {
        if (!_isStarted)
        {
            throw new InvalidOperationException(Strings.LoadStartNotCalled);
        }

        long streamSize = _reader!.BaseStream.Length - _seekOrigin;
        if (streamSize < AxHeader.HeaderSize)
        {
            throw new HspDecoderException("AxData", Strings.FileHeaderMissing);
        }

        try
        {
            _header = AxHeader.FromBinaryReader(_reader);
        }
        catch (SystemException e)
        {
            throw new HspDecoderException(
                "AxHeader",
                Strings.UnexpectedErrorDuringHeaderAnalysis,
                e
            );
        }
        return;
    }

    internal void ReadPreprocessor(Hsp3Dictionary? dictionary)
    {
        if (!_isStarted)
        {
            throw new InvalidOperationException(Strings.LoadStartNotCalled);
        }

        if (_header == null)
        {
            throw new InvalidOperationException(Strings.HeaderNotLoaded);
        }

        if (_header.RuntimeStart != 0)
        {
            string runtimeName = ReadString(
                (int)_header.RuntimeStart,
                (int)(_header.CodeStart - _header.RuntimeStart)
            );
            if (runtimeName != null)
            {
                _runtime = new Runtime(runtimeName);
            }
        }
        uint count = _header.LabelCount;
        for (int i = 0; i < count; i++)
        {
            long offset = _seekOrigin + _header.LabelStart + ((int)HeaderDataSize.Label * i);
            _reader!.BaseStream.Seek(offset, SeekOrigin.Begin);
            _labels.Add(Label.FromBinaryReader(_reader, this, i));
        }

        count = _header.DllCount;
        for (int i = 0; i < count; i++)
        {
            long offset = _seekOrigin + _header.DllStart + ((int)HeaderDataSize.Dll * i);
            _reader!.BaseStream.Seek(offset, SeekOrigin.Begin);
            _dlls.Add(Usedll.FromBinaryReader(_reader, this, i));
        }

        count = _header.ParameterCount;
        for (int i = 0; i < count; i++)
        {
            long offset =
                _seekOrigin + _header.ParameterStart + ((int)HeaderDataSize.Parameter * i);
            _reader!.BaseStream.Seek(offset, SeekOrigin.Begin);
            _functionParams.Add(Param.FromBinaryReader(_reader, this, i));
        }

        count = _header.FunctionCount;
        for (int i = 0; i < count; i++)
        {
            long offset = _seekOrigin + _header.FunctionStart + ((int)HeaderDataSize.Function * i);
            _reader!.BaseStream.Seek(offset, SeekOrigin.Begin);
            _functions.Add(Function.FromBinaryReader(_reader, this, i));
        }

        count = _header.PluginCount;
        for (int i = 0; i < count; i++)
        {
            long offset = _seekOrigin + _header.PluginStart + ((int)HeaderDataSize.Plugin * i);
            _reader!.BaseStream.Seek(offset, SeekOrigin.Begin);
            _plugIns.Add(PlugIn.FromBinaryReader(_reader, this, i));
        }
        if ((count != 0) && (_header.PluginParameterCount != 0))
        {
            _plugIns[0].ExtendedTypeCount = (int)_header.PluginParameterCount;
        }

        foreach (Param param in _functionParams)
        {
            param.SetFunction(this);
        }
        RenameFunctions(dictionary!);

        ReadDebugInfo();
    }

    // Fix #25: renamed from DeleteInvisibleLables
    internal void DeleteInvisibleLabels() => _labels = _labels.FindAll(LabelIsVisible);

    private bool LabelIsVisible(Label label) => label.Visible;

    // Fix #9: extracted rename loops into dedicated private methods
    private void RenameFunctions(Hsp3Dictionary dictionary)
    {
        var functionNames = new List<string>();
        var initializer = new List<Function>();
        var comfuncs = new List<Function>();
        var dllfuncs = new List<Function>();
        functionNames.AddRange(dictionary.GetAllFuncName());
        foreach (Function func in _functions)
        {
            switch (func.Type)
            {
                case FunctionType.cfunc:
                case FunctionType.func:
                    dllfuncs.Add(func);
                    break;
                case FunctionType.comfunc:
                    comfuncs.Add(func);
                    break;
                case FunctionType.defcfunc:
                case FunctionType.deffunc:
                case FunctionType.module:
                    if (func.ParentModule != null)
                    {
                        initializer.Add(func);
                    }
                    else
                    {
                        func.SetName(func.DefaultName ?? "");
                        functionNames.Add(
                            (func.DefaultName ?? "").ToLower(
                                System.Globalization.CultureInfo.CurrentCulture
                            )
                        );
                    }
                    break;
                case FunctionType.NULL:
                    break;
                default:
                    break;
            }
        }
        RenameUserFunctions(functionNames, initializer);
        RenameDllFunctions(functionNames, dllfuncs);
        RenameComFunctions(functionNames, comfuncs);
    }

    private static void RenameUserFunctions(List<string> functionNames, List<Function> initializer)
    {
        foreach (Function func in initializer)
        {
            string defName = func.DefaultName ?? "";
            if (
                !functionNames.Contains(
                    defName.ToLower(System.Globalization.CultureInfo.CurrentCulture)
                )
            )
            {
                func.SetName(defName);
                functionNames.Add(defName.ToLower(System.Globalization.CultureInfo.CurrentCulture));
                continue;
            }
            string newName = defName;
            int index = 1;
            do
            {
                newName = string.Format(
                    CultureInfo.InvariantCulture,
                    s_userFunctionNameFormat,
                    defName,
                    index
                );
                index++;
            } while (functionNames.Contains(newName));
            func.SetName(newName);
            functionNames.Add(newName.ToLower(System.Globalization.CultureInfo.CurrentCulture));
        }
    }

    private static void RenameDllFunctions(List<string> functionNames, List<Function> dllfuncs)
    {
        foreach (Function func in dllfuncs)
        {
            string defName = func.DefaultName ?? "";
            string newName = defName;
            if (newName.StartsWith('_') && (newName.Length > 1))
            {
                newName = newName[1..];
            }

            int atIndex = newName.IndexOf('@');
            if (atIndex > 0)
            {
                newName = newName[..atIndex];
            }

            if (
                !functionNames.Contains(
                    newName.ToLower(System.Globalization.CultureInfo.CurrentCulture)
                )
            )
            {
                func.SetName(newName);
                functionNames.Add(newName.ToLower(System.Globalization.CultureInfo.CurrentCulture));
                continue;
            }
            int index = 1;
            do
            {
                newName = string.Format(
                    CultureInfo.InvariantCulture,
                    s_dllFunctionNameFormat,
                    index
                );
                index++;
            } while (functionNames.Contains(newName));
            func.SetName(newName);
            functionNames.Add(newName.ToLower(System.Globalization.CultureInfo.CurrentCulture));
        }
    }

    private static void RenameComFunctions(List<string> functionNames, List<Function> comfuncs)
    {
        foreach (Function func in comfuncs)
        {
            string newName = "";
            int index = 1;
            do
            {
                newName = string.Format(
                    CultureInfo.InvariantCulture,
                    s_comFunctionNameFormat,
                    index
                );
                index++;
            } while (functionNames.Contains(newName));
            func.SetName(newName);
            functionNames.Add(newName.ToLower(System.Globalization.CultureInfo.CurrentCulture));
        }
    }

    internal void RenameLables()
    {
        if (_labels.Count <= 0)
        {
            return;
        }

        _labels.Sort();
        int keta = ((int)System.Math.Log10(_labels.Count)) + 1;
        var labelFormat = CompositeFormat.Parse(
            "*label_{0:D0" + keta.ToString(CultureInfo.InvariantCulture) + "}"
        );
        for (int i = 0; i < _labels.Count; i++)
        {
            _labels[i].LabelName = string.Format(CultureInfo.InvariantCulture, labelFormat, i);
        }
        return;
    }

    private bool ReadDebugInfo()
    {
        // Fix #21: removed commented-out dead code
        for (uint i = 0; i < _header!.DebugSize; i++)
        {
            long offset = _seekOrigin + _header.DebugStart + i;
            _reader!.BaseStream.Seek(offset, SeekOrigin.Begin);

            // Fix #30: use named constants instead of magic bytes
            switch (_reader.ReadByte())
            {
                case DebugMarkerFileName:
                    i += 2;
                    break;
                case DebugMarkerVariable:
                    int literalOffset =
                        _reader.ReadByte() ^ (_reader.ReadByte() << 8) ^ (_reader.ReadByte() << 16);
                    _variableName.Add(ReadStringLiteral(literalOffset));
                    i += 5;
                    break;
                case DebugMarkerLineNumber:
                    i += 5;
                    break;
                case DebugMarkerEnd:
                    return true;
                default:
                    break;
            }
        }
        return false;
    }
}

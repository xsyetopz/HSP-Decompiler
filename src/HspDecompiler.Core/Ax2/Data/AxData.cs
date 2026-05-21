using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using HspDecompiler.Core.Exceptions;
using HspDecompiler.Core.Resources;

namespace HspDecompiler.Core.Ax2.Data;

internal class AxData
{
    private const int LabelIndexBaseOffset = 0x1000;

    internal AxData() { }

    private Header? _header;
    private Label[]? _labels;
    private Dll[]? _dlls;
    private Func[]? _funcs;
    private Deffunc[]? _deffuncs;
    private Module[]? _modules;

    private byte[]? _labelData;
    private byte[]? _dllData;
    private byte[]? _funcData;
    private byte[]? _deffuncData;
    private byte[]? _moduleData;
    private byte[]? _tokenData;

    internal byte[] TokenData => _tokenData!;

    private byte[]? _stringData;

    private void ReadData(Stream stream)
    {
        long startPosition = stream.Position;
        byte[] headerBuffer = new byte[80];
        if (stream.Read(headerBuffer, 0, 80) < 80)
        {
            throw new HspDecoderException("AxData", Strings.FileHeaderMissing);
        }

        int[] buffer = new int[20];
        for (int i = 0; i < 20; i++)
        {
            buffer[i] = BitConverter.ToInt32(headerBuffer, i * 4);
        }
        try
        {
            _header = Header.FromIntArray(buffer);
        }
        catch (Exception e)
        {
            throw new HspDecoderException(
                "AxHeader",
                Strings.UnexpectedErrorDuringHeaderAnalysis,
                e
            );
        }
        if (_header == null)
        {
            throw new HspDecoderException("AxHeader", Strings.HeaderAnalysisFailed);
        }

        try
        {
            Header head = _header;
            _tokenData = new byte[head.ScriptByte];
            stream.Seek(startPosition + head.ScriptOffset, SeekOrigin.Begin);
            stream.ReadExactly(_tokenData, 0, head.ScriptByte);

            _dllData = new byte[head.DllByte];
            stream.Seek(startPosition + head.DllOffset, SeekOrigin.Begin);
            stream.ReadExactly(_dllData, 0, head.DllByte);

            _funcData = new byte[head.FuncByte];
            stream.Seek(startPosition + head.FuncOffset, SeekOrigin.Begin);
            stream.ReadExactly(_funcData, 0, head.FuncByte);

            _deffuncData = new byte[head.DeffuncByte];
            stream.Seek(startPosition + head.DeffuncOffset, SeekOrigin.Begin);
            stream.ReadExactly(_deffuncData, 0, head.DeffuncByte);

            _moduleData = new byte[head.ModuleByte];
            stream.Seek(startPosition + head.ModuleOffset, SeekOrigin.Begin);
            stream.ReadExactly(_moduleData, 0, head.ModuleByte);

            _labelData = new byte[head.LabelByte];
            stream.Seek(startPosition + head.LabelOffset, SeekOrigin.Begin);
            stream.ReadExactly(_labelData, 0, head.LabelByte);

            _stringData = new byte[head.TextByte];
            stream.Seek(startPosition + head.TextOffset, SeekOrigin.Begin);
            stream.ReadExactly(_stringData, 0, head.TextByte);
        }
        catch (Exception e)
        {
            throw new HspDecoderException("AxHeader", Strings.UnexpectedErrorDuringStreamRead, e);
        }
        stream.Seek(startPosition, SeekOrigin.Begin);
    }

    #region create
    internal static AxData FromStream(Stream stream)
    {
        var data = new AxData();
        data.ReadData(stream);
        return data;
    }
    #endregion

    #region read
    internal string GetString(int offset) => ReadString(offset, _stringData!);

    private string ReadString(int offset) => ReadString(offset, _stringData!);

    private static string ReadString(int offset, byte[] dumpData)
    {
        System.Text.Encoding encode = HspDecompiler.Core.Encoding.ShiftJisHelper.Encoding;
        var buffer = new List<byte>();
        byte token;
        while (offset < dumpData.Length)
        {
            token = dumpData[offset];
            offset++;
            if (token == 0)
            {
                break;
            }

            buffer.Add(token);
        }
        if (buffer.Count == 0)
        {
            return "";
        }

        byte[] bytes = new byte[buffer.Count];
        buffer.CopyTo(bytes);
        return encode.GetString(bytes);
    }

    private void ReadLabels()
    {
        _labels = new Label[_header!.LabelCount];
        for (int i = 0; i < _header.LabelCount; i++)
        {
            int offset = i * 4;
            _labels[i] = new Label(i, BitConverter.ToInt32(_labelData!, offset));
        }
    }

    private void ReadDlls()
    {
        _dlls = new Dll[_header!.DllCount];

        for (int i = 0; i < _header.DllCount; i++)
        {
            int offset = 4 + (i * 24);
            _dlls[i].Name = ReadString(offset, _dllData!);
        }
    }

    private void ReadFuncs()
    {
        _funcs = new Func[_header!.FuncCount];
        for (int i = 0; i < _header.FuncCount; i++)
        {
            int offset = i * 16;
            _funcs[i].DllIndex = BitConverter.ToInt16(_funcData!, offset);
            offset += 4;
            _funcs[i].HikiType = BitConverter.ToInt16(_funcData!, offset);
            offset += 4;
            int funcnameOffset = BitConverter.ToInt32(_funcData!, offset);
            _funcs[i].Name = ReadString(funcnameOffset);
        }
    }

    private void ReadModules()
    {
        if (_header!.ModuleCount == 0)
        {
            return;
        }

        _modules = new Module[_header.ModuleCount];

        for (int i = 0; i < _header.ModuleCount; i++)
        {
            int offset = 4 + (i * 24);
            // Pre-existing: modules are stored in dllData in the HSP2 format (shared segment).
            _modules[i].Name = ReadString(offset, _dllData!);
        }
    }

    private void ReadDeffuncs()
    {
        _deffuncs = new Deffunc[_header!.DeffuncCount];

        for (int i = 0; i < _header.DeffuncCount; i++)
        {
            int offset = i * 16;
            int labelIndex = BitConverter.ToInt32(_deffuncData!, offset) - LabelIndexBaseOffset;
            _labels![labelIndex].Deffunc = i;

            offset += 4;
            _deffuncs[i].HikiType = BitConverter.ToInt16(_deffuncData!, offset);
            offset += 2;
            _deffuncs[i].HikiCount = BitConverter.ToInt16(_deffuncData!, offset);
            offset += 2;
            int deffuncnameOffset = BitConverter.ToInt32(_deffuncData!, offset);
            _deffuncs[i].Name = ReadString(deffuncnameOffset);
            _labels[labelIndex].Name = _deffuncs[i].ToString();
        }
    }

    #endregion

    private readonly List<string> _lines = new();

    internal void Decompile()
    {
        _tabNo = 1;
        _ifEnd.Clear();

        var cursor = new TokenCursor(this);
        _lines.Clear();

        ReadLabels();
        ReadDlls();
        ReadFuncs();
        ReadModules();
        ReadDeffuncs();

        if (_dlls != null)
        {
            for (int i = 0; i < _dlls.Length; i++)
            {
                _lines.Add(_dlls[i].ToString());
                if (_funcs != null)
                {
                    for (int j = 0; j < _funcs.Length; j++)
                    {
                        if (_funcs[j].DllIndex == i)
                        {
                            _lines.Add(_funcs[j].ToString());
                        }
                    }
                }
            }
        }

        cursor.SetZero();
        Token? token;
        try
        {
            while ((token = cursor.GetNext()) != null)
            {
                if (token.LabelIndex != -1)
                {
                    _labels![token.LabelIndex].LoadCount += 1;
                }
            }
        }
        catch (Exception e)
        {
            throw new HspDecoderException("AxHeader", Strings.UnrecoverableErrorDuringLabelRead, e);
        }

        for (int i = 0; i < _labels!.Length; i++)
        {
            _labels[i].Enabled = _labels[i].LoadCount > 0;
        }

        string? line;
        cursor.SetZero();
        while ((line = GetLine(cursor)) != null)
        {
            _lines.Add(line);
        }

        return;
    }

    private void AddLabel(TokenCursor cursor)
    {
        for (int i = 0; i < _labels!.Length; i++)
        {
            if (!_labels[i].Enabled)
            {
                continue;
            }

            if (cursor.Index >= _labels[i].TokenIndex)
            {
                _lines.Add(_labels[i].ToString() ?? string.Empty);
                _labels[i].Enabled = false;
            }
        }
    }

    private static string GetTab(int tab)
    {
        Debug.Assert(tab >= 0);
        return new string('\t', tab);
    }

    private int _tabNo = 1;
    private readonly List<int> _ifEnd = new();

    private void EmitLabel(Token token)
    {
        for (int i = 0; i < _labels!.Length; i++)
        {
            if (!_labels[i].Enabled)
            {
                continue;
            }

            if (token.Id == _labels[i].TokenIndex)
            {
                _lines.Add(_labels[i].Name);
                _labels[i].Enabled = false;
            }
        }
    }

    private string FormatTokenOutput(Token first, TokenCursor cursor)
    {
        string line = GetTab(_tabNo) + first.GetString();

        if (first._isLineend)
        {
            return line;
        }

        Token? token;
        while ((token = cursor.GetNext()) != null)
        {
            string add = token.GetString();
            line += token.IsArg ? ", " : " ";
            line += add;
            if (token._isLineend)
            {
                break;
            }
        }
        return line;
    }

    private string? GetLine(TokenCursor cursor)
    {
        Token? token = cursor.GetNext();
        if (token == null)
        {
            return null;
        }

        for (int i = 0; i < _ifEnd.Count; i++)
        {
            if ((token.Id == _ifEnd[i]) || (token.IfJumpId == _ifEnd[i]))
            {
                _tabNo--;
                _lines.Add(GetTab(_tabNo) + "}");
                _ifEnd.RemoveAt(i);
                i--;
            }
        }

        EmitLabel(token);

        bool tabPlus = token.TabPlus;
        int ifJumpTo = token._ifJumpTo;
        if (token.TabMinus)
        {
            _tabNo--;
        }

        string line = FormatTokenOutput(token, cursor);

        if (tabPlus)
        {
            _tabNo++;
        }

        if (ifJumpTo >= 0)
        {
            line += " {";
            _ifEnd.Add(ifJumpTo);
        }
        return line;
    }

    internal string? GetDeffuncName(int index) =>
        _deffuncs == null || (index >= _deffuncs.Length) || (index < 0)
            ? null
            : _deffuncs[index].Name;

    internal string? GetFuncName(int index) =>
        _funcs == null || (index >= _funcs.Length) || (index < 0) ? null : _funcs[index].Name;

    internal List<string> GetLines() => _lines;
}

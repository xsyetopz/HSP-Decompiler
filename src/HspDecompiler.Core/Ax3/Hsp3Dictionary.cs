using System;
using System.Collections.Generic;
using System.IO;
using HspDecompiler.Core.Ax3.Dictionary;
using HspDecompiler.Core.Encoding;

namespace HspDecompiler.Core.Ax3;

internal class Hsp3Dictionary
{
    private const int ExtendedCommandType = 0x11;
    private const int ExtendedCommandValueThreshold = 0x1000;
    private const int PluginFunctionTypeBase = 0x12;

    private Hsp3Dictionary() { }

    private readonly Dictionary<HspDictionaryKey, HspDictionaryValue> _codeDictionary = new();
    private readonly Dictionary<int, string> _paramDictionary = new();

    internal static Hsp3Dictionary? FromFile(string filePath)
    {
        try
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var reader = new StreamReader(stream, ShiftJisHelper.Encoding);
            return FromReader(reader);
        }
        catch (Exception ex) when (ex is IOException or FormatException or ArgumentException)
        {
            return null;
        }
    }

    private static IEnumerable<string[]> ReadCsvFields(TextReader reader)
    {
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            int commentIndex = line.IndexOf('#');
            if (commentIndex >= 0)
            {
                line = line[..commentIndex];
            }

            line = line.Trim();
            if (line.Length == 0)
            {
                continue;
            }

            yield return line.Split(',');
        }
    }

    private static Hsp3Dictionary FromReader(TextReader reader)
    {
        var dictionary = new Hsp3Dictionary();
        foreach (string[] tokens in ReadCsvFields(reader))
        {
            if (tokens.Length == 0)
            {
                continue;
            }

            string first = tokens[0].Trim();
            if (first.StartsWith('$'))
            {
                switch (first)
                {
                    case "$Code":
                        dictionary.LoadCodeDictionaryFromReader(reader);
                        break;
                    case "$ParamType":
                        dictionary.LoadParamDictionaryFromReader(reader);
                        break;
                    default:
                        break;
                }
            }
        }
        return dictionary;
    }

    private void LoadCodeDictionaryFromReader(TextReader reader)
    {
        foreach (string[] tokens in ReadCsvFields(reader))
        {
            if (tokens.Length == 0)
            {
                continue;
            }

            if (tokens[0].Trim() == "$End")
            {
                return;
            }

            if (tokens.Length >= 4)
            {
                string[] extraFlags = new string[tokens.Length - 4];
                Array.Copy(tokens, 4, extraFlags, 0, tokens.Length - 4);
                var key = new HspDictionaryKey(tokens[0].Trim(), tokens[1].Trim());
                var value = new HspDictionaryValue(tokens[2].Trim(), tokens[3].Trim(), extraFlags);
                _codeDictionary.Add(key, value);
            }
        }
    }

    private void LoadParamDictionaryFromReader(TextReader reader)
    {
        foreach (string[] tokens in ReadCsvFields(reader))
        {
            if (tokens.Length == 0)
            {
                continue;
            }

            if (tokens[0].Trim() == "$End")
            {
                return;
            }

            if (tokens.Length >= 2)
            {
                int key = DicParser.StringToInt32(tokens[0].Trim());
                string value = tokens[1].Trim();
                _paramDictionary.Add(key, value);
            }
        }
    }

    internal bool CodeLookUp(HspDictionaryKey key, out HspDictionaryValue value)
    {
        if (_codeDictionary.TryGetValue(key, out value))
        {
            return true;
        }

        var newkey = new HspDictionaryKey(key) { _value = -1, _allValue = true };
        if (_codeDictionary.TryGetValue(newkey, out value))
        {
            return true;
        }

        if ((key._type == ExtendedCommandType) && (key._value >= ExtendedCommandValueThreshold))
        {
            value._name = "comfunc";
            value._type = HspCodeType.ComFunction;
            value._extra = HspCodeExtraOptions.NONE;
            return true;
        }
        if (key._type >= PluginFunctionTypeBase)
        {
            value._name = "pluginFunction";
            value._operatorPriority = key._type - PluginFunctionTypeBase;
            value._type = HspCodeType.PlugInFunction;
            value._extra = HspCodeExtraOptions.NONE;
            return true;
        }
        return false;
    }

    internal bool ParamLookUp(int paramKey, out string? paramTypeName) =>
        _paramDictionary.TryGetValue(paramKey, out paramTypeName);

    internal List<string> GetAllFuncName()
    {
        var functionNames = new List<string>();
        foreach (KeyValuePair<HspDictionaryKey, HspDictionaryValue> pair in _codeDictionary)
        {
            switch (pair.Value._type)
            {
                case HspCodeType.HspFunction:
                case HspCodeType.IfStatement:
                case HspCodeType.OnEventStatement:
                case HspCodeType.OnStatement:
                case HspCodeType.McallStatement:
                    functionNames.Add(
                        pair.Value._name.ToLower(System.Globalization.CultureInfo.CurrentCulture)
                    );
                    break;
                case HspCodeType.NONE:
                    break;
                case HspCodeType.Operator:
                    break;
                case HspCodeType.Symbol:
                    break;
                case HspCodeType.Variable:
                    break;
                case HspCodeType.StringValue:
                    break;
                case HspCodeType.DoubleValue:
                    break;
                case HspCodeType.IntegerValue:
                    break;
                case HspCodeType.Param:
                    break;
                case HspCodeType.Label:
                    break;
                case HspCodeType.UserFunction:
                    break;
                case HspCodeType.DllFunction:
                    break;
                case HspCodeType.ComFunction:
                    break;
                case HspCodeType.PlugInFunction:
                    break;
                case HspCodeType.ElseStatement:
                    break;
                default:
                    break;
            }
        }

        foreach (KeyValuePair<int, string> pair in _paramDictionary)
        {
            functionNames.Add(pair.Value.ToLower(System.Globalization.CultureInfo.CurrentCulture));
        }
        return functionNames;
    }
}

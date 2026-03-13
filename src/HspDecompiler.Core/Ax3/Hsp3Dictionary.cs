using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using HspDecompiler.Core.Ax3.Dictionary;
using HspDecompiler.Core.Encoding;

namespace HspDecompiler.Core.Ax3
{
    class Hsp3Dictionary
    {
        private Hsp3Dictionary()
        {
        }

        private Dictionary<HspDictionaryKey, HspDictionaryValue> codeDictionary = new Dictionary<HspDictionaryKey, HspDictionaryValue>();
        private Dictionary<int, string> paramDictionary = new Dictionary<int, string>();

        internal static Hsp3Dictionary FromFile(string filePath)
        {
            try
            {
                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                using var reader = new StreamReader(stream, ShiftJisHelper.Encoding);
                return FromReader(reader);
            }
            catch
            {
                return null;
            }
        }

        private static IEnumerable<string[]> ReadCsvFields(TextReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                int commentIndex = line.IndexOf('#');
                if (commentIndex >= 0)
                    line = line.Substring(0, commentIndex);
                line = line.Trim();
                if (line.Length == 0)
                    continue;
                yield return line.Split(',');
            }
        }

        private static Hsp3Dictionary FromReader(TextReader reader)
        {
            Hsp3Dictionary ret = new Hsp3Dictionary();
            var fields = new List<string[]>();
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                int commentIndex = line.IndexOf('#');
                if (commentIndex >= 0)
                    line = line.Substring(0, commentIndex);
                line = line.Trim();
                if (line.Length == 0)
                    continue;
                string[] tokens = line.Split(',');
                if (tokens.Length == 0)
                    continue;
                string first = tokens[0].Trim();
                if (first.StartsWith("$"))
                {
                    switch (first)
                    {
                        case "$Code":
                            ret.loadCodeDictionaryFromReader(reader);
                            break;
                        case "$ParamType":
                            ret.loadParamDictionaryFromReader(reader);
                            break;
                    }
                }
            }
            return ret;
        }

        private void loadCodeDictionaryFromReader(TextReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                int commentIndex = line.IndexOf('#');
                if (commentIndex >= 0)
                    line = line.Substring(0, commentIndex);
                line = line.Trim();
                if (line.Length == 0)
                    continue;
                string[] tokens = line.Split(',');
                if (tokens.Length == 0)
                    continue;
                if (tokens[0].Trim() == "$End")
                    return;
                if (tokens.Length >= 4)
                {
                    string[] extraFlags = new string[tokens.Length - 4];
                    Array.Copy(tokens, 4, extraFlags, 0, tokens.Length - 4);
                    HspDictionaryKey key = new HspDictionaryKey(tokens[0].Trim(), tokens[1].Trim());
                    HspDictionaryValue value = new HspDictionaryValue(tokens[2].Trim(), tokens[3].Trim(), extraFlags);
                    codeDictionary.Add(key, value);
                }
            }
        }

        private void loadParamDictionaryFromReader(TextReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                int commentIndex = line.IndexOf('#');
                if (commentIndex >= 0)
                    line = line.Substring(0, commentIndex);
                line = line.Trim();
                if (line.Length == 0)
                    continue;
                string[] tokens = line.Split(',');
                if (tokens.Length == 0)
                    continue;
                if (tokens[0].Trim() == "$End")
                    return;
                if (tokens.Length >= 2)
                {
                    int key = DicParser.StringToInt32(tokens[0].Trim());
                    string value = tokens[1].Trim();
                    paramDictionary.Add(key, value);
                }
            }
        }

        internal bool CodeLookUp(HspDictionaryKey key, out HspDictionaryValue value)
        {
            if (codeDictionary.TryGetValue(key, out value))
                return true;
            HspDictionaryKey newkey = new HspDictionaryKey(key);
            newkey.Value = -1;
            newkey.AllValue = true;
            if (codeDictionary.TryGetValue(newkey, out value))
                return true;
            if ((key.Type == 0x11) && (key.Value >= 0x1000))
            {
                value.Name = "comfunc";
                value.Type = HspCodeType.ComFunction;
                value.Extra = HspCodeExtraFlags.NONE;
                return true;
            }
            if (key.Type >= 0x12)
            {
                value.Name = "pluginFuction";
                value.OparatorPriority = key.Type - 0x12;
                value.Type = HspCodeType.PlugInFunction;
                value.Extra = HspCodeExtraFlags.NONE;
                return true;
            }
            return false;
        }

        internal bool ParamLookUp(int paramKey, out string paramTypeName)
        {
            return paramDictionary.TryGetValue(paramKey, out paramTypeName);
        }

        internal List<string> GetAllFuncName()
        {
            List<string> ret = new List<string>();
            foreach (KeyValuePair<HspDictionaryKey, HspDictionaryValue> pair in codeDictionary)
            {
                switch (pair.Value.Type)
                {
                    case HspCodeType.HspFunction:
                    case HspCodeType.IfStatement:
                    case HspCodeType.OnEventStatement:
                    case HspCodeType.OnStatement:
                    case HspCodeType.McallStatement:
                        ret.Add(pair.Value.Name.ToLower());
                        break;
                }
            }

            foreach (KeyValuePair<int, string> pair in paramDictionary)
            {
                ret.Add(pair.Value.ToLower());
            }
            return ret;
        }
    }
}

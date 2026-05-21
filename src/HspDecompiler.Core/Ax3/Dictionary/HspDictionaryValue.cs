using System;
using System.Globalization;

namespace HspDecompiler.Core.Ax3.Dictionary;

internal struct HspDictionaryValue
{
    internal HspDictionaryValue(string theName, string theType, string[] theExtras)
    {
        _name = theName;
        _type = ParseCodeType(theType);
        _extra = HspCodeExtraOptions.NONE;
        _operatorPriority = -1;
        foreach (string theExtra in theExtras)
        {
            string testString = theExtra.Trim();
            if (testString.Length == 0)
            {
                continue;
            }

            if (testString.StartsWith("Priority_", StringComparison.Ordinal))
            {
                _operatorPriority = int.Parse(testString[9..], CultureInfo.InvariantCulture);
                continue;
            }
            _extra |= (HspCodeExtraOptions)Enum.Parse(typeof(HspCodeExtraOptions), testString);
        }
    }

    internal string _name;
    internal HspCodeType _type;
    internal HspCodeExtraOptions _extra;
    internal int _operatorPriority;

    public override readonly string ToString() =>
        _name.Length == 0 ? _type.ToString() : _type.ToString() + "  \"" + _name + "\"";

    private static HspCodeType ParseCodeType(string value) =>
        value switch
        {
            "String" => HspCodeType.StringValue,
            "Double" => HspCodeType.DoubleValue,
            "Integer" => HspCodeType.IntegerValue,
            _ => (HspCodeType)Enum.Parse(typeof(HspCodeType), value),
        };
}

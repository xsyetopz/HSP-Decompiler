using System;
using System.Globalization;

namespace HspDecompiler.Core.Ax3.Dictionary;

internal static class DicParser
{
    internal static int StringToInt32(string str, int defaultValue)
    {
        try
        {
            str = str.Trim();
            if (str.StartsWith("0x", StringComparison.Ordinal))
            {
                str = str[2..];
                return Int32.Parse(
                    str,
                    System.Globalization.NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture
                );
            }
            return Int32.Parse(str, CultureInfo.InvariantCulture);
        }
        catch (Exception)
        {
            return defaultValue;
        }
    }

    internal static int StringToInt32(string str) => StringToInt32(str, 0);

    internal static double StringToDouble(string str, double defaultValue)
    {
        try
        {
            return Double.Parse(str, CultureInfo.InvariantCulture);
        }
        catch (Exception)
        {
            return defaultValue;
        }
    }

    internal static double StringToDouble(string str) => StringToDouble(str, 0.0);

    internal static object StringToEnum(Type enumType, string str, int defaultValue)
    {
        try
        {
            return Enum.Parse(enumType, str);
        }
        catch (Exception)
        {
            try
            {
                return Int32.Parse(str, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }
    }

    internal static object StringToEnum(Type enumType, string str) =>
        StringToEnum(enumType, str, 0);
}

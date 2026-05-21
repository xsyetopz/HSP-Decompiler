using System;
using System.Globalization;

namespace HspDecompiler.Core.Ax3.Dictionary;

internal struct HspDictionaryKey : IComparable<HspDictionaryKey>, IEquatable<HspDictionaryKey>
{
    internal HspDictionaryKey(HspDictionaryKey key)
    {
        _type = key._type;
        _value = key._value;
        _allValue = key._allValue;
    }

    internal HspDictionaryKey(string theType, string theValue)
    {
        _type = DicParser.StringToInt32(theType);
        _value = DicParser.StringToInt32(theValue);
        _allValue = false;
        if (_value == -1)
        {
            _allValue = true;
        }
    }

    internal int _type;
    internal int _value;
    internal bool _allValue;

    public override readonly string ToString()
    {
        return _value == -1
            ? "Type:0x" + _type.ToString("X02", CultureInfo.InvariantCulture) + "Value:0xFFFF"
            : "Type:0x"
                + _type.ToString("X02", CultureInfo.InvariantCulture)
                + "Value:0x"
                + _value.ToString("X04", CultureInfo.InvariantCulture);
    }

    public override readonly bool Equals(object? obj) => obj is HspDictionaryKey key && Equals(key);

    public override readonly int GetHashCode() => _type.GetHashCode() ^ _value.GetHashCode();

    public readonly bool Equals(HspDictionaryKey other) =>
        _type.Equals(other._type) && _value.Equals(other._value);

    public readonly int CompareTo(HspDictionaryKey other)
    {
        int ret = _type.CompareTo(other._type);
        return ret != 0 ? ret : _value.CompareTo(other._value);
    }
}

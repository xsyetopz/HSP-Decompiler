using System.Globalization;

namespace HspDecompiler.Core.DpmToAx.Crypto;

internal struct XorAddTransform
{
    internal byte _xorByte;
    internal byte _addByte;
    internal bool _xorSum;

    public override readonly string ToString() =>
        "xor:0x"
        + _xorByte.ToString("X02", CultureInfo.InvariantCulture)
        + "    "
        + "add:0x"
        + _addByte.ToString("X02", CultureInfo.InvariantCulture)
        + "    "
        + "Farst xor:"
        + _xorSum.ToString(CultureInfo.InvariantCulture);

    internal readonly byte Encode(byte b) =>
        _xorSum ? Sum(Xor(b, _xorByte), _addByte) : Xor(Sum(b, _addByte), _xorByte);

    internal readonly byte Decode(byte b) =>
        _xorSum ? Xor(Dif(b, _addByte), _xorByte) : Dif(Xor(b, _xorByte), _addByte);

    internal static byte GetXorByte(byte add, byte plain, byte encrypted, bool xorSum) =>
        xorSum ? (byte)(Dif(encrypted, add) ^ plain) : Xor(encrypted, Sum(plain, add));

    internal static byte Xor(byte b1, byte b2) => (byte)(b1 ^ b2);

    internal static byte Sum(byte b1, byte b2) => (byte)((b1 + b2) & 0xFF);

    internal static byte Dif(byte b1, byte b2) => (byte)((0x100 + b1 - b2) & 0xFF);
}

namespace HspDecompiler.Core.DpmToAx.Crypto
{
    internal struct XorAddTransform
    {
        internal byte XorByte;
        internal byte AddByte;
        internal bool XorSum;

        public override string ToString()
        {
            return "xor:0x" + XorByte.ToString("X02") + "    " + "add:0x" + AddByte.ToString("X02") + "    " + "Farst xor:" + XorSum.ToString();
        }

        internal byte Encode(byte b)
        {
            if (XorSum)
                return Sum(Xor(b, XorByte), AddByte);
            return Xor(Sum(b, AddByte), XorByte);
        }

        internal byte Decode(byte b)
        {
            if (XorSum)
                return Xor(Dif(b, AddByte), XorByte);
            return Dif(Xor(b, XorByte), AddByte);
        }

        internal static byte GetXorByte(byte add, byte plain, byte encrypted, bool XorSum)
        {
            if (XorSum)
                return (byte)(Dif(encrypted, add) ^ plain);
            return Xor(encrypted, Sum(plain, add));
        }

        internal static byte Xor(byte b1, byte b2)
        {
            return (byte)(b1 ^ b2);
        }

        internal static byte Sum(byte b1, byte b2)
        {
            return (byte)((b1 + b2) & 0xFF);
        }

        internal static byte Dif(byte b1, byte b2)
        {
            return (byte)((0x100 + b1 - b2) & 0xFF);
        }
    }
}

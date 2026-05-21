using System;
using System.Globalization;

namespace HspDecompiler.Core.DpmToAx.Crypto;

internal sealed class Dpm2CryptoTransform
{
    private const uint A = 1103515245;
    private const uint C = 12345;

    private readonly int _accum1;
    private readonly int _accum2;

    private Dpm2CryptoTransform(int accum1, int accum2)
    {
        _accum1 = accum1;
        _accum2 = accum2;
    }

    internal static Dpm2CryptoTransform FromCrcSeed(int crcSeed, int salt)
    {
        int encode = RecoverEncode(crcSeed, salt);
        int passwordValue = crcSeed + encode;
        string password = string.Create(
            CultureInfo.InvariantCulture,
            $"HSP3Encode:{passwordValue}"
        );
        byte[] passwordBytes = System.Text.Encoding.ASCII.GetBytes(password);
        uint crc = Crc32(passwordBytes);

        ushort v6 = (ushort)(crc & 0xFFFF);
        ushort v7 = (ushort)((crc >> 16) & 0xFFFF);

        int accum1 = ComputeAccum(v6);
        int accum2 = ComputeAccum(v7);

        return new Dpm2CryptoTransform(accum1, accum2);
    }

    private static int RecoverEncode(int seed, int salt)
    {
        if (salt == 0)
        {
            return 0;
        }

        for (int encode = 0; encode <= 0xFFFF; encode++)
        {
            int passwordValue = seed + encode;
            string password = string.Create(
                CultureInfo.InvariantCulture,
                $"HSP3Encode:{passwordValue}"
            );
            byte[] passwordBytes = System.Text.Encoding.ASCII.GetBytes(password);
            uint crc = Crc32(passwordBytes);

            int seedSum = (int)((crc & 0xFFFF) + ((crc >> 16) & 0xFFFF));

            uint lcgState = unchecked(A * (uint)(encode + seed) + C);
            int lcgOut = (int)((lcgState >> 16) & 0x7FFF);
            int computedSalt = seedSum + lcgOut;

            if (computedSalt == salt)
            {
                return encode;
            }
        }

        throw new InvalidOperationException(
            string.Create(
                CultureInfo.InvariantCulture,
                $"Unable to recover DPM2 encode value for seed={seed}, salt={salt}."
            )
        );
    }

    private static int ComputeAccum(ushort seedValue)
    {
        uint seed = seedValue;
        int accum = 0;
        for (int i = 0; i < 0x4000; i++)
        {
            for (int j = 0; j < 256; j++)
            {
                accum += (int)(Lcg(ref seed) & 0xFF);
            }
        }
        return accum;
    }

    internal byte[] Decrypt(byte[] encrypted, int checksum)
    {
        byte[] decrypted = new byte[encrypted.Length];
        int currentPage = -1;
        byte[] keyTable = new byte[256];

        for (int pos = 0; pos < encrypted.Length; pos++)
        {
            int page = pos / 256;
            if (page != currentPage)
            {
                GenerateKeyTable(keyTable, page, checksum);
                currentPage = page;
            }
            decrypted[pos] = (byte)((encrypted[pos] - keyTable[pos & 0xFF]) & 0xFF);
        }

        return decrypted;
    }

    private void GenerateKeyTable(byte[] keyTable, int page, int checksum)
    {
        uint seed = unchecked((uint)(page + checksum + _accum1));
        for (int i = 0; i < 128; i++)
        {
            keyTable[i] = (byte)(Lcg(ref seed) & 0xFF);
        }

        seed = unchecked((uint)(page + checksum + _accum2));
        for (int i = 0; i < 128; i++)
        {
            keyTable[128 + i] = (byte)(Lcg(ref seed) & 0xFF);
        }
    }

    private static uint Lcg(ref uint seed)
    {
        seed = unchecked(A * seed + C);
        return (seed >> 16) & 0x7FFF;
    }

    private static uint Crc32(byte[] data)
    {
        uint crc = 0xFFFFFFFF;
        foreach (byte b in data)
        {
            crc ^= b;
            for (int i = 0; i < 8; i++)
            {
                crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320u : crc >> 1;
            }
        }
        return ~crc;
    }
}

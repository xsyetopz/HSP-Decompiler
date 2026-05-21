using System;
using System.Collections.Generic;
using HspDecompiler.Core.Resources;

namespace HspDecompiler.Core.DpmToAx.Crypto;

internal class HspCryptoTransform
{
    private const string Hsp2Magic = "HSP2";
    private const string Hsp3Magic = "HSP3";

    private XorAddTransform _xorAdd;

    internal XorAddTransform XorAdd
    {
        get => _xorAdd;
        set => _xorAdd = value;
    }

    public override string ToString() => _xorAdd.ToString();

    internal byte[] Encryption(byte[] plain)
    {
        byte[] encrypted = new byte[plain.Length];
        byte prevByte = 0;
        for (int i = 0; i < encrypted.Length; i++)
        {
            encrypted[i] = _xorAdd.Encode(XorAddTransform.Dif(plain[i], prevByte));
            prevByte = plain[i];
        }
        return encrypted;
    }

    internal byte[] Decryption(byte[] encrypted)
    {
        byte[] plain = new byte[encrypted.Length];
        byte prevByte = 0;
        for (int i = 0; i < encrypted.Length; i++)
        {
            byte plainByte = _xorAdd.Decode(encrypted[i]);
            plain[i] = XorAddTransform.Sum(plainByte, prevByte);
            prevByte = plain[i];
        }
        return plain;
    }

    internal static HspCryptoTransform? CrackEncryption(
        byte[] encrypted,
        Func<byte[], bool> validator
    )
    {
        byte[] plain3 = System.Text.Encoding.ASCII.GetBytes(Hsp3Magic);
        HspCryptoTransform? hsp3crypto = CrackEncryption(plain3, encrypted, validator);
        if (hsp3crypto != null)
        {
            return hsp3crypto;
        }

        byte[] plain2 = System.Text.Encoding.ASCII.GetBytes(Hsp2Magic);
        HspCryptoTransform? hsp2crypto = CrackEncryption(plain2, encrypted, validator);
        return hsp2crypto;
    }

    internal static HspCryptoTransform? CrackEncryption(
        byte[] plain,
        byte[] encrypted,
        Func<byte[], bool> validator
    )
    {
        int count = Math.Min(plain.Length, encrypted.Length);
        if (count < 2)
        {
            throw new InvalidOperationException(Strings.BufferSizeTooSmall);
        }

        byte[] difBuffer = new byte[count];
        byte prevByte = 0;
        byte andByte = 0xFF;
        byte orByte = 0x00;
        for (int i = 0; i < count; i++)
        {
            difBuffer[i] = XorAddTransform.Dif(plain[i], prevByte);
            prevByte = plain[i];
            andByte &= difBuffer[i];
            orByte |= difBuffer[i];
        }
        if ((andByte != 0x00) || (orByte != 0xFF))
        {
            throw new InvalidOperationException(Strings.InsufficientDecryptionInfo);
        }

        var transformList = new List<XorAddTransform>();
        for (int i = 0; i < 0x100; i++)
        {
            XorAddTransform xoradd;
            bool ok = true;
            byte add = (byte)(i & 0x7F);
            xoradd._xorSum = (i >= 0x80);
            xoradd._addByte = add;
            xoradd._xorByte = XorAddTransform.GetXorByte(
                add,
                difBuffer[0],
                encrypted[0],
                xoradd._xorSum
            );
            for (int index = 1; index < count; index++)
            {
                if (encrypted[index] != xoradd.Encode(difBuffer[index]))
                {
                    ok = false;
                    break;
                }
            }
            if (ok)
            {
                var decryptor = new HspCryptoTransform { _xorAdd = xoradd };
                byte[] buffer = (byte[])encrypted.Clone();
                buffer = decryptor.Decryption(buffer);

                if (validator(buffer))
                {
                    transformList.Add(xoradd);
                    break;
                }
            }
        }
        if (transformList.Count == 1)
        {
            var ret = new HspCryptoTransform { _xorAdd = transformList[0] };
            return ret;
        }
        return null;
    }
}

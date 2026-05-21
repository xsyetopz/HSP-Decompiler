using System;
using HspDecompiler.Core.DpmToAx.Crypto;
using Xunit;

namespace HspDecompiler.Core.Tests;

public class HspCryptoTransformTests
{
    [Fact]
    public void EncryptionDecryptionRoundtrip()
    {
        var transform = new HspCryptoTransform
        {
            XorAdd = new XorAddTransform
            {
                _xorByte = 0xAB,
                _addByte = 0x34,
                _xorSum = false,
            },
        };

        byte[] plaintext = new byte[] { 0x48, 0x53, 0x50, 0x33, 0x00, 0xFF, 0x80 };
        byte[] encrypted = transform.Encryption(plaintext);
        byte[] decrypted = transform.Decryption(encrypted);

        Assert.Equal(plaintext, decrypted);
    }

    [Fact]
    public void EncryptionProducesDifferentOutput()
    {
        var transform = new HspCryptoTransform
        {
            XorAdd = new XorAddTransform
            {
                _xorByte = 0xAB,
                _addByte = 0x34,
                _xorSum = false,
            },
        };

        byte[] plaintext = "HSP3"u8.ToArray();
        byte[] encrypted = transform.Encryption(plaintext);

        bool allSame = true;
        for (int i = 0; i < plaintext.Length; i++)
        {
            if (plaintext[i] != encrypted[i])
            {
                allSame = false;
                break;
            }
        }

        Assert.False(allSame);
    }

    [Fact]
    public void CrackEncryptionFindsKeyForHsp3Header()
    {
        var originalTransform = new HspCryptoTransform
        {
            XorAdd = new XorAddTransform
            {
                _xorByte = 0x42,
                _addByte = 0x17,
                _xorSum = false,
            },
        };

        byte[] plaintext = new byte[] { 0x48, 0x53, 0x50, 0x33, 0x01, 0x02, 0x03, 0x04 };
        byte[] encrypted = originalTransform.Encryption(plaintext);

        static bool validator(byte[] data) =>
            data.Length >= 4
            && data[0] == 0x48
            && data[1] == 0x53
            && data[2] == 0x50
            && (data[3] == 0x33 || data[3] == 0x32);

        var cracked = HspCryptoTransform.CrackEncryption(encrypted, validator);

        Assert.NotNull(cracked);
        byte[] decrypted = cracked.Decryption(encrypted);
        Assert.Equal(plaintext, decrypted);
    }

    [Fact]
    public void Dpm2FromCrcSeedThrowsWhenEncodeCannotBeRecovered()
    {
        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
            Dpm2CryptoTransform.FromCrcSeed(1, int.MaxValue)
        );

        Assert.Contains("seed=1", ex.Message, StringComparison.Ordinal);
        Assert.Contains("salt=2147483647", ex.Message, StringComparison.Ordinal);
    }
}

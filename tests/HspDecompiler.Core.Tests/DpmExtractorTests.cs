using System.IO;
using HspDecompiler.Core.DpmToAx;
using HspDecompiler.Core.Encoding;
using Xunit;

namespace HspDecompiler.Core.Tests;

public class DpmExtractorTests
{
    [Fact]
    public void FromBinaryReaderValidDpmHeader()
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        // 16-byte header: magic, unknown, fileCount, unknown
        bw.Write("DPMX"u8.ToArray()); // DPMX
        bw.Write(0); // unknown
        bw.Write(1); // fileCount = 1
        bw.Write(0); // unknown

        // 32-byte file entry: 16-byte name + 4 int32 fields
        byte[] name = new byte[16];
        System.Text.Encoding.ASCII.GetBytes("test.ax").CopyTo(name, 0);
        bw.Write(name);
        bw.Write(0); // unknown
        bw.Write(0); // EncryptionKey = 0 (not encrypted)
        bw.Write(0); // FileOffset = 0
        bw.Write(100); // FileSize = 100

        // file data (fileOffsetStart=48, fileOffset=0 => seek to 48; data must be 100 bytes)
        bw.Write(new byte[100]);
        bw.Flush();

        ms.Seek(0, SeekOrigin.Begin);
        using var br = new BinaryReader(ms, ShiftJisHelper.Encoding);
        var extractor = DpmExtractor.FromBinaryReader(br);

        Assert.NotNull(extractor);
        Assert.Single(extractor.FileList);
        Assert.Equal("test.ax", extractor.FileList[0].FileName);
        Assert.False(extractor.FileList[0].IsEncrypted);
        Assert.Equal(100, extractor.FileList[0].FileSize);
    }

    [Fact]
    public void FromBinaryReaderInvalidMagicReturnsNull()
    {
        byte[] data = new byte[16];
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms, ShiftJisHelper.Encoding);
        var extractor = DpmExtractor.FromBinaryReader(br);
        Assert.Null(extractor);
    }

    [Fact]
    public void FromBinaryReaderDpm2RejectsNegativeDataOffset()
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        bw.Write("DPM2"u8.ToArray());
        bw.Write(1);  // fileCount
        bw.Write(64); // stringTableOffset
        bw.Write(72); // dataSectionOffset
        bw.Write(8);  // stringTableSize
        bw.Write(0);
        bw.Write(0);
        bw.Write(0);

        bw.Write(0);  // flags
        bw.Write(0);  // nameOffset
        bw.Write(1);  // fileSize
        bw.Write(0);
        bw.Write(-1); // dataOffset must not seek before data section
        bw.Write(0);
        bw.Write(0);  // dirOffset
        bw.Write(0);  // checksum

        bw.Write("file.ax\0"u8.ToArray());
        bw.Write(new byte[10]);
        bw.Flush();

        ms.Seek(0, SeekOrigin.Begin);
        using var br = new BinaryReader(ms, ShiftJisHelper.Encoding);
        var extractor = DpmExtractor.FromBinaryReader(br);

        Assert.Null(extractor);
    }

    [Fact]
    public void DpmFileEntryIsEncryptedWhenKeyNonZero()
    {
        var entry = new DpmFileEntry { EncryptionKey = 0x1234 };
        Assert.True(entry.IsEncrypted);
    }

    [Fact]
    public void DpmFileEntryNotEncryptedWhenKeyZero()
    {
        var entry = new DpmFileEntry { EncryptionKey = 0 };
        Assert.False(entry.IsEncrypted);
    }
}

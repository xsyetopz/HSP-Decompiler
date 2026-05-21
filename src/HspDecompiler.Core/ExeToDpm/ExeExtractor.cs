using System.IO;
using HspDecompiler.Core.Exceptions;

namespace HspDecompiler.Core.ExeToDpm;

// Legacy/unused: no callers exist outside this file in the current codebase.
internal sealed class ExeExtractor
{
    // Known offsets where DPMX data begins in HSP runtime executables.
    private const long DpmxOffsetLarge = 0x25000;
    private const long DpmxOffsetSmall = 0x1BE00;

    // DPMX and DPM2 magic bytes: 'D', 'P', 'M', 'X'/'2'
    private const byte DpmMagicD = 0x44;
    private const byte DpmMagicP = 0x50;
    private const byte DpmMagicM = 0x4D;
    private const byte DpmxMagicX = 0x58;
    private const byte Dpm2Magic2 = 0x32;

    internal static void GetDpmFile(string exeFilePath, string dpmFilePath)
    {
        try
        {
            var stream = new FileStream(exeFilePath, FileMode.Open, FileAccess.Read);
            using (stream)
            {
                var dpmStream = new FileStream(dpmFilePath, FileMode.Create, FileAccess.Write);
                using (dpmStream)
                {
                    GetDpmFile(stream, dpmStream);
                }
            }
        }
        catch (IOException ex)
        {
            throw new HspDecoderException(ex.Message, ex);
        }
    }

    internal static void GetDpmFile(Stream exeStream, Stream dpmStream)
    {
        try
        {
            long dpmOffset = seekDpmStart(exeStream);
            if (dpmOffset < 0)
            {
                return;
            }

            exeStream.Seek(dpmOffset, SeekOrigin.Begin);
            int dpmSize = (int)(exeStream.Length - dpmOffset);
            byte[] data = new byte[dpmSize];
            exeStream.ReadExactly(data, 0, dpmSize);
            dpmStream.Write(data, 0, dpmSize);
        }
        catch (IOException ex)
        {
            throw new HspDecoderException(ex.Message, ex);
        }
    }

    private static bool IsDpmMagic(byte[] header)
    {
        return header.Length >= 4
            && header[0] == DpmMagicD
            && header[1] == DpmMagicP
            && header[2] == DpmMagicM
            && (header[3] == DpmxMagicX || header[3] == Dpm2Magic2);
    }

    private static long seekDpmStart(Stream exeStream)
    {
        byte[] header = new byte[4];
        if (exeStream.Length >= DpmxOffsetLarge + 4)
        {
            exeStream.Seek(DpmxOffsetLarge, SeekOrigin.Begin);
            exeStream.ReadExactly(header, 0, 4);
            if (IsDpmMagic(header))
            {
                return DpmxOffsetLarge;
            }
        }

        if (exeStream.Length >= DpmxOffsetSmall + 4)
        {
            exeStream.Seek(DpmxOffsetSmall, SeekOrigin.Begin);
            exeStream.ReadExactly(header, 0, 4);
            if (IsDpmMagic(header))
            {
                return DpmxOffsetSmall;
            }
        }

        exeStream.Seek(0, SeekOrigin.Begin);
        long index = 0;
        long length = exeStream.Length;
        while (index < length)
        {
            exeStream.Seek(index, SeekOrigin.Begin);
            if (exeStream.Read(header, 0, 4) < 4)
            {
                break;
            }

            if (IsDpmMagic(header))
            {
                return index;
            }

            index += 0x04;
        }
        return -1;
    }
}

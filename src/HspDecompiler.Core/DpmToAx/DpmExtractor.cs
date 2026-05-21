using System;
using System.Collections.Generic;
using System.IO;
using HspDecompiler.Core.ExeToDpm;

namespace HspDecompiler.Core.DpmToAx;

internal sealed class DpmExtractor
{
    private DpmExtractor() { }

    internal static DpmExtractor? FromBinaryReader(BinaryReader reader)
    {
        var ret = new DpmExtractor();
        try
        {
            ret._reader = reader;
            if (ret.ReadHeader())
            {
                return ret;
            }
        }
        catch (IOException)
        {
            return null;
        }
        return null;
    }

    private long _startPosition;
    private long _streamLength;
    private long _fileOffsetStart;
    private bool _isDpm2;
    private int _crcSeed;
    private int _salt;

    internal bool IsDpm2 => _isDpm2;
    internal int CrcSeed => _crcSeed;
    internal int Salt => _salt;

    private bool ReadHeader()
    {
        _startPosition = _reader!.BaseStream.Position;
        _streamLength = _reader.BaseStream.Length - _startPosition;
        char[] identifier = _reader.ReadChars(4);
        if (identifier.Length < 4)
        {
            return false;
        }

        _reader.BaseStream.Seek(_startPosition, SeekOrigin.Begin);
        if ((identifier[0] == 'M') && (identifier[1] == 'Z'))
        {
            var winHeader = Win32PeHeader.FromBinaryReader(_reader);
            if (winHeader == null)
            {
                return false;
            }
            _startPosition += winHeader.EndOfExecutableRegion;
            _streamLength = _reader.BaseStream.Length - _startPosition;
            _reader.BaseStream.Seek(_startPosition, SeekOrigin.Begin);
            identifier = _reader.ReadChars(4);
            if (identifier.Length < 4)
            {
                return false;
            }
        }
        if (!((identifier[0] == 'D') && (identifier[1] == 'P') && (identifier[2] == 'M')))
        {
            return false;
        }
        bool isDpm2;
        if (identifier[3] == 'X')
        {
            isDpm2 = false;
        }
        else if (identifier[3] == '2')
        {
            isDpm2 = true;
        }
        else
        {
            return false;
        }
        _reader.BaseStream.Seek(_startPosition, SeekOrigin.Begin);
        if (isDpm2)
        {
            return ReadDpm2Header();
        }
        return ReadDpmxHeader();
    }

    private BinaryReader? _reader;
    private readonly List<DpmFileEntry> _files = new();

    /// <summary>
    /// DPMX header (16 bytes):
    ///   [0-3]  magic "DPMX"
    ///   [4-7]  ?
    ///   [8-11] fileCount
    ///   [12-15] ?
    /// Entry (32 bytes): 16-char name + unknown(4) + encryptionKey(4) + fileOffset(4) + fileSize(4)
    /// </summary>
    private bool ReadDpmxHeader()
    {
        _reader!.ReadInt32(); // DPMX magic
        _reader.ReadInt32();
        int fileCount = _reader.ReadInt32();
        _reader.ReadInt32();
        _files.Capacity = fileCount;
        _fileOffsetStart = _startPosition + 0x10 + fileCount * 0x20;
        for (int i = 0; i < fileCount; i++)
        {
            var file = new DpmFileEntry();
            char[] chars = _reader.ReadChars(16);
            int stringLength = 16;
            for (int j = 0; j < 16; j++)
            {
                if (chars[j] == '\0')
                {
                    stringLength = j;
                    break;
                }
            }
            file.FileName = new string(chars, 0, stringLength);
            file.Unknown = _reader.ReadInt32();
            file.EncryptionKey = _reader.ReadInt32();
            file.FileOffset = _reader.ReadInt32();
            file.FileSize = _reader.ReadInt32();
            if ((file.FileOffset + file.FileSize) > _streamLength)
            {
                return false;
            }

            _files.Add(file);
        }

        return true;
    }

    /// <summary>
    /// DPM2 header (32 bytes):
    ///   [0-3]   magic "DPM2"
    ///   [4-7]   entryCount
    ///   [8-11]  stringTableOffset (from header start)
    ///   [12-15] dataSectionOffset (from header start)
    ///   [16-19] stringTableSize
    ///   [20-23] ?
    ///   [24-27] crcSeed
    ///   [28-31] crcValue
    ///
    /// Entry (32 bytes each), starting at header+32:
    ///   [0-3]   type/flags
    ///   [4-7]   nameOffset (in string table)
    ///   [8-11]  fileSize
    ///   [12-15] ?
    ///   [16-19] dataOffset (in data section)
    ///   [20-23] ?
    ///   [24-27] dirOffset (in string table, 0=default)
    ///   [28-31] checksum (0=none)
    ///
    /// String table: at header + stringTableOffset, null-terminated strings
    /// Data section: at header + dataSectionOffset, raw file data
    /// </summary>
    private bool ReadDpm2Header()
    {
        _isDpm2 = true;
        _reader!.ReadInt32(); // DPM2 magic
        int fileCount = _reader.ReadInt32(); // dword[1]: entry count
        int stringTableOffset = _reader.ReadInt32(); // dword[2]: string table offset
        int dataSectionOffset = _reader.ReadInt32(); // dword[3]: data section offset
        _reader.ReadInt32(); // dword[4]: string table size
        _reader.ReadInt32(); // dword[5]
        _crcSeed = _reader.ReadInt32(); // dword[6]: CRC seed
        _salt = _reader.ReadInt32(); // dword[7]: salt

        _files.Capacity = fileCount;

        // Read string table
        long stringTableAbsOffset = _startPosition + stringTableOffset;
        long savedPosition = _reader.BaseStream.Position;
        _reader.BaseStream.Seek(stringTableAbsOffset, SeekOrigin.Begin);

        // Calculate string table size: from stringTableOffset to dataSectionOffset
        int stringTableSize = dataSectionOffset - stringTableOffset;
        byte[] stringTableBytes = new byte[stringTableSize];
        _reader.BaseStream.ReadExactly(stringTableBytes, 0, stringTableSize);

        // Return to entry reading position
        _reader.BaseStream.Seek(savedPosition, SeekOrigin.Begin);

        // Read entries
        _fileOffsetStart = _startPosition + dataSectionOffset;
        for (int i = 0; i < fileCount; i++)
        {
            var file = new DpmFileEntry();
            _reader.ReadInt32(); // [0-3]: type/flags
            int nameOffset = _reader.ReadInt32(); // [4-7]: filename string offset
            file.FileSize = _reader.ReadInt32(); // [8-11]: file size
            _reader.ReadInt32(); // [12-15]
            file.FileOffset = _reader.ReadInt32(); // [16-19]: data offset (relative to data section)
            _reader.ReadInt32(); // [20-23]
            int dirOffset = _reader.ReadInt32(); // [24-27]: directory string offset
            file.Checksum = _reader.ReadInt32(); // [28-31]: checksum (used as key table seed)

            // Resolve filename from string table
            file.FileName = ReadStringFromTable(stringTableBytes, nameOffset);

            // Resolve directory from string table
            if (dirOffset != 0)
            {
                string dir = ReadStringFromTable(stringTableBytes, dirOffset);
                if (!string.IsNullOrEmpty(dir))
                {
                    file.FileName = dir + "\\" + file.FileName;
                }
            }

            long absoluteOffset = dataSectionOffset + (long)file.FileOffset;
            long absoluteEnd = absoluteOffset + file.FileSize;
            if (
                file.FileOffset < 0
                || file.FileSize < 0
                || absoluteOffset < dataSectionOffset
                || absoluteEnd > _streamLength
            )
            {
                return false;
            }

            _files.Add(file);
        }

        return true;
    }

    private static string ReadStringFromTable(byte[] table, int offset)
    {
        if (offset < 0 || offset >= table.Length)
        {
            return string.Empty;
        }
        int end = offset;
        while (end < table.Length && table[end] != 0)
        {
            end++;
        }
        return System.Text.Encoding.ASCII.GetString(table, offset, end - offset);
    }

    internal List<DpmFileEntry> FileList => _files;

    internal byte[] GetFile(int fileOffset, int fileSize)
    {
        _reader!.BaseStream.Seek(fileOffset, SeekOrigin.Begin);
        byte[] buffer = new byte[fileSize];
        _reader.BaseStream.ReadExactly(buffer, 0, fileSize);
        return buffer;
    }

    /// <summary>
    /// Returns the "start.ax" entry, or null if not present.
    /// Consolidated from the former GetStartAx/SeekStartAx pair.
    /// </summary>
    internal DpmFileEntry? GetStartAx()
    {
        foreach (DpmFileEntry file in _files)
        {
            if (file.FileName != null && file.FileName.Equals("start.ax", StringComparison.Ordinal))
            {
                return file;
            }
        }
        return null;
    }

    internal bool Seek(DpmFileEntry file)
    {
        try
        {
            _reader!.BaseStream.Seek(file.FileOffset + _fileOffsetStart, SeekOrigin.Begin);
        }
        catch (IOException)
        {
            return false;
        }
        return true;
    }
}

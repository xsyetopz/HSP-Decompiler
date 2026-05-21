using System.Collections.Generic;
using System.IO;

namespace HspDecompiler.Core.ExeToDpm;

/// <summary>
/// "PE\0\0" で始まる
/// </summary>
internal sealed class IMAGE_NT_HEADERS
{
    /// <summary>
    /// リトルエンディアンで PE(50 45)
    /// </summary>
    private const int ImageNtSignature = 0x4550;
    internal uint _signature;
    internal IMAGE_FILE_HEADER? _fileHeader;
    internal IMAGE_OPTIONAL_HEADER? _optionalHeader;

    internal static IMAGE_NT_HEADERS? FromBinaryReader(BinaryReader reader)
    {
        var ret = new IMAGE_NT_HEADERS { _signature = reader.ReadUInt32() };
        if (ret._signature != ImageNtSignature)
        {
            return null;
        }

        var fileHeader = IMAGE_FILE_HEADER.FromBinaryReader(reader);
        if (fileHeader == null)
        {
            return null;
        }

        ret._fileHeader = fileHeader;
        var optHeader = IMAGE_OPTIONAL_HEADER.FromBinaryReader(reader);
        if (optHeader == null)
        {
            return null;
        }

        ret._optionalHeader = optHeader;
        return ret;
    }
}

internal sealed class IMAGE_FILE_HEADER
{
    internal ushort _machine;
    internal ushort _numberOfSections;
    internal uint _timeDateStamp;
    internal uint _pointerToSymbolTable;
    internal uint _numberOfSymbols;
    internal ushort _sizeOfOptionalHeader;
    internal ushort _characteristics;

    internal static IMAGE_FILE_HEADER? FromBinaryReader(BinaryReader reader)
    {
        var ret = new IMAGE_FILE_HEADER();
        try
        {
            ret._machine = reader.ReadUInt16();
            ret._numberOfSections = reader.ReadUInt16();
            ret._timeDateStamp = reader.ReadUInt32();
            ret._pointerToSymbolTable = reader.ReadUInt32();
            ret._numberOfSymbols = reader.ReadUInt32();
            ret._sizeOfOptionalHeader = reader.ReadUInt16();
            ret._characteristics = reader.ReadUInt16();
        }
        catch (IOException)
        {
            return null;
        }
        return ret;
    }
}

internal sealed class IMAGE_OPTIONAL_HEADER
{
    private const int ImageNumberofDirectoryEntries = 16;
    internal ushort _magic;
    internal byte _majorLinkerVersion;
    internal byte _minorLinkerVersion;
    internal uint _sizeOfCode;
    internal uint _sizeOfInitializedData;
    internal uint _sizeOfUninitializedData;
    internal uint _addressOfEntryPoint;
    internal uint _baseOfCode;
    internal uint _baseOfData;
    internal uint _imageBase;
    internal uint _sectionAlignment;
    internal uint _fileAlignment;
    internal ushort _majorOperatingSystemVersion;
    internal ushort _minorOperatingSystemVersion;
    internal ushort _majorImageVersion;
    internal ushort _minorImageVersion;
    internal ushort _majorSubsystemVersion;
    internal ushort _minorSubsystemVersion;
    internal uint _win32VersionValue;
    internal uint _sizeOfImage;
    internal uint _sizeOfHeaders;
    internal uint _checkSum;
    internal ushort _subsystem;
    internal ushort _dllCharacteristics;
    internal uint _sizeOfStackReserve;
    internal uint _sizeOfStackCommit;
    internal uint _sizeOfHeapReserve;
    internal uint _sizeOfHeapCommit;
    internal uint _loaderFlags;
    internal uint _numberOfRvaAndSizes;
    internal IMAGE_DATA_DIRECTORY[] _dataDirectory = new IMAGE_DATA_DIRECTORY[
        ImageNumberofDirectoryEntries
    ];

    internal static IMAGE_OPTIONAL_HEADER? FromBinaryReader(BinaryReader reader)
    {
        var ret = new IMAGE_OPTIONAL_HEADER();
        try
        {
            ret._magic = reader.ReadUInt16();
            ret._majorLinkerVersion = reader.ReadByte();
            ret._minorLinkerVersion = reader.ReadByte();
            ret._sizeOfCode = reader.ReadUInt32();
            ret._sizeOfInitializedData = reader.ReadUInt32();
            ret._sizeOfUninitializedData = reader.ReadUInt32();
            ret._addressOfEntryPoint = reader.ReadUInt32();
            ret._baseOfCode = reader.ReadUInt32();
            ret._baseOfData = reader.ReadUInt32();
            ret._imageBase = reader.ReadUInt32();
            ret._sectionAlignment = reader.ReadUInt32();
            ret._fileAlignment = reader.ReadUInt32();
            ret._majorOperatingSystemVersion = reader.ReadUInt16();
            ret._minorOperatingSystemVersion = reader.ReadUInt16();
            ret._majorImageVersion = reader.ReadUInt16();
            ret._minorImageVersion = reader.ReadUInt16();
            ret._majorSubsystemVersion = reader.ReadUInt16();
            ret._minorSubsystemVersion = reader.ReadUInt16();
            ret._win32VersionValue = reader.ReadUInt32();
            ret._sizeOfImage = reader.ReadUInt32();
            ret._sizeOfHeaders = reader.ReadUInt32();
            ret._checkSum = reader.ReadUInt32();
            ret._subsystem = reader.ReadUInt16();
            ret._dllCharacteristics = reader.ReadUInt16();
            ret._sizeOfStackReserve = reader.ReadUInt32();
            ret._sizeOfStackCommit = reader.ReadUInt32();
            ret._sizeOfHeapReserve = reader.ReadUInt32();
            ret._sizeOfHeapCommit = reader.ReadUInt32();
            ret._loaderFlags = reader.ReadUInt32();
            ret._numberOfRvaAndSizes = reader.ReadUInt32();

            for (int i = 0; i < ImageNumberofDirectoryEntries; i++)
            {
                ret._dataDirectory[i] = new IMAGE_DATA_DIRECTORY
                {
                    _virtualAddress = reader.ReadUInt32(),
                    _size = reader.ReadUInt32(),
                };
            }
        }
        catch (IOException)
        {
            return null;
        }
        return ret;
    }
}

internal sealed class IMAGE_DATA_DIRECTORY
{
    internal uint _virtualAddress;
    internal uint _size;
}

internal sealed class IMAGE_SECTION_HEADER
{
    internal const int ImageSizeofShortName = 8;
    internal byte[] _name = new byte[ImageSizeofShortName];
    internal uint _physicalAddress;
    internal uint _virtualSize
    {
        get => _physicalAddress;
        set => _physicalAddress = value;
    }
    internal uint _virtualAddress;
    internal uint _sizeOfRawData;
    internal uint _pointerToRawData;
    internal uint _pointerToRelocations;
    internal uint _pointerToLinenumbers;
    internal ushort _numberOfRelocations;
    internal ushort _numberOfLinenumbers;
    internal uint _characteristics;

    internal static IMAGE_SECTION_HEADER? FromBinaryReader(BinaryReader reader)
    {
        var ret = new IMAGE_SECTION_HEADER();
        try
        {
            bool allZero = true;
            for (int i = 0; i < ImageSizeofShortName; i++)
            {
                ret._name[i] = reader.ReadByte();
                allZero &= (ret._name[i] == 0);
            }
            if (allZero)
            {
                return null;
            }

            ret._physicalAddress = reader.ReadUInt32();
            ret._virtualAddress = reader.ReadUInt32();
            ret._sizeOfRawData = reader.ReadUInt32();
            ret._pointerToRawData = reader.ReadUInt32();
            ret._pointerToRelocations = reader.ReadUInt32();
            ret._pointerToLinenumbers = reader.ReadUInt32();
            ret._numberOfRelocations = reader.ReadUInt16();
            ret._numberOfLinenumbers = reader.ReadUInt16();
            ret._characteristics = reader.ReadUInt32();
        }
        catch (IOException)
        {
            return null;
        }
        return ret;
    }
}

internal sealed class IMAGE_DOS_HEADER
{
    /// <summary>
    /// リトルエンディアンで MZ(4D 5A)
    /// </summary>
    private const int ImageDosSignature = 0x5A4D;
    internal ushort _e_magic; // Magic number
    internal ushort _e_cblp; // Bytes on last page of file
    internal ushort _e_cp; // Pages in file
    internal ushort _e_crlc; // Relocations
    internal ushort _e_cparhdr; // Size of header in paragraphs
    internal ushort _e_minalloc; // Minimum extra paragraphs needed
    internal ushort _e_maxalloc; // Maximum extra paragraphs needed
    internal ushort _e_ss; // Initial (relative) SS value
    internal ushort _e_sp; // Initial SP value
    internal ushort _e_csum; // Checksum
    internal ushort _e_ip; // Initial IP value
    internal ushort _e_cs; // Initial (relative) CS value
    internal ushort _e_lfarlc; // File address of relocation table
    internal ushort _e_ovno; // Overlay number
    internal ushort[] _e_res = new ushort[4]; // Reserved words
    internal ushort _e_oemid; // OEM identifier (for e_oeminfo)
    internal ushort _e_oeminfo; // OEM information; e_oemid specific
    internal ushort[] _e_res2 = new ushort[10]; // Reserved words
    internal uint _e_lfanew; // File address of new exe header

    internal static IMAGE_DOS_HEADER? FromBinaryReader(BinaryReader reader)
    {
        var ret = new IMAGE_DOS_HEADER();
        try
        {
            ret._e_magic = reader.ReadUInt16(); // Magic number
            if (ret._e_magic != ImageDosSignature)
            {
                return null;
            }

            ret._e_cblp = reader.ReadUInt16(); // Bytes on last page of file
            ret._e_cp = reader.ReadUInt16(); // Pages in file
            ret._e_crlc = reader.ReadUInt16(); // Relocations
            ret._e_cparhdr = reader.ReadUInt16(); // Size of header in paragraphs
            ret._e_minalloc = reader.ReadUInt16(); // Minimum extra paragraphs needed
            ret._e_maxalloc = reader.ReadUInt16(); // Maximum extra paragraphs needed
            ret._e_ss = reader.ReadUInt16(); // Initial (relative) SS value
            ret._e_sp = reader.ReadUInt16(); // Initial SP value
            ret._e_csum = reader.ReadUInt16(); // Checksum
            ret._e_ip = reader.ReadUInt16(); // Initial IP value
            ret._e_cs = reader.ReadUInt16(); // Initial (relative) CS value
            ret._e_lfarlc = reader.ReadUInt16(); // File address of relocation table
            ret._e_ovno = reader.ReadUInt16(); // Overlay number
            for (int i = 0; i < 4; i++)
            {
                ret._e_res[i] = reader.ReadUInt16();
            }

            ret._e_oemid = reader.ReadUInt16();
            ret._e_oeminfo = reader.ReadUInt16();
            for (int i = 0; i < 10; i++)
            {
                ret._e_res2[i] = reader.ReadUInt16();
            }

            ret._e_lfanew = reader.ReadUInt32();
        }
        catch (IOException)
        {
            return null;
        }
        return ret;
    }
}

internal sealed class IMAGE_IMPORT_DESCRIPTOR
{
    internal uint _originalFirstThunk;
    internal uint _timeDataStamp;
    internal uint _forwarderChain;
    internal uint _name;
    internal uint _firstThunk;

    internal static IMAGE_IMPORT_DESCRIPTOR? FromBinaryReader(BinaryReader reader)
    {
        var ret = new IMAGE_IMPORT_DESCRIPTOR();
        try
        {
            ret._originalFirstThunk = reader.ReadUInt32();
            ret._timeDataStamp = reader.ReadUInt32();
            ret._forwarderChain = reader.ReadUInt32();
            ret._name = reader.ReadUInt32();
            ret._firstThunk = reader.ReadUInt32();
        }
        catch (IOException)
        {
            return null;
        }
        return ret;
    }
}

internal sealed class Win32PeHeader
{
    private IMAGE_DOS_HEADER? _dosHeader;
    private IMAGE_NT_HEADERS? _ntHeader;
    private readonly List<IMAGE_SECTION_HEADER> _sectionHeaders = new();

    internal long EndOfExecutableRegion
    {
        get
        {
            if (_sectionHeaders.Count == 0)
            {
                return -1;
            }

            IMAGE_SECTION_HEADER section = _sectionHeaders[^1];
            return (section._pointerToRawData + section._sizeOfRawData);
        }
    }

    internal static Win32PeHeader? FromBinaryReader(BinaryReader reader)
    {
        try
        {
            long startPosition = reader.BaseStream.Position;
            long length = reader.BaseStream.Length - startPosition;
            if (length < 0x1000)
            {
                return null;
            }

            var ret = new Win32PeHeader { _dosHeader = IMAGE_DOS_HEADER.FromBinaryReader(reader) };
            if (ret._dosHeader == null)
            {
                return null;
            }

            if (ret._dosHeader._e_lfanew <= 0)
            {
                return ret;
            }

            reader.BaseStream.Seek(startPosition + ret._dosHeader._e_lfanew, SeekOrigin.Begin);
            ret._ntHeader = IMAGE_NT_HEADERS.FromBinaryReader(reader);
            if (ret._ntHeader == null)
            {
                return null;
            }

            var section = IMAGE_SECTION_HEADER.FromBinaryReader(reader);
            while (section != null)
            {
                ret._sectionHeaders.Add(section);
                section = IMAGE_SECTION_HEADER.FromBinaryReader(reader);
            }
            return ret;
        }
        catch (IOException)
        {
            return null;
        }
    }
}

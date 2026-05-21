using System;
using System.Globalization;
using System.IO;
using System.Text;
using HspDecompiler.Core.Exceptions;
using HspDecompiler.Core.Resources;

namespace HspDecompiler.Core.Ax3.Data;

internal enum HeaderDataSize
{
    Code = 2,
    Label = 4,
    Dll = 16,
    Function = 28,
    Parameter = 8,
    Plugin = Dll,
}

internal sealed class AxHeader
{
    private static readonly CompositeFormat s_regionStartFormat = CompositeFormat.Parse(
        Strings.HeaderRegionStartExceedsFileSize
    );
    private static readonly CompositeFormat s_regionEndFormat = CompositeFormat.Parse(
        Strings.HeaderRegionEndExceedsFileSize
    );

    internal static AxHeader FromBinaryReader(BinaryReader reader)
    {
        long seekOrigin = reader.BaseStream.Position;
        var header = new AxHeader();
        try
        {
            header._fileType = new string(reader.ReadChars(4));
            for (int i = 1; i < 21; i++)
            {
                header._data[i] = reader.ReadUInt32();
            }
            header._data[21] = reader.ReadUInt16();
            header._data[22] = reader.ReadUInt16();
            header._data[23] = reader.ReadUInt32();
            header._data[24] = reader.ReadUInt32();
        }
        catch (Exception e)
        {
            throw new HspDecoderException("AxHeader", Strings.InvalidFileFormat, e);
        }
        header.checkHeader(reader.BaseStream.Length - seekOrigin);
        return header;
    }

    private void checkHeader(long fileSize)
    {
        if (_fileType.Equals("HSP2", StringComparison.Ordinal))
        {
            throw HSPDecoderException(Strings.CompiledWithHsp2OrEarlier);
        }

        if (!_fileType.Equals("HSP3", StringComparison.Ordinal))
        {
            throw HSPDecoderException(Strings.InvalidFileFormat);
        }

        if (AllDataSize > fileSize)
        {
            throw HSPDecoderException(Strings.HeaderFileSizeExceedsActual);
        }

        if (CodeStart > AllDataSize)
        {
            throw HSPDecoderException(
                string.Format(CultureInfo.InvariantCulture, s_regionStartFormat, Strings.RegionCode)
            );
        }

        if (LiteralStart > AllDataSize)
        {
            throw HSPDecoderException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    s_regionStartFormat,
                    Strings.RegionLiteral
                )
            );
        }

        if (LabelStart > AllDataSize)
        {
            throw HSPDecoderException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    s_regionStartFormat,
                    Strings.RegionLabel
                )
            );
        }

        if (DllStart > AllDataSize)
        {
            throw HSPDecoderException(
                string.Format(CultureInfo.InvariantCulture, s_regionStartFormat, Strings.RegionDll)
            );
        }

        if (FunctionStart > AllDataSize)
        {
            throw HSPDecoderException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    s_regionStartFormat,
                    Strings.RegionFunction
                )
            );
        }

        if (PluginStart > AllDataSize)
        {
            throw HSPDecoderException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    s_regionStartFormat,
                    Strings.RegionPlugin
                )
            );
        }

        if (ParameterStart > AllDataSize)
        {
            throw HSPDecoderException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    s_regionStartFormat,
                    Strings.RegionParameter
                )
            );
        }

        if (CodeEnd > AllDataSize)
        {
            throw HSPDecoderException(
                string.Format(CultureInfo.InvariantCulture, s_regionEndFormat, Strings.RegionCode)
            );
        }

        if (LiteralEnd > AllDataSize)
        {
            throw HSPDecoderException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    s_regionEndFormat,
                    Strings.RegionLiteral
                )
            );
        }

        if (LabelEnd > AllDataSize)
        {
            throw HSPDecoderException(
                string.Format(CultureInfo.InvariantCulture, s_regionEndFormat, Strings.RegionLabel)
            );
        }

        if (DllEnd > AllDataSize)
        {
            throw HSPDecoderException(
                string.Format(CultureInfo.InvariantCulture, s_regionEndFormat, Strings.RegionDll)
            );
        }

        if (FunctionEnd > AllDataSize)
        {
            throw HSPDecoderException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    s_regionEndFormat,
                    Strings.RegionFunction
                )
            );
        }

        if (PluginEnd > AllDataSize)
        {
            throw HSPDecoderException(
                string.Format(CultureInfo.InvariantCulture, s_regionEndFormat, Strings.RegionPlugin)
            );
        }

        if (ParameterEnd > AllDataSize)
        {
            throw HSPDecoderException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    s_regionEndFormat,
                    Strings.RegionParameter
                )
            );
        }
    }

    private static HspDecoderException HSPDecoderException(string str) => new("AxHeader", str);

    private string _fileType = "";
    private readonly uint[] _data = new uint[25];
    internal const uint HeaderSize = 0x60;
    internal string FileType => _fileType;

    internal uint AllDataSize => _data[3];
    internal uint CodeStart => _data[4];
    internal uint CodeSize => _data[5] / (int)HeaderDataSize.Code;
    internal uint CodeEnd => CodeStart + CodeSize;
    internal uint LiteralStart => _data[6];
    internal uint LiteralSize => _data[7];
    internal uint LiteralEnd => LiteralStart + LiteralSize;
    internal uint LabelStart => _data[8];
    internal uint LabelSize => _data[9];
    internal uint LabelCount => _data[9] / (int)HeaderDataSize.Label;
    internal uint LabelEnd => LabelStart + LabelSize;
    internal uint DebugStart => _data[10];
    internal uint DebugSize => _data[11];
    internal uint DebugEnd => DebugStart + DebugSize;
    internal uint DllStart => _data[12];
    internal uint DllSize => _data[13];
    internal uint DllCount => _data[13] / (int)HeaderDataSize.Dll;
    internal uint DllEnd => DllStart + DllSize;
    internal uint FunctionStart => _data[14];
    internal uint FunctionSize => _data[15];
    internal uint FunctionCount => _data[15] / (int)HeaderDataSize.Function;
    internal uint FunctionEnd => FunctionStart + FunctionSize;
    internal uint ParameterStart => _data[16];
    internal uint ParameterSize => _data[17];
    internal uint ParameterCount => _data[17] / (int)HeaderDataSize.Parameter;
    internal uint ParameterEnd => ParameterStart + ParameterSize;
    internal uint PluginStart => _data[20];
    internal uint PluginSize => _data[21];
    internal uint PluginCount => _data[21] / (int)HeaderDataSize.Plugin;
    internal uint PluginParameterCount => _data[22];
    internal uint PluginEnd => PluginStart + PluginSize;
    internal uint RuntimeStart => _data[24];
}

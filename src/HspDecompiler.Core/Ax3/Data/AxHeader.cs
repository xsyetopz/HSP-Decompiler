using System;
using System.IO;
using HspDecompiler.Core.Exceptions;
using HspDecompiler.Core.Resources;

namespace HspDecompiler.Core.Ax3.Data
{
    internal enum HeaderDataSize
    {
        Code = 2,
        Label = 4,
        Dll = 16,
        Function = 28,
        Parameter = 8,
        Plugin = 16,
    }

    internal sealed class AxHeader
    {
        static internal AxHeader FromBinaryReader(BinaryReader reader)
        {
            long seekOrigin = reader.BaseStream.Position;
            AxHeader header = new AxHeader();
            try
            {
                header.fileType = new string(reader.ReadChars(4));
                for (int i = 1; i < 21; i++)
                {
                    header.data[i] = reader.ReadUInt32();
                }
                header.data[21] = reader.ReadUInt16();
                header.data[22] = reader.ReadUInt16();
                header.data[23] = reader.ReadUInt32();
                header.data[24] = reader.ReadUInt32();
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
            if (fileType.Equals("HSP2", StringComparison.Ordinal))
                throw HSPDecoderException(Strings.CompiledWithHsp2OrEarlier);
            if (!fileType.Equals("HSP3", StringComparison.Ordinal))
                throw HSPDecoderException(Strings.InvalidFileFormat);
            if (AllDataSize > fileSize)
                throw HSPDecoderException(Strings.HeaderFileSizeExceedsActual);

            if (CodeStart > AllDataSize)
                throw HSPDecoderException(string.Format(Strings.HeaderRegionStartExceedsFileSize, Strings.RegionCode));
            if (LiteralStart > AllDataSize)
                throw HSPDecoderException(string.Format(Strings.HeaderRegionStartExceedsFileSize, Strings.RegionLiteral));
            if (LabelStart > AllDataSize)
                throw HSPDecoderException(string.Format(Strings.HeaderRegionStartExceedsFileSize, Strings.RegionLabel));
            if (DllStart > AllDataSize)
                throw HSPDecoderException(string.Format(Strings.HeaderRegionStartExceedsFileSize, Strings.RegionDll));
            if (FunctionStart > AllDataSize)
                throw HSPDecoderException(string.Format(Strings.HeaderRegionStartExceedsFileSize, Strings.RegionFunction));
            if (PluginStart > AllDataSize)
                throw HSPDecoderException(string.Format(Strings.HeaderRegionStartExceedsFileSize, Strings.RegionPlugin));
            if (ParameterStart > AllDataSize)
                throw HSPDecoderException(string.Format(Strings.HeaderRegionStartExceedsFileSize, Strings.RegionParameter));

            if (CodeEnd > AllDataSize)
                throw HSPDecoderException(string.Format(Strings.HeaderRegionEndExceedsFileSize, Strings.RegionCode));
            if (LiteralEnd > AllDataSize)
                throw HSPDecoderException(string.Format(Strings.HeaderRegionEndExceedsFileSize, Strings.RegionLiteral));
            if (LabelEnd > AllDataSize)
                throw HSPDecoderException(string.Format(Strings.HeaderRegionEndExceedsFileSize, Strings.RegionLabel));
            if (DllEnd > AllDataSize)
                throw HSPDecoderException(string.Format(Strings.HeaderRegionEndExceedsFileSize, Strings.RegionDll));
            if (FunctionEnd > AllDataSize)
                throw HSPDecoderException(string.Format(Strings.HeaderRegionEndExceedsFileSize, Strings.RegionFunction));
            if (PluginEnd > AllDataSize)
                throw HSPDecoderException(string.Format(Strings.HeaderRegionEndExceedsFileSize, Strings.RegionPlugin));
            if (ParameterEnd > AllDataSize)
                throw HSPDecoderException(string.Format(Strings.HeaderRegionEndExceedsFileSize, Strings.RegionParameter));
        }

        private static HspDecoderException HSPDecoderException(string str)
        {
            return new HspDecoderException("AxHeader", str);
        }

        string fileType;
        uint[] data = new uint[25];
        internal const uint HeaderSize = 0x60;
        internal string FileType
        {
            get { return fileType; }
        }

        internal uint AllDataSize    { get { return data[3]; } }
        internal uint CodeStart      { get { return data[4]; } }
        internal uint CodeSize       { get { return data[5] / (int)HeaderDataSize.Code; } }
        internal uint CodeEnd        { get { return CodeStart + CodeSize; } }
        internal uint LiteralStart   { get { return data[6]; } }
        internal uint LiteralSize    { get { return data[7]; } }
        internal uint LiteralEnd     { get { return LiteralStart + LiteralSize; } }
        internal uint LabelStart     { get { return data[8]; } }
        internal uint LabelSize      { get { return data[9]; } }
        internal uint LabelCount     { get { return data[9] / (int)HeaderDataSize.Label; } }
        internal uint LabelEnd       { get { return LabelStart + LabelSize; } }
        internal uint DebugStart     { get { return data[10]; } }
        internal uint DebugSize      { get { return data[11]; } }
        internal uint DebugEnd       { get { return DebugStart + DebugSize; } }
        internal uint DllStart       { get { return data[12]; } }
        internal uint DllSize        { get { return data[13]; } }
        internal uint DllCount       { get { return data[13] / (int)HeaderDataSize.Dll; } }
        internal uint DllEnd         { get { return DllStart + DllSize; } }
        internal uint FunctionStart  { get { return data[14]; } }
        internal uint FunctionSize   { get { return data[15]; } }
        internal uint FunctionCount  { get { return data[15] / (int)HeaderDataSize.Function; } }
        internal uint FunctionEnd    { get { return FunctionStart + FunctionSize; } }
        internal uint ParameterStart { get { return data[16]; } }
        internal uint ParameterSize  { get { return data[17]; } }
        internal uint ParameterCount { get { return data[17] / (int)HeaderDataSize.Parameter; } }
        internal uint ParameterEnd   { get { return ParameterStart + ParameterSize; } }
        internal uint PluginStart    { get { return data[20]; } }
        internal uint PluginSize     { get { return data[21]; } }
        internal uint PluginCount    { get { return data[21] / (int)HeaderDataSize.Plugin; } }
        internal uint PluginParameterCount { get { return data[22]; } }
        internal uint PluginEnd      { get { return PluginStart + PluginSize; } }
        internal uint RuntimeStart   { get { return data[24]; } }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HspDecompiler.Core.Abstractions;
using HspDecompiler.Core.Ax2;
using HspDecompiler.Core.Ax3;
using HspDecompiler.Core.DpmToAx;
using HspDecompiler.Core.DpmToAx.Crypto;
using HspDecompiler.Core.Encoding;
using HspDecompiler.Core.Exceptions;
using HspDecompiler.Core.Resources;

namespace HspDecompiler.Core.Pipeline
{
    public sealed class DecompilerPipeline
    {
        private readonly IDecompilerLogger _logger;
        private readonly IProgressReporter _progress;
        private Hsp3Dictionary _dictionary;

        public DecompilerPipeline(IDecompilerLogger logger, IProgressReporter progress)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _progress = progress ?? throw new ArgumentNullException(nameof(progress));
        }

        public bool Initialize(string dictionaryPath)
        {
            try
            {
                _dictionary = Hsp3Dictionary.FromFile(dictionaryPath);
            }
            catch
            {
                _dictionary = null;
            }

            if (_dictionary != null)
            {
                _logger.Write(string.Format(Strings.DictionaryLoadSuccess, Path.GetFileName(dictionaryPath)));
                return true;
            }

            _logger.Write(string.Format(Strings.DictionaryLoadFailed, Path.GetFileName(dictionaryPath)));
            return false;
        }

        public async Task<DecompilerResult> RunAsync(DecompilerOptions options, CancellationToken ct = default)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var result = new DecompilerResult();
            string errorLogPath = options.InputPath + ".log";

            try
            {
                _logger.Write(string.Format(Strings.ReadingFile, Path.GetFileName(options.InputPath)));
                using var stream = new FileStream(options.InputPath, FileMode.Open, FileAccess.Read);
                using var reader = new BinaryReader(stream, ShiftJisHelper.Encoding);

                char[] buffer = reader.ReadChars(4);
                string magic = new string(buffer);
                reader.BaseStream.Seek(0, SeekOrigin.Begin);

                string inputDir = (Path.GetDirectoryName(options.InputPath) ?? ".") + Path.DirectorySeparatorChar;
                string inputBaseName = Path.GetFileNameWithoutExtension(options.InputPath);

                if (magic.StartsWith("MZ", StringComparison.Ordinal) || magic.StartsWith("DPM", StringComparison.Ordinal))
                {
                    string outputDir = BuildAutoIncrementDirName(inputDir, inputBaseName);
                    errorLogPath = outputDir.TrimEnd(Path.DirectorySeparatorChar) + ".log";
                    outputDir = outputDir + Path.DirectorySeparatorChar;

                    var dpmResult = DecompressDpm(reader, outputDir, options);
                    result.OutputPath = outputDir;

                    if (dpmResult.AllEncrypted || dpmResult.Cancelled)
                    {
                        result.Success = !dpmResult.AllEncrypted;
                        if (dpmResult.AllEncrypted)
                            result.ErrorMessage = Strings.AllFilesEncrypted;
                        return result;
                    }

                    foreach (var entry in dpmResult.Files)
                    {
                        if (entry.IsEncrypted)
                            continue;

                        string axPath = Path.Combine(outputDir, entry.FileName);
                        if (!File.Exists(axPath))
                            continue;
                        if (!entry.FileName.EndsWith(".ax", StringComparison.OrdinalIgnoreCase))
                            continue;

                        using var axStream = new FileStream(axPath, FileMode.Open, FileAccess.Read);
                        using var axReader = new BinaryReader(axStream, ShiftJisHelper.Encoding);
                        char[] axMagicChars = axReader.ReadChars(4);
                        string axMagic = new string(axMagicChars);
                        axReader.BaseStream.Seek(0, SeekOrigin.Begin);

                        string ext = axMagic.StartsWith("HSP2", StringComparison.Ordinal) ? ".as" : ".hsp";
                        string axBaseName = Path.GetFileNameWithoutExtension(entry.FileName);
                        string outputPath = BuildAutoIncrementFileName(outputDir, axBaseName, ext);

                        await DecodeAsync(axReader, outputPath, ct);
                    }
                }
                else if (magic.StartsWith("HSP2", StringComparison.Ordinal) || magic.StartsWith("HSP3", StringComparison.Ordinal))
                {
                    string ext = magic.StartsWith("HSP2", StringComparison.Ordinal) ? ".as" : ".hsp";
                    string outputPath = BuildAutoIncrementFileName(inputDir, inputBaseName, ext);
                    errorLogPath = outputPath + ".log";
                    result.OutputPath = outputPath;

                    await DecodeAsync(reader, outputPath, ct);
                }
                else
                {
                    throw new HspDecoderException(Strings.UnrecognizedFileFormat);
                }

                if (_logger.Warnings.Count > 0)
                {
                    using (var errorLog = new StreamWriter(errorLogPath, false, ShiftJisHelper.Encoding))
                    {
                        foreach (string warning in _logger.Warnings)
                            errorLog.WriteLine(warning);
                    }
                    result.Warnings.AddRange(_logger.Warnings);
                }

                result.Success = true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        public DpmExtractionResult DecompressDpm(BinaryReader reader, string outputDir, DecompilerOptions options)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            var result = new DpmExtractionResult();

            _logger.Write(Strings.SearchingDpmHeader);
            DpmExtractor extractor = DpmExtractor.FromBinaryReader(reader);
            if (extractor == null)
                throw new HspDecoderException(Strings.DpmHeaderNotFound);
            if (extractor.FileList == null || extractor.FileList.Count == 0)
                throw new HspDecoderException(Strings.DpmNoFiles);

            int encryptedCount = 0;
            foreach (DpmFileEntry file in extractor.FileList)
            {
                result.Files.Add(file);
                if (file.IsEncrypted)
                    encryptedCount++;
            }

            result.EncryptedCount = encryptedCount;

            int totalCount = extractor.FileList.Count;
            if (totalCount - encryptedCount <= 0)
            {
                result.AllEncrypted = true;
                _logger.Write(Strings.ExtractionCancelled);
                return result;
            }

            if (encryptedCount > 0 && !options.SkipEncrypted && !options.AllowDecryption)
            {
                result.Cancelled = true;
                _logger.Write(Strings.ExtractionCancelled);
                return result;
            }

            if (!Directory.Exists(outputDir))
            {
                try
                {
                    Directory.CreateDirectory(outputDir);
                }
                catch
                {
                    throw new HspDecoderException(string.Format(Strings.DirectoryCreateFailed, outputDir));
                }
            }

            foreach (DpmFileEntry file in extractor.FileList)
            {
                if (file.IsEncrypted && !options.AllowDecryption)
                {
                    _logger.Write(string.Format(Strings.FileEncrypted, file.FileName));
                    continue;
                }

                string outputPath = Path.Combine(outputDir, file.FileName);
                if (File.Exists(outputPath))
                {
                    _logger.Write(string.Format(Strings.FileAlreadyExists, file.FileName));
                    continue;
                }

                if (!extractor.Seek(file))
                {
                    _logger.Write(string.Format(Strings.FileSeekFailed, file.FileName));
                    continue;
                }

                byte[] fileData = reader.ReadBytes(file.FileSize);

                if (file.IsEncrypted)
                {
                    _logger.Write(string.Format(Strings.DecryptingFile, file.FileName));

                    string decryptedExt = ".hsp";
                    string decryptedBaseName = Path.GetFileNameWithoutExtension(file.FileName);
                    string decryptedOutputPath = BuildAutoIncrementFileName(outputDir, decryptedBaseName, decryptedExt);

                    bool decryptionSucceeded = false;
                    Func<byte[], bool> validator = (decryptedData) =>
                    {
                        try
                        {
                            using var ms = new MemoryStream(decryptedData);
                            using var br = new BinaryReader(ms, ShiftJisHelper.Encoding);
                            char[] magicChars = br.ReadChars(4);
                            string fileMagic = new string(magicChars);
                            br.BaseStream.Seek(0, SeekOrigin.Begin);

                            if (!fileMagic.StartsWith("HSP2", StringComparison.Ordinal) &&
                                !fileMagic.StartsWith("HSP3", StringComparison.Ordinal))
                                return false;

                            string resolvedExt = fileMagic.StartsWith("HSP2", StringComparison.Ordinal) ? ".as" : ".hsp";
                            string resolvedPath = BuildAutoIncrementFileName(outputDir, decryptedBaseName, resolvedExt);

                            IAxDecoder decoder = CreateDecoder(br);
                            List<string> lines = decoder.DecodeAsync(br, _logger, _progress).GetAwaiter().GetResult();
                            WriteOutputLines(lines, resolvedPath);
                            decryptedOutputPath = resolvedPath;
                            decryptionSucceeded = true;
                            return true;
                        }
                        catch
                        {
                            return false;
                        }
                    };

                    HspCryptoTransform decryptor = HspCryptoTransform.CrackEncryption(fileData, validator);
                    if (decryptor == null || !decryptionSucceeded)
                    {
                        _logger.Write(string.Format(Strings.DecryptionFailed, file.FileName));
                        continue;
                    }

                    byte[] decryptedBytes = decryptor.Decryption(fileData);
                    try
                    {
                        using var saveStream = new FileStream(outputPath, FileMode.CreateNew, FileAccess.Write);
                        saveStream.Write(decryptedBytes, 0, decryptedBytes.Length);
                    }
                    catch
                    {
                        _logger.Warning(string.Format(Strings.FileSaveFailed, file.FileName));
                    }
                }
                else
                {
                    try
                    {
                        using var saveStream = new FileStream(outputPath, FileMode.CreateNew, FileAccess.Write);
                        saveStream.Write(fileData, 0, fileData.Length);
                    }
                    catch
                    {
                        _logger.Warning(string.Format(Strings.FileSaveFailed, file.FileName));
                    }
                }
            }

            _logger.Write(Strings.ExtractionComplete);
            return result;
        }

        public async Task DecodeAsync(BinaryReader reader, string outputPath, CancellationToken ct = default)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            if (_dictionary == null)
                throw new InvalidOperationException(Strings.DictionaryNotInitialized);

            _logger.StartSection();
            _logger.Write(Strings.Decompiling);
            _logger.StartSection();

            IAxDecoder decoder = CreateDecoder(reader);
            List<string> lines = await decoder.DecodeAsync(reader, _logger, _progress, ct);

            _logger.EndSection();
            _logger.Write(Strings.DecompileComplete);
            _logger.EndSection();
            _logger.Write(string.Format(Strings.OutputtingTo, Path.GetFileName(outputPath)));

            WriteOutputLines(lines, outputPath);
            _logger.Write(Strings.OutputComplete);
        }

        private IAxDecoder CreateDecoder(BinaryReader reader)
        {
            long startPosition = reader.BaseStream.Position;
            char[] buffer = reader.ReadChars(4);
            string magic = new string(buffer);
            reader.BaseStream.Seek(startPosition, SeekOrigin.Begin);

            if (magic.Equals("HSP2", StringComparison.Ordinal))
                return new Ax2Decoder();

            if (magic.Equals("HSP3", StringComparison.Ordinal))
            {
                var decoder = new Ax3Decoder();
                decoder.Dictionary = _dictionary;
                return decoder;
            }

            throw new HspDecoderException(Strings.NotHsp2OrHsp3);
        }

        private static void WriteOutputLines(List<string> lines, string outputPath)
        {
            using var writer = new StreamWriter(outputPath, false, ShiftJisHelper.Encoding);
            foreach (string line in lines)
                writer.WriteLine(line);
        }

        private static string BuildAutoIncrementDirName(string parentDir, string baseName)
        {
            string candidate = Path.Combine(parentDir, baseName);
            if (!Directory.Exists(candidate))
                return candidate;

            int i = 1;
            string result;
            do
            {
                result = Path.Combine(parentDir, $"{baseName} ({i})");
                i++;
            }
            while (Directory.Exists(result));

            return result;
        }

        private static string BuildAutoIncrementFileName(string dir, string baseName, string extension)
        {
            string candidate = Path.Combine(dir, baseName + extension);
            if (!File.Exists(candidate))
                return candidate;

            int i = 1;
            string result;
            do
            {
                result = Path.Combine(dir, $"{baseName} ({i}){extension}");
                i++;
            }
            while (File.Exists(result));

            return result;
        }
    }
}

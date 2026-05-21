using System;
using System.IO;
using System.Threading.Tasks;
using HspDecompiler.Core.Pipeline;
using Xunit;

namespace HspDecompiler.Core.Tests.IntegrationTests;

public sealed class DecompileVocalShifterTests
{
    private static string FixturesDir => Path.Combine(AppContext.BaseDirectory, "Fixtures");

    private static string VocalShifterExePath => Path.Combine(FixturesDir, "vocalshifter.exe");

    private static string StartOrigHspPath => Path.Combine(FixturesDir, "start_orig.hsp");

    private static string DictionaryPath =>
        Path.Combine(AppContext.BaseDirectory, "Dictionary.csv");

    [Fact]
    public async Task DecompileVocalShifterProducesStartHspMatchingReference()
    {
        if (!File.Exists(VocalShifterExePath) || !File.Exists(StartOrigHspPath))
        {
            return;
        }

        var logger = new NullLogger();
        var progress = new NullProgressReporter();
        var pipeline = new DecompilerPipeline(logger, progress);

        if (File.Exists(DictionaryPath))
        {
            bool initialized = pipeline.Initialize(DictionaryPath);
            if (!initialized)
            {
                return;
            }
        }

        string tempDir = Path.Combine(
            Path.GetTempPath(),
            $"hsp-decompiler-test-{Guid.NewGuid():N}"
        );
        Directory.CreateDirectory(tempDir);

        try
        {
            string inputExePath = Path.Combine(tempDir, "vocalshifter.exe");
            File.Copy(VocalShifterExePath, inputExePath);

            var options = new DecompilerOptions
            {
                InputPath = inputExePath,
                AllowDecryption = true,
                SkipEncrypted = false,
            };

            DecompilerResult decompilerResult = await pipeline
                .RunAsync(options)
                .ConfigureAwait(true);

            Assert.True(
                decompilerResult.Success,
                $"Pipeline failed: {decompilerResult.ErrorMessage}"
            );

            string extractDir = Path.Combine(tempDir, "vocalshifter");
            string[] hspFiles = Directory.GetFiles(
                extractDir,
                "start.hsp",
                SearchOption.TopDirectoryOnly
            );
            Assert.True(hspFiles.Length > 0, $"No start.hsp found under {extractDir}");

            string produced = await File.ReadAllTextAsync(hspFiles[0]).ConfigureAwait(true);
            string reference = await File.ReadAllTextAsync(StartOrigHspPath).ConfigureAwait(true);

            Assert.Equal(reference, produced);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }
}

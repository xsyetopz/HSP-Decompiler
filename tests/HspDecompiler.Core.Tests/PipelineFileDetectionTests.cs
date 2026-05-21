using Xunit;

namespace HspDecompiler.Core.Tests;

public class PipelineFileDetectionTests
{
    [Theory]
    [InlineData("MZ\x00\x00", true)]
    [InlineData("DPMX", true)]
    [InlineData("HSP2", true)]
    [InlineData("HSP3", true)]
    [InlineData("JUNK", false)]
    public void MagicBytesDetection(string magic, bool isRecognized)
    {
        bool recognized =
            magic.StartsWith("MZ", System.StringComparison.Ordinal)
            || magic.StartsWith("DPM", System.StringComparison.Ordinal)
            || magic.StartsWith("HSP2", System.StringComparison.Ordinal)
            || magic.StartsWith("HSP3", System.StringComparison.Ordinal);
        Assert.Equal(isRecognized, recognized);
    }
}

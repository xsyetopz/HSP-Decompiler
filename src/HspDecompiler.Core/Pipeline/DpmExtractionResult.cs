using System.Collections.Generic;
using HspDecompiler.Core.DpmToAx;

namespace HspDecompiler.Core.Pipeline
{
    public sealed class DpmExtractionResult
    {
        public List<DpmFileEntry> Files { get; set; } = new List<DpmFileEntry>();
        public int EncryptedCount { get; set; }
        public bool AllEncrypted { get; set; }
        public bool Cancelled { get; set; }
    }
}

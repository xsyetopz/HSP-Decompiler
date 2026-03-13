using System.Collections.Generic;

namespace HspDecompiler.Core.Pipeline
{
    public sealed class DecompilerResult
    {
        public bool Success { get; set; }
        public string OutputPath { get; set; }
        public List<string> Warnings { get; } = new List<string>();
        public string ErrorMessage { get; set; }
    }
}

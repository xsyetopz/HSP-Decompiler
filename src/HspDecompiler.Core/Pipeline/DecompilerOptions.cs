namespace HspDecompiler.Core.Pipeline
{
    public sealed class DecompilerOptions
    {
        public string InputPath { get; set; }
        public string OutputDirectory { get; set; }
        public string DictionaryPath { get; set; }
        public bool AllowDecryption { get; set; } = true;
        public bool SkipEncrypted { get; set; }
        public bool Verbose { get; set; }
    }
}

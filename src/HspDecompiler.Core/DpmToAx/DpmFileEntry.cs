namespace HspDecompiler.Core.DpmToAx
{
    public sealed class DpmFileEntry
    {
        public string FileName { get; set; }
        public int Unknown { get; set; }
        public int EncryptionKey { get; set; }
        public int FileOffset { get; set; }
        public int FileSize { get; set; }
        public bool IsEncrypted => EncryptionKey != 0;
    }
}

using System.Text;

namespace HspDecompiler.Core.Encoding
{
    public static class ShiftJisHelper
    {
        private static System.Text.Encoding _encoding;
        private static readonly object _lock = new object();

        public static System.Text.Encoding Encoding
        {
            get
            {
                if (_encoding == null)
                {
                    lock (_lock)
                    {
                        if (_encoding == null)
                        {
                            System.Text.Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                            _encoding = System.Text.Encoding.GetEncoding("SHIFT-JIS");
                        }
                    }
                }
                return _encoding;
            }
        }
    }
}

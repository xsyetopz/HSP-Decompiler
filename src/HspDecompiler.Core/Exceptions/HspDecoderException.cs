using System;

namespace HspDecompiler.Core.Exceptions
{
    public sealed class HspDecoderException : Exception
    {
        public HspDecoderException()
            : base()
        {
        }

        public HspDecoderException(string message)
            : base(message)
        {
        }

        public HspDecoderException(string source, string message)
            : base(message)
        {
            Source = source;
        }

        public HspDecoderException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public HspDecoderException(string source, string message, Exception innerException)
            : base(message, innerException)
        {
            Source = source;
        }

        public HspDecoderException(HspDecoderException other)
            : base(other.Message, other.InnerException)
        {
            Source = other.Source;
        }
    }
}

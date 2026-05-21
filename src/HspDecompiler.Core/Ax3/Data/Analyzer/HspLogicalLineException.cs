using System;

namespace HspDecompiler.Core.Ax3.Data.Analyzer;

internal class HspLogicalLineException : Exception
{
    internal HspLogicalLineException()
        : base() { }

    internal HspLogicalLineException(string message)
        : base(message) { }

    internal HspLogicalLineException(string source, string message)
        : base(message)
    {
        Source = source;
    }

    internal HspLogicalLineException(string message, Exception e)
        : base(message, e) { }

    internal HspLogicalLineException(string source, string message, Exception e)
        : base(message, e)
    {
        Source = source;
    }
}

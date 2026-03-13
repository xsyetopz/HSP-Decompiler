using System;
using System.Collections.Generic;

namespace HspDecompiler.Core.Abstractions
{
    public interface IDecompilerLogger
    {
        void Write(string message);
        void Warning(string message, int lineNumber = -1);
        void Error(string message);
        void Error(Exception exception);
        void StartSection();
        void EndSection();
        IReadOnlyList<string> Warnings { get; }
    }
}

using System;
using System.Collections.Generic;
using HspDecompiler.Core.Abstractions;

namespace HspDecompiler.Cli;

internal sealed class CliLogger(bool verbose) : IDecompilerLogger
{
    private readonly bool _verbose = verbose;
    private readonly List<string> _warnings = new();
    private readonly int _startTime = Environment.TickCount;
    private int _indentLevel;

    public IReadOnlyList<string> Warnings => _warnings;

    public void Write(string message)
    {
        int elapsed = Environment.TickCount - _startTime;
        string indent = new(' ', _indentLevel * 2);
        Console.WriteLine($"[{elapsed, 8}] {indent}{message}");
    }

    public void Warning(string message, int lineNumber = -1)
    {
        string warning = lineNumber >= 0 ? $"{lineNumber:D6}: {message}" : message;
        _warnings.Add(warning);
        if (_verbose)
        {
            Console.Error.WriteLine($"WARNING: {warning}");
        }
    }

    public void LogError(string message) => Console.Error.WriteLine($"ERROR: {message}");

    public void LogError(Exception exception) =>
        Console.Error.WriteLine($"ERROR: {exception.Message}");

    public void StartSection() => _indentLevel++;

    public void EndSection() => _indentLevel--;
}

using System;
using System.Collections.Generic;
using Avalonia.Threading;
using HspDecompiler.Core.Abstractions;
using HspDecompiler.Gui.ViewModels;

namespace HspDecompiler.Gui.Services
{
    internal sealed class GuiLogger : IDecompilerLogger
    {
        private readonly MainWindowViewModel _vm;
        private readonly List<string> _warnings = new List<string>();
        private readonly int _startTime = Environment.TickCount;
        private int _indentLevel;

        public GuiLogger(MainWindowViewModel vm) { _vm = vm; }

        public IReadOnlyList<string> Warnings => _warnings;

        public void Write(string message)
        {
            int elapsedMs = Environment.TickCount - _startTime;
            double elapsedSec = elapsedMs / 1000.0;
            string indent = new string(' ', _indentLevel * 2);
            string line = $"[{elapsedSec,7:F3}s]  {indent}{message}";
            Dispatcher.UIThread.Post(() => _vm.AppendLog(line));
        }

        public void Warning(string message, int lineNumber = -1)
        {
            string warning = lineNumber >= 0 ? $"{lineNumber:D6}: {message}" : message;
            _warnings.Add(warning);
        }

        public void Error(string message)
        {
            int elapsedMs = Environment.TickCount - _startTime;
            double elapsedSec = elapsedMs / 1000.0;
            string line = $"[{elapsedSec,7:F3}s]  ERROR: {message}";
            Dispatcher.UIThread.Post(() => _vm.AppendLog(line));
        }

        public void Error(Exception exception)
        {
            int elapsedMs = Environment.TickCount - _startTime;
            double elapsedSec = elapsedMs / 1000.0;
            string line = $"[{elapsedSec,7:F3}s]  ERROR: {exception.Message}";
            Dispatcher.UIThread.Post(() => _vm.AppendLog(line));
        }

        public void StartSection() => _indentLevel++;
        public void EndSection() => _indentLevel--;
    }
}

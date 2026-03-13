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
            int elapsed = Environment.TickCount - _startTime;
            string indent = new string(' ', _indentLevel * 2);
            string line = $"{elapsed:D8}:{indent}{message}";
            Dispatcher.UIThread.Post(() => _vm.AppendLog(line));
        }

        public void Warning(string message, int lineNumber = -1)
        {
            string warning = lineNumber >= 0 ? $"{lineNumber:D6}: {message}" : message;
            _warnings.Add(warning);
        }

        public void Error(string message)
        {
            Dispatcher.UIThread.Post(() => _vm.AppendLog("ERROR: " + message));
        }

        public void Error(Exception exception)
        {
            Dispatcher.UIThread.Post(() => _vm.AppendLog("ERROR: " + exception.Message));
        }

        public void StartSection() => _indentLevel++;
        public void EndSection() => _indentLevel--;
    }
}

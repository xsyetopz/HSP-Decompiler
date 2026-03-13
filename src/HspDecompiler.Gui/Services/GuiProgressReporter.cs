using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using HspDecompiler.Core.Abstractions;

namespace HspDecompiler.Gui.Services
{
    internal sealed class GuiProgressReporter : IProgressReporter
    {
        public void Report(string status) { }

        public async Task YieldAsync(CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            if (Dispatcher.UIThread.CheckAccess())
                await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
            else
                await Task.Yield();
        }
    }
}

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
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);
        }
    }
}

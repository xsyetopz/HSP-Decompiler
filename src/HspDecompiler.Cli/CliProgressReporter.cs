using System.Threading;
using System.Threading.Tasks;
using HspDecompiler.Core.Abstractions;

namespace HspDecompiler.Cli;

internal sealed class CliProgressReporter : IProgressReporter
{
    public void Report(string status) { }

    public Task YieldAsync(CancellationToken ct) => Task.CompletedTask;
}

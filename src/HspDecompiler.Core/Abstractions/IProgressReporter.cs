using System.Threading;
using System.Threading.Tasks;

namespace HspDecompiler.Core.Abstractions
{
    public interface IProgressReporter
    {
        void Report(string status);
        Task YieldAsync(CancellationToken ct = default);
    }
}

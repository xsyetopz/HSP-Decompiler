using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HspDecompiler.Core.Abstractions
{
    public interface IAxDecoder
    {
        Task<List<string>> DecodeAsync(BinaryReader reader, IDecompilerLogger logger, IProgressReporter progress, CancellationToken ct = default);
    }
}

#nullable enable
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Download.Core
{
    public interface IDownloadResponse : IDisposable
    {
        Uri ResponseUri { get; }
        bool IsSuccessful { get; }
        string? StatusMessage { get; }
        long? Length { get; }
        Task<Stream> GetStreamAsync(CancellationToken cancellationToken);
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Download.Core
{
    public interface IDownloadOutput : IDisposable
    {
        /// <summary>
        /// Get the total amount of the written bytes
        /// </summary>
        long Length { get; }

        /// <summary>
        /// Write a sequence of bytes to the current <see cref="IDownloadOutput"/>
        /// </summary>
        /// <param name="download">The download object passing the received bytes</param>
        /// <param name="ctx">The current download context</param>
        /// <param name="bytes">The received bytes</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests</param>
        Task WriteAsync(IDownload download, DownloadContext ctx, DownloadBytes bytes, CancellationToken cancellationToken);

        /// <summary>
        /// clear all buffers for this <see cref="IDownloadOutput"/>
        /// </summary>
        /// <param name="download">The download object requesting to clear buffers</param>
        /// <param name="ctx">The current download context</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests</param>
        Task FlushAsync(IDownload download, DownloadContext ctx, CancellationToken cancellationToken);
    }
}

using Download.Core;
using System.Threading;
using System.Threading.Tasks;

namespace Download.Output
{
    public sealed class NullDownloadOutput : IDownloadOutput
    {
        public long Length => 0;
        public Task WriteAsync(IDownload download, DownloadContext ctx, DownloadBytes bytes, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        public Task FlushAsync(IDownload download, DownloadContext ctx, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        public void Dispose()
        {
            // NOP
        }
    }
}

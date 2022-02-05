#nullable enable
using Download.Core;
using System.Collections.Generic;
using System.Linq;

namespace Download.Http
{
    public partial class HttpRangesDownload
    {
        private readonly List<DownloadFailedEventArgs> _failedEventArgs = new List<DownloadFailedEventArgs>();

        private void OnDownloadCanceled(object sender, DownloadEventArgs args)
        {
            if (_downloads.Keys.All(d => d.Status == DownloadStatus.Canceled))
            {
                base.OnDownloadCanceled();
            }
        }
        private void OnDownloadCompleted(object sender, DownloadEventArgs args)
        {
            if (_downloads.Keys.All(d => d.Status == DownloadStatus.Completed))
            {
                base.OnDownloadCompleted();
            }
        }
        private void OnDownloadFailed(object sender, DownloadFailedEventArgs args)
        {
            _failedEventArgs.Add(args);
            if (_downloads.Keys.All(d => d.Status == DownloadStatus.Completed || d.Status == DownloadStatus.Failed))
            {
                base.OnDownloadFailed(new HttpRangesDownloadFailedEventArgs(Id, _failedEventArgs.ToArray()));
            }
        }
        private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs args)
        {
            base.OnDownloadProgressChanged(args);
        }
    }
}

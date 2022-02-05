using Download.Core;
using System;
using System.Linq;

namespace Download.Http
{
    public class HttpRangesDownloadStats : DownloadStats
    {
        #region Fields
        private readonly HttpRangesDownload _download;
        #endregion

        #region Properties
        public override double? Progress
        {
            get
            {
                double? percentage = null;
                long? totalBytesToDownload = _download.Downloads.Sum(d => d.Length);
                long? totalBytesDownloaded = _download.Downloads.Sum(d => d.Downloaded);
                if (totalBytesToDownload.HasValue)
                {
                    percentage = totalBytesDownloaded * 100D / totalBytesToDownload.Value;
                }
                return percentage;
            }
        }
        public override TimeSpan Remaining
        {
            get
            {
                // TODO: use current or average transfer rate instead
                var totalBytesToDownload = _download.Downloads.Sum(d => d.Length);
                var totalBytesDownloaded = _download.Downloads.Sum(d => d.Downloaded);
                if (_download.Status == DownloadStatus.Downloading && totalBytesToDownload > 0)
                {
                    var remainingBytes = Math.Max(1, totalBytesToDownload.Value - totalBytesDownloaded);
                    var remainingTime = TimeSpan.FromSeconds(Elapsed.TotalSeconds * remainingBytes / totalBytesDownloaded);
                    return remainingTime;
                }

                return TimeSpan.Zero;
            }
        }
        #endregion

        #region Constructor
        public HttpRangesDownloadStats(HttpRangesDownload download) : base(download)
        {
            _download = download;
        }
        #endregion
    }
}

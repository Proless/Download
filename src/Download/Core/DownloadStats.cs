#nullable enable
using System;
using System.Threading;

namespace Download.Core
{
    public class DownloadStats : IDownloadStats
    {
        #region Fields
        private readonly IDownload _download;

        private double _currentRate;
        private double _averageRate;
        private double _minRate;
        private double _maxRate;

        private long _lastBytes;
        private long? _totalBytesToDownload;
        private long _totalBytesDownloaded;

        private int _averageCount;

        private DateTime _lastDownloadTime;
        private DateTime _start;
        private DateTime _end;
        #endregion

        #region Properties
        public virtual double? Progress
        {
            get
            {
                double? percentage = null;
                if (_totalBytesToDownload.HasValue)
                {
                    percentage = _totalBytesDownloaded * 100D / _totalBytesToDownload.Value;
                }
                return percentage;
            }
        }
        public double CurrentRate => _currentRate;
        public double AverageRate => _averageRate;
        public double MinRate => _minRate;
        public double MaxRate => _maxRate;

        public virtual DateTime Start => _start;
        public virtual DateTime End => _end;

        public virtual TimeSpan Elapsed => _download.Status == DownloadStatus.Downloading ? DateTime.UtcNow - _start : TimeSpan.Zero;
        public virtual TimeSpan Duration => _download.Status == DownloadStatus.Downloading ? Elapsed : _end - _start;
        public virtual TimeSpan Remaining
        {
            get
            {
                // TODO: use current or average transfer rate instead
                if (_download.Status == DownloadStatus.Downloading && _totalBytesToDownload > 0)
                {
                    var remainingBytes = Math.Max(1, _totalBytesToDownload.Value - _totalBytesDownloaded);
                    var remainingTime = TimeSpan.FromSeconds(Elapsed.TotalSeconds * remainingBytes / _totalBytesDownloaded);
                    return remainingTime;
                }

                return TimeSpan.Zero;
            }
        }
        #endregion

        #region Constructor
        public DownloadStats(IDownload download)
        {
            _download = download;

            _download.DownloadProgressChanged += OnDownloadProgressChanged;

            _download.DownloadCompleted += OnDownloadStatusChanged;
            _download.DownloadCanceled += OnDownloadStatusChanged;
            _download.DownloadStarted += OnDownloadStatusChanged;
            _download.DownloadFailed += OnDownloadStatusChanged;
        }
        #endregion

        #region Methods
        public double OnBytesDownloaded(long bytes, long? totalBytesToDownload)
        {
            _totalBytesToDownload = totalBytesToDownload;

            Interlocked.Add(ref _totalBytesDownloaded, bytes);
            Interlocked.Add(ref _lastBytes, bytes);

            if (_lastDownloadTime == default) // first bytes downloaded
            {
                _lastDownloadTime = DateTime.UtcNow;
            }

            var elapsedTime = Math.Max(1, (DateTime.UtcNow - _lastDownloadTime).TotalSeconds);

            if (elapsedTime > 1d)
            {
                _currentRate = _lastBytes / elapsedTime;

                _averageRate = (_averageRate * _averageCount + _currentRate) / ++_averageCount;

                if (_minRate == 0 && _maxRate == 0)
                {
                    _minRate = _currentRate;
                    _maxRate = _currentRate;
                }
                else
                {
                    _minRate = Math.Min(_minRate, _currentRate);
                    _maxRate = Math.Max(_maxRate, _currentRate);
                }

                Interlocked.Exchange(ref _lastBytes, 0);
                _lastDownloadTime = DateTime.UtcNow;
            }

            return _currentRate;
        }
        #endregion

        #region Helpers
        private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            OnBytesDownloaded(e.ReceivedBytes.Count, e.Length);
        }
        private void OnDownloadStatusChanged(object sender, DownloadEventArgs e)
        {
            var status = ((IDownload)sender).Status;
            if (status == DownloadStatus.Started)
            {
                _start = DateTime.UtcNow;
            }
            else if (status == DownloadStatus.Completed || status == DownloadStatus.Canceled || status == DownloadStatus.Failed)
            {
                _end = DateTime.UtcNow;
            }
        }
        #endregion
    }
}

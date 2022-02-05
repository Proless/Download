#nullable enable
using System;

namespace Download.Core
{
    /// <summary>
    /// The base abstract class for a download.
    /// </summary>
    public abstract partial class Download
    {
        public event EventHandler<DownloadEventArgs> DownloadStarted = delegate { };
        public event EventHandler<DownloadEventArgs> DownloadCanceled = delegate { };
        public event EventHandler<DownloadEventArgs> DownloadCompleted = delegate { };
        public event EventHandler<DownloadFailedEventArgs> DownloadFailed = delegate { };
        public event EventHandler<DownloadProgressChangedEventArgs> DownloadProgressChanged = delegate { };

        protected virtual void OnDownloadProgressChanged(DownloadProgressChangedEventArgs args)
        {
            SetStatus(DownloadStatus.Downloading);
            DownloadProgressChanged.Invoke(this, args);
        }
        protected virtual void OnDownloadFailed(DownloadFailedEventArgs args)
        {
            SetStatus(DownloadStatus.Failed);
            DownloadFailed.Invoke(this, args);
        }
        protected virtual void OnDownloadCompleted()
        {
            SetStatus(DownloadStatus.Completed);
            DownloadCompleted.Invoke(this, new DownloadEventArgs(Id));
        }
        protected virtual void OnDownloadCanceled()
        {
            SetStatus(DownloadStatus.Canceled);
            DownloadCanceled.Invoke(this, new DownloadEventArgs(Id));
        }
        protected virtual void OnDownloadStarted()
        {
            SetStatus(DownloadStatus.Started);
            DownloadStarted.Invoke(this, new DownloadEventArgs(Id));
        }
    }
}

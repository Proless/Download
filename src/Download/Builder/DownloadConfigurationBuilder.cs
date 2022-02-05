using Download.Core;
using System;

namespace Download.Builder
{
    public abstract class DownloadConfigurationBuilder<T, TDownload> where T : DownloadConfigurationBuilder<T, TDownload> where TDownload : IDownload
    {
        protected IDownloadConfiguration Configuration { get; }

        protected DownloadConfigurationBuilder()
        {
            Configuration = new DownloadConfiguration();
        }

        public virtual T BufferSize(int value)
        {
            Configuration.BufferSize = value;
            return (T)this;
        }
        public virtual T MaxRetries(int value)
        {
            Configuration.MaxRetries = value;
            return (T)this;
        }
        public virtual T ForceDownload(bool value)
        {
            Configuration.ForceDownload = value;
            return (T)this;
        }
        public virtual T MaxBytesPerSecond(double value)
        {
            Configuration.MaxBytesPerSecond = value;
            return (T)this;
        }
        public virtual T RetryDelay(TimeSpan value)
        {
            Configuration.RetryDelay = value;
            return (T)this;
        }
        public virtual T Timeout(TimeSpan value)
        {
            Configuration.Timeout = value;
            return (T)this;
        }
        public virtual T Authenticator(IDownloadAuthenticator value)
        {
            Configuration.Authenticator = value;
            return (T)this;
        }

        protected virtual void SetConfigurations(TDownload download)
        {
            download.RetryDelay = Configuration.RetryDelay;
            download.Timeout = Configuration.Timeout;
            download.ForceDownload = Configuration.ForceDownload;
            download.BufferSize = Configuration.BufferSize;
            download.Authenticator = Configuration.Authenticator;
            download.MaxBytesPerSecond = Configuration.MaxBytesPerSecond;
            download.MaxRetries = Configuration.MaxRetries;
        }
    }
}

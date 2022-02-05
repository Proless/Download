#nullable enable
using Download.Core;
using System;

namespace Download.Builder
{
    public class DownloadConfiguration : IDownloadConfiguration
    {
        public int BufferSize { get; set; }
        public int MaxRetries { get; set; }
        public bool ForceDownload { get; set; }
        public double MaxBytesPerSecond { get; set; }
        public TimeSpan RetryDelay { get; set; }
        public TimeSpan Timeout { get; set; }
        public IDownloadAuthenticator? Authenticator { get; set; }
    }
}

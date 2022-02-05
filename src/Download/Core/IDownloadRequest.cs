using System;

namespace Download.Core
{
    public interface IDownloadRequest : IDisposable
    {
        Uri RequestUri { get; }
    }
}
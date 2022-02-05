using Download.Core;
using System;

namespace Download.Http;

public class HttpRangesDownloadFailedEventArgs : DownloadFailedEventArgs
{
    public DownloadFailedEventArgs[] Args { get; }

    public HttpRangesDownloadFailedEventArgs(Guid id, params DownloadFailedEventArgs[] args)
        : base(id, args[0].ErrorMessage, args[0].Exception)
    {
        Args = args;
    }
}
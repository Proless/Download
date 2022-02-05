#nullable enable
using System;

namespace Download.Core
{
    public class DownloadEventArgs : EventArgs
    {
        public Guid Id { get; }

        public DownloadEventArgs(Guid id)
        {
            Id = id;
        }
    }
}

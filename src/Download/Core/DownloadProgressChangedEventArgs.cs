#nullable enable
using System;

namespace Download.Core
{
    public class DownloadProgressChangedEventArgs : DownloadEventArgs
    {
        /// <summary>
        /// Get the total amount of bytes to receive
        /// </summary>
        public long? Length { get; }

        /// <summary>
        /// Get the last received bytes
        /// </summary>
        public DownloadBytes ReceivedBytes { get; }

        /// <summary>
        /// Get the total amount of bytes received since the download started
        /// </summary>
        public long TotalBytesReceived { get; }

        public DownloadProgressChangedEventArgs(Guid id, long totalBytesReceived, long? length, DownloadBytes receivedBytes) : base(id)
        {
            Length = length;
            ReceivedBytes = receivedBytes;
            TotalBytesReceived = totalBytesReceived;
        }
    }
}

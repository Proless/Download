#nullable enable
using System;

namespace Download.Core
{
    public class DownloadFailedEventArgs : DownloadEventArgs
    {
        /// <summary>
        /// Get the reason of the download failure
        /// </summary>
        public string? ErrorMessage { get; }

        /// <summary>
        /// Get the exception that caused the failure if thrown
        /// </summary>
        public Exception? Exception { get; }

        /// <summary>
        /// Get a value to determine if the download is retrying
        /// </summary>
        public bool Retrying { get; }

        public DownloadFailedEventArgs(Guid id, string? errorMessage, Exception? exception, bool retrying = false) : base(id)
        {
            Exception = exception;
            ErrorMessage = errorMessage;
            Retrying = retrying;
        }
    }
}

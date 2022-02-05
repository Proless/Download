#nullable enable
using System;

namespace Download.Core
{
    [Serializable]
    public class DownloadContext
    {
        #region Properties
        /// <summary>
        /// Get or set the requested download range
        /// </summary>
        public DownloadRange Range { get; set; } = DownloadRange.Default;

        /// <summary>
        /// Get or set the total amount of bytes received
        /// </summary>
        public long Downloaded { get; set; }

        /// <summary>
        /// Get or set the download start offset within a byte range
        /// </summary>
        public long Offset { get; set; }

        /// <summary>
        /// Get or set a value to determine if the download is retrying
        /// </summary>
        public bool Retrying { get; set; }

        /// <summary>
        /// Get or set the exception that caused the download to fail
        /// </summary>
        public Exception? Exception { get; set; }
        #endregion
    }
}

#nullable enable
using System;
using System.IO;

namespace Download.Core
{
    public interface IDownloadConfiguration
    {
        /// <summary>
        /// Get or set a value to determine the download buffer size in bytes
        /// </summary>
        /// <remarks>The default value is 4096 bytes</remarks>
        int BufferSize { get; set; }

        /// <summary>
        /// Get or set a value to determine the max retries before the download completely fails
        /// </summary>
        /// <remarks>The default value is 0</remarks>
        int MaxRetries { get; set; }

        /// <summary>
        /// Get or set a value to determine if the download should ignore the <see cref="IDownloadResponse.IsSuccessful"/> value and continue to download the received content
        /// </summary>
        /// <remarks>The default value is <see langword="false"/></remarks>
        bool ForceDownload { get; set; }

        /// <summary>
        ///  Get or set a value to determine the maximum bytes per second that can be downloaded
        /// </summary>
        /// <remarks>The default value is 0, which is unlimited</remarks>
        double MaxBytesPerSecond { get; set; }

        /// <summary>
        /// Get or set a value to determine the time span to wait between retry attempts
        /// </summary>
        /// <remarks>The default value is <see cref="TimeSpan.Zero"/></remarks>
        TimeSpan RetryDelay { get; set; }

        /// <summary>
        /// Get or set a value to determine the timespan to wait before a download operation times out. 
        /// </summary>
        /// <remarks>The default value is 2 Minutes.<br/><br/>
        /// A download operation is a call to any of the following methods:<br/><see cref="IDownloadResponse.GetStreamAsync"/><br/><see cref="IDownloadOutput.WriteAsync"/><br/><see cref="Stream.ReadAsync(byte[],int,int)"/></remarks>
        TimeSpan Timeout { get; set; }

        /// <summary>
        /// Get or set a value to determine the download authenticator if one should be used
        /// </summary>
        IDownloadAuthenticator? Authenticator { get; set; }
    }
}

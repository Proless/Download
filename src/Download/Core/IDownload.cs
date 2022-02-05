#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Download.Core
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDownload : IDownloadConfiguration, IDisposable
    {
        /// <summary>
        /// Occurs when the download operation starts
        /// </summary>
        event EventHandler<DownloadEventArgs> DownloadStarted;

        /// <summary>
        /// Occurs when the download operation is canceled
        /// </summary>
        event EventHandler<DownloadEventArgs> DownloadCanceled;

        /// <summary>
        /// Occurs when the download operation completes successfully
        /// </summary>
        event EventHandler<DownloadEventArgs> DownloadCompleted;

        /// <summary>
        /// Occurs when the download operation fails
        /// </summary>
        event EventHandler<DownloadFailedEventArgs> DownloadFailed;

        /// <summary>
        /// Occurs when the download operation successfully transfers some or all of the data
        /// </summary>
        event EventHandler<DownloadProgressChangedEventArgs> DownloadProgressChanged;

        /// <summary>
        /// The download Id
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Determine if the download supports resuming
        /// </summary>
        bool CanResume { get; }

        /// <summary>
        /// The download protocol
        /// </summary>
        string Protocol { get; }

        /// <summary>
        /// Determines if the download object is disposed
        /// </summary>
        bool Disposed { get; }

        /// <summary>
        /// The total amount of bytes received
        /// </summary>
        long Downloaded { get; }

        /// <summary>
        /// The total length of the data in bytes or <see langword="null"/> if undetermined
        /// </summary>
        long? Length { get; }

        /// <summary>
        /// The Uri used to initialize the download
        /// </summary>
        Uri Uri { get; }

        /// <summary>
        /// The download Uri
        /// </summary>
        Uri DownloadUri { get; }

        /// <summary>
        /// Provides additional information about the download operation
        /// </summary>
        IDownloadStats Stats { get; }

        /// <summary>
        /// The current status of the download
        /// </summary>
        DownloadStatus Status { get; }

        /// <summary>
        /// Start the download operation
        /// </summary>
        /// <exception cref="InvalidOperationException">The download has already been started or finished</exception>
        /// <exception cref="ObjectDisposedException">The current download instance has been disposed</exception>
        Task StartAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Try to fetch and populate the download information like <see cref="Length"/> and <see cref="DownloadUri"/> and any other download specific information
        /// </summary>
        /// <returns> true if retrieving the required information for this download was successful, false if any exception occurs or the required information couldn't be retrieved</returns>
        /// <exception cref="InvalidOperationException">The download has already been started or finished</exception>
        /// <exception cref="ObjectDisposedException">The current download instance has been disposed</exception>
        Task FetchInfoAsync(CancellationToken cancellationToken);
    }
}

#nullable enable
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Download.Core
{
    /// <summary>
    /// The base abstract class for a download.
    /// </summary>
    public abstract partial class Download : IDownload
    {
        #region Fields
        private readonly object _lock;

        private bool _started;
        private bool _fetchingInfo;

        private bool _disposed;
        private DateTime _lastThrottle;
        private long _bytesSinceLastThrottle;
        private Uri? _downloadUri;

        private int _maxRetries;
        private double _maxBytesPerSecond;
        private int _bufferSize;
        private DownloadStatus _status;
        private long? _length;
        #endregion

        #region Configuration
        public virtual int BufferSize
        {
            get => _bufferSize; set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(BufferSize));
                }
                _bufferSize = value;
            }
        }
        public virtual int MaxRetries
        {
            get => _maxRetries; set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(MaxRetries));
                }
                _maxRetries = value;
            }
        }
        public virtual double MaxBytesPerSecond
        {
            get => _maxBytesPerSecond; set
            {
                if (_maxBytesPerSecond < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(MaxBytesPerSecond));
                }
                _maxBytesPerSecond = value;
            }
        }
        public virtual bool ForceDownload { get; set; }
        public virtual TimeSpan RetryDelay { get; set; }
        public virtual TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(2);
        public virtual IDownloadAuthenticator? Authenticator { get; set; }
        #endregion

        #region Properties
        public Guid Id { get; }
        public virtual IDownloadStats Stats { get; }
        public bool Disposed => _disposed;
        public virtual long Downloaded => Context.Downloaded;
        public virtual long? Length => _length;
        public virtual Uri DownloadUri => _downloadUri ?? Uri;
        public virtual DownloadStatus Status => _status;

        public abstract Uri Uri { get; }
        public abstract bool CanResume { get; }
        public abstract string Protocol { get; }
        public abstract DownloadContext Context { get; }
        #endregion

        #region Constructors
        protected Download()
        {
            Id = Guid.NewGuid();
            Stats = new DownloadStats(this);

            SetStatus(DownloadStatus.Created);

            _lastThrottle = DateTime.UtcNow;
            _bufferSize = 4096;
            _lock = new object();
        }
        #endregion

        #region Methods
        public virtual async Task FetchInfoAsync(CancellationToken cancellationToken)
        {
            DownloadStatus prevStatus;
            lock (_lock)
            {
                if (_fetchingInfo)
                {
                    throw new InvalidOperationException();
                }

                _fetchingInfo = true;
                prevStatus = Status;
            }

            EnsureNotDisposed();

            SetStatus(DownloadStatus.FetchingInfo);

            IDownloadRequest? request = null;
            IDownloadResponse? response = null;

            await ExecuteTask(
                async (token) =>
                {
                    request = await GetRequestAsync(token);

                    await Authenticate(request, token);

                    response = await GetResponseAsync(request, token);

                    PopulateInfo(response);
                },
                null,
                null,
                () =>
                {
                    request?.Dispose();
                    response?.Dispose();
                    return Task.CompletedTask;
                },
                null,
                cancellationToken);

            lock (_lock)
            {
                _fetchingInfo = false;
                SetStatus(prevStatus);
            }
        }
        public virtual async Task StartAsync(CancellationToken cancellationToken)
        {
            lock (_lock)
            {
                if (_started)
                {
                    throw new InvalidOperationException();
                }

                _started = true;
            }

            EnsureNotDisposed();

            OnDownloadStarted();

            await ExecuteTask(
                DownloadAsync,
                (ex) =>
                {
                    if (!Context.Retrying)
                    {
                        OnDownloadFailed(new DownloadFailedEventArgs(Id, ex.Message, ex));
                    }
                    return Task.CompletedTask;
                },
                (_) =>
                {
                    OnDownloadCanceled();
                    return Task.CompletedTask;
                },
                () => Output.FlushAsync(this, Context, cancellationToken),
                () => Status == DownloadStatus.Canceled || (Status == DownloadStatus.Failed && !Context.Retrying) || Status == DownloadStatus.Completed,
                cancellationToken);

            lock (_lock)
            {
                _started = false;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Operations
        protected abstract Task<IDownloadResponse> GetResponseAsync(IDownloadRequest request, CancellationToken cancellationToken);
        protected abstract Task<IDownloadRequest> GetRequestAsync(CancellationToken cancellationToken);
        protected virtual Task Authenticate(IDownloadRequest request, CancellationToken cancellationToken)
        {
            return Authenticator != null ? Authenticator.Authenticate(request, cancellationToken) : Task.CompletedTask;
        }
        protected virtual async Task DownloadAsync(CancellationToken cancellationToken)
        {
            IDownloadRequest? request = null;
            IDownloadResponse? response = null;

            Stream? inputStream = null;

            var buffer = new byte[BufferSize];

            try
            {
                SetStatus(DownloadStatus.FetchingInfo);

                request = await GetRequestAsync(cancellationToken);

                await Authenticate(request, cancellationToken);

                response = await GetResponseAsync(request, cancellationToken);

                PopulateInfo(response);

                if (!response.IsSuccessful && !ForceDownload)
                {
                    throw new DownloadException(response.StatusMessage ?? "");
                }

                inputStream = await DownloadTimeout.WaitAsync
                (
                    token => response.GetStreamAsync(token),
                    () => throw new TimeoutException(),
                    Timeout,
                    cancellationToken
                );

                int readBytes;
                do
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    readBytes = await DownloadTimeout.WaitAsync
                    (
                        token => inputStream.ReadAsync(buffer, 0, buffer.Length, token),
                        () => throw new TimeoutException(),
                        Timeout,
                        cancellationToken
                    );

                    var downloadBytes = new DownloadBytes(buffer, readBytes, Context.Downloaded);

                    await DownloadTimeout.WaitAsync
                    (
                        token => Output.WriteAsync(this, Context, downloadBytes, token),
                        () => throw new TimeoutException(),
                        Timeout,
                        cancellationToken
                    );

                    Context.Downloaded += readBytes;

                    if (readBytes == 0)
                    {
                        continue;
                    }

                    OnDownloadProgressChanged(new DownloadProgressChangedEventArgs(Id, Context.Downloaded, Length, downloadBytes));

                    await Throttle(readBytes);

                } while (readBytes != 0);

                OnDownloadCompleted();
            }
            finally
            {
                inputStream?.Dispose();
                response?.Dispose();
                request?.Dispose();
            }
        }
        protected virtual void PopulateInfo(IDownloadResponse downloadResponse)
        {
            _length = downloadResponse.Length;
            _downloadUri = downloadResponse.ResponseUri;
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Output.Dispose();
            }

            _disposed = true;
        }
        protected abstract IDownloadOutput Output { get; }
        #endregion

        #region Helpers
        protected async Task ExecuteTask(Func<CancellationToken, Task> task, Func<Exception, Task>? onException, Func<OperationCanceledException, Task>? onCancelled, Func<Task>? onFinally, Func<bool>? retryBreakCondition, CancellationToken cancellationToken)
        {
            var retryCount = 0;
            var canRetry = true;

            while (canRetry)
            {
                try
                {
                    await task(cancellationToken);
                    canRetry = false;
                    Context.Retrying = false;
                }
                catch (OperationCanceledException operationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    Context.Retrying = false;
                    if (onCancelled != null)
                    {
                        await onCancelled(operationCanceledException);
                    }

                    break;
                }
                catch (Exception ex)
                {
                    canRetry = retryCount < MaxRetries;
                    Context.Exception = ex;
                    Context.Retrying = canRetry;

                    if (canRetry)
                    {
                        SetStatus(DownloadStatus.Retrying);

                        OnDownloadFailed(new DownloadFailedEventArgs(Id, ex.Message, ex, true));

                        await DelayRetry(cancellationToken);

                        retryCount++;
                    }

                    if (onException != null)
                    {
                        await onException(ex);
                    }
                }
                finally
                {
                    if (onFinally != null)
                    {
                        await onFinally();
                    }
                }

                if (retryBreakCondition != null && retryBreakCondition())
                {
                    Context.Retrying = false;
                    break;
                }
            }
        }
        protected Task DelayRetry(CancellationToken cancellationToken)
        {
            return RetryDelay == TimeSpan.Zero ? Task.CompletedTask : Task.Delay(RetryDelay, cancellationToken);
        }
        protected void SetStatus(DownloadStatus downloadStatus)
        {
            _status = downloadStatus;
        }
        protected async Task Throttle(long receivedBytes)
        {
            if (MaxBytesPerSecond <= 0 || receivedBytes <= 0)
            {
                return;
            }

            _bytesSinceLastThrottle += receivedBytes;

            var elapsed = DateTime.UtcNow - _lastThrottle;

            if (elapsed > TimeSpan.Zero)
            {
                var bps = _bytesSinceLastThrottle * 1000L / elapsed.TotalMilliseconds;
                if (bps > MaxBytesPerSecond)
                {
                    var wakeElapsed = _bytesSinceLastThrottle * 1000L / MaxBytesPerSecond;
                    var timeToDelay = TimeSpan.FromMilliseconds(wakeElapsed - elapsed.TotalMilliseconds);

                    if (timeToDelay.TotalMilliseconds > 1)
                    {
                        await Task.Delay(timeToDelay);

                        if (elapsed.TotalSeconds > 1)
                        {
                            // reset
                            _lastThrottle = DateTime.UtcNow;
                            _bytesSinceLastThrottle = 0;
                        }
                    }
                }
            }

        }
        protected void EnsureNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(Download));
            }
        }
        #endregion
    }
}
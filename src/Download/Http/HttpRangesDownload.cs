#nullable enable
using Download.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Download.Http
{
    public partial class HttpRangesDownload : HttpDownload
    {
        #region Fields
        private readonly Dictionary<HttpRangeDownload, HttpRangeDownloadContext> _downloads;
        private readonly HttpRangesDownloadContext _context;

        private readonly object _lock;

        private bool _started;
        private bool _fetchingInfo;

        private bool _downloadsDisposed;
        private bool _downloadsCreated;

        private Action<HttpDownloadRequest>? _requestConfigurator;
        private IDownloadAuthenticator? _authenticator;
        private double _maxBytesPerSecond;
        private TimeSpan _retryDelay;
        private TimeSpan _timeout;
        private bool _forceDownload;
        private string? _userAgent;
        private int _bufferSize;
        private int _maxRetries;
        #endregion

        #region Configuration
        public override int BufferSize
        {
            get => _bufferSize; set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(BufferSize));
                }
                _bufferSize = value;
                SetConfiguration(download => download.BufferSize = value);
            }
        }
        public override int MaxRetries
        {
            get => _maxRetries; set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(MaxRetries));
                }
                _maxRetries = value;
                SetConfiguration(download => download.MaxRetries = value);
            }
        }
        public override double MaxBytesPerSecond
        {
            get => _maxBytesPerSecond; set
            {
                if (_maxBytesPerSecond < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(MaxBytesPerSecond));
                }
                _maxBytesPerSecond = value;
                SetConfiguration(download => download.MaxBytesPerSecond = value);
            }
        }
        public override bool ForceDownload
        {
            get => _forceDownload;
            set
            {
                _forceDownload = value;
                SetConfiguration(download => download.ForceDownload = value);
            }
        }
        public override TimeSpan RetryDelay
        {
            get => _retryDelay; set
            {
                _retryDelay = value;
                SetConfiguration(download => download.RetryDelay = value);
            }
        }
        public override TimeSpan Timeout
        {
            get => _timeout; set
            {
                _timeout = value;
                SetConfiguration(download => download.Timeout = value);
            }
        }
        public override IDownloadAuthenticator? Authenticator
        {
            get => _authenticator;
            set
            {
                _authenticator = value;
                SetConfiguration(download => download.Authenticator = value);
            }
        }
        public override string? UserAgent
        {
            get => _userAgent;
            set
            {
                _userAgent = value;
                SetConfiguration(download => download.UserAgent = value);
            }
        }
        public override Action<HttpDownloadRequest>? RequestConfigurator
        {
            get => _requestConfigurator;
            set
            {
                _requestConfigurator = value;
                SetConfiguration(download => download.RequestConfigurator = value);
            }
        }
        #endregion

        #region Properties
        public override IDownloadStats Stats { get; }
        public IEnumerable<HttpRangeDownload> Downloads => _downloads.Keys;
        public override bool CanResume => SupportsRanges;
        public override long? Length => _downloads.Keys.Sum(d => d.Length);
        public override long Downloaded => _downloads.Keys.Sum(d => d.Downloaded);
        public override DownloadContext Context => _context;

        #endregion

        #region Constructor
        public HttpRangesDownload(Uri uri, IDownloadOutput output, HttpClient client, IEnumerable<DownloadRange> ranges, bool disposeClient = false)
            : this(uri, output, client, new HttpRangesDownloadContext(ranges), disposeClient) { }
        public HttpRangesDownload(Uri uri, IDownloadOutput output, HttpClient client, HttpRangesDownloadContext context, bool disposeClient = false)
            : base(uri, output, client, disposeClient)
        {
            Stats = new HttpRangesDownloadStats(this);

            _context = context;
            _bufferSize = 4096;
            _lock = new object();
            _downloads = new Dictionary<HttpRangeDownload, HttpRangeDownloadContext>();
        }
        #endregion

        #region Methods
        public override Task StartAsync(CancellationToken cancellationToken)
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

            CreateRangeDownloads();

            return Task.WhenAll
            (
                _downloads
                    .Keys
                    .Select(download => download.StartAsync(cancellationToken))
                    .ToArray()
            ).ContinueWith(_ =>
            {
                lock (_lock)
                {
                    _started = false;
                }
            }, cancellationToken);
        }
        public override Task FetchInfoAsync(CancellationToken cancellationToken)
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

            CreateRangeDownloads();

            return Task.WhenAll
            (
                _downloads
                    .Keys
                    .Select(download => download.FetchInfoAsync(cancellationToken))
                    .ToArray()
            ).ContinueWith(_ =>
            {
                lock (_lock)
                {
                    _fetchingInfo = false;
                    SetStatus(prevStatus);
                }
            }, cancellationToken);
        }
        #endregion

        #region Operations
        protected override void Dispose(bool disposing)
        {
            if (_downloadsDisposed)
            {
                return;
            }

            if (disposing)
            {
                foreach (var download in _downloads.Keys)
                {
                    download.DownloadFailed -= OnDownloadFailed;
                    download.DownloadCanceled -= OnDownloadCanceled;
                    download.DownloadCompleted -= OnDownloadCompleted;
                    download.DownloadProgressChanged -= OnDownloadProgressChanged;

                    download.Dispose();
                }

                _downloads.Clear();

                _downloadsDisposed = true;
            }

            base.Dispose(disposing);
        }
        #endregion

        #region Helpers
        protected void SetConfiguration(Action<HttpRangeDownload> setter)
        {
            foreach (var download in _downloads)
            {
                setter(download.Key);
            }
        }
        protected void CreateRangeDownloads()
        {
            if (_downloadsCreated)
            {
                return;
            }

            foreach (var context in _context.Contexts)
            {
                var download = new HttpRangeDownload(Uri, Output, HttpClient, context)
                {
                    // Configuration
                    Timeout = Timeout,
                    UserAgent = UserAgent,
                    BufferSize = BufferSize,
                    MaxRetries = MaxRetries,
                    RetryDelay = RetryDelay,
                    Authenticator = Authenticator,
                    ForceDownload = ForceDownload,
                    MaxBytesPerSecond = MaxBytesPerSecond,
                    RequestConfigurator = RequestConfigurator
                };

                // Events
                download.DownloadFailed += OnDownloadFailed;
                download.DownloadCanceled += OnDownloadCanceled;
                download.DownloadCompleted += OnDownloadCompleted;
                download.DownloadProgressChanged += OnDownloadProgressChanged;

                _downloads.Add(download, context);
            }

            _downloadsCreated = true;
        }
        #endregion
    }
}
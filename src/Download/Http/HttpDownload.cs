#nullable enable
using Download.Core;
using System;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Download.Http
{
    public class HttpDownload : Core.Download
    {
        #region Static
        /// <summary>
        /// The default UserAgent
        /// </summary>
        public static string DefaultUserAgent
        {
            get
            {
                var name = $"{nameof(Download)}";
                var version = typeof(HttpDownload).Assembly.GetName().Version;
                var comment = $"{RuntimeInformation.OSDescription}; {RuntimeInformation.OSArchitecture}; {RuntimeInformation.FrameworkDescription}";
                return $"{name}/{(version == null ? "" : version.ToString(3))} ({comment})";
            }
        }
        #endregion

        #region Fields
        private readonly HttpDownloadContext _context;
        private readonly bool _disposeClient;

        private bool _clientDisposed;
        #endregion

        #region Configuration
        public virtual string? UserAgent { get; set; }
        public virtual Action<HttpDownloadRequest>? RequestConfigurator { get; set; }
        #endregion

        #region Properties
        public override Uri Uri { get; }
        public virtual bool SupportsRanges => _context.SupportsRanges;
        public override DownloadContext Context => _context;
        public override string Protocol => "HTTP";
        public override bool CanResume => false;
        #endregion

        #region Constructors
        public HttpDownload(Uri uri, IDownloadOutput output, HttpClient client, bool disposeClient = false)
            : this(uri, output, client, new HttpDownloadContext(), disposeClient) { }
        public HttpDownload(Uri uri, IDownloadOutput output, HttpClient client, HttpDownloadContext context, bool disposeClient = false)
        {
            Uri = uri ?? throw new ArgumentNullException(nameof(uri));

            if (!(uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                throw new ArgumentException($"This download type doesn't support the specified Uri Scheme ", nameof(uri));
            }

            Output = output ?? throw new ArgumentNullException(nameof(output));
            HttpClient = client ?? throw new ArgumentNullException(nameof(client));

            _context = context;
            _disposeClient = disposeClient;
        }
        #endregion

        #region Protected
        protected HttpClient HttpClient { get; }
        protected override IDownloadOutput Output { get; }
        protected override Task<IDownloadRequest> GetRequestAsync(CancellationToken cancellationToken)
        {
            var request = new HttpDownloadRequest
            {
                RequestUri = Uri,
                Method = HttpMethod.Get
            };

            UserAgent ??= DefaultUserAgent;

            request.Headers.UserAgent.TryParseAdd(UserAgent);

            RequestConfigurator?.Invoke(request);

            return Task.FromResult<IDownloadRequest>(request);
        }
        protected override async Task<IDownloadResponse> GetResponseAsync(IDownloadRequest request, CancellationToken cancellationToken)
        {
            var downloadRequest = (HttpDownloadRequest)request;

            var responseMessage = await HttpClient.SendAsync(downloadRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            return new HttpDownloadResponse(request.RequestUri, responseMessage);
        }
        protected override void PopulateInfo(IDownloadResponse downloadResponse)
        {
            base.PopulateInfo(downloadResponse);

            var httpResponse = (HttpDownloadResponse)downloadResponse;

            _context.ContentDisposition = httpResponse.ResponseMessage.Content.Headers.ContentDisposition.ToString();
            _context.SupportsRanges = httpResponse.ResponseMessage.Headers.AcceptRanges.Any() ||
                                      httpResponse.ResponseMessage.StatusCode == System.Net.HttpStatusCode.PartialContent;
        }
        protected override void Dispose(bool disposing)
        {
            if (_clientDisposed)
            {
                return;
            }

            if (disposing)
            {
                if (_disposeClient)
                {
                    HttpClient.Dispose();
                }

                _clientDisposed = true;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}

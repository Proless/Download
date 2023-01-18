#nullable enable
using Download.Builder;
using Download.Core;
using Download.Output;
using System;
using System.Net.Http;

namespace Download.Http
{
    public class HttpDownloadBuilder : DownloadBuilder<HttpDownloadBuilder, HttpDownload>
    {
        #region Fields
        protected string? UserAgent;
        protected bool DisposeClient;
        protected HttpClient HttpClient;

        protected Action<HttpDownloadRequest>? RequestConfigurator;

        protected DownloadRange? Range;
        #endregion

        #region Constructor
        public HttpDownloadBuilder()
        {
            Output = new NullDownloadOutput();
            HttpClient = HttpClientFactory.Instance;
        }
        #endregion

        #region Methods
        public HttpDownloadBuilder WithRange(DownloadRange value)
        {
            Range = value;
            return this;
        }
        public HttpDownloadBuilder WithUserAgent(string value)
        {
            UserAgent = value;
            return this;
        }
        public HttpDownloadBuilder WithHttpClient(HttpClient value, bool disposeClient = false)
        {
            DisposeClient = disposeClient;
            HttpClient = value;
            return this;
        }
        public HttpDownloadBuilder WithRequestConfigurator(Action<HttpDownloadRequest> value)
        {
            RequestConfigurator = value;
            return this;
        }
        #endregion

        #region Protected
        protected override HttpDownload InternalBuild(Uri uri)
        {
            return Range.HasValue
                ? new HttpRangeDownload(uri, Range.Value, Output, HttpClient, DisposeClient)
                : new HttpDownload(uri, Output, HttpClient, DisposeClient);
        }
        protected override void SetConfigurations(HttpDownload download)
        {
            download.RequestConfigurator = RequestConfigurator;
            download.UserAgent = UserAgent;
            base.SetConfigurations(download);
        }
        #endregion
    }
}

#nullable enable
using Download.Core;
using System;
using System.Net.Http;

namespace Download.Http
{
    public class HttpDownloadRequest : HttpRequestMessage, IDownloadRequest
    {
        public HttpDownloadRequest() { }
        public HttpDownloadRequest(HttpMethod method, string? requestUri) : base(method, requestUri) { }
        public HttpDownloadRequest(HttpMethod method, Uri? requestUri) : base(method, requestUri) { }
    }
}

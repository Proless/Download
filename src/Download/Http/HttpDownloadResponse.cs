#nullable enable
using Download.Core;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Download.Http
{
    public sealed class HttpDownloadResponse : IDownloadResponse
    {
        private readonly Uri _requestUri;
        private bool _invalidated;

        public HttpResponseMessage ResponseMessage { get; }
        public Uri ResponseUri => ResponseMessage.RequestMessage?.RequestUri ?? _requestUri;
        public bool IsSuccessful => !_invalidated && ResponseMessage.IsSuccessStatusCode;
        public string? StatusMessage => ResponseMessage.ReasonPhrase;
        public long? Length => ResponseMessage.Content.Headers.ContentLength;
        public HttpStatusCode StatusCode => ResponseMessage.StatusCode;

        public HttpDownloadResponse(Uri requestUri, HttpResponseMessage responseMessage)
        {
            ResponseMessage = responseMessage;

            _requestUri = requestUri;
            _invalidated = false;
        }

        public Task<Stream> GetStreamAsync(CancellationToken cancellationToken)
        {
            return ResponseMessage.Content.ReadAsStreamAsync();
        }
        public void Invalidate()
        {
            _invalidated = true;
        }
        public void Dispose()
        {
            ResponseMessage.Dispose();
        }
    }
}

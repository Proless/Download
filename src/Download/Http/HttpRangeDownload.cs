#nullable enable
using Download.Core;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Download.Http
{
    public class HttpRangeDownload : HttpDownload
    {
        #region Fields
        private readonly HttpRangeDownloadContext _context;
        #endregion

        #region Properties
        /// <summary>
        /// Get the requested byte range
        /// </summary>
        public DownloadRange Range => _context.Range;

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public override bool CanResume => SupportsRanges;

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public override DownloadContext Context => _context;
        #endregion

        #region Constructor
        public HttpRangeDownload(Uri uri, DownloadRange range, IDownloadOutput output, HttpClient client, bool disposeClient = false)
            : this(uri, output, client, new HttpRangeDownloadContext { Range = range }, disposeClient) { }
        public HttpRangeDownload(Uri uri, IDownloadOutput output, HttpClient client, HttpRangeDownloadContext context, bool disposeClient = false)
            : base(uri, output, client, context, disposeClient)
        {
            _context = context;
        }
        #endregion

        #region Protected
        protected override async Task<IDownloadRequest> GetRequestAsync(CancellationToken cancellationToken)
        {
            var request = await base.GetRequestAsync(cancellationToken);
            var httpRequest = (HttpDownloadRequest)request;
            var rangeHeader = new RangeHeaderValue(Range.Start, Range.End);

            if (SupportsRanges)
            {
                var start = Range.Start;
                var end = Range.End;

                if (!start.HasValue && end.HasValue)
                {
                    end = end.Value - Downloaded;
                }
                else
                {
                    start += Downloaded;
                }

                rangeHeader = new RangeHeaderValue(start, end);
            }

            httpRequest.Headers.Range = rangeHeader;

            return request;
        }
        protected override void PopulateInfo(IDownloadResponse downloadResponse)
        {
            base.PopulateInfo(downloadResponse);
            var httpResponse = (HttpDownloadResponse)downloadResponse;

            _context.Offset = httpResponse.ResponseMessage.Content.Headers.ContentRange.From.GetValueOrDefault();
            _context.Range = new DownloadRange(httpResponse.ResponseMessage.Content.Headers.ContentRange.From, httpResponse.ResponseMessage.Content.Headers.ContentRange.To);

            if (!_context.Retrying || !SupportsRanges)
            {
                _context.Etag = httpResponse.ResponseMessage.Headers.ETag.ToString();
                _context.LastModified = httpResponse.ResponseMessage.Content.Headers.LastModified;
            }

            if (!IsResponseValid(httpResponse))
            {
                httpResponse.Invalidate();
            }
        }
        #endregion

        #region Helpers
        private bool IsResponseValid(HttpDownloadResponse response)
        {
            var isPartialContent = response.ResponseMessage.StatusCode == System.Net.HttpStatusCode.PartialContent;

            var matchesPreviousContentEtag = true;
            var matchesPreviousContentLastModified = true;

            if (_context.Retrying && SupportsRanges)
            {
                if (EntityTagHeaderValue.TryParse(_context.Etag, out var prevEtag))
                {
                    var etag = response.ResponseMessage.Headers.ETag;
                    matchesPreviousContentEtag = prevEtag?.IsWeak == etag?.IsWeak && prevEtag?.Tag == etag?.Tag;
                }

                matchesPreviousContentLastModified = _context.LastModified == response.ResponseMessage.Content.Headers.LastModified;
            }

            return isPartialContent && matchesPreviousContentEtag && matchesPreviousContentLastModified;
        }
        #endregion
    }
}

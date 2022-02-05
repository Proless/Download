using Download.Core;
using System;
using System.Collections.Generic;

namespace Download.Http
{
    public class HttpDownloadContext : DownloadContext
    {
        public bool SupportsRanges { get; set; }
        public string ContentDisposition { get; set; }
    }
    public class HttpRangeDownloadContext : HttpDownloadContext
    {
        public DateTimeOffset? LastModified { get; set; }
        public string Etag { get; set; }
    }
    public class HttpRangesDownloadContext : HttpDownloadContext
    {
        public List<HttpRangeDownloadContext> Contexts { get; set; }

        public HttpRangesDownloadContext(IEnumerable<DownloadRange> ranges)
        {
            Contexts = new List<HttpRangeDownloadContext>();

            foreach (var range in ranges)
            {
                Contexts.Add(new HttpRangeDownloadContext { Range = range });
            }
        }
    }
}

#nullable enable
using Download.Core;
using Download.Output.File;
using System.Net.Http.Headers;

namespace Download.Http
{
    public class ContentDispositionFileNameProvider : FileNameProvider
    {
        public override string? GetName(IDownload download, DownloadContext context)
        {
            if (context is HttpDownloadContext httpContext && ContentDispositionHeaderValue.TryParse(httpContext.ContentDisposition, out var dispositionHeader))
            {
                return !string.IsNullOrWhiteSpace(dispositionHeader.FileNameStar)
                    ? dispositionHeader.FileNameStar
                    : dispositionHeader.FileName;
            }

            return null;
        }
    }
}

using Download.Http;

namespace Download
{
    public static class DownloadFactoryExtensions
    {
        public static HttpDownloadBuilder Http(this DownloadFactory factory)
        {
            return new HttpDownloadBuilder();
        }
    }
}

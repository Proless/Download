using Download.Http;

namespace Download
{
    public static class DownloadFactory
    {
        public static HttpDownloadBuilder Http()
        {
            return new HttpDownloadBuilder();
        }
    }
}

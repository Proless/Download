using System.Net.Http;

namespace Download.Http.Client
{
    internal class HttpDownloadClient : HttpClient
    {
        private bool _disposed;
        public bool Disposed => _disposed;

        protected override void Dispose(bool disposing)
        {
            _disposed = true;
            base.Dispose(disposing);
        }
    }
}

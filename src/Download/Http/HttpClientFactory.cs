#nullable enable
using Download.Http.Client;
using System.Net.Http;
using System.Threading;

namespace Download.Http
{
    public static class HttpClientFactory
    {
        #region Fields
        private static HttpDownloadClient? _client;
        #endregion

        #region Properties
        public static HttpClient Instance
        {
            get
            {
                if (_client == null || _client.Disposed)
                {
                    _client = new HttpDownloadClient
                    {
                        Timeout = Timeout.InfiniteTimeSpan
                    };
                }
                return _client;
            }
        }
        #endregion
    }
}

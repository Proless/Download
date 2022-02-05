using Download.Core;
using System;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Download.Http.Authenticator
{
    public class HttpBasicDownloadAuthenticator : IDownloadAuthenticator
    {
        private readonly byte[] _encodedCredentials;

        public string AuthenticationType => "Basic";

        public HttpBasicDownloadAuthenticator(string userName, string password) : this(userName, password, Encoding.UTF8) { }

        public HttpBasicDownloadAuthenticator(string userName, string password, Encoding encoding)
        {
            _encodedCredentials = encoding.GetBytes($"{userName}:{password}");
        }

        public Task Authenticate(IDownloadRequest request, CancellationToken cancellationToken)
        {
            if (request is HttpDownloadRequest httpDownloadRequest)
            {
                httpDownloadRequest.Headers.Authorization = new AuthenticationHeaderValue(AuthenticationType, Convert.ToBase64String(_encodedCredentials));
            }
            return Task.CompletedTask;
        }
    }
}
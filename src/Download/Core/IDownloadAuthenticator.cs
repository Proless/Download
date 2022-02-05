using System.Threading;
using System.Threading.Tasks;

namespace Download.Core
{
    public interface IDownloadAuthenticator
    {
        string AuthenticationType { get; }
        Task Authenticate(IDownloadRequest request, CancellationToken cancellationToken);
    }
}

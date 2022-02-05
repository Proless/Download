#nullable enable
using Download.Core;
using System;

namespace Download.Output.File
{
    public abstract class FileNameProvider
    {
        /// <summary>
        /// Get the default <see cref="FileNameProvider"/> instance which provides the <see cref="IDownload.Id"/> as the file name
        /// </summary>
        public static FileNameProvider Default { get; } = new DelegateFileNameProvider((download, _) => $"{download.Id:N}");

        /// <summary>
        /// Get a string value for a file name, or <see langword="null"/> if a name couldn't be determined, in this case the <see cref="Default"/> instance will be used
        /// </summary>
        public abstract string? GetName(IDownload download, DownloadContext context);
    }

    public class DelegateFileNameProvider : FileNameProvider
    {
        private readonly Func<IDownload, DownloadContext, string> _provider;

        public DelegateFileNameProvider(Func<IDownload, DownloadContext, string> provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public override string GetName(IDownload download, DownloadContext context)
        {
            return _provider(download, context);
        }
    }
}

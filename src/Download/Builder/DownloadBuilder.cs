using Download.Core;
using System;

namespace Download.Builder
{
    public abstract class DownloadBuilder<TBuilder, TDownload> : DownloadConfigurationBuilder<TBuilder, TDownload> where TBuilder : DownloadBuilder<TBuilder, TDownload> where TDownload : IDownload
    {
        protected IDownloadOutput Output;
        
        public TDownload Build(Uri uri)
        {
            var download = InternalBuild(uri);

            SetConfigurations(download);

            return download;
        }
        public TBuilder WithOutput(IDownloadOutput output)
        {
            Output = output ?? throw new ArgumentNullException(nameof(output));
            return (TBuilder)this;
        }
        
        protected abstract TDownload InternalBuild(Uri uri);
    }
}

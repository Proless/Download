#nullable enable
using Download.Core;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Download.Output.File
{
    public abstract class FileOutput : IDownloadOutput
    {
        #region Fields
        private readonly FileNameProvider? _fileNameProvider;
        private readonly string? _directory;
        private readonly string? _file;
        private bool _disposed;
        #endregion

        #region Configuration
        /// <summary>
        /// Get or set a value to determine the buffer size to use for the <see cref="FileStream"/>
        /// </summary>
        /// <remarks>The default value is 4096</remarks>
        public virtual int BufferSize { get; set; } = 4096;

        /// <summary>
        /// <inheritdoc cref="System.IO.FileMode"/>
        /// </summary>
        /// <remarks>The default value is <see cref="System.IO.FileMode.OpenOrCreate"/></remarks>
        public virtual FileMode FileMode { get; set; } = FileMode.OpenOrCreate;

        /// <summary>
        /// <inheritdoc cref="System.IO.FileOptions"/>
        /// </summary>
        /// <remarks>The default value is <see cref="System.IO.FileOptions.WriteThrough"/> | <see cref="System.IO.FileOptions.Asynchronous"/></remarks>
        public virtual FileOptions FileOptions { get; set; } = FileOptions.WriteThrough | FileOptions.Asynchronous;
        #endregion

        #region Properties
        public abstract long Length { get; }
        #endregion

        #region Constructor
        protected FileOutput(string file)
        {
            if (string.IsNullOrWhiteSpace(file) || file.IndexOfAny(Path.GetInvalidPathChars(), 0) > -1)
            {
                throw new ArgumentException(@"file is an empty string (""), contains only white space, or contains one or more invalid characters", nameof(file));
            }
            _file = file;
        }
        protected FileOutput(string directory, FileNameProvider fileNameProvider)
        {
            if (string.IsNullOrWhiteSpace(directory) || directory.IndexOfAny(Path.GetInvalidPathChars(), 0) > -1)
            {
                throw new ArgumentException(@"directory is an empty string (""), contains only white space, or contains one or more invalid characters", nameof(directory));
            }

            _directory = directory;
            _fileNameProvider = fileNameProvider ?? throw new ArgumentNullException(nameof(fileNameProvider));
        }
        #endregion

        #region Methods
        public virtual async Task WriteAsync(IDownload download, DownloadContext ctx, DownloadBytes bytes, CancellationToken cancellationToken)
        {
            EnsureNotDisposed();

            var stream = GetStream(download, ctx, bytes);
            await stream.WriteAsync(bytes.Bytes, 0, bytes.Count, cancellationToken);
        }
        public virtual async Task FlushAsync(IDownload download, DownloadContext ctx, CancellationToken cancellationToken)
        {
            EnsureNotDisposed();

            var stream = GetStream(download, ctx, default);
            await stream.FlushAsync(cancellationToken);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Protected
        protected abstract FileStream GetStream(IDownload download, DownloadContext context, DownloadBytes bytes);
        protected virtual string GetPath(IDownload download, DownloadContext context)
        {
            if (_file != null)
            {
                return _file;
            }

            var name = _fileNameProvider!.GetName(download, context);

            if (string.IsNullOrWhiteSpace(name))
            {
                name = FileNameProvider.Default.GetName(download, context);
            }

            return Path.Combine(Path.GetFullPath(_directory!), name!);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // Ignore
            }

            _disposed = true;
        }
        #endregion

        #region Helpers
        protected void EnsureNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(FileOutput));
            }
        }
        #endregion
    }
}

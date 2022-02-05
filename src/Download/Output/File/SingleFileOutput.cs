#nullable enable
using Download.Core;
using System;
using System.IO;

namespace Download.Output.File
{
    public class SingleFileOutput : FileOutput
    {
        #region Fields
        private FileStream? _fileStream;
        private IDownload? _download;
        private bool _disposed;
        #endregion

        #region Properties
        public override long Length => (_fileStream?.Length).GetValueOrDefault();
        #endregion

        #region Constructor
        public SingleFileOutput(string file)
            : base(file) { }
        public SingleFileOutput(string directory, FileNameProvider fileNameProvider)
            : base(directory, fileNameProvider) { }
        #endregion

        #region Protected
        protected override FileStream GetStream(IDownload download, DownloadContext context, DownloadBytes bytes)
        {
            if (_download != null && download != _download)
            {
                throw new InvalidOperationException("The output is can't be shared between multiple download instances");
            }

            _download ??= download;

            if (_fileStream == null)
            {
                var path = GetPath(download, context);
                _fileStream = new FileStream(path, FileMode, FileAccess.Write, FileShare.Read, BufferSize, FileOptions);
            }

            if (_fileStream.CanSeek && _fileStream.Position != bytes.Offset)
            {
                _fileStream.Seek(bytes.Offset, SeekOrigin.Begin);
            }

            return _fileStream;
        }
        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _fileStream?.Dispose();
                _fileStream = null;

                _disposed = true;
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}

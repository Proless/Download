#nullable enable
using System;
using System.Runtime.Serialization;

namespace Download.Core
{
    [Serializable]
    public class DownloadException : Exception
    {
        public DownloadException() { }
        public DownloadException(string message) : base(message) { }
        public DownloadException(string message, Exception inner) : base(message, inner) { }
        protected DownloadException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}

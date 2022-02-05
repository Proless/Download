using System;

namespace Download.Core
{
    public readonly struct DownloadBytes
    {
        /// <summary>
        /// The bytes count
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// The bytes start offset within the download
        /// </summary>
        public long Offset { get; }

        /// <summary>
        /// The bytes buffer
        /// </summary>
        public byte[] Bytes { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadBytes"/> using the specified byte array, count and offset of the received bytes.
        /// </summary>
        /// <remarks>The <paramref name="buffer"/> parameter will be copied to a new byte array that has the length set to the <paramref name="count"/> parameter</remarks>
        public DownloadBytes(byte[] buffer, int count, long offset)
        {
            Bytes = new byte[count];
            Buffer.BlockCopy(buffer, 0, Bytes, 0, count);
            Count = count;
            Offset = offset;
        }
    }
}

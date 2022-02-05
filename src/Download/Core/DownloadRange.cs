using System;

namespace Download.Core
{
    [Serializable]
    public readonly struct DownloadRange : IEquatable<DownloadRange>
    {
        /// <summary>
        /// Get a default <see cref="DownloadRange"/> instance which indicates a full range.
        /// </summary>
        public static DownloadRange Default { get; } = new DownloadRange(0);

        public long? Start { get; }
        public long? End { get; }

        public long? Length => End - Start == 0 ? 0 : End - Start + 1;

        public DownloadRange(long? start) : this(start, null) { }
        public DownloadRange(long? start, long? end)
        {
            if (start < 0 || end < 0 || start > end)
            {
                throw new ArgumentOutOfRangeException($"{start}, {end}");
            }

            Start = start;
            End = end;
        }

        public bool Equals(DownloadRange other)
        {
            return Start == other.Start && End == other.End;
        }
        public override bool Equals(object obj)
        {
            return obj is DownloadRange other && Equals(other);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return (Start.GetHashCode() * 397) ^ End.GetHashCode();
            }
        }
        public override string ToString()
        {
            return $"{Start}/{End}";
        }

        public static bool operator ==(DownloadRange x, DownloadRange y)
        {
            return x.Equals(y);
        }
        public static bool operator !=(DownloadRange x, DownloadRange y)
        {
            return !x.Equals(y);
        }
    }
}
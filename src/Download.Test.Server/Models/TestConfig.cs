#nullable enable
using System;

namespace Download.Test.Server.Models
{
    public class TestConfig
    {
        public bool SupportRanges { get; set; }
        public TimeSpan? Timeout { get; set; }
        public string? Etag { get; set; }
        public string? FileName { get; set; }
        public DateTimeOffset? LastModified { get; set; }
    }
}
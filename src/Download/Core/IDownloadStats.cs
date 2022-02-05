using System;

namespace Download.Core
{
    public interface IDownloadStats
    {
        double? Progress { get; }
        double CurrentRate { get; }
        double AverageRate { get; }
        double MinRate { get; }
        double MaxRate { get; }

        DateTime Start { get; }
        DateTime End { get; }

        TimeSpan Duration { get; }
        TimeSpan Elapsed { get; }
        TimeSpan Remaining { get; }
    }

}

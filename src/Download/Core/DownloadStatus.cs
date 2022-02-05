namespace Download.Core
{
    public enum DownloadStatus
    {
        Created,
        Started,
        FetchingInfo,
        Downloading,
        Canceled,
        Failed,
        Completed,
        Retrying
    }
}
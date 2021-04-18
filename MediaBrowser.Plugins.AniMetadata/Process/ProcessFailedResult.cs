namespace Jellyfin.AniDbMetaStructure.Process
{
    internal class ProcessFailedResult
    {
        public ProcessFailedResult(string sourceName, string mediaItemName, IMediaItemType mediaItemType, string reason)
        {
            this.SourceName = sourceName;
            this.MediaItemName = mediaItemName;
            this.MediaItemType = mediaItemType;
            this.Reason = reason;
        }

        public string SourceName { get; }

        public string MediaItemName { get; }

        public IMediaItemType MediaItemType { get; }

        public string Reason { get; }

        public static ProcessFailedResult Append(ProcessFailedResult a, ProcessFailedResult b)
        {
            return new ProcessFailedResult(a.SourceName, a.MediaItemName, a.MediaItemType, $@"{a.Reason}
{b.Reason}");
        }
    }
}
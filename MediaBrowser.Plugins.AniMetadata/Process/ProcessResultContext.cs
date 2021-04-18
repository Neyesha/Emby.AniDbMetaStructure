namespace Jellyfin.AniDbMetaStructure.Process
{
    internal class ProcessResultContext
    {
        private readonly string mediaItemName;
        private readonly IMediaItemType mediaItemType;
        private readonly string sourceName;

        public ProcessResultContext(string sourceName, string mediaItemName, IMediaItemType mediaItemType)
        {
            this.sourceName = sourceName;
            this.mediaItemName = mediaItemName;
            this.mediaItemType = mediaItemType;
        }

        public ProcessFailedResult Failed(string reason)
        {
            return new ProcessFailedResult(this.sourceName, this.mediaItemName, this.mediaItemType, reason);
        }
    }
}
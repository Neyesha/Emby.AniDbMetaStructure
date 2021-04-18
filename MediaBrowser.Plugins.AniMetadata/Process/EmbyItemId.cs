namespace Jellyfin.AniDbMetaStructure.Process
{
    internal class EmbyItemId
    {
        public EmbyItemId(IMediaItemType itemType, string sourceName, int id)
        {
            this.ItemType = itemType;
            this.SourceName = sourceName;
            this.Id = id;
        }

        public IMediaItemType ItemType { get; }

        public string SourceName { get; }

        public int Id { get; }
    }
}
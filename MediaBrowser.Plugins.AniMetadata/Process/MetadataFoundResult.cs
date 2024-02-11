using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;

namespace Jellyfin.AniDbMetaStructure.Process
{
    internal class MetadataFoundResult<TJellyfinItem> : IMetadataFoundResult<TJellyfinItem> where TJellyfinItem : BaseItem
    {
        public MetadataFoundResult(IMediaItem mediaItem, MetadataResult<TJellyfinItem> metadataResult)
        {
            this.JellyfinMetadataResult = metadataResult;
            this.MediaItem = mediaItem;
        }

        /// <summary>
        ///     The item this result is for
        /// </summary>
        public IMediaItem MediaItem { get; }

        /// <summary>
        ///     The result that can be passed back to Jellyfin cast to the expected type
        /// </summary>
        public MetadataResult<TJellyfinItem> JellyfinMetadataResult { get; }
    }
}
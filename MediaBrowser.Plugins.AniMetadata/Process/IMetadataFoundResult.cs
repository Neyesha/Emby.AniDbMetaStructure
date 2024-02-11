using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;

namespace Jellyfin.AniDbMetaStructure.Process
{
    internal interface IMetadataFoundResult<TJellyfinItem> where TJellyfinItem : BaseItem
    {
        /// <summary>
        ///     The item this result is for
        /// </summary>
        IMediaItem MediaItem { get; }

        /// <summary>
        ///     The result that can be passed back to Jellyfin cast to the expected type
        /// </summary>
        MetadataResult<TJellyfinItem> JellyfinMetadataResult { get; }
    }
}
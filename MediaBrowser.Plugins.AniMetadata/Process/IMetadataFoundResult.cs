using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;

namespace Jellyfin.AniDbMetaStructure.Process
{
    internal interface IMetadataFoundResult<TEmbyItem> where TEmbyItem : BaseItem
    {
        /// <summary>
        ///     The item this result is for
        /// </summary>
        IMediaItem MediaItem { get; }

        /// <summary>
        ///     The result that can be passed back to Emby cast to the expected type
        /// </summary>
        MetadataResult<TEmbyItem> EmbyMetadataResult { get; }
    }
}
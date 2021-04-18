using System.Collections.Generic;
using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;

namespace Jellyfin.AniDbMetaStructure.Process
{
    /// <summary>
    ///     Coordinates the overall process of retrieving metadata from Emby identification data
    /// </summary>
    internal interface IMediaItemProcessor
    {
        /// <summary>
        ///     Get the result containing the metadata for this media item, if any could be found
        /// </summary>
        Task<Either<ProcessFailedResult, IMetadataFoundResult<TEmbyItem>>> GetResultAsync<TEmbyItem>(
            ItemLookupInfo embyInfo, IMediaItemType<TEmbyItem> itemType, IEnumerable<EmbyItemId> parentIds)
            where TEmbyItem : BaseItem;
    }
}
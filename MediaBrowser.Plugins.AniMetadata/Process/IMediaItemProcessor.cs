using System.Collections.Generic;
using System.Threading.Tasks;
using LanguageExt;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;

namespace Jellyfin.AniDbMetaStructure.Process
{
    /// <summary>
    ///     Coordinates the overall process of retrieving metadata from Jellyfin identification data
    /// </summary>
    internal interface IMediaItemProcessor
    {
        /// <summary>
        ///     Get the result containing the metadata for this media item, if any could be found
        /// </summary>
        Task<Either<ProcessFailedResult, IMetadataFoundResult<TJellyfinItem>>> GetResultAsync<TJellyfinItem>(
            ItemLookupInfo JellyfinInfo, IMediaItemType<TJellyfinItem> itemType, IEnumerable<JellyfinItemId> parentIds)
            where TJellyfinItem : BaseItem;
    }
}
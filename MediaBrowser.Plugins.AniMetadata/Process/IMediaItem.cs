using System.Collections.Generic;
using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.Process
{
    internal interface IMediaItem
    {
        /// <summary>
        ///     The data for this item supplied by Emby at the start of the process
        /// </summary>
        IJellyfinItemData EmbyData { get; }
        
        IMediaItemType ItemType { get; }

        /// <summary>
        ///     Get the data from a particular source
        /// </summary>
        Option<ISourceData> GetDataFromSource(ISource source);

        /// <summary>
        /// Get all the source data attached to this media item
        /// </summary>
        IEnumerable<ISourceData> GetAllSourceData();

        /// <summary>
        ///     Add metadata from a source to the collection of metadata for this item
        /// </summary>
        /// <param name="sourceData">The metadata to add</param>
        /// <returns>A new <see cref="IMediaItem" /> with the metadata added</returns>
        Either<ProcessFailedResult, IMediaItem> AddData(ISourceData sourceData);
    }
}
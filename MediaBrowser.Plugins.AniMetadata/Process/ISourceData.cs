using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.Process
{
    /// <summary>
    ///     The metadata for an item from a single source
    /// </summary>
    internal interface ISourceData
    {
        /// <summary>
        ///     The source of the metadata
        /// </summary>
        ISource Source { get; }

        /// <summary>
        ///     The Id of the item in this source
        /// </summary>
        Option<int> Id { get; }

        /// <summary>
        ///     The identity of the media item according to this source,
        ///     and which was used to retrieve this metadata
        /// </summary>
        IItemIdentifier Identifier { get; }

        IMediaItemType ItemType { get; }

        /// <summary>
        ///     The metadata about the item from this source
        /// </summary>
        object Data { get; }

        /// <summary>
        ///     Get the metadata about the item from this source
        /// </summary>
        /// <typeparam name="TRequestedData">The type of data returned by this source</typeparam>
        Option<TRequestedData> GetData<TRequestedData>() where TRequestedData : class;
    }
}
using Jellyfin.AniDbMetaStructure.Process.Sources;
using Jellyfin.AniDbMetaStructure.SourceDataLoaders;
using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.Process
{
    /// <summary>
    ///     A source of metadata
    /// </summary>
    internal interface ISource
    {
        SourceName Name { get; }

        Either<ProcessFailedResult, IEmbySourceDataLoader> GetEmbySourceDataLoader(IMediaItemType mediaItemType);

        bool ShouldUsePlaceholderSourceData(IMediaItemType mediaItemType);
    }
}
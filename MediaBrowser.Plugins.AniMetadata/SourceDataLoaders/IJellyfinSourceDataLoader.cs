using System.Threading.Tasks;
using Jellyfin.AniDbMetaStructure.Process;
using Jellyfin.AniDbMetaStructure.Process.Sources;
using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.SourceDataLoaders
{
    internal interface IJellyfinSourceDataLoader
    {
        SourceName SourceName { get; }

        bool CanLoadFrom(IMediaItemType mediaItemType);

        Task<Either<ProcessFailedResult, ISourceData>> LoadFrom(IJellyfinItemData embyItemData);
    }
}
using System.Threading.Tasks;
using Jellyfin.AniDbMetaStructure.Process;
using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.SourceDataLoaders
{
    internal interface ISourceDataLoader
    {
        bool CanLoadFrom(object sourceData);

        Task<Either<ProcessFailedResult, ISourceData>> LoadFrom(IMediaItem mediaItem, object sourceData);
    }
}
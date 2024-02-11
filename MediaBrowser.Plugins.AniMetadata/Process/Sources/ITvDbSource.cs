using System.Threading.Tasks;
using Jellyfin.AniDbMetaStructure.TvDb.Data;
using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.Process.Sources
{
    internal interface ITvDbSource : ISource
    {
        Task<Either<ProcessFailedResult, TvDbSeriesData>> GetSeriesData(IJellyfinItemData JellyfinItemData,
            ProcessResultContext resultContext);

        Task<Either<ProcessFailedResult, TvDbSeriesData>> GetSeriesData(int tvDbSeriesId,
            ProcessResultContext resultContext);
    }
}
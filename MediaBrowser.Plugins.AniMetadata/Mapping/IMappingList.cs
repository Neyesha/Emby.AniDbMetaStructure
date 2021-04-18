using System.Collections.Generic;
using System.Threading.Tasks;
using Jellyfin.AniDbMetaStructure.Process;
using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.Mapping
{
    internal interface IMappingList
    {
        Task<Either<ProcessFailedResult, ISeriesMapping>> GetSeriesMappingFromAniDb(int aniDbSeriesId,
            ProcessResultContext resultContext);

        Task<Either<ProcessFailedResult, IEnumerable<ISeriesMapping>>> GetSeriesMappingsFromTvDb(int tvDbSeriesId,
            ProcessResultContext resultContext);
    }
}
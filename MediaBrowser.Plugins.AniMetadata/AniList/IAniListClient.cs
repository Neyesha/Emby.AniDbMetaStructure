using System.Collections.Generic;
using System.Threading.Tasks;
using Jellyfin.AniDbMetaStructure.AniList.Data;
using Jellyfin.AniDbMetaStructure.Process;
using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.AniList
{
    internal interface IAniListClient
    {
        Task<Either<ProcessFailedResult, IEnumerable<AniListSeriesData>>> FindSeriesAsync(string title,
            ProcessResultContext resultContext);
    }
}
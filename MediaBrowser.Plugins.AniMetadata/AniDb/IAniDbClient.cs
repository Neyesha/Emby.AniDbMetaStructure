using System.Collections.Generic;
using System.Threading.Tasks;
using Jellyfin.AniDbMetaStructure.AniDb.Seiyuu;
using Jellyfin.AniDbMetaStructure.AniDb.SeriesData;
using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.AniDb
{
    public interface IAniDbClient
    {
        Task<Option<AniDbSeriesData>> FindSeriesAsync(string title);

        Task<Option<AniDbSeriesData>> GetSeriesAsync(string aniDbSeriesIdString);

        Task<Option<AniDbSeriesData>> GetSeriesAsync(int aniDbSeriesId);

        IEnumerable<SeiyuuData> FindSeiyuu(string name);

        Option<SeiyuuData> GetSeiyuu(int seiyuuId);
    }
}
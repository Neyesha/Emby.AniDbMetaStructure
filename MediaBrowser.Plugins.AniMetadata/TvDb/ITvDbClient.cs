using System.Collections.Generic;
using System.Threading.Tasks;
using Jellyfin.AniDbMetaStructure.TvDb.Data;
using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.TvDb
{
    public interface ITvDbClient
    {
        Task<Option<TvDbSeriesData>> GetSeriesAsync(int tvDbSeriesId);

        Task<Option<TvDbSeriesData>> FindSeriesAsync(string seriesName);

        Task<Option<List<TvDbEpisodeData>>> GetEpisodesAsync(int tvDbSeriesId);

        Task<Option<TvDbEpisodeData>> GetEpisodeAsync(int tvDbSeriesId, int seasonIndex,
            int episodeIndex);

        Task<Option<TvDbEpisodeData>> GetEpisodeAsync(int tvDbSeriesId, int absoluteEpisodeIndex);
    }
}
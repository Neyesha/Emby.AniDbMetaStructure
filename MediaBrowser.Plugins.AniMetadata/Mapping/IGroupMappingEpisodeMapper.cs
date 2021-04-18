using Jellyfin.AniDbMetaStructure.AniDb.SeriesData;
using Jellyfin.AniDbMetaStructure.TvDb.Data;
using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.Mapping
{
    internal interface IGroupMappingEpisodeMapper
    {
        OptionAsync<TvDbEpisodeData> MapAniDbEpisodeAsync(int aniDbEpisodeIndex,
            EpisodeGroupMapping episodeGroupMapping, int tvDbSeriesId);

        OptionAsync<AniDbEpisodeData> MapTvDbEpisodeAsync(int tvDbEpisodeIndex,
            EpisodeGroupMapping episodeGroupMapping, int aniDbSeriesId);
    }
}
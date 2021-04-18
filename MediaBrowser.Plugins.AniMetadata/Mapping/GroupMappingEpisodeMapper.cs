using Jellyfin.AniDbMetaStructure.AniDb;
using Jellyfin.AniDbMetaStructure.AniDb.SeriesData;
using Jellyfin.AniDbMetaStructure.TvDb;
using Jellyfin.AniDbMetaStructure.TvDb.Data;
using LanguageExt;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Jellyfin.AniDbMetaStructure.Mapping
{
    /// <summary>
    ///     Maps an AniDb episode to a TvDb episode (and vice versa) using an <see cref="EpisodeGroupMapping" />
    /// </summary>
    internal class GroupMappingEpisodeMapper : IGroupMappingEpisodeMapper
    {
        private readonly IAniDbClient aniDbClient;
        private readonly ILogger logger;
        private readonly ITvDbClient tvDbClient;

        public GroupMappingEpisodeMapper(ITvDbClient tvDbClient, IAniDbClient aniDbClient, ILogger logger)
        {
            this.logger = logger;
            this.tvDbClient = tvDbClient;
            this.aniDbClient = aniDbClient;
        }

        public OptionAsync<TvDbEpisodeData> MapAniDbEpisodeAsync(int aniDbEpisodeIndex,
            EpisodeGroupMapping episodeGroupMapping, int tvDbSeriesId)
        {
            var episodeMapping =
                episodeGroupMapping.EpisodeMappings?.FirstOrDefault(m => m.AniDbEpisodeIndex == aniDbEpisodeIndex);

            int tvDbEpisodeIndex =
                GetTvDbEpisodeIndex(aniDbEpisodeIndex, episodeGroupMapping.TvDbEpisodeIndexOffset,
                    episodeMapping);

            return GetTvDbEpisodeAsync(tvDbSeriesId, episodeGroupMapping.TvDbSeasonIndex, tvDbEpisodeIndex)
                .Map(tvDbEpisodeData =>
                {
                    this.logger.LogDebug($"Found mapped TvDb episode: {tvDbEpisodeData}");

                    return tvDbEpisodeData;
                });
        }

        public OptionAsync<AniDbEpisodeData> MapTvDbEpisodeAsync(int tvDbEpisodeIndex,
            EpisodeGroupMapping episodeGroupMapping, int aniDbSeriesId)
        {
            var episodeMapping = GetTvDbEpisodeMapping(tvDbEpisodeIndex, episodeGroupMapping);

            int aniDbEpisodeIndex =
                GetAniDbEpisodeIndex(tvDbEpisodeIndex, episodeGroupMapping.TvDbEpisodeIndexOffset,
                    episodeMapping);

            return GetAniDbEpisodeAsync(aniDbSeriesId, episodeGroupMapping.AniDbSeasonIndex, aniDbEpisodeIndex)
                .Map(aniDbEpisodeData =>
                {
                    this.logger.LogDebug(
                        $"Found mapped AniDb episode: {aniDbEpisodeData}");

                    return aniDbEpisodeData;
                });
        }

        private Option<EpisodeMapping> GetTvDbEpisodeMapping(int tvDbEpisodeIndex,
            EpisodeGroupMapping episodeGroupMapping)
        {
            var episodeMapping =
                episodeGroupMapping.EpisodeMappings?.FirstOrDefault(m => m.TvDbEpisodeIndex == tvDbEpisodeIndex);

            return episodeMapping;
        }

        private int GetTvDbEpisodeIndex(int aniDbEpisodeIndex, int tvDbEpisodeIndexOffset,
            Option<EpisodeMapping> episodeMapping)
        {
            return episodeMapping.Match(m => m.TvDbEpisodeIndex,
                () => aniDbEpisodeIndex + tvDbEpisodeIndexOffset);
        }

        private int GetAniDbEpisodeIndex(int tvDbEpisodeIndex, int tvDbEpisodeIndexOffset,
            Option<EpisodeMapping> episodeMapping)
        {
            return episodeMapping.Match(m => m.AniDbEpisodeIndex,
                () => tvDbEpisodeIndex - tvDbEpisodeIndexOffset);
        }

        private OptionAsync<TvDbEpisodeData> GetTvDbEpisodeAsync(int tvDbSeriesId, int seasonIndex,
            int episodeIndex)
        {
            return this.tvDbClient.GetEpisodesAsync(tvDbSeriesId)
                .MapAsync(episodes =>
                    episodes.Find(e => e.AiredSeason == seasonIndex && e.AiredEpisodeNumber == episodeIndex));
        }

        private OptionAsync<AniDbEpisodeData> GetAniDbEpisodeAsync(int aniDbSeriesId, int seasonIndex,
            int episodeIndex)
        {
            return this.aniDbClient.GetSeriesAsync(aniDbSeriesId)
                .BindAsync(aniDbSeries =>
                    aniDbSeries.Episodes.Find(e =>
                        e.EpisodeNumber.SeasonNumber == seasonIndex && e.EpisodeNumber.Number == episodeIndex));
        }
    }
}
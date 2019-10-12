﻿using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;

namespace MediaBrowser.Plugins.AniMetadata.Mapping
{
    internal class EpisodeMapper : IEpisodeMapper
    {
        private readonly IDefaultSeasonEpisodeMapper defaultSeasonEpisodeMapper;
        private readonly IGroupMappingEpisodeMapper groupMappingEpisodeMapper;

        public EpisodeMapper(IDefaultSeasonEpisodeMapper defaultSeasonEpisodeMapper,
            IGroupMappingEpisodeMapper groupMappingEpisodeMapper)
        {
            this.defaultSeasonEpisodeMapper = defaultSeasonEpisodeMapper;
            this.groupMappingEpisodeMapper = groupMappingEpisodeMapper;
        }

        public OptionAsync<TvDbEpisodeData> MapAniDbEpisodeAsync(int aniDbEpisodeIndex,
            ISeriesMapping seriesMapping, Option<EpisodeGroupMapping> episodeGroupMapping)
        {
            return episodeGroupMapping.Match(
                m => seriesMapping.Ids.TvDbSeriesId.BindAsync(
                    tvDbSeriesId =>
                        this.groupMappingEpisodeMapper.MapAniDbEpisodeAsync(aniDbEpisodeIndex, m,
                            tvDbSeriesId)),
                () => this.defaultSeasonEpisodeMapper.MapEpisodeAsync(aniDbEpisodeIndex,
                        seriesMapping)
                    .ToAsync());
        }

        public OptionAsync<AniDbEpisodeData> MapTvDbEpisodeAsync(int tvDbEpisodeIndex,
            ISeriesMapping seriesMapping, Option<EpisodeGroupMapping> episodeGroupMapping)
        {
            return episodeGroupMapping.BindAsync(
                m => this.groupMappingEpisodeMapper.MapTvDbEpisodeAsync(tvDbEpisodeIndex, m,
                    seriesMapping.Ids.AniDbSeriesId));
        }
    }
}
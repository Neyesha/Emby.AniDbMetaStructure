using Jellyfin.AniDbMetaStructure.TvDb;
using Jellyfin.AniDbMetaStructure.TvDb.Data;
using LanguageExt;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Jellyfin.AniDbMetaStructure.Mapping
{
    /// <summary>
    ///     Maps an AniDb episode to a TvDb episode using a default season
    /// </summary>
    internal class DefaultSeasonEpisodeMapper : IDefaultSeasonEpisodeMapper
    {
        private readonly ILogger logger;
        private readonly ITvDbClient tvDbClient;

        public DefaultSeasonEpisodeMapper(ITvDbClient tvDbClient, ILogger logger)
        {
            this.logger = logger;
            this.tvDbClient = tvDbClient;
        }

        public Task<Option<TvDbEpisodeData>> MapEpisodeAsync(int aniDbEpisodeIndex,
            ISeriesMapping seriesMapping)
        {
            return seriesMapping.Ids.TvDbSeriesId.MatchAsync(tvDbSeriesId =>
                    seriesMapping.DefaultTvDbSeason.Match(
                        tvDbSeason =>
                            MapEpisodeWithDefaultSeasonAsync(aniDbEpisodeIndex, tvDbSeriesId,
                                seriesMapping.DefaultTvDbEpisodeIndexOffset, tvDbSeason.Index),
                        absoluteTvDbSeason =>
                            MapEpisodeViaAbsoluteEpisodeIndexAsync(aniDbEpisodeIndex, tvDbSeriesId)),
                () =>
                {
                    this.logger.LogDebug($"Failed to map AniDb episode {aniDbEpisodeIndex}");
                    return Option<TvDbEpisodeData>.None;
                });
        }

        private async Task<Option<TvDbEpisodeData>> MapEpisodeWithDefaultSeasonAsync(int aniDbEpisodeIndex,
            int tvDbSeriesId, int defaultTvDbEpisodeIndexOffset, int defaultTvDbSeasonIndex)
        {
            int tvDbEpisodeIndex = aniDbEpisodeIndex + defaultTvDbEpisodeIndexOffset;

            var tvDbEpisodeData =
                await this.tvDbClient.GetEpisodeAsync(tvDbSeriesId, defaultTvDbSeasonIndex, tvDbEpisodeIndex);

            return tvDbEpisodeData.Match(d =>
            {
                this.logger.LogDebug(
                    $"Found mapped TvDb episode: {tvDbEpisodeData}");

                return d;
            }, () => Option<TvDbEpisodeData>.None);
        }

        private Task<Option<TvDbEpisodeData>> MapEpisodeViaAbsoluteEpisodeIndexAsync(int aniDbEpisodeIndex,
            int tvDbSeriesId)
        {
            return this.tvDbClient.GetEpisodeAsync(tvDbSeriesId, aniDbEpisodeIndex)
                .MatchAsync(tvDbEpisodeData =>
                {
                    this.logger.LogDebug(
                        $"Found mapped TvDb episode via absolute episode index: {tvDbEpisodeData}");

                    return tvDbEpisodeData;
                }, () => Option<TvDbEpisodeData>.None);
        }
    }
}
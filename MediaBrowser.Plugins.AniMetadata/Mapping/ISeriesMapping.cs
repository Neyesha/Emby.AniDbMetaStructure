using System.Collections.Generic;
using Jellyfin.AniDbMetaStructure.AniDb.SeriesData;
using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.Mapping
{
    public interface ISeriesMapping
    {
        int DefaultTvDbEpisodeIndexOffset { get; }

        Either<AbsoluteTvDbSeason, TvDbSeason> DefaultTvDbSeason { get; }

        IEnumerable<EpisodeGroupMapping> EpisodeGroupMappings { get; }

        SeriesIds Ids { get; }

        IEnumerable<SpecialEpisodePosition> SpecialEpisodePositions { get; }

        Option<EpisodeGroupMapping> GetEpisodeGroupMapping(IAniDbEpisodeNumber aniDbEpisodeNumber);

        Option<EpisodeGroupMapping> GetEpisodeGroupMapping(int tvDbEpisodeIndex, int tvDbSeasonIndex);

        Option<SpecialEpisodePosition> GetSpecialEpisodePosition(IAniDbEpisodeNumber aniDbEpisodeNumber);
    }
}
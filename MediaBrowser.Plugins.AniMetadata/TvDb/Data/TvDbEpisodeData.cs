using System;
using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.TvDb.Data
{
    public class TvDbEpisodeData : TvDbEpisodeSummaryData
    {
        public TvDbEpisodeData(int id, string episodeName, Option<long> absoluteNumber, int airedEpisodeNumber,
            int airedSeason, int lastUpdated, Option<DateTime> firstAired, string overview, float siteRating,
            int siteRatingCount) : base(id, episodeName, absoluteNumber, airedEpisodeNumber, airedSeason, lastUpdated,
            firstAired, overview)
        {
            this.SiteRating = siteRating;
            this.SiteRatingCount = siteRatingCount;
        }

        public float SiteRating { get; }

        public int SiteRatingCount { get; }

        public override string ToString()
        {
            return $"Season {this.AiredSeason} Episode {this.AiredEpisodeNumber} '{this.EpisodeName}'";
        }
    }
}
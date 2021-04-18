using System;
using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.TvDb.Data
{
    public class TvDbEpisodeSummaryData
    {
        public TvDbEpisodeSummaryData(int id, string episodeName, Option<long> absoluteNumber, int airedEpisodeNumber,
            int airedSeason, int lastUpdated, Option<DateTime> firstAired, string overview)
        {
            this.Id = id;
            this.EpisodeName = episodeName;
            this.AbsoluteNumber = absoluteNumber;
            this.AiredEpisodeNumber = airedEpisodeNumber;
            this.AiredSeason = airedSeason;
            this.LastUpdated = lastUpdated;
            this.FirstAired = firstAired;
            this.Overview = overview;
        }

        public int Id { get; }

        public string EpisodeName { get; }

        public Option<long> AbsoluteNumber { get; }

        public int AiredEpisodeNumber { get; }

        public int AiredSeason { get; }

        public int LastUpdated { get; }

        public Option<DateTime> FirstAired { get; }

        public string Overview { get; }
    }
}
using System;
using System.Collections.Generic;
using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.TvDb.Data
{
    public class TvDbSeriesData
    {
        public TvDbSeriesData(int id, string seriesName, Option<DateTime> firstAired, string network, int runtime,
            Option<AirDay> airsDayOfWeek, string airsTime, float siteRating, IEnumerable<string> aliases,
            IEnumerable<string> genre, string overview)
        {
            this.Id = id;
            this.SeriesName = seriesName;
            this.FirstAired = firstAired;
            this.Network = network;
            this.Runtime = runtime;
            this.AirsDayOfWeek = airsDayOfWeek;
            this.AirsTime = airsTime;
            this.SiteRating = siteRating;
            this.Aliases = aliases ?? new List<string>();
            this.Genres = genre ?? new List<string>();
            this.Overview = overview;
        }

        public int Id { get; }

        public string SeriesName { get; }

        public Option<DateTime> FirstAired { get; }

        public string Network { get; }

        public int Runtime { get; }

        public Option<AirDay> AirsDayOfWeek { get; }

        public string AirsTime { get; }

        public float SiteRating { get; }

        public IEnumerable<string> Aliases { get; }

        public IEnumerable<string> Genres { get; }

        public string Overview { get; }
    }
}
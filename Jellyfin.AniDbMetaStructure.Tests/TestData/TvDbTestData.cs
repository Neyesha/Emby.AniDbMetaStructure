using System;
using System.Collections.Generic;
using Jellyfin.AniDbMetaStructure.TvDb;
using Jellyfin.AniDbMetaStructure.TvDb.Data;
using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.Tests.TestData
{
    public static class TvDbTestData
    {
        public static TvDbSeriesData Series(int id, string name = "")
        {
            return new TvDbSeriesData(id, name, new DateTime(2017, 1, 2), string.Empty, 0, AirDay.Friday, string.Empty, 0,
                new List<string>(), new List<string>(), string.Empty);
        }

        public static TvDbEpisodeData Episode(int id, int episodeIndex = 0, int seasonIndex = 0,
            long? absoluteEpisodeIndex = null, string name = "")
        {
            return new TvDbEpisodeData(id, name, absoluteEpisodeIndex.ToOption(), episodeIndex, seasonIndex, 0,
                new DateTime(2017, 1, 2), string.Empty, 0, 0);
        }
    }
}
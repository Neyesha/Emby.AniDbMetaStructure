﻿using Functional.Maybe;

namespace MediaBrowser.Plugins.Anime.TvDb.Data
{
    public class TvDbEpisodeData
    {
        public TvDbEpisodeData(int id, string episodeName, int? absoluteNumber, int airedEpisodeNumber, int airedSeason,
            int lastUpdated)
        {
            Id = id;
            EpisodeName = episodeName;
            AbsoluteNumber = absoluteNumber.ToMaybe();
            AiredEpisodeNumber = airedEpisodeNumber;
            AiredSeason = airedSeason;
            LastUpdated = lastUpdated;
        }

        public int Id { get; }

        public string EpisodeName { get; }

        public Maybe<int> AbsoluteNumber { get; }

        public int AiredEpisodeNumber { get; }

        public int AiredSeason { get; }

        public int LastUpdated { get; }
    }
}
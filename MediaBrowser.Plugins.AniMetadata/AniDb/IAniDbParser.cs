using System.Collections.Generic;
using Jellyfin.AniDbMetaStructure.AniDb.SeriesData;
using MediaBrowser.Controller.Entities;

namespace Jellyfin.AniDbMetaStructure.AniDb
{
    internal interface IAniDbParser
    {
        string FormatDescription(string description);

        IEnumerable<string> GetGenres(AniDbSeriesData aniDbSeriesData, int maxGenres, bool addAnimeGenre);

        IEnumerable<string> GetStudios(AniDbSeriesData aniDbSeriesData);

        IEnumerable<string> GetTags(AniDbSeriesData aniDbSeriesData, int maxGenres, bool addAnimeGenre);

        IEnumerable<PersonInfo> GetPeople(AniDbSeriesData aniDbSeriesData);
    }
}
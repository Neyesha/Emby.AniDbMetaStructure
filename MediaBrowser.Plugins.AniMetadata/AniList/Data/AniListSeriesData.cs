using System.Collections.Generic;
using LanguageExt;
using Newtonsoft.Json;

namespace Jellyfin.AniDbMetaStructure.AniList.Data
{
    internal class AniListSeriesData
    {
        public int Id { get; set; }

        [JsonProperty("idMal")]
        public Option<long> MyAnimeListId { get; set; }

        [JsonProperty("type")]
        public AniListSeriesType SeriesType { get; set; }

        public AniListTitleData Title { get; set; }

        public AniListMediaFormat Format { get; set; }

        public AniListFuzzyDate StartDate { get; set; }

        public AniListFuzzyDate EndDate { get; set; }

        public string Description { get; set; }

        public IEnumerable<string> Genres { get; set; }

        public double AverageScore { get; set; }

        public int Popularity { get; set; }

        [JsonProperty("siteUrl")]
        public string AniListUrl { get; set; }

        public AniListImageUrlData CoverImage { get; set; }

        public AniListImageUrlData BannerImage { get; set; }

        [JsonProperty("duration")]
        public Option<long> EpisodeDurationMinutes { get; set; }

        [JsonProperty("status")]
        public AniListAiringStatus AiringStatus { get; set; }

        public GraphQlEdge<AniListStudioData> Studios { get; set; }

        public GraphQlEdge<AniListStaffData> Staff { get; set; }

        public GraphQlEdge<AniListCharacterData> Characters { get; set; }
    }
}
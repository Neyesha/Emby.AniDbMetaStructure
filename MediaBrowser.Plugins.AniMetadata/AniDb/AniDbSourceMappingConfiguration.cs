using Jellyfin.AniDbMetaStructure.AniDb.SeriesData;
using Jellyfin.AniDbMetaStructure.Configuration;
using Jellyfin.AniDbMetaStructure.Process;
using Jellyfin.AniDbMetaStructure.Process.Sources;
using Jellyfin.AniDbMetaStructure.PropertyMapping;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Jellyfin.AniDbMetaStructure.AniDb
{
    internal class AniDbSourceMappingConfiguration : ISourceMappingConfiguration
    {
        private readonly IAniDbParser aniDbParser;
        private readonly IAniDbTitleSelector titleSelector;

        public AniDbSourceMappingConfiguration(IAniDbParser aniDbParser, IAniDbTitleSelector titleSelector)
        {
            this.aniDbParser = aniDbParser;
            this.titleSelector = titleSelector;
        }

        public IEnumerable<PropertyMappingDefinition> GetSeriesMappingDefinitions()
        {
            return GetSeriesMappings(0, false, false, TitleType.Localized, string.Empty)
                .Select(m => new PropertyMappingDefinition(m.FriendlyName, m.SourceName, m.TargetPropertyName));
        }

        public IEnumerable<IPropertyMapping> GetSeriesMappings(int maxGenres, bool addAnimeGenre,
            bool moveExcessGenresToTags, TitleType preferredTitleType, string metadataLanguage)
        {
            return new IPropertyMapping[]
            {
                MapSeries("Name", t => t.Item.Name,
                    (s, t) => t.Item.Name = SelectTitle(s, preferredTitleType, metadataLanguage)),
                MapSeries("Release date", t => t.Item.PremiereDate, (s, t) => t.Item.PremiereDate = s.StartDate,
                    (s, t) => s.StartDate.HasValue),
                MapSeries("End date", t => t.Item.EndDate, (s, t) => t.Item.EndDate = s.EndDate,
                    (s, t) => s.EndDate.HasValue),
                MapSeries("Community rating", t => t.Item.CommunityRating,
                    (s, t) => t.Item.CommunityRating = s.Ratings?.OfType<PermanentRatingData>().Single().Value,
                    (s, t) => s.Ratings?.OfType<PermanentRatingData>().Count() == 1),
                MapSeries("Overview", t => t.Item.Overview,
                    (s, t) => t.Item.Overview = this.aniDbParser.FormatDescription(s.Description),
                    (s, t) => !string.IsNullOrWhiteSpace(s.Description)),
                MapSeries("Studios", t => t.Item.Studios,
                    (s, t) => t.Item.Studios = this.aniDbParser.GetStudios(s).ToArray()),
                MapSeries("Genres", t => t.Item.Genres,
                    (s, t) => t.Item.Genres = AddGenres(s, t.Item.Genres, maxGenres, addAnimeGenre)),
                MapSeries("Tags", t => t.Item.Tags,
                    (s, t) => t.Item.Tags = this.aniDbParser.GetTags(s, maxGenres, addAnimeGenre).ToArray()),
                MapSeries("People", t => t.People, (s, t) => t.People = this.aniDbParser.GetPeople(s).ToList())
            };
        }

        public IEnumerable<PropertyMappingDefinition> GetSeasonMappingDefinitions()
        {
            return GetSeasonMappings(0, false, TitleType.Localized, string.Empty)
                .Select(m => new PropertyMappingDefinition(m.FriendlyName, m.SourceName, m.TargetPropertyName));
        }

        public IEnumerable<IPropertyMapping> GetSeasonMappings(int maxGenres, bool addAnimeGenre,
            TitleType preferredTitleType, string metadataLanguage)
        {
            return new IPropertyMapping[]
            {
                MapSeasonIdentifier("Name", t => t.Item.Name,
                    (s, t) => t.Item.Name = s.Identifier.Name,
                    (s, t) => !string.IsNullOrWhiteSpace(s.Identifier.Name)),
                MapSeason("Release date", t => t.Item.PremiereDate, (s, t) => t.Item.PremiereDate = s.StartDate,
                    (s, t) => s.StartDate.HasValue),
                MapSeason("End date", t => t.Item.EndDate, (s, t) => t.Item.EndDate = s.EndDate,
                    (s, t) => s.EndDate.HasValue),
                MapSeason("Community rating", t => t.Item.CommunityRating,
                    (s, t) => t.Item.CommunityRating = s.Ratings?.OfType<PermanentRatingData>().Single().Value,
                    (s, t) => s.Ratings?.OfType<PermanentRatingData>().Count() == 1),
                MapSeason("Overview", t => t.Item.Overview,
                    (s, t) => t.Item.Overview = this.aniDbParser.FormatDescription(s.Description),
                    (s, t) => !string.IsNullOrWhiteSpace(s.Description)),
                MapSeason("Studios", t => t.Item.Studios,
                    (s, t) => t.Item.Studios = this.aniDbParser.GetStudios(s).ToArray()),
                MapSeason("Genres", t => t.Item.Genres,
                    (s, t) => t.Item.Genres = AddGenres(s, t.Item.Genres, maxGenres, addAnimeGenre)),
                MapSeason("Tags", t => t.Item.Tags,
                    (s, t) => t.Item.Tags = this.aniDbParser.GetTags(s, maxGenres, addAnimeGenre).ToArray()),
                MapSeason("Name", t => t.Item.Name,
                    (s, t) => t.Item.Name = SelectTitle(s, preferredTitleType, metadataLanguage),
                    (s, t) => s.Titles.Any())
            };
        }

        public IEnumerable<PropertyMappingDefinition> GetEpisodeMappingDefinitions()
        {
            return GetEpisodeMappings(0, false, false, TitleType.Localized, string.Empty)
                .Select(m => new PropertyMappingDefinition(m.FriendlyName, m.SourceName, m.TargetPropertyName));
        }

        public IEnumerable<IPropertyMapping> GetEpisodeMappings(int maxGenres, bool addAnimeGenre,
            bool moveExcessGenresToTags, TitleType preferredTitleType, string metadataLanguage)
        {
            return new IPropertyMapping[]
            {
                MapEpisode("Name", t => t.Item.Name,
                    (s, t) => t.Item.Name = SelectTitle(s, preferredTitleType, metadataLanguage)),
                MapEpisode("Release date", t => t.Item.PremiereDate, (s, t) => t.Item.PremiereDate = s.AirDate),
                MapEpisode("Runtime", t => t.Item.RunTimeTicks,
                    (s, t) => t.Item.RunTimeTicks = new TimeSpan(0, s.TotalMinutes, 0).Ticks,
                    (s, t) => s.TotalMinutes > 0),
                MapEpisode("Community rating", t => t.Item.CommunityRating,
                    (s, t) => t.Item.CommunityRating = s.Rating?.Rating,
                    (s, t) => s.Rating?.Rating > 0),
                MapEpisode("Overview", t => t.Item.Overview, (s, t) => t.Item.Overview = s.Summary,
                    (s, t) => !string.IsNullOrWhiteSpace(s.Summary)),
                MapEpisodeFromSeriesData("Studios", t => t.Item.Studios,
                    (s, t) => t.Item.Studios = this.aniDbParser.GetStudios(s).ToArray()),
                MapEpisodeFromSeriesData("Genres", t => t.Item.Genres,
                    (s, t) =>  t.Item.Genres = AddGenres(s, t.Item.Genres, maxGenres, addAnimeGenre)),
                MapEpisodeFromSeriesData("Tags", t => t.Item.Tags,
                    (s, t) => t.Item.Tags = this.aniDbParser.GetTags(s, maxGenres, addAnimeGenre).ToArray()),
                MapEpisodeFromSeriesData("People", t => t.People,
                    (s, t) => t.People = this.aniDbParser.GetPeople(s).ToList())
            };
        }

        private string[] AddGenres(AniDbSeriesData s, string[] genres, int maxGenres, bool addAnimeGenre)
        {
            var genresList = genres.ToList();
            genresList.AddRange(this.aniDbParser.GetGenres(s, maxGenres, addAnimeGenre));
            return genresList.ToArray();
        }

        private static PropertyMapping<AniDbSeriesData, MetadataResult<Series>, TTargetProperty> MapSeries<
            TTargetProperty>(string friendlyName,
            Expression<Func<MetadataResult<Series>, TTargetProperty>> targetPropertySelector,
            Action<AniDbSeriesData, MetadataResult<Series>> apply)
        {
            return new PropertyMapping<AniDbSeriesData, MetadataResult<Series>, TTargetProperty>
                (friendlyName, targetPropertySelector, apply, SourceNames.AniDb);
        }

        private static PropertyMapping<AniDbSeriesData, MetadataResult<Series>, TTargetProperty> MapSeries<
            TTargetProperty>(string friendlyName,
            Expression<Func<MetadataResult<Series>, TTargetProperty>> targetPropertySelector,
            Action<AniDbSeriesData, MetadataResult<Series>> apply,
            Func<AniDbSeriesData, MetadataResult<Series>, bool> canApply)
        {
            return new PropertyMapping<AniDbSeriesData, MetadataResult<Series>, TTargetProperty>
                (friendlyName, targetPropertySelector, apply, SourceNames.AniDb, canApply);
        }

        private static PropertyMapping<IdentifierOnlySourceData, MetadataResult<Season>, TTargetProperty> MapSeasonIdentifier<
            TTargetProperty>(string friendlyName,
            Expression<Func<MetadataResult<Season>, TTargetProperty>> targetPropertySelector,
            Action<IdentifierOnlySourceData, MetadataResult<Season>> apply,
            Func<IdentifierOnlySourceData, MetadataResult<Season>, bool> canApply)
        {
            return new PropertyMapping<IdentifierOnlySourceData, MetadataResult<Season>, TTargetProperty>
                (friendlyName, targetPropertySelector, apply, SourceNames.AniDb, canApply);
        }

        private static PropertyMapping<AniDbSeriesData, MetadataResult<Season>, TTargetProperty> MapSeason<
            TTargetProperty>(string friendlyName,
            Expression<Func<MetadataResult<Season>, TTargetProperty>> targetPropertySelector,
            Action<AniDbSeriesData, MetadataResult<Season>> apply)
        {
            return new PropertyMapping<AniDbSeriesData, MetadataResult<Season>, TTargetProperty>
                (friendlyName, targetPropertySelector, apply, SourceNames.AniDb);
        }

        private static PropertyMapping<AniDbSeriesData, MetadataResult<Season>, TTargetProperty> MapSeason<
            TTargetProperty>(string friendlyName,
            Expression<Func<MetadataResult<Season>, TTargetProperty>> targetPropertySelector,
            Action<AniDbSeriesData, MetadataResult<Season>> apply,
            Func<AniDbSeriesData, MetadataResult<Season>, bool> canApply)
        {
            return new PropertyMapping<AniDbSeriesData, MetadataResult<Season>, TTargetProperty>
                (friendlyName, targetPropertySelector, apply, SourceNames.AniDb, canApply);
        }

        private static PropertyMapping<AniDbEpisodeData, MetadataResult<Episode>, TTargetProperty> MapEpisode<
            TTargetProperty>(string friendlyName,
            Expression<Func<MetadataResult<Episode>, TTargetProperty>> targetPropertySelector,
            Action<AniDbEpisodeData, MetadataResult<Episode>> apply)
        {
            return new PropertyMapping<AniDbEpisodeData, MetadataResult<Episode>, TTargetProperty>
                (friendlyName, targetPropertySelector, apply, SourceNames.AniDb);
        }

        private static PropertyMapping<AniDbEpisodeData, MetadataResult<Episode>, TTargetProperty> MapEpisode<
            TTargetProperty>(string friendlyName,
            Expression<Func<MetadataResult<Episode>, TTargetProperty>> targetPropertySelector,
            Action<AniDbEpisodeData, MetadataResult<Episode>> apply,
            Func<AniDbEpisodeData, MetadataResult<Episode>, bool> canApply)
        {
            return new PropertyMapping<AniDbEpisodeData, MetadataResult<Episode>, TTargetProperty>
                (friendlyName, targetPropertySelector, apply, SourceNames.AniDb, canApply);
        }

        private static PropertyMapping<AniDbSeriesData, MetadataResult<Episode>, TTargetProperty>
            MapEpisodeFromSeriesData<
                TTargetProperty>(string friendlyName,
                Expression<Func<MetadataResult<Episode>, TTargetProperty>> targetPropertySelector,
                Action<AniDbSeriesData, MetadataResult<Episode>> apply)
        {
            return new PropertyMapping<AniDbSeriesData, MetadataResult<Episode>, TTargetProperty>
                (friendlyName, targetPropertySelector, apply, SourceNames.AniDb);
        }

        private string SelectTitle(AniDbSeriesData aniDbSeriesData, TitleType preferredTitleType,
            string metadataLanguage)
        {
            return this.titleSelector.SelectTitle(aniDbSeriesData.Titles, preferredTitleType, metadataLanguage)
                .Match(t => t.Title, () => string.Empty);
        }

        private string SelectTitle(AniDbEpisodeData aniDbEpisodeData, TitleType preferredTitleType,
            string metadataLanguage)
        {
            return this.titleSelector.SelectTitle(aniDbEpisodeData.Titles, preferredTitleType, metadataLanguage)
                .Match(t => t.Title, () => string.Empty);
        }
    }
}
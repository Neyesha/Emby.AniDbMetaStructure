using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Jellyfin.AniDbMetaStructure.AniList.Data;
using Jellyfin.AniDbMetaStructure.Configuration;
using Jellyfin.AniDbMetaStructure.Process.Sources;
using Jellyfin.AniDbMetaStructure.PropertyMapping;
using LanguageExt;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;

namespace Jellyfin.AniDbMetaStructure.AniList
{
    internal class AniListSourceMappingConfiguration : ISourceMappingConfiguration
    {
        private readonly IAniListNameSelector nameSelector;

        public AniListSourceMappingConfiguration(IAniListNameSelector nameSelector)
        {
            this.nameSelector = nameSelector;
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
                    (s, t) => this.nameSelector.SelectTitle(s.Title, preferredTitleType, metadataLanguage)
                        .Map(title => t.Item.Name = title),
                    (s, t) => this.nameSelector.SelectTitle(s.Title, preferredTitleType, metadataLanguage).IsSome),
                MapSeries("Release date", t => t.Item.PremiereDate,
                    (s, t) => AniListFuzzyDate.ToDate(s.StartDate).Map(d => t.Item.PremiereDate = d),
                    (s, t) => AniListFuzzyDate.ToDate(s.StartDate).IsSome),
                MapSeries("End date", t => t.Item.EndDate,
                    (s, t) => AniListFuzzyDate.ToDate(s.EndDate).Map(d => t.Item.EndDate = d),
                    (s, t) => AniListFuzzyDate.ToDate(s.EndDate).IsSome),
                MapSeries("Community rating", t => t.Item.CommunityRating,
                    (s, t) => t.Item.CommunityRating = (float?)s.AverageScore,
                    (s, t) => s.AverageScore > 0),
                MapSeries("Overview", t => t.Item.Overview,
                    (s, t) => t.Item.Overview = s.Description,
                    (s, t) => !string.IsNullOrWhiteSpace(s.Description)),
                MapSeries("Studios", t => t.Item.Studios,
                    (s, t) => t.Item.Studios = s.Studios.Edges.Map(studio => studio.Name).ToArray(),
                    (s, t) => s.Studios.Edges.Any()),
                MapSeries("Genres", t => t.Item.Genres,
                    (s, t) =>
                    {
                        var genresList = t.Item.Genres.ToList();
                        genresList.AddRange(s.Genres.Take(maxGenres));
                        t.Item.Genres = genresList.ToArray();
                    },
                    (s, t) => s.Genres.Any()),
                MapSeries("Tags", t => t.Item.Tags,
                    (s, t) => t.Item.Tags =
                        moveExcessGenresToTags ? s.Genres.Skip(maxGenres).ToArray() : new string[] { },
                    (s, t) => s.Genres.Skip(maxGenres).Any()),
                MapSeries("People", t => t.People,
                    (s, t) => t.People = s.Staff.Edges
                        .Map(staff => ToPersonInfo(this.nameSelector, preferredTitleType, metadataLanguage, staff))
                        .Concat(s.Characters.Edges.Map(c =>
                            ToPersonInfo(this.nameSelector, preferredTitleType, metadataLanguage, c)))
                        .Somes()
                        .ToList(),
                    (s, t) => s.Staff.Edges.Any() || s.Characters.Edges.Any())
            };
        }

        public IEnumerable<PropertyMappingDefinition> GetSeasonMappingDefinitions()
        {
            return new PropertyMappingDefinition[] { };
        }

        public IEnumerable<IPropertyMapping> GetSeasonMappings(int maxGenres, bool addAnimeGenre,
            TitleType preferredTitleType, string metadataLanguage)
        {
            return new IPropertyMapping[] { };
        }

        public IEnumerable<PropertyMappingDefinition> GetEpisodeMappingDefinitions()
        {
            return new PropertyMappingDefinition[] { };
        }

        public IEnumerable<IPropertyMapping> GetEpisodeMappings(int maxGenres, bool addAnimeGenre,
            bool moveExcessGenresToTags, TitleType preferredTitleType, string metadataLanguage)
        {
            return new IPropertyMapping[] { };
        }

        private static PropertyMapping<AniListSeriesData, MetadataResult<Series>, TTargetProperty> MapSeries<
            TTargetProperty>(string friendlyName,
            Expression<Func<MetadataResult<Series>, TTargetProperty>> targetPropertySelector,
            Action<AniListSeriesData, MetadataResult<Series>> apply)
        {
            return new PropertyMapping<AniListSeriesData, MetadataResult<Series>, TTargetProperty>
                (friendlyName, targetPropertySelector, apply, SourceNames.AniList);
        }

        private static PropertyMapping<AniListSeriesData, MetadataResult<Series>, TTargetProperty> MapSeries<
            TTargetProperty>(string friendlyName,
            Expression<Func<MetadataResult<Series>, TTargetProperty>> targetPropertySelector,
            Action<AniListSeriesData, MetadataResult<Series>> apply,
            Func<AniListSeriesData, MetadataResult<Series>, bool> canApply)
        {
            return new PropertyMapping<AniListSeriesData, MetadataResult<Series>, TTargetProperty>
                (friendlyName, targetPropertySelector, apply, SourceNames.AniList, canApply);
        }

        private static Option<PersonInfo> ToPersonInfo(IAniListNameSelector nameSelector,
            TitleType preferredTitleType, string language, AniListStaffData aniListStaffData)
        {
            return nameSelector.SelectName(aniListStaffData.Name, preferredTitleType, language)
                .Map(n => new PersonInfo
                {
                    Name = n,
                    Role = aniListStaffData.Role,
                    ImageUrl = aniListStaffData.Image.Large
                });
        }

        private static Option<PersonInfo> ToPersonInfo(IAniListNameSelector nameSelector,
            TitleType preferredTitleType, string language, AniListCharacterData aniListCharacterData)
        {
            var voiceActor = aniListCharacterData.VoiceActors.Find(va =>
                string.Equals(va.Language, "JAPANESE", StringComparison.InvariantCultureIgnoreCase));

            var characterName = nameSelector.SelectName(aniListCharacterData.Name, preferredTitleType, language);
            var voiceActorName = voiceActor.Bind(va => nameSelector.SelectName(va.Name, preferredTitleType, language));

            return characterName.Bind(c => voiceActor.Bind(va => voiceActorName.Map(van => new PersonInfo
            {
                Name = van,
                Type = PersonType.Actor,
                Role = c,
                ImageUrl = va.Image.Large
            })));
        }
    }
}
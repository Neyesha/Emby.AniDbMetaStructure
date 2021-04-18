using Jellyfin.AniDbMetaStructure.AniDb;
using Jellyfin.AniDbMetaStructure.AniList;
using Jellyfin.AniDbMetaStructure.Process.Sources;
using Jellyfin.AniDbMetaStructure.TvDb;
using MediaBrowser.Model.Plugins;
using System.Collections.Generic;
using System.Linq;

namespace Jellyfin.AniDbMetaStructure.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        private PropertyMappingDefinitionCollection[] episodeMappings;
        private PropertyMappingDefinitionCollection[] seasonMappings;
        private PropertyMappingDefinitionCollection[] seriesMappings;

        public PluginConfiguration()
        {
            TitlePreference = TitleType.Localized;
            MaxGenres = 5;
            MoveExcessGenresToTags = true;
            AddAnimeGenre = true;
            ExcludedSeriesNames = string.Empty;
            AniListAuthorisationCode = string.Empty;
            LibraryStructureSourceName = SourceNames.AniDb;
            FileStructureSourceName = SourceNames.AniDb;

            var mappingConfiguration = new MappingConfiguration(new ISourceMappingConfiguration[]
            {
                new AniDbSourceMappingConfiguration(null, null),
                new TvDbSourceMappingConfiguration(),
                new AniListSourceMappingConfiguration(null)
            });

            this.seriesMappings = GetDefaultSeriesMappings(mappingConfiguration);
            this.seasonMappings = GetDefaultSeasonMappings(mappingConfiguration);
            this.episodeMappings = GetDefaultEpisodeMappings(mappingConfiguration);
        }

        public string LibraryStructureSourceName { get; set; }

        public string FileStructureSourceName { get; set; }

        public int MaxGenres { get; set; }

        public bool MoveExcessGenresToTags { get; set; }

        public bool AddAnimeGenre { get; set; }

        public string ExcludedSeriesNames { get; set; }

        public string TvDbApiKey { get; set; }

        public string AniListAuthorisationCode { get; set; }

        public string AniListAccessToken { get; set; }

        public PropertyMappingDefinitionCollection[] SeriesMappings
        {
            get => this.seriesMappings;
            set => this.seriesMappings = MergeMappings(this.seriesMappings, value);
        }

        public PropertyMappingDefinitionCollection[] SeasonMappings
        {
            get => this.seasonMappings;
            set => this.seasonMappings = MergeMappings(this.seasonMappings, value);
        }

        public PropertyMappingDefinitionCollection[] EpisodeMappings
        {
            get => this.episodeMappings;
            set => this.episodeMappings = MergeMappings(this.episodeMappings, value);
        }

        public TitleType TitlePreference { get; set; }

        private PropertyMappingDefinitionCollection[] GetDefaultSeriesMappings(
            IMappingConfiguration mappingConfiguration)
        {
            var propertyMappings = mappingConfiguration.GetSeriesMappingDefinitions();

            return ToCollection(propertyMappings);
        }

        private PropertyMappingDefinitionCollection[] GetDefaultSeasonMappings(
            IMappingConfiguration mappingConfiguration)
        {
            var propertyMappings = mappingConfiguration.GetSeasonMappingDefinitions();

            return ToCollection(propertyMappings);
        }

        private PropertyMappingDefinitionCollection[] GetDefaultEpisodeMappings(
            IMappingConfiguration mappingConfiguration)
        {
            var propertyMappings = mappingConfiguration.GetEpisodeMappingDefinitions();

            return ToCollection(propertyMappings);
        }

        private PropertyMappingDefinitionCollection[] ToCollection(
            IEnumerable<PropertyMappingDefinition> propertyMappings)
        {
            return propertyMappings
                .GroupBy(m => m.FriendlyName)
                .Select(g => new PropertyMappingDefinitionCollection(g.Key, g))
                .ToArray();
        }

        private PropertyMappingDefinitionCollection[] MergeMappings(PropertyMappingDefinitionCollection[] defaults,
            PropertyMappingDefinitionCollection[] configured)
        {
            if (configured == null)
            {
                return defaults;
            }

            return configured.Where(m => defaults.Any(dm => AreEquivalent(dm, m)))
                .Concat(defaults.Where(dm =>
                    !configured.Any(m => AreEquivalent(dm, m))))
                .Select(m => MergeMappings(defaults.Single(d => AreEquivalent(d, m)), m))
                .ToArray();
        }

        private PropertyMappingDefinitionCollection MergeMappings(PropertyMappingDefinitionCollection defaults,
            PropertyMappingDefinitionCollection configured)
        {
            var mergedMappings = configured.Mappings.Where(m => defaults.Mappings.Any(dm => AreEquivalent(dm, m)))
                .Concat(defaults.Mappings.Where(dm =>
                    !configured.Mappings.Any(m => AreEquivalent(dm, m))));

            configured.Mappings = mergedMappings.ToArray();

            return configured;
        }

        private bool AreEquivalent(PropertyMappingDefinition definitionA, PropertyMappingDefinition definitionB)
        {
            return definitionA.SourceName == definitionB.SourceName &&
                definitionA.TargetPropertyName == definitionB.TargetPropertyName;
        }

        private bool AreEquivalent(PropertyMappingDefinitionCollection definitionCollectionA,
            PropertyMappingDefinitionCollection definitionCollectionB)
        {
            return definitionCollectionA.FriendlyName == definitionCollectionB.FriendlyName;
        }
    }
}
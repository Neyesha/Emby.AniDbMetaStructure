using System.Collections.Generic;
using System.Linq;
using Jellyfin.AniDbMetaStructure.PropertyMapping;

namespace Jellyfin.AniDbMetaStructure.Configuration
{
    internal class MappingConfiguration : IMappingConfiguration
    {
        private readonly IEnumerable<ISourceMappingConfiguration> sourceMappingConfigurations;

        public MappingConfiguration(IEnumerable<ISourceMappingConfiguration> sourceMappingConfigurations)
        {
            this.sourceMappingConfigurations = sourceMappingConfigurations;
        }

        public IEnumerable<PropertyMappingDefinition> GetEpisodeMappingDefinitions()
        {
            return this.sourceMappingConfigurations.SelectMany(c =>
                c.GetEpisodeMappingDefinitions());
        }

        public IPropertyMappingCollection GetEpisodeMappings(int maxGenres, bool addAnimeGenre,
            bool moveExcessGenresToTags, TitleType preferredTitleType, string metadataLanguage)
        {
            return new PropertyMappingCollection(this.sourceMappingConfigurations.SelectMany(c =>
                c.GetEpisodeMappings(maxGenres, addAnimeGenre, moveExcessGenresToTags, preferredTitleType,
                    metadataLanguage)));
        }

        public IEnumerable<PropertyMappingDefinition> GetSeriesMappingDefinitions()
        {
            return this.sourceMappingConfigurations.SelectMany(c =>
                c.GetSeriesMappingDefinitions());
        }

        public IPropertyMappingCollection GetSeriesMappings(int maxGenres,
            bool addAnimeGenre, bool moveExcessGenresToTags, TitleType preferredTitleType, string metadataLanguage)
        {
            return new PropertyMappingCollection(this.sourceMappingConfigurations.SelectMany(c =>
                c.GetSeriesMappings(maxGenres, addAnimeGenre, moveExcessGenresToTags, preferredTitleType,
                    metadataLanguage)));
        }

        public IEnumerable<PropertyMappingDefinition> GetSeasonMappingDefinitions()
        {
            return this.sourceMappingConfigurations.SelectMany(c =>
                c.GetSeasonMappingDefinitions());
        }

        public IPropertyMappingCollection GetSeasonMappings(int maxGenres, bool addAnimeGenre,
            TitleType preferredTitleType, string metadataLanguage)
        {
            return new PropertyMappingCollection(this.sourceMappingConfigurations.SelectMany(c =>
                c.GetSeasonMappings(maxGenres, addAnimeGenre, preferredTitleType, metadataLanguage)));
        }
    }
}
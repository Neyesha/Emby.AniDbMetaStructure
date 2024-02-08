using Jellyfin.AniDbMetaStructure.AniDb.SeriesData;
using Jellyfin.AniDbMetaStructure.Configuration;
using Jellyfin.AniDbMetaStructure.PropertyMapping;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace Jellyfin.AniDbMetaStructure.Tests
{
    [TestFixture]
    public class MappingConfigurationTests
    {
        [Test]
        public void GetEpisodeMappings_CombinesSourceMappings()
        {
            var sourceMappingConfigurationA = Substitute.For<ISourceMappingConfiguration>();
            var sourceMappingConfigurationB = Substitute.For<ISourceMappingConfiguration>();

            var propertyMappingA =
                new PropertyMapping<AniDbEpisodeData, AniDbEpisodeData, string>(string.Empty, o => o.Summary, (s, v) => { },
                    "TestSource1");
            var propertyMappingB =
                new PropertyMapping<AniDbEpisodeData, AniDbEpisodeData, string>(string.Empty, o => o.Summary, (s, v) => { },
                    "TestSource2");

            sourceMappingConfigurationA.GetEpisodeMappings(0, false, false, TitleType.Localized, "en")
                .Returns(new[] { propertyMappingA });
            sourceMappingConfigurationB.GetEpisodeMappings(0, false, false, TitleType.Localized, "en")
                .Returns(new[] { propertyMappingB });

            var mappingConfiguration =
                new MappingConfiguration(new[] { sourceMappingConfigurationA, sourceMappingConfigurationB });

            mappingConfiguration.GetEpisodeMappings(0, false, false, TitleType.Localized, "en")
                .Should()
                .BeEquivalentTo(new[] { propertyMappingA, propertyMappingB });
        }

        [Test]
        public void GetSeasonMappings_CombinesSourceMappings()
        {
            var sourceMappingConfigurationA = Substitute.For<ISourceMappingConfiguration>();
            var sourceMappingConfigurationB = Substitute.For<ISourceMappingConfiguration>();

            var propertyMappingA =
                new PropertyMapping<AniDbEpisodeData, AniDbEpisodeData, string>(string.Empty, o => o.Summary, (s, v) => { },
                    "TestSource1");
            var propertyMappingB =
                new PropertyMapping<AniDbEpisodeData, AniDbEpisodeData, string>(string.Empty, o => o.Summary, (s, v) => { },
                    "TestSource2");

            sourceMappingConfigurationA.GetSeasonMappings(1, true, TitleType.Localized, "en")
                .Returns(new[] { propertyMappingA });
            sourceMappingConfigurationB.GetSeasonMappings(1, true, TitleType.Localized, "en")
                .Returns(new[] { propertyMappingB });

            var mappingConfiguration =
                new MappingConfiguration(new[] { sourceMappingConfigurationA, sourceMappingConfigurationB });

            mappingConfiguration.GetSeasonMappings(1, true, TitleType.Localized, "en")
                .Should()
                .BeEquivalentTo(new[] { propertyMappingA, propertyMappingB });
        }

        [Test]
        public void GetSeriesMappings_CombinesSourceMappings()
        {
            var sourceMappingConfigurationA = Substitute.For<ISourceMappingConfiguration>();
            var sourceMappingConfigurationB = Substitute.For<ISourceMappingConfiguration>();

            var propertyMappingA =
                new PropertyMapping<AniDbEpisodeData, AniDbEpisodeData, string>(string.Empty, o => o.Summary, (s, v) => { },
                    "TestSource1");
            var propertyMappingB =
                new PropertyMapping<AniDbEpisodeData, AniDbEpisodeData, string>(string.Empty, o => o.Summary, (s, v) => { },
                    "TestSource2");

            sourceMappingConfigurationA.GetSeriesMappings(1, true, false, TitleType.Localized, "en")
                .Returns(new[] { propertyMappingA });
            sourceMappingConfigurationB.GetSeriesMappings(1, true, false, TitleType.Localized, "en")
                .Returns(new[] { propertyMappingB });

            var mappingConfiguration =
                new MappingConfiguration(new[] { sourceMappingConfigurationA, sourceMappingConfigurationB });

            mappingConfiguration.GetSeriesMappings(1, true, false, TitleType.Localized, "en")
                .Should()
                .BeEquivalentTo(new[] { propertyMappingA, propertyMappingB });
        }
    }
}
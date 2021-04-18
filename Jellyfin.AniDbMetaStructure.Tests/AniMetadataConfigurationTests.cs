using Emby.AniDbMetaStructure.AniDb;
using Emby.AniDbMetaStructure.Configuration;
using Emby.AniDbMetaStructure.Tests.TestHelpers;
using Emby.AniDbMetaStructure.TvDb;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace Emby.AniDbMetaStructure.Tests
{
    [TestFixture]
    public class AniMetadataConfigurationTests
    {
        [Test]
        public void GetEpisodeMetadataMapping_ReturnsMappings()
        {
            var aniMetadataConfiguration = new AniDbMetaStructureConfiguration(new PluginConfiguration(),
                new MappingConfiguration(new ISourceMappingConfiguration[]
                {
                    new AniDbSourceMappingConfiguration(new AniDbParser(), Substitute.For<IAniDbTitleSelector>()),
                    new TvDbSourceMappingConfiguration()
                }), new TestSources());

            aniMetadataConfiguration.GetEpisodeMetadataMapping("en").Should().NotBeNullOrEmpty();
        }

        [Test]
        public void GetSeasonMetadataMapping_ReturnsMappings()
        {
            var aniMetadataConfiguration = new AniDbMetaStructureConfiguration(new PluginConfiguration(),
                new MappingConfiguration(new ISourceMappingConfiguration[]
                {
                    new AniDbSourceMappingConfiguration(new AniDbParser(), Substitute.For<IAniDbTitleSelector>()),
                    new TvDbSourceMappingConfiguration()
                }), new TestSources());

            aniMetadataConfiguration.GetSeasonMetadataMapping("en").Should().NotBeNullOrEmpty();
        }

        [Test]
        public void GetSeriesMetadataMapping_ReturnsMappings()
        {
            var aniMetadataConfiguration = new AniDbMetaStructureConfiguration(new PluginConfiguration(),
                new MappingConfiguration(new ISourceMappingConfiguration[]
                {
                    new AniDbSourceMappingConfiguration(new AniDbParser(), Substitute.For<IAniDbTitleSelector>()),
                    new TvDbSourceMappingConfiguration()
                }), new TestSources());

            aniMetadataConfiguration.GetSeriesMetadataMapping("en").Should().NotBeNullOrEmpty();
        }
    }
}
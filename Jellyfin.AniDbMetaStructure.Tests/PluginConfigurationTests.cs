using System.Linq;
using Jellyfin.AniDbMetaStructure.Configuration;
using Jellyfin.AniDbMetaStructure.Files;
using Jellyfin.AniDbMetaStructure.Tests.TestHelpers;
using FluentAssertions;
using NUnit.Framework;

namespace Emby.AniDbMetaStructure.Tests
{
    [TestFixture]
    public class PluginConfigurationTests
    {
        private const string SerialisedForm = @"<?xml version=""1.0"" encoding=""utf-16""?>
<PluginConfiguration xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <LibraryStructureSourceName>AniDb</LibraryStructureSourceName>
  <FileStructureSourceName>AniDb</FileStructureSourceName>
  <MaxGenres>5</MaxGenres>
  <MoveExcessGenresToTags>true</MoveExcessGenresToTags>
  <AddAnimeGenre>true</AddAnimeGenre>
  <ExcludedSeriesNames />
  <AniListAuthorisationCode />
  <SeriesMappings>
    <PropertyMappingDefinitionCollection>
      <Mappings>
        <PropertyMappingDefinition>
          <FriendlyName>Name</FriendlyName>
          <SourceName>AniDb</SourceName>
          <TargetPropertyName>Name</TargetPropertyName>
        </PropertyMappingDefinition>
        <PropertyMappingDefinition>
          <FriendlyName>Name</FriendlyName>
          <SourceName>Tvdb</SourceName>
          <TargetPropertyName>Name</TargetPropertyName>
        </PropertyMappingDefinition>
        <PropertyMappingDefinition>
          <FriendlyName>Name</FriendlyName>
          <SourceName>AniList</SourceName>
          <TargetPropertyName>Name</TargetPropertyName>
        </PropertyMappingDefinition>
      </Mappings>
    </PropertyMappingDefinitionCollection>
    <PropertyMappingDefinitionCollection>
      <Mappings>
        <PropertyMappingDefinition>
          <FriendlyName>Release date</FriendlyName>
          <SourceName>AniDb</SourceName>
          <TargetPropertyName>PremiereDate</TargetPropertyName>
        </PropertyMappingDefinition>
        <PropertyMappingDefinition>
          <FriendlyName>Release date</FriendlyName>
          <SourceName>Tvdb</SourceName>
          <TargetPropertyName>PremiereDate</TargetPropertyName>
        </PropertyMappingDefinition>
        <PropertyMappingDefinition>
          <FriendlyName>Release date</FriendlyName>
          <SourceName>AniList</SourceName>
          <TargetPropertyName>PremiereDate</TargetPropertyName>
        </PropertyMappingDefinition>
      </Mappings>
    </PropertyMappingDefinitionCollection>
    <PropertyMappingDefinitionCollection>
      <Mappings>
        <PropertyMappingDefinition>
          <FriendlyName>End date</FriendlyName>
          <SourceName>AniDb</SourceName>
          <TargetPropertyName>EndDate</TargetPropertyName>
        </PropertyMappingDefinition>
        <PropertyMappingDefinition>
          <FriendlyName>End date</FriendlyName>
          <SourceName>AniList</SourceName>
          <TargetPropertyName>EndDate</TargetPropertyName>
        </PropertyMappingDefinition>
      </Mappings>
    </PropertyMappingDefinitionCollection>
    <PropertyMappingDefinitionCollection>
      <Mappings>
        <PropertyMappingDefinition>
          <FriendlyName>Community rating</FriendlyName>
          <SourceName>AniDb</SourceName>
          <TargetPropertyName>CommunityRating</TargetPropertyName>
        </PropertyMappingDefinition>
        <PropertyMappingDefinition>
          <FriendlyName>Community rating</FriendlyName>
          <SourceName>Tvdb</SourceName>
          <TargetPropertyName>CommunityRating</TargetPropertyName>
        </PropertyMappingDefinition>
        <PropertyMappingDefinition>
          <FriendlyName>Community rating</FriendlyName>
          <SourceName>AniList</SourceName>
          <TargetPropertyName>CommunityRating</TargetPropertyName>
        </PropertyMappingDefinition>
      </Mappings>
    </PropertyMappingDefinitionCollection>
    <PropertyMappingDefinitionCollection>
      <Mappings>
        <PropertyMappingDefinition>
          <FriendlyName>Overview</FriendlyName>
          <SourceName>AniDb</SourceName>
          <TargetPropertyName>Overview</TargetPropertyName>
        </PropertyMappingDefinition>
        <PropertyMappingDefinition>
          <FriendlyName>Overview</FriendlyName>
          <SourceName>Tvdb</SourceName>
          <TargetPropertyName>Overview</TargetPropertyName>
        </PropertyMappingDefinition>
        <PropertyMappingDefinition>
          <FriendlyName>Overview</FriendlyName>
          <SourceName>AniList</SourceName>
          <TargetPropertyName>Overview</TargetPropertyName>
        </PropertyMappingDefinition>
      </Mappings>
    </PropertyMappingDefinitionCollection>
    <PropertyMappingDefinitionCollection>
      <Mappings>
        <PropertyMappingDefinition>
          <FriendlyName>Studios</FriendlyName>
          <SourceName>AniDb</SourceName>
          <TargetPropertyName>Studios</TargetPropertyName>
        </PropertyMappingDefinition>
        <PropertyMappingDefinition>
          <FriendlyName>Studios</FriendlyName>
          <SourceName>AniList</SourceName>
          <TargetPropertyName>Studios</TargetPropertyName>
        </PropertyMappingDefinition>
      </Mappings>
    </PropertyMappingDefinitionCollection>
    <PropertyMappingDefinitionCollection>
      <Mappings>
        <PropertyMappingDefinition>
          <FriendlyName>Genres</FriendlyName>
          <SourceName>AniDb</SourceName>
          <TargetPropertyName>Genres</TargetPropertyName>
        </PropertyMappingDefinition>
        <PropertyMappingDefinition>
          <FriendlyName>Genres</FriendlyName>
          <SourceName>Tvdb</SourceName>
          <TargetPropertyName>Genres</TargetPropertyName>
        </PropertyMappingDefinition>
        <PropertyMappingDefinition>
          <FriendlyName>Genres</FriendlyName>
          <SourceName>AniList</SourceName>
          <TargetPropertyName>Genres</TargetPropertyName>
        </PropertyMappingDefinition>
      </Mappings>
    </PropertyMappingDefinitionCollection>
    <PropertyMappingDefinitionCollection>
      <Mappings>
        <PropertyMappingDefinition>
          <FriendlyName>Tags</FriendlyName>
          <SourceName>AniDb</SourceName>
          <TargetPropertyName>Tags</TargetPropertyName>
        </PropertyMappingDefinition>
        <PropertyMappingDefinition>
          <FriendlyName>Tags</FriendlyName>
          <SourceName>Tvdb</SourceName>
          <TargetPropertyName>Tags</TargetPropertyName>
        </PropertyMappingDefinition>
        <PropertyMappingDefinition>
          <FriendlyName>Tags</FriendlyName>
          <SourceName>AniList</SourceName>
          <TargetPropertyName>Tags</TargetPropertyName>
        </PropertyMappingDefinition>
      </Mappings>
    </PropertyMappingDefinitionCollection>
    <PropertyMappingDefinitionCollection>
      <Mappings>
        <PropertyMappingDefinition>
          <FriendlyName>People</FriendlyName>
          <SourceName>AniDb</SourceName>
          <TargetPropertyName>People</TargetPropertyName>
        </PropertyMappingDefinition>
        <PropertyMappingDefinition>
          <FriendlyName>People</FriendlyName>
          <SourceName>AniList</SourceName>
          <TargetPropertyName>People</TargetPropertyName>
        </PropertyMappingDefinition>
      </Mappings>
    </PropertyMappingDefinitionCollection>
    <PropertyMappingDefinitionCollection>
      <Mappings>
        <PropertyMappingDefinition>
          <FriendlyName>Air days</FriendlyName>
          <SourceName>Tvdb</SourceName>
          <TargetPropertyName>AirDays</TargetPropertyName>
        </PropertyMappingDefinition>
      </Mappings>
    </PropertyMappingDefinitionCollection>
    <PropertyMappingDefinitionCollection>
      <Mappings>
        <PropertyMappingDefinition>
          <FriendlyName>Air time</FriendlyName>
          <SourceName>Tvdb</SourceName>
          <TargetPropertyName>AirTime</TargetPropertyName>
        </PropertyMappingDefinition>
      </Mappings>
    </PropertyMappingDefinitionCollection>
  </SeriesMappings>
  <SeasonMappings>
    <PropertyMappingDefinitionCollection>
      <Mappings>
        <PropertyMappingDefinition>
          <FriendlyName>Name</FriendlyName>
          <SourceName>AniDb</SourceName>
          <TargetPropertyName>Name</TargetPropertyName>
        </PropertyMappingDefinition>
        <PropertyMappingDefinition>
          <FriendlyName>Name</FriendlyName>
          <SourceName>Tvdb</SourceName>
          <TargetPropertyName>Name</TargetPropertyName>
        </PropertyMappingDefinition>
      </Mappings>
    </PropertyMappingDefinitionCollection>
    <PropertyMappingDefinitionCollection>
      <Mappings>
        <PropertyMappingDefinition>
          <FriendlyName>Release date</FriendlyName>
          <SourceName>AniDb</SourceName>
          <TargetPropertyName>PremiereDate</TargetPropertyName>
        </PropertyMappingDefinition>
      </Mappings>
    </PropertyMappingDefinitionCollection>
    <PropertyMappingDefinitionCollection>
      <Mappings>
        <PropertyMappingDefinition>
          <FriendlyName>End date</FriendlyName>
          <SourceName>AniDb</SourceName>
          <TargetPropertyName>EndDate</TargetPropertyName>
        </PropertyMappingDefinition>
      </Mappings>
    </PropertyMappingDefinitionCollection>
    <PropertyMappingDefinitionCollection>
      <Mappings>
        <PropertyMappingDefinition>
          <FriendlyName>Community rating</FriendlyName>
          <SourceName>AniDb</SourceName>
          <TargetPropertyName>CommunityRating</TargetPropertyName>
        </PropertyMappingDefinition>
      </Mappings>
    </PropertyMappingDefinitionCollection>
    <PropertyMappingDefinitionCollection>
      <Mappings>
        <PropertyMappingDefinition>
          <FriendlyName>Overview</FriendlyName>
          <SourceName>AniDb</SourceName>
          <TargetPropertyName>Overview</TargetPropertyName>
        </PropertyMappingDefinition>
      </Mappings>
    </PropertyMappingDefinitionCollection>
    <PropertyMappingDefinitionCollection>
      <Mappings>
        <PropertyMappingDefinition>
          <FriendlyName>Studios</FriendlyName>
          <SourceName>AniDb</SourceName>
          <TargetPropertyName>Studios</TargetPropertyName>
        </PropertyMappingDefinition>
      </Mappings>
    </PropertyMappingDefinitionCollection>
    <PropertyMappingDefinitionCollection>
      <Mappings>
        <PropertyMappingDefinition>
          <FriendlyName>Genres</FriendlyName>
          <SourceName>AniDb</SourceName>
          <TargetPropertyName>Genres</TargetPropertyName>
        </PropertyMappingDefinition>
      </Mappings>
    </PropertyMappingDefinitionCollection>
    <PropertyMappingDefinitionCollection>
      <Mappings>
        <PropertyMappingDefinition>
          <FriendlyName>Tags</FriendlyName>
          <SourceName>AniDb</SourceName>
          <TargetPropertyName>Tags</TargetPropertyName>
        </PropertyMappingDefinition>
      </Mappings>
    </PropertyMappingDefinitionCollection>
  </SeasonMappings>
  <EpisodeMappings>
    <PropertyMappingDefinitionCollection>
      <Mappings>
        <PropertyMappingDefinition>
          <FriendlyName>Name</FriendlyName>
          <SourceName>AniDb</SourceName>
          <TargetPropertyName>Name</TargetPropertyName>
        </PropertyMappingDefinition>
        <PropertyMappingDefinition>
          <FriendlyName>Name</FriendlyName>
          <SourceName>Tvdb</SourceName>
          <TargetPropertyName>Name</TargetPropertyName>
        </PropertyMappingDefinition>
      </Mappings>
    </PropertyMappingDefinitionCollection>
    <PropertyMappingDefinitionCollection>
      <Mappings>
        <PropertyMappingDefinition>
          <FriendlyName>Release date</FriendlyName>
          <SourceName>AniDb</SourceName>
          <TargetPropertyName>PremiereDate</TargetPropertyName>
        </PropertyMappingDefinition>
        <PropertyMappingDefinition>
          <FriendlyName>Release date</FriendlyName>
          <SourceName>Tvdb</SourceName>
          <TargetPropertyName>PremiereDate</TargetPropertyName>
        </PropertyMappingDefinition>
      </Mappings>
    </PropertyMappingDefinitionCollection>
    <PropertyMappingDefinitionCollection>
      <Mappings>
        <PropertyMappingDefinition>
          <FriendlyName>Runtime</FriendlyName>
          <SourceName>AniDb</SourceName>
          <TargetPropertyName>RunTimeTicks</TargetPropertyName>
        </PropertyMappingDefinition>
      </Mappings>
    </PropertyMappingDefinitionCollection>
    <PropertyMappingDefinitionCollection>
      <Mappings>
        <PropertyMappingDefinition>
          <FriendlyName>Community rating</FriendlyName>
          <SourceName>AniDb</SourceName>
          <TargetPropertyName>CommunityRating</TargetPropertyName>
        </PropertyMappingDefinition>
        <PropertyMappingDefinition>
          <FriendlyName>Community rating</FriendlyName>
          <SourceName>Tvdb</SourceName>
          <TargetPropertyName>CommunityRating</TargetPropertyName>
        </PropertyMappingDefinition>
      </Mappings>
    </PropertyMappingDefinitionCollection>
    <PropertyMappingDefinitionCollection>
      <Mappings>
        <PropertyMappingDefinition>
          <FriendlyName>Overview</FriendlyName>
          <SourceName>AniDb</SourceName>
          <TargetPropertyName>Overview</TargetPropertyName>
        </PropertyMappingDefinition>
        <PropertyMappingDefinition>
          <FriendlyName>Overview</FriendlyName>
          <SourceName>Tvdb</SourceName>
          <TargetPropertyName>Overview</TargetPropertyName>
        </PropertyMappingDefinition>
      </Mappings>
    </PropertyMappingDefinitionCollection>
    <PropertyMappingDefinitionCollection>
      <Mappings>
        <PropertyMappingDefinition>
          <FriendlyName>Studios</FriendlyName>
          <SourceName>AniDb</SourceName>
          <TargetPropertyName>Studios</TargetPropertyName>
        </PropertyMappingDefinition>
      </Mappings>
    </PropertyMappingDefinitionCollection>
    <PropertyMappingDefinitionCollection>
      <Mappings>
        <PropertyMappingDefinition>
          <FriendlyName>Genres</FriendlyName>
          <SourceName>AniDb</SourceName>
          <TargetPropertyName>Genres</TargetPropertyName>
        </PropertyMappingDefinition>
        <PropertyMappingDefinition>
          <FriendlyName>Genres</FriendlyName>
          <SourceName>Tvdb</SourceName>
          <TargetPropertyName>Genres</TargetPropertyName>
        </PropertyMappingDefinition>
      </Mappings>
    </PropertyMappingDefinitionCollection>
    <PropertyMappingDefinitionCollection>
      <Mappings>
        <PropertyMappingDefinition>
          <FriendlyName>Tags</FriendlyName>
          <SourceName>AniDb</SourceName>
          <TargetPropertyName>Tags</TargetPropertyName>
        </PropertyMappingDefinition>
        <PropertyMappingDefinition>
          <FriendlyName>Tags</FriendlyName>
          <SourceName>Tvdb</SourceName>
          <TargetPropertyName>Tags</TargetPropertyName>
        </PropertyMappingDefinition>
      </Mappings>
    </PropertyMappingDefinitionCollection>
    <PropertyMappingDefinitionCollection>
      <Mappings>
        <PropertyMappingDefinition>
          <FriendlyName>People</FriendlyName>
          <SourceName>AniDb</SourceName>
          <TargetPropertyName>People</TargetPropertyName>
        </PropertyMappingDefinition>
      </Mappings>
    </PropertyMappingDefinitionCollection>
  </EpisodeMappings>
  <TitlePreference>Localized</TitlePreference>
</PluginConfiguration>";

        [Test]
        public void CanBeDeserialised()
        {
            var expected = new PluginConfiguration();

            var actual = new XmlSerialiser(new ConsoleLogManager()).Deserialise<PluginConfiguration>(SerialisedForm);

            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void CanBeSerialised()
        {
            var configuration = new PluginConfiguration();

            var serialised = new XmlSerialiser(new ConsoleLogManager()).Serialise(configuration);

            serialised.Should().Be(SerialisedForm);
        }

        [Test]
        public void SeriesMappings_ExtraMappingInConfigured_RemovesExtraMapping()
        {
            var configured = new PluginConfiguration().SeriesMappings;
            configured[0].Mappings = configured[0]
                .Mappings.Concat(new[] { new PropertyMappingDefinition(string.Empty, "ExtraSource", "ExtraTarget") })
                .ToArray();

            var pluginConfiguration = new PluginConfiguration();

            pluginConfiguration.SeriesMappings = configured;

            pluginConfiguration.SeriesMappings.Should()
                .NotContain(mc =>
                    mc.Mappings.Any(m => m.SourceName == "ExtraSource" || m.TargetPropertyName == "ExtraTarget"));
        }

        [Test]
        public void SeriesMappings_ExtraPropertyInConfigured_RemovesExtraProperty()
        {
            var configured = new PluginConfiguration().SeriesMappings.Concat(new[]
                {
                    new PropertyMappingDefinitionCollection("Extra name",
                        new[] { new PropertyMappingDefinition("Extra name", "ExtraSource", "ExtraTarget") })
                })
                .ToArray();

            var pluginConfiguration = new PluginConfiguration();

            pluginConfiguration.SeriesMappings = configured;

            pluginConfiguration.SeriesMappings.Should().NotContain(m => m.FriendlyName == "Extra name");
        }

        [Test]
        public void SeriesMappings_MatchingMappinngInConfigured_UpdatesMappingOrderToConfigured()
        {
            var configured = new PluginConfiguration().SeriesMappings;
            configured[0].Mappings = configured[0].Mappings.Reverse().ToArray();

            var pluginConfiguration = new PluginConfiguration();

            pluginConfiguration.SeriesMappings = configured;

            pluginConfiguration.SeriesMappings[0].Mappings.Should().BeEquivalentTo(configured[0].Mappings);
        }

        [Test]
        public void SeriesMappings_MissingMappingInConfigured_AddsMissingMappingAtEnd()
        {
            var configured = new PluginConfiguration().SeriesMappings;
            var missing = configured[0].Mappings[0];
            configured[0].Mappings = configured[0].Mappings.Skip(1).ToArray();

            var pluginConfiguration = new PluginConfiguration();

            pluginConfiguration.SeriesMappings = configured;

            pluginConfiguration.SeriesMappings[0].Mappings.Last().Should().BeEquivalentTo(missing);
        }

        [Test]
        public void SeriesMappings_MissingPropertyInConfigured_AddsMissingPropertyAtEnd()
        {
            var configured = new PluginConfiguration().SeriesMappings;
            var missing = configured[0];
            configured = configured.Skip(1).ToArray();

            var pluginConfiguration = new PluginConfiguration();

            pluginConfiguration.SeriesMappings = configured;

            pluginConfiguration.SeriesMappings.Last().Should().BeEquivalentTo(missing);
        }

        [Test]
        public void SeasonMappings_ExtraMappingInConfigured_RemovesExtraMapping()
        {
            var configured = new PluginConfiguration().SeasonMappings;
            configured[0].Mappings = configured[0]
                .Mappings.Concat(new[] { new PropertyMappingDefinition(string.Empty, "ExtraSource", "ExtraTarget") })
                .ToArray();

            var pluginConfiguration = new PluginConfiguration();

            pluginConfiguration.SeasonMappings = configured;

            pluginConfiguration.SeasonMappings.Should()
                .NotContain(mc =>
                    mc.Mappings.Any(m => m.SourceName == "ExtraSource" || m.TargetPropertyName == "ExtraTarget"));
        }

        [Test]
        public void SeasonMappings_ExtraPropertyInConfigured_RemovesExtraProperty()
        {
            var configured = new PluginConfiguration().SeasonMappings.Concat(new[]
                {
                    new PropertyMappingDefinitionCollection("Extra name",
                        new[] { new PropertyMappingDefinition("Extra name", "ExtraSource", "ExtraTarget") })
                })
                .ToArray();

            var pluginConfiguration = new PluginConfiguration();

            pluginConfiguration.SeasonMappings = configured;

            pluginConfiguration.SeasonMappings.Should().NotContain(m => m.FriendlyName == "Extra name");
        }

        [Test]
        public void SeasonMappings_MatchingMappinngInConfigured_UpdatesMappingOrderToConfigured()
        {
            var configured = new PluginConfiguration().SeasonMappings;
            configured[0].Mappings = configured[0].Mappings.Reverse().ToArray();

            var pluginConfiguration = new PluginConfiguration();

            pluginConfiguration.SeasonMappings = configured;

            pluginConfiguration.SeasonMappings[0].Mappings.Should().BeEquivalentTo(configured[0].Mappings);
        }

        [Test]
        public void SeasonMappings_MissingMappingInConfigured_AddsMissingMappingAtEnd()
        {
            var configured = new PluginConfiguration().SeasonMappings;
            var missing = configured[0].Mappings[0];
            configured[0].Mappings = configured[0].Mappings.Skip(1).ToArray();

            var pluginConfiguration = new PluginConfiguration();

            pluginConfiguration.SeasonMappings = configured;

            pluginConfiguration.SeasonMappings[0].Mappings.Last().Should().BeEquivalentTo(missing);
        }

        [Test]
        public void SeasonMappings_MissingPropertyInConfigured_AddsMissingPropertyAtEnd()
        {
            var configured = new PluginConfiguration().SeasonMappings;
            var missing = configured[0];
            configured = configured.Skip(1).ToArray();

            var pluginConfiguration = new PluginConfiguration();

            pluginConfiguration.SeasonMappings = configured;

            pluginConfiguration.SeasonMappings.Last().Should().BeEquivalentTo(missing);
        }

        [Test]
        public void EpisodeMappings_ExtraMappingInConfigured_RemovesExtraMapping()
        {
            var configured = new PluginConfiguration().EpisodeMappings;
            configured[0].Mappings = configured[0]
                .Mappings.Concat(new[] { new PropertyMappingDefinition(string.Empty, "ExtraSource", "ExtraTarget") })
                .ToArray();

            var pluginConfiguration = new PluginConfiguration();

            pluginConfiguration.EpisodeMappings = configured;

            pluginConfiguration.EpisodeMappings.Should()
                .NotContain(mc =>
                    mc.Mappings.Any(m => m.SourceName == "ExtraSource" || m.TargetPropertyName == "ExtraTarget"));
        }

        [Test]
        public void EpisodeMappings_ExtraPropertyInConfigured_RemovesExtraProperty()
        {
            var configured = new PluginConfiguration().EpisodeMappings.Concat(new[]
                {
                    new PropertyMappingDefinitionCollection("Extra name",
                        new[] { new PropertyMappingDefinition("Extra name", "ExtraSource", "ExtraTarget") })
                })
                .ToArray();

            var pluginConfiguration = new PluginConfiguration();

            pluginConfiguration.EpisodeMappings = configured;

            pluginConfiguration.EpisodeMappings.Should().NotContain(m => m.FriendlyName == "Extra name");
        }

        [Test]
        public void EpisodeMappings_MatchingMappinngInConfigured_UpdatesMappingOrderToConfigured()
        {
            var configured = new PluginConfiguration().EpisodeMappings;
            configured[0].Mappings = configured[0].Mappings.Reverse().ToArray();

            var pluginConfiguration = new PluginConfiguration();

            pluginConfiguration.EpisodeMappings = configured;

            pluginConfiguration.EpisodeMappings[0].Mappings.Should().BeEquivalentTo(configured[0].Mappings);
        }

        [Test]
        public void EpisodeMappings_MissingMappingInConfigured_AddsMissingMappingAtEnd()
        {
            var configured = new PluginConfiguration().EpisodeMappings;
            var missing = configured[0].Mappings[0];
            configured[0].Mappings = configured[0].Mappings.Skip(1).ToArray();

            var pluginConfiguration = new PluginConfiguration();

            pluginConfiguration.EpisodeMappings = configured;

            pluginConfiguration.EpisodeMappings[0].Mappings.Last().Should().BeEquivalentTo(missing);
        }

        [Test]
        public void EpisodeMappings_MissingPropertyInConfigured_AddsMissingPropertyAtEnd()
        {
            var configured = new PluginConfiguration().EpisodeMappings;
            var missing = configured[0];
            configured = configured.Skip(1).ToArray();

            var pluginConfiguration = new PluginConfiguration();

            pluginConfiguration.EpisodeMappings = configured;

            pluginConfiguration.EpisodeMappings.Last().Should().BeEquivalentTo(missing);
        }
    }
}
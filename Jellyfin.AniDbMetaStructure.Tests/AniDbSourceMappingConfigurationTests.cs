using System;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.AniDbMetaStructure.AniDb;
using Jellyfin.AniDbMetaStructure.AniDb.SeriesData;
using Jellyfin.AniDbMetaStructure.Configuration;
using FluentAssertions;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using NSubstitute;
using NUnit.Framework;

namespace Jellyfin.AniDbMetaStructure.Tests
{
    [TestFixture]
    public class AniDbSourceMappingConfigurationTests
    {
        [SetUp]
        public void Setup()
        {
            this.titleSelector = Substitute.For<IAniDbTitleSelector>();

            this.titleSelector.SelectTitle(Arg.Any<ItemTitleData[]>(), TitleType.Localized, "en")
                .Returns(new ItemTitleData { Title = "SelectedTitle" });
        }

        private IAniDbTitleSelector titleSelector;

        [Test]
        public void EpisodeMappings_DontMapEmptyFields()
        {
            var source = new AniDbEpisodeData
            {
                TotalMinutes = 0,
                Summary = string.Empty,
                Rating = null
            };

            var target = new MetadataResult<Episode>
            {
                Item = new Episode()
            };

            var aniDbSourceMappingConfiguration =
                new AniDbSourceMappingConfiguration(Substitute.For<IAniDbParser>(), this.titleSelector);

            aniDbSourceMappingConfiguration.GetEpisodeMappings(0, false, false, TitleType.Localized, "en")
                .Where(m => !m.CanApply(source, target))
                .Select(m => m.TargetPropertyName)
                .Should()
                .BeEquivalentTo(nameof(target.Item.RunTimeTicks), nameof(target.Item.Overview),
                    nameof(target.Item.CommunityRating), nameof(target.Item.Studios), nameof(target.Item.Genres),
                    nameof(target.Item.Tags), nameof(target.People));
        }

        [Test]
        public void EpisodeMappings_HasMappingsForAllFields()
        {
            var expectedMappedFields = new[]
            {
                nameof(Episode.PremiereDate),
                nameof(Episode.RunTimeTicks),
                nameof(Episode.Name),
                nameof(Episode.Overview),
                nameof(Episode.CommunityRating),
                nameof(Episode.Studios),
                nameof(Episode.Genres),
                nameof(Episode.Tags),
                nameof(MetadataResult<Episode>.People)
            };

            var aniDbSourceMappingConfiguration =
                new AniDbSourceMappingConfiguration(Substitute.For<IAniDbParser>(), this.titleSelector);

            aniDbSourceMappingConfiguration.GetEpisodeMappings(0, false, false, TitleType.Localized, "en")
                .Select(m => m.TargetPropertyName)
                .Should()
                .BeEquivalentTo(expectedMappedFields);
        }

        [Test]
        public void EpisodeMappings_MapsAllFields()
        {
            var seriesSource = new AniDbSeriesData
            {
                Ratings = new[]
                {
                    new PermanentRatingData
                    {
                        Value = 45
                    }
                }
            };

            var aniDbParser = Substitute.For<IAniDbParser>();

            aniDbParser.GetGenres(seriesSource, 1, true).Returns(new List<string> { "Genre" });
            aniDbParser.GetTags(seriesSource, 1, true).Returns(new List<string> { "Tags" });
            aniDbParser.GetStudios(seriesSource).Returns(new List<string> { "Studio" });
            aniDbParser.GetPeople(seriesSource)
                .Returns(new List<PersonInfo>
                {
                    new PersonInfo
                    {
                        Name = "Person",
                        Role = "Role"
                    }
                });

            var source = new AniDbEpisodeData
            {
                AirDate = new DateTime(2017, 1, 2, 3, 4, 5),
                TotalMinutes = 35,
                Summary = "Description",
                Rating = new EpisodeRatingData { Rating = 45 }
            };

            var target = new MetadataResult<Episode>
            {
                Item = new Episode()
            };

            var aniDbSourceMappingConfiguration =
                new AniDbSourceMappingConfiguration(aniDbParser, this.titleSelector);
            var mappings = aniDbSourceMappingConfiguration.GetEpisodeMappings(1, true, true, TitleType.Localized, "en").ToList();

            mappings
                .Select(m => m.CanApply(source, target) || m.CanApply(seriesSource, target))
                .All(v => v)
                .Should()
                .BeTrue();

            mappings
                .Where(m => m.CanApply(source, target))
                .Iter(m => m.Apply(source, target));

            mappings
                .Where(m => m.CanApply(seriesSource, target))
                .Iter(m => m.Apply(seriesSource, target));

            target.Item.Name.Should().Be("SelectedTitle");
            target.Item.PremiereDate.Should().Be(new DateTime(2017, 1, 2, 3, 4, 5));
            target.Item.RunTimeTicks.Should().Be(21000000000L);
            target.Item.Overview.Should().Be("Description");
            target.Item.CommunityRating.Should().Be(45);
            target.Item.Genres.Should().BeEquivalentTo("Genre");
            target.Item.Studios.Should().BeEquivalentTo("Studio");
            target.Item.Tags.Should().BeEquivalentTo("Tags");

            target.People.Should()
                .BeEquivalentTo(new List<PersonInfo>
                {
                    new PersonInfo
                    {
                        Name = "Person",
                        Role = "Role"
                    }
                });
        }

        [Test]
        public void SeasonMappings_DontMapEmptyFields()
        {
            var source = new AniDbSeriesData
            {
                StartDate = null,
                EndDate = null,
                Description = string.Empty,
                Ratings = new PermanentRatingData[] { }
            };

            var aniDbParser = Substitute.For<IAniDbParser>();

            var target = new MetadataResult<Season>
            {
                Item = new Season()
            };

            var aniDbSourceMappingConfiguration =
                new AniDbSourceMappingConfiguration(aniDbParser, this.titleSelector);

            aniDbSourceMappingConfiguration.GetSeasonMappings(1, true, TitleType.Localized, "en")
                .Where(m => !m.CanApply(source, target))
                .Select(m => m.TargetPropertyName)
                .Should()
                .BeEquivalentTo(
                    nameof(target.Item.Name),
                    nameof(target.Item.PremiereDate),
                    nameof(target.Item.EndDate),
                    nameof(target.Item.Overview),
                    nameof(target.Item.CommunityRating),
                    nameof(target.Item.Name));
        }

        [Test]
        public void SeasonMappings_HasMappingsForAllFields()
        {
            var aniDbParser = Substitute.For<IAniDbParser>();

            var expectedMappedFields = new[]
            {
                nameof(Season.Name),
                nameof(Season.PremiereDate),
                nameof(Season.EndDate),
                nameof(Season.Overview),
                nameof(Season.CommunityRating),
                nameof(Season.Studios),
                nameof(Season.Genres),
                nameof(Season.Tags),
                nameof(Season.Name)
            };

            var aniDbSourceMappingConfiguration =
                new AniDbSourceMappingConfiguration(aniDbParser, this.titleSelector);

            aniDbSourceMappingConfiguration.GetSeasonMappings(1, true, TitleType.Localized, "en")
                .Select(m => m.TargetPropertyName)
                .Should()
                .BeEquivalentTo(expectedMappedFields);
        }

        [Test]
        public void SeasonMappings_MapsAllFields()
        {
            var source = new AniDbSeriesData
            {
                StartDate = new DateTime(2017, 1, 2, 3, 4, 5),
                EndDate = new DateTime(2017, 5, 4, 3, 2, 1),
                Description = "Description",
                Ratings = new[]
                {
                    new PermanentRatingData
                    {
                        Value = 45
                    }
                }
            };

            var aniDbParser = Substitute.For<IAniDbParser>();

            aniDbParser.FormatDescription("Description").Returns("FormattedDescription");
            aniDbParser.GetGenres(source, 1, true).Returns(new List<string> { "Genre" });
            aniDbParser.GetTags(source, 1, true).Returns(new List<string> { "Tags" });
            aniDbParser.GetStudios(source).Returns(new List<string> { "Studio" });

            var target = new MetadataResult<Season>
            {
                Item = new Season()
            };

            var aniDbSourceMappingConfiguration =
                new AniDbSourceMappingConfiguration(aniDbParser, this.titleSelector);

            aniDbSourceMappingConfiguration.GetSeasonMappings(1, true, TitleType.Localized, "en")
                .Select(m => m.CanApply(source, target))
                .Count(v => v)
                .Should()
                .Be(7);

            aniDbSourceMappingConfiguration.GetSeasonMappings(1, true, TitleType.Localized, "en")
                .Iter(m => m.Apply(source, target));

            target.Item.PremiereDate.Should().Be(new DateTime(2017, 1, 2, 3, 4, 5));
            target.Item.EndDate.Should().Be(new DateTime(2017, 5, 4, 3, 2, 1));
            target.Item.Overview.Should().Be("FormattedDescription");
            target.Item.Genres.Should().BeEquivalentTo("Genre");
            target.Item.Studios.Should().BeEquivalentTo("Studio");
            target.Item.Tags.Should().BeEquivalentTo("Tags");
            target.Item.CommunityRating.Should().Be(45);
        }

        [Test]
        public void SeriesMappings_DontMapEmptyFields()
        {
            var source = new AniDbSeriesData
            {
                StartDate = null,
                EndDate = null,
                Description = null,
                Ratings = new PermanentRatingData[] { }
            };

            var aniDbParser = Substitute.For<IAniDbParser>();

            var target = new MetadataResult<Series>
            {
                Item = new Series()
            };

            var aniDbSourceMappingConfiguration =
                new AniDbSourceMappingConfiguration(aniDbParser, this.titleSelector);

            aniDbSourceMappingConfiguration.GetSeriesMappings(1, true, true, TitleType.Localized, "en")
                .Where(m => !m.CanApply(source, target))
                .Select(m => m.TargetPropertyName)
                .Should()
                .BeEquivalentTo(nameof(target.Item.PremiereDate), nameof(target.Item.EndDate),
                    nameof(target.Item.Overview),
                    nameof(target.Item.CommunityRating));
        }

        [Test]
        public void SeriesMappings_HasMappingsForAllFields()
        {
            var aniDbParser = Substitute.For<IAniDbParser>();

            var expectedMappedFields = new[]
            {
                nameof(Series.PremiereDate),
                nameof(Series.EndDate),
                nameof(Series.Name),
                nameof(Series.Overview),
                nameof(Series.CommunityRating),
                nameof(Series.Studios),
                nameof(Series.Genres),
                nameof(Series.Tags),
                nameof(MetadataResult<Series>.People)
            };

            var aniDbSourceMappingConfiguration =
                new AniDbSourceMappingConfiguration(aniDbParser, this.titleSelector);

            aniDbSourceMappingConfiguration.GetSeriesMappings(1, true, true, TitleType.Localized, "en")
                .Select(m => m.TargetPropertyName)
                .Should()
                .BeEquivalentTo(expectedMappedFields);
        }

        [Test]
        public void SeriesMappings_MapsAllFields()
        {
            var source = new AniDbSeriesData
            {
                StartDate = new DateTime(2017, 1, 2, 3, 4, 5),
                EndDate = new DateTime(2017, 5, 4, 3, 2, 1),
                Description = "Description",
                Ratings = new[]
                {
                    new PermanentRatingData
                    {
                        Value = 45
                    }
                }
            };

            var aniDbParser = Substitute.For<IAniDbParser>();

            aniDbParser.FormatDescription("Description").Returns("FormattedDescription");
            aniDbParser.GetGenres(source, 1, true).Returns(new List<string> { "Genre" });
            aniDbParser.GetTags(source, 1, true).Returns(new List<string> { "Tags" });
            aniDbParser.GetStudios(source).Returns(new List<string> { "Studio" });
            aniDbParser.GetPeople(source)
                .Returns(new List<PersonInfo>
                {
                    new PersonInfo
                    {
                        Name = "Person",
                        Role = "Role"
                    }
                });

            var target = new MetadataResult<Series>
            {
                Item = new Series()
            };

            var aniDbSourceMappingConfiguration =
                new AniDbSourceMappingConfiguration(aniDbParser, this.titleSelector);

            aniDbSourceMappingConfiguration.GetSeriesMappings(1, true, true, TitleType.Localized, "en")
                .Select(m => m.CanApply(source, target))
                .All(v => v)
                .Should()
                .BeTrue();

            aniDbSourceMappingConfiguration.GetSeriesMappings(1, true, true, TitleType.Localized, "en")
                .Iter(m => m.Apply(source, target));

            target.Item.Name.Should().Be("SelectedTitle");
            target.Item.PremiereDate.Should().Be(new DateTime(2017, 1, 2, 3, 4, 5));
            target.Item.EndDate.Should().Be(new DateTime(2017, 5, 4, 3, 2, 1));
            target.Item.Overview.Should().Be("FormattedDescription");
            target.Item.Genres.Should().BeEquivalentTo("Genre");
            target.Item.Studios.Should().BeEquivalentTo("Studio");
            target.Item.Tags.Should().BeEquivalentTo("Tags");
            target.Item.CommunityRating.Should().Be(45);

            target.People.Should()
                .BeEquivalentTo(new List<PersonInfo>
                {
                    new PersonInfo
                    {
                        Name = "Person",
                        Role = "Role"
                    }
                });
        }
    }
}
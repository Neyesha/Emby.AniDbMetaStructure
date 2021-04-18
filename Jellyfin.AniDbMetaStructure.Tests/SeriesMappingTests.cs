using System.Linq;
using Jellyfin.AniDbMetaStructure.Mapping;
using Jellyfin.AniDbMetaStructure.Mapping.Data;
using FluentAssertions;
using LanguageExt.UnsafeValueAccess;
using NUnit.Framework;

namespace Jellyfin.AniDbMetaStructure.Tests
{
    [TestFixture]
    public class SeriesMappingTests
    {
        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("4444")]
        [TestCase(";;;;")]
        [TestCase(";1;4;")]
        public void Create_InvalidSpecialEpisodePositionsString_EmptySpecialEpisodePositions(
            string specialEpisodePositionsString)
        {
            var data = new AniDbSeriesMappingData
            {
                AnidbId = "3",
                DefaultTvDbSeason = "35",
                SpecialEpisodePositionsString = specialEpisodePositionsString
            };

            var seriesMapping = SeriesMapping.FromData(data);

            seriesMapping.ValueUnsafe().SpecialEpisodePositions.Should().BeEmpty();
        }

        [Test]
        public void Create_OneSpecialEpisodePositionsString_CreatesOneSpecialEpisodePosition()
        {
            var data = new AniDbSeriesMappingData
            {
                AnidbId = "3",
                DefaultTvDbSeason = "35",
                SpecialEpisodePositionsString = ";5-3;"
            };

            var seriesMapping = SeriesMapping.FromData(data);

            seriesMapping.ValueUnsafe().SpecialEpisodePositions.Should().HaveCount(1);

            var episodeMapping = seriesMapping.ValueUnsafe().SpecialEpisodePositions.Single();

            episodeMapping.SpecialEpisodeIndex.Should().Be(5);
            episodeMapping.FollowingStandardEpisodeIndex.Should().Be(3);
        }

        [Test]
        public void Create_TwoSpecialEpisodePositionsStrings_CreatesTwoSpecialEpisodePositions()
        {
            var data = new AniDbSeriesMappingData
            {
                AnidbId = "3",
                DefaultTvDbSeason = "35",
                SpecialEpisodePositionsString = ";5-3;22-55;"
            };

            var seriesMapping = SeriesMapping.FromData(data);

            seriesMapping.ValueUnsafe().SpecialEpisodePositions.Should().HaveCount(2);

            var episodeMapping = seriesMapping.ValueUnsafe().SpecialEpisodePositions.First();

            episodeMapping.SpecialEpisodeIndex.Should().Be(5);
            episodeMapping.FollowingStandardEpisodeIndex.Should().Be(3);

            episodeMapping = seriesMapping.ValueUnsafe().SpecialEpisodePositions.Last();

            episodeMapping.SpecialEpisodeIndex.Should().Be(22);
            episodeMapping.FollowingStandardEpisodeIndex.Should().Be(55);
        }

        [Test]
        public void FromData_AbsoluteTvDbSeason_ReturnsAbsoluteTvDbSeason()
        {
            var data = new AniDbSeriesMappingData
            {
                AnidbId = "3",
                DefaultTvDbSeason = "a"
            };

            var seriesMapping = SeriesMapping.FromData(data);

            seriesMapping.IsSome.Should().BeTrue();
            seriesMapping.ValueUnsafe().DefaultTvDbSeason.IsLeft.Should().BeTrue();
        }

        [Test]
        public void FromData_DefaultTvDbEpisodeIndexOffset_ReturnsValue()
        {
            var data = new AniDbSeriesMappingData
            {
                AnidbId = "3",
                DefaultTvDbSeason = "35",
                EpisodeOffset = 55
            };

            var seriesMapping = SeriesMapping.FromData(data);

            seriesMapping.IsSome.Should().BeTrue();
            seriesMapping.ValueUnsafe().DefaultTvDbEpisodeIndexOffset.Should().Be(55);
        }

        [Test]
        public void FromData_EmptyGroupMappingList_CreatesNoEpisodeGroupMappings()
        {
            var data = new AniDbSeriesMappingData
            {
                AnidbId = "3",
                DefaultTvDbSeason = "35",
                GroupMappingList = new AnimeEpisodeGroupMappingData[0]
            };

            var seriesMapping = SeriesMapping.FromData(data);

            seriesMapping.ValueUnsafe().EpisodeGroupMappings.Should().BeEmpty();
        }

        [Test]
        public void FromData_IntegerAniDbId_ReturnsAniDbSeriesId()
        {
            var data = new AniDbSeriesMappingData
            {
                AnidbId = "3",
                DefaultTvDbSeason = "a"
            };

            var seriesMapping = SeriesMapping.FromData(data);

            seriesMapping.IsSome.Should().BeTrue();
            seriesMapping.ValueUnsafe().Ids.AniDbSeriesId.Should().Be(3);
        }

        [Test]
        public void FromData_IntegerTvDbSeason_ReturnsTvDbSeasonIndex()
        {
            var data = new AniDbSeriesMappingData
            {
                AnidbId = "3",
                DefaultTvDbSeason = "35"
            };

            var seriesMapping = SeriesMapping.FromData(data);

            seriesMapping.IsSome.Should().BeTrue();
            seriesMapping.ValueUnsafe().DefaultTvDbSeason.IsRight.Should().BeTrue();

            var tvDbSeasonIndex = -1;

            seriesMapping.ValueUnsafe().DefaultTvDbSeason.IfRight(s => tvDbSeasonIndex = s.Index);
            tvDbSeasonIndex.Should().Be(35);
        }

        [Test]
        [TestCase(null)]
        [TestCase("And")]
        [TestCase("")]
        [TestCase("999999999999")]
        public void FromData_InvalidAniDbId_ReturnsNone(string aniDbId)
        {
            var data = new AniDbSeriesMappingData
            {
                AnidbId = aniDbId,
                DefaultTvDbSeason = "a"
            };

            var seriesMapping = SeriesMapping.FromData(data);

            seriesMapping.IsSome.Should().BeFalse();
        }

        [Test]
        [TestCase(null)]
        [TestCase("And")]
        [TestCase("")]
        [TestCase("999999999999")]
        public void FromData_InvalidTvDbSeason_ReturnsNone(string tvDbSeason)
        {
            var data = new AniDbSeriesMappingData
            {
                AnidbId = "3",
                DefaultTvDbSeason = tvDbSeason
            };

            var seriesMapping = SeriesMapping.FromData(data);

            seriesMapping.IsSome.Should().BeFalse();
        }

        [Test]
        public void FromData_MultipleEpisodeGroupMappings_CreatesEpisodeGroupMappings()
        {
            var data = new AniDbSeriesMappingData
            {
                AnidbId = "3",
                DefaultTvDbSeason = "35",
                GroupMappingList = new[]
                {
                    new AnimeEpisodeGroupMappingData(),
                    new AnimeEpisodeGroupMappingData()
                }
            };

            var seriesMapping = SeriesMapping.FromData(data);

            seriesMapping.ValueUnsafe().EpisodeGroupMappings.Should().HaveCount(2);
        }

        [Test]
        public void FromData_NullData_ReturnsNone()
        {
            var seriesMapping = SeriesMapping.FromData(null);

            seriesMapping.IsSome.Should().BeFalse();
        }

        [Test]
        public void FromData_NullGroupMappingList_SubstitutesEmptyList()
        {
            var data = new AniDbSeriesMappingData
            {
                AnidbId = "3",
                DefaultTvDbSeason = "35"
            };

            var seriesMapping = SeriesMapping.FromData(data);

            seriesMapping.ValueUnsafe().EpisodeGroupMappings.Should().BeEmpty();
        }

        [Test]
        public void FromData_NullItemsGroupMappingList_DoesNotCreateEpisodeGroupMapping()
        {
            var data = new AniDbSeriesMappingData
            {
                AnidbId = "3",
                DefaultTvDbSeason = "35",
                GroupMappingList = new AnimeEpisodeGroupMappingData[5]
            };

            var seriesMapping = SeriesMapping.FromData(data);

            seriesMapping.ValueUnsafe().EpisodeGroupMappings.Should().BeEmpty();
        }

        [Test]
        public void FromData_OneEpisodeGroupMapping_CreatesEpisodeGroupMapping()
        {
            var data = new AniDbSeriesMappingData
            {
                AnidbId = "3",
                DefaultTvDbSeason = "35",
                GroupMappingList = new[]
                {
                    new AnimeEpisodeGroupMappingData()
                }
            };

            var seriesMapping = SeriesMapping.FromData(data);

            seriesMapping.ValueUnsafe().EpisodeGroupMappings.Should().HaveCount(1);
        }
    }
}
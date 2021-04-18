using System.Collections.Generic;
using System.Threading.Tasks;
using Jellyfin.AniDbMetaStructure.AniDb.SeriesData;
using Jellyfin.AniDbMetaStructure.Mapping;
using Jellyfin.AniDbMetaStructure.Tests.TestData;
using FluentAssertions;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using NSubstitute;
using NUnit.Framework;

namespace Jellyfin.AniDbMetaStructure.Tests
{
    [TestFixture]
    public class EpisodeMapperTests
    {
        [SetUp]
        public void Setup()
        {
            this.defaultSeasonEpisodeMapper = Substitute.For<IDefaultSeasonEpisodeMapper>();
            this.groupMappingEpisodeMapper = Substitute.For<IGroupMappingEpisodeMapper>();

            this.seriesMapping = new SeriesMapping(new SeriesIds(123, Option<int>.None, Option<int>.None, Option<int>.None),
                new AbsoluteTvDbSeason(), 2, new List<EpisodeGroupMapping>(), new List<SpecialEpisodePosition>());
        }

        private IDefaultSeasonEpisodeMapper defaultSeasonEpisodeMapper;
        private IGroupMappingEpisodeMapper groupMappingEpisodeMapper;
        private SeriesMapping seriesMapping;

        [Test]
        public async Task MapAniDbEpisodeAsync_GroupMappingWithoutTvDbSeriesId_ReturnsNone()
        {
            var episodeMapper = new EpisodeMapper(this.defaultSeasonEpisodeMapper, this.groupMappingEpisodeMapper);

            var episodeData = await episodeMapper.MapAniDbEpisodeAsync(3, this.seriesMapping,
                    new EpisodeGroupMapping(1, 1, 3, 1, 1, new List<EpisodeMapping>()))
                .ToOption();

            episodeData.IsNone.Should().BeTrue();
        }

        [Test]
        public async Task MapAniDbEpisodeAsync_GroupMappingWithTvDbSeriesId_UsesGroupMappingEpisodeMapper()
        {
            this.seriesMapping = new SeriesMapping(new SeriesIds(123, 523, Option<int>.None, Option<int>.None),
                new AbsoluteTvDbSeason(), 2, new List<EpisodeGroupMapping>(), new List<SpecialEpisodePosition>());

            var episodeGroupMapping = new EpisodeGroupMapping(1, 1, 3, 1, 1, new List<EpisodeMapping>());

            var expected = TvDbTestData.Episode(3);
            this.groupMappingEpisodeMapper.MapAniDbEpisodeAsync(3, episodeGroupMapping, 523).Returns(expected);

            var episodeMapper = new EpisodeMapper(this.defaultSeasonEpisodeMapper, this.groupMappingEpisodeMapper);

            var episodeData =
                await episodeMapper.MapAniDbEpisodeAsync(3, this.seriesMapping, episodeGroupMapping).ToOption();

            episodeData.ValueUnsafe().Should().Be(expected);
        }

        [Test]
        public async Task MapAniDbEpisodeAsync_NoGroupMapping_UsesDefaultSeasonMapper()
        {
            var expected = TvDbTestData.Episode(3);
            this.defaultSeasonEpisodeMapper.MapEpisodeAsync(3, this.seriesMapping).Returns(expected);

            var episodeMapper = new EpisodeMapper(this.defaultSeasonEpisodeMapper, this.groupMappingEpisodeMapper);

            var episodeData = await episodeMapper
                .MapAniDbEpisodeAsync(3, this.seriesMapping, Option<EpisodeGroupMapping>.None)
                .ToOption();

            episodeData.ValueUnsafe().Should().Be(expected);
        }

        [Test]
        public async Task MapTvDbEpisodeAsync_UsesGroupMappingEpisodeMapper()
        {
            this.seriesMapping = new SeriesMapping(new SeriesIds(324, 523, Option<int>.None, Option<int>.None),
                new AbsoluteTvDbSeason(), 2, new List<EpisodeGroupMapping>(), new List<SpecialEpisodePosition>());

            var episodeGroupMapping = new EpisodeGroupMapping(1, 1, 3, 1, 1, new List<EpisodeMapping>());

            var expected = new AniDbEpisodeData();
            this.groupMappingEpisodeMapper.MapTvDbEpisodeAsync(3, episodeGroupMapping, 324).Returns(expected);

            var episodeMapper = new EpisodeMapper(this.defaultSeasonEpisodeMapper, this.groupMappingEpisodeMapper);

            var episodeData =
                await episodeMapper.MapTvDbEpisodeAsync(3, this.seriesMapping, episodeGroupMapping).ToOption();

            episodeData.ValueUnsafe().Should().Be(expected);
        }
    }
}
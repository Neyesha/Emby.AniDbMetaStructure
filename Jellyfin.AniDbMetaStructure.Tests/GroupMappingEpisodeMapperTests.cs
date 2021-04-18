using System.Collections.Generic;
using System.Threading.Tasks;
using Emby.AniDbMetaStructure.AniDb;
using Emby.AniDbMetaStructure.AniDb.SeriesData;
using Emby.AniDbMetaStructure.Mapping;
using Emby.AniDbMetaStructure.Tests.TestHelpers;
using Emby.AniDbMetaStructure.TvDb;
using Emby.AniDbMetaStructure.TvDb.Data;
using FluentAssertions;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using NSubstitute;
using NUnit.Framework;

namespace Emby.AniDbMetaStructure.Tests
{
    [TestFixture]
    public class GroupMappingEpisodeMapperTests
    {
        private GroupMappingEpisodeMapper mapper;
        private ITvDbClient tvDbClient;
        private IAniDbClient aniDbClient;

        private TvDbEpisodeData tvDbEpisodeA;
        private TvDbEpisodeData tvDbEpisodeB;
        private AniDbEpisodeData aniDbEpisodeA;
        private AniDbEpisodeData aniDbEpisodeB;

        [SetUp]
        public void Setup()
        {
            this.tvDbClient = Substitute.For<ITvDbClient>();
            this.aniDbClient = Substitute.For<IAniDbClient>();

            this.tvDbEpisodeA = TestData.TvDbTestData.Episode(12, 48, 2);
            this.tvDbEpisodeB = TestData.TvDbTestData.Episode(45, 7, 2);
            this.aniDbEpisodeA = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "44",
                    RawType = 1
                }
            };
            this.aniDbEpisodeB = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "23",
                    RawType = 1
                }
            };

            this.tvDbClient.GetEpisodesAsync(123)
                .Returns(Option<List<TvDbEpisodeData>>.Some(new List<TvDbEpisodeData> { this.tvDbEpisodeA, this.tvDbEpisodeB }));
            this.aniDbClient.GetSeriesAsync(77)
                .Returns(Option<AniDbSeriesData>.Some(new AniDbSeriesData
                {
                    Episodes = new[] { this.aniDbEpisodeA, this.aniDbEpisodeB }
                }));

            this.mapper = new GroupMappingEpisodeMapper(this.tvDbClient, this.aniDbClient, new ConsoleLogManager());
        }

        [Test]
        public async Task MapAniDbEpisodeAsync_NoEpisodeMapping_MapsUsingOffset()
        {
            var groupMapping = new EpisodeGroupMapping(1, 2, 4, null, null, new List<EpisodeMapping>());

            var result = await this.mapper.MapAniDbEpisodeAsync(44, groupMapping, 123).ToOption();

            result.IsSome.Should().BeTrue();
            result.ValueUnsafe().Should().Be(this.tvDbEpisodeA);
        }

        [Test]
        public async Task MapAniDbEpisodeAsync_HasEpisodeMapping_MapsUsingEpisodeMapping()
        {
            var groupMapping = new EpisodeGroupMapping(1, 2, 4, null, null, new List<EpisodeMapping>
            {
                new EpisodeMapping(44, 7)
            });

            var result = await this.mapper.MapAniDbEpisodeAsync(44, groupMapping, 123).ToOption();

            result.IsSome.Should().BeTrue();
            result.ValueUnsafe().Should().Be(this.tvDbEpisodeB);
        }

        [Test]
        public async Task MapAniDbEpisodeAsync_NoEpisodeInTvDbData_ReturnsNone()
        {
            var groupMapping = new EpisodeGroupMapping(1, 2, 4, null, null, new List<EpisodeMapping>());

            var result = await this.mapper.MapAniDbEpisodeAsync(1, groupMapping, 123).ToOption();

            result.IsSome.Should().BeFalse();
        }

        [Test]
        public async Task MapTvDbEpisodeAsync_NoEpisodeMapping_MapsUsingOffset()
        {
            var groupMapping = new EpisodeGroupMapping(1, 2, 4, null, null, new List<EpisodeMapping>());

            var result = await this.mapper.MapTvDbEpisodeAsync(48, groupMapping, 77).ToOption();

            result.IsSome.Should().BeTrue();
            result.ValueUnsafe().Should().Be(this.aniDbEpisodeA);
        }

        [Test]
        public async Task MapTvDbEpisodeAsync_HasEpisodeMapping_MapsUsingEpisodeMapping()
        {
            var groupMapping = new EpisodeGroupMapping(1, 2, 4, null, null, new List<EpisodeMapping>
            {
                new EpisodeMapping(23, 7)
            });

            var result = await this.mapper.MapTvDbEpisodeAsync(7, groupMapping, 77).ToOption();

            result.IsSome.Should().BeTrue();
            result.ValueUnsafe().Should().Be(this.aniDbEpisodeB);
        }

        [Test]
        public async Task MapTvDbEpisodeAsync_NoEpisodeInAniDbData_ReturnsNone()
        {
            var groupMapping = new EpisodeGroupMapping(1, 2, 4, null, null, new List<EpisodeMapping>());

            var result = await this.mapper.MapTvDbEpisodeAsync(1, groupMapping, 77).ToOption();

            result.IsSome.Should().BeFalse();
        }
    }
}

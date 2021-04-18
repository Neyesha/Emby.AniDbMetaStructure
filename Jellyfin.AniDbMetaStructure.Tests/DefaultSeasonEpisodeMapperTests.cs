using System.Threading.Tasks;
using Jellyfin.AniDbMetaStructure.Mapping;
using Jellyfin.AniDbMetaStructure.Tests.TestData;
using Jellyfin.AniDbMetaStructure.Tests.TestHelpers;
using Jellyfin.AniDbMetaStructure.TvDb;
using FluentAssertions;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using MediaBrowser.Model.Logging;
using NSubstitute;
using NUnit.Framework;

namespace Emby.AniDbMetaStructure.Tests
{
    [TestFixture]
    public class DefaultSeasonEpisodeMapperTests
    {
        [SetUp]
        public void Setup()
        {
            this.logManager = new ConsoleLogManager();
            this.tvDbClient = Substitute.For<ITvDbClient>();
            this.seriesMapping = Substitute.For<ISeriesMapping>();
        }

        private ILogManager logManager;
        private ITvDbClient tvDbClient;
        private ISeriesMapping seriesMapping;

        [Test]
        public async Task MapEpisodeAsync_AbsoluteDefaultTvDbSeason_MapsOnAbsoluteEpisodeIndex()
        {
            this.seriesMapping.Ids.Returns(new SeriesIds(12, 33, Option<int>.None, Option<int>.None));
            this.seriesMapping.DefaultTvDbSeason.Returns(new AbsoluteTvDbSeason());

            var tvDbEpisodeData = TvDbTestData.Episode(3);

            this.tvDbClient.GetEpisodeAsync(33, 10).Returns(tvDbEpisodeData);

            var defaultSeasonEpisodeMapper = new DefaultSeasonEpisodeMapper(this.tvDbClient, this.logManager);

            var result = await defaultSeasonEpisodeMapper.MapEpisodeAsync(10, this.seriesMapping);

            await this.tvDbClient.Received(1).GetEpisodeAsync(33, 10);
            result.ValueUnsafe().Should().Be(tvDbEpisodeData);
        }

        [Test]
        public async Task MapEpisodeAsync_AbsoluteDefaultTvDbSeason_NoEpisodeData_ReturnsNone()
        {
            this.seriesMapping.Ids.Returns(new SeriesIds(12, 33, Option<int>.None, Option<int>.None));
            this.seriesMapping.DefaultTvDbSeason.Returns(new AbsoluteTvDbSeason());

            var defaultSeasonEpisodeMapper = new DefaultSeasonEpisodeMapper(this.tvDbClient, this.logManager);

            var result = await defaultSeasonEpisodeMapper.MapEpisodeAsync(10, this.seriesMapping);

            result.IsNone.Should().BeTrue();
        }

        [Test]
        public async Task MapEpisodeAsync_DefaultTvDbSeason_MapsOnSeasonAndEpisodeIndex()
        {
            this.seriesMapping.Ids.Returns(new SeriesIds(12, 33, Option<int>.None, Option<int>.None));
            this.seriesMapping.DefaultTvDbSeason.Returns(new TvDbSeason(54));
            this.seriesMapping.DefaultTvDbEpisodeIndexOffset.Returns(7);

            var tvDbEpisodeData = TvDbTestData.Episode(3);

            this.tvDbClient.GetEpisodeAsync(33, 54, 17).Returns(tvDbEpisodeData);

            var defaultSeasonEpisodeMapper = new DefaultSeasonEpisodeMapper(this.tvDbClient, this.logManager);

            var result = await defaultSeasonEpisodeMapper.MapEpisodeAsync(10, this.seriesMapping);

            await this.tvDbClient.Received(1).GetEpisodeAsync(33, 54, 17);
            result.ValueUnsafe().Should().Be(tvDbEpisodeData);
        }

        [Test]
        public async Task MapEpisodeAsync_DefaultTvDbSeason_NoEpisodeData_ReturnsNone()
        {
            this.seriesMapping.Ids.Returns(new SeriesIds(12, 33, Option<int>.None, Option<int>.None));
            this.seriesMapping.DefaultTvDbSeason.Returns(new TvDbSeason(54));
            this.seriesMapping.DefaultTvDbEpisodeIndexOffset.Returns(7);

            var defaultSeasonEpisodeMapper = new DefaultSeasonEpisodeMapper(this.tvDbClient, this.logManager);

            var result = await defaultSeasonEpisodeMapper.MapEpisodeAsync(10, this.seriesMapping);

            result.IsNone.Should().BeTrue();
        }

        [Test]
        public async Task MapEpisodeAsync_NoTvDbSeriesId_ReturnsNone()
        {
            this.seriesMapping.Ids.Returns(new SeriesIds(12, Option<int>.None, Option<int>.None, Option<int>.None));

            var defaultSeasonEpisodeMapper = new DefaultSeasonEpisodeMapper(this.tvDbClient, this.logManager);

            var result = await defaultSeasonEpisodeMapper.MapEpisodeAsync(10, this.seriesMapping);

            result.IsNone.Should().BeTrue();
        }
    }
}
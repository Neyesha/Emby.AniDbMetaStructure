using FluentAssertions;
using Jellyfin.AniDbMetaStructure.Files;
using Jellyfin.AniDbMetaStructure.Mapping;
using Jellyfin.AniDbMetaStructure.Mapping.Data;
using Jellyfin.AniDbMetaStructure.Tests.TestHelpers;
using MediaBrowser.Common.Configuration;
using NSubstitute;
using NUnit.Framework;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xem.Api;

namespace Jellyfin.AniDbMetaStructure.Tests
{
    [TestFixture]
    public class MappingListTests
    {
        [SetUp]
        public void Setup()
        {
            this.applicationPaths = Substitute.For<IApplicationPaths>();
            this.fileCache = Substitute.For<IFileCache>();
            this.apiClient = Substitute.For<IApiClient>();

            this.mappingListData = new AnimeMappingListData();

            this.applicationPaths.CachePath.Returns(string.Empty);
            this.fileCache.GetFileContentAsync(Arg.Is<MappingsFileSpec>(s => s.LocalPath == "anime-list.xml"),
                    CancellationToken.None)
                .Returns(x => this.mappingListData);

            this.mappingList = new MappingList(this.applicationPaths, this.fileCache, this.apiClient);
        }

        private IApplicationPaths applicationPaths;
        private IFileCache fileCache;
        private MappingList mappingList;
        private AnimeMappingListData mappingListData;
        private IApiClient apiClient;

        private AniDbSeriesMappingData MappingData(int aniDbSeriesId, int tvDbSeriesId)
        {
            return new AniDbSeriesMappingData
            {
                AnidbId = aniDbSeriesId.ToString(),
                TvDbId = tvDbSeriesId.ToString(),
                DefaultTvDbSeason = "1"
            };
        }

        [Test]
        public async Task GetSeriesMappingFromAniDb_MatchingData_ReturnsSeriesMapping()
        {
            this.mappingListData.AnimeSeriesMapping = new[]
            {
                MappingData(56, 1)
            };

            var result = await this.mappingList.GetSeriesMappingFromAniDb(56, TestProcessResultContext.Instance);

            result.IsRight.Should().BeTrue();
            result.IfRight(r => r.Ids.AniDbSeriesId.Should().Be(56));
        }

        [Test]
        public async Task GetSeriesMappingFromAniDb_MultipleMatchingData_ThrowsException()
        {
            this.mappingListData.AnimeSeriesMapping = new[]
            {
                MappingData(56, 1),
                MappingData(56, 2)
            };

            var result = await this.mappingList.GetSeriesMappingFromAniDb(56, TestProcessResultContext.Instance);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("Multiple series mappings match AniDb series Id '56'"));
        }

        [Test]
        public async Task GetSeriesMappingFromAniDb_NoMatchingData_ReturnsNone()
        {
            this.mappingListData.AnimeSeriesMapping = new[]
            {
                MappingData(5, 1)
            };

            var result = await this.mappingList.GetSeriesMappingFromAniDb(56, TestProcessResultContext.Instance);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("No series mapping for AniDb series Id '56'"));
        }

        [Test]
        public async Task GetSeriesMappingFromTvDb_MatchingData_ReturnsSeriesMapping()
        {
            this.mappingListData.AnimeSeriesMapping = new[]
            {
                MappingData(5, 56)
            };

            var result = await this.mappingList.GetSeriesMappingsFromTvDb(56, TestProcessResultContext.Instance);

            result.IsRight.Should().BeTrue();
            result.IfRight(r => r.Select(m => m.Ids.AniDbSeriesId).Should().BeEquivalentTo(5));
        }

        [Test]
        public async Task GetSeriesMappingFromTvDb_MultipleMatchingData_ReturnsAll()
        {
            this.mappingListData.AnimeSeriesMapping = new[]
            {
                MappingData(12, 56),
                MappingData(42, 56)
            };

            var result = await this.mappingList.GetSeriesMappingsFromTvDb(56, TestProcessResultContext.Instance);

            result.IsRight.Should().BeTrue();
            result.IfRight(r => r.Select(m => m.Ids.AniDbSeriesId).Should().BeEquivalentTo(12, 42));
        }

        [Test]
        public async Task GetSeriesMappingFromTvDb_NoMatchingData_ReturnsNone()
        {
            this.mappingListData.AnimeSeriesMapping = new[]
            {
                MappingData(5, 25)
            };

            var result = await this.mappingList.GetSeriesMappingsFromTvDb(56, TestProcessResultContext.Instance);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("No series mapping for TvDb series Id '56'"));
        }
    }
}
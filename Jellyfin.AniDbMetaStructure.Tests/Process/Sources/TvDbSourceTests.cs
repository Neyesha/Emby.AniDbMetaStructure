using System.Collections.Generic;
using System.Threading.Tasks;
using Jellyfin.AniDbMetaStructure.Process;
using Jellyfin.AniDbMetaStructure.Process.Sources;
using Jellyfin.AniDbMetaStructure.SourceDataLoaders;
using Jellyfin.AniDbMetaStructure.Tests.TestData;
using Jellyfin.AniDbMetaStructure.Tests.TestHelpers;
using Jellyfin.AniDbMetaStructure.TvDb;
using Jellyfin.AniDbMetaStructure.TvDb.Data;
using FluentAssertions;
using LanguageExt;
using NSubstitute;
using NUnit.Framework;

namespace Emby.AniDbMetaStructure.Tests.Process.Sources
{
    [TestFixture]
    public class TvDbSourceTests
    {
        [SetUp]
        public virtual void Setup()
        {
            this.tvDbClient = Substitute.For<ITvDbClient>();
            this.loaders = new List<IEmbySourceDataLoader>();

            this.tvDbSource = new TvDbSource(this.tvDbClient, this.loaders);
        }

        private ITvDbClient tvDbClient;
        private TvDbSource tvDbSource;
        private IList<IEmbySourceDataLoader> loaders;

        private EmbyItemData EmbyItemData(string name, int? parentTvDbSeriesId)
        {
            var parentIds = new List<EmbyItemId>();

            if (parentTvDbSeriesId.HasValue)
            {
                parentIds.Add(new EmbyItemId(MediaItemTypes.Series, SourceNames.TvDb, parentTvDbSeriesId.Value));
            }

            return new EmbyItemData(MediaItemTypes.Episode,
                new ItemIdentifier(Option<int>.None, Option<int>.None, name),
                null, "en", parentIds);
        }

        private EmbyItemData SeriesEmbyItemData(string name, int? tvDbSeriesId)
        {
            var existingIds = new Dictionary<string, int>();

            if (tvDbSeriesId.HasValue)
            {
                existingIds.Add(SourceNames.TvDb, tvDbSeriesId.Value);
            }

            return new EmbyItemData(MediaItemTypes.Series,
                new ItemIdentifier(Option<int>.None, Option<int>.None, name),
                existingIds, "en", new List<EmbyItemId>());
        }

        [Test]
        public void Name_ReturnsTvDbSourceName()
        {
            this.tvDbSource.Name.Should().BeSameAs(SourceNames.TvDb);
        }

        [Test]
        [TestCaseSource(typeof(MediaItemTypeTestCases))]
        public void GetEmbySourceDataLoader_MatchingLoader_ReturnsLoader(IMediaItemType mediaItemType)
        {
            var loader = Substitute.For<IEmbySourceDataLoader>();
            loader.SourceName.Returns(SourceNames.TvDb);
            loader.CanLoadFrom(mediaItemType).Returns(true);

            this.loaders.Add(loader);

            var result = this.tvDbSource.GetEmbySourceDataLoader(mediaItemType);

            result.IsRight.Should().BeTrue();
            result.IfRight(r => r.Should().BeSameAs(loader));
        }

        [Test]
        [TestCaseSource(typeof(MediaItemTypeTestCases))]
        public void GetEmbySourceDataLoader_NoMatchingLoader_ReturnsFailed(IMediaItemType mediaItemType)
        {
            var sourceMismatch = Substitute.For<IEmbySourceDataLoader>();
            sourceMismatch.SourceName.Returns(SourceNames.AniDb);
            sourceMismatch.CanLoadFrom(mediaItemType).Returns(true);

            var cannotLoad = Substitute.For<IEmbySourceDataLoader>();
            cannotLoad.SourceName.Returns(SourceNames.TvDb);
            cannotLoad.CanLoadFrom(mediaItemType).Returns(false);

            this.loaders.Add(sourceMismatch);
            this.loaders.Add(cannotLoad);

            var result = this.tvDbSource.GetEmbySourceDataLoader(mediaItemType);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("No Emby source data loader for this source and media item type"));
        }

        [Test]
        public async Task GetSeriesData_SeriesMediaItem_NoExistingId_ReturnsFailed()
        {
            var embyItemData = this.SeriesEmbyItemData("Name", null);

            var result = await this.tvDbSource.GetSeriesData(embyItemData, TestProcessResultContext.Instance);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("No TvDb Id found on this series"));
        }

        [Test]
        public async Task GetSeriesData_SeriesMediaItem_NoSeriesLoaded_ReturnsFailed()
        {
            var embyItemData = this.SeriesEmbyItemData("Name", 56);

            this.tvDbClient.GetSeriesAsync(56).Returns(Option<TvDbSeriesData>.None);

            var result = await this.tvDbSource.GetSeriesData(embyItemData, TestProcessResultContext.Instance);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("Failed to load parent series with TvDb Id '56'"));
        }

        [Test]
        public async Task GetSeriesData_NoTvDbIdOnParent_ReturnsFailed()
        {
            var embyItemData = this.EmbyItemData("Name", null);

            var result = await this.tvDbSource.GetSeriesData(embyItemData, TestProcessResultContext.Instance);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("No TvDb Id found on parent series"));
        }

        [Test]
        public async Task GetSeriesData_NoSeriesLoaded_ReturnsFailed()
        {
            var embyItemData = this.EmbyItemData("Name", 56);

            this.tvDbClient.GetSeriesAsync(56).Returns(Option<TvDbSeriesData>.None);

            var result = await this.tvDbSource.GetSeriesData(embyItemData, TestProcessResultContext.Instance);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("Failed to load parent series with TvDb Id '56'"));
        }

        [Test]
        public async Task GetSeriesData_SeriesMediaItem_ReturnsSeries()
        {
            var embyItemData = this.SeriesEmbyItemData("Name", 56);

            var seriesData = TvDbTestData.Series(56, "Name");

            this.tvDbClient.GetSeriesAsync(56).Returns(Option<TvDbSeriesData>.Some(seriesData));

            var result = await this.tvDbSource.GetSeriesData(embyItemData, TestProcessResultContext.Instance);

            result.IsRight.Should().BeTrue();
            result.IfRight(r => r.Should().BeSameAs(seriesData));
        }

        [Test]
        public async Task GetSeriesData_ReturnsSeries()
        {
            var embyItemData = this.EmbyItemData("Name", 56);

            var seriesData = TvDbTestData.Series(56, "Name");

            this.tvDbClient.GetSeriesAsync(56).Returns(Option<TvDbSeriesData>.Some(seriesData));

            var result = await this.tvDbSource.GetSeriesData(embyItemData, TestProcessResultContext.Instance);

            result.IsRight.Should().BeTrue();
            result.IfRight(r => r.Should().BeSameAs(seriesData));
        }
    }
}
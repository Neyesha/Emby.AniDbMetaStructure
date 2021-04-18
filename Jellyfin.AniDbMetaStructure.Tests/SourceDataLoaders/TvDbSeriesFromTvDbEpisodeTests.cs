using System.Threading.Tasks;
using Jellyfin.AniDbMetaStructure.Process;
using Jellyfin.AniDbMetaStructure.Process.Sources;
using Jellyfin.AniDbMetaStructure.SourceDataLoaders;
using Jellyfin.AniDbMetaStructure.Tests.TestData;
using Jellyfin.AniDbMetaStructure.TvDb.Data;
using FluentAssertions;
using LanguageExt;
using NSubstitute;
using NUnit.Framework;
using static LanguageExt.Prelude;

namespace Jellyfin.AniDbMetaStructure.Tests.SourceDataLoaders
{
    [TestFixture]
    public class TvDbSeriesFromTvDbEpisodeTests
    {
        [SetUp]
        public void Setup()
        {
            this.tvDbSource = Substitute.For<ITvDbSource>();
            this.tvDbSource.Name.Returns(SourceNames.TvDb);

            this.sources = Substitute.For<ISources>();
            this.sources.TvDb.Returns(this.tvDbSource);

            var embyItemData = Substitute.For<IEmbyItemData>();
            embyItemData.Identifier.Returns(new ItemIdentifier(67, 1, "Name"));
            embyItemData.Language.Returns("en");

            this.mediaItem = Substitute.For<IMediaItem>();
            this.mediaItem.EmbyData.Returns(embyItemData);

            this.tvDbSeriesData = TvDbTestData.Series(3, "Title");

            this.tvDbSource.GetSeriesData(this.mediaItem.EmbyData, Arg.Any<ProcessResultContext>())
                .Returns(this.tvDbSeriesData);
        }

        private ISources sources;
        private TvDbSeriesData tvDbSeriesData;
        private ITvDbSource tvDbSource;
        private IMediaItem mediaItem;

        [Test]
        public void CanLoadFrom_CorrectItemType_IsTrue()
        {
            var sourceData = Substitute.For<ISourceData<TvDbEpisodeData>>();

            var loader = new TvDbSeriesFromTvDbEpisode(this.sources);

            loader.CanLoadFrom(sourceData).Should().BeTrue();
        }

        [Test]
        public void CanLoadFrom_Null_IsFalse()
        {
            var loader = new TvDbSeriesFromTvDbEpisode(this.sources);

            loader.CanLoadFrom(null).Should().BeFalse();
        }

        [Test]
        public void CanLoadFrom_WrongItemType_IsFalse()
        {
            var sourceData = Substitute.For<ISourceData<TvDbSeriesData>>();

            var loader = new TvDbSeriesFromTvDbEpisode(this.sources);

            loader.CanLoadFrom(sourceData).Should().BeFalse();
        }

        [Test]
        public async Task LoadFrom_CreatesSourceData()
        {
            var loader = new TvDbSeriesFromTvDbEpisode(this.sources);

            var result = await loader.LoadFrom(this.mediaItem, null);

            result.IsRight.Should().BeTrue();
            result.IfRight(sd => sd.Data.Should().Be(this.tvDbSeriesData));
            result.IfRight(sd => sd.Source.Should().BeEquivalentTo(this.sources.TvDb.ForAdditionalData()));
            result.IfRight(sd =>
                sd.Identifier.Should().BeEquivalentTo(new ItemIdentifier(67, Option<int>.None, "Title")));
        }

        [Test]
        public async Task LoadFrom_NoMatchingSeries_Fails()
        {
            this.tvDbSource.GetSeriesData(this.mediaItem.EmbyData, Arg.Any<ProcessResultContext>())
                .Returns(Left<ProcessFailedResult, TvDbSeriesData>(new ProcessFailedResult(string.Empty, string.Empty,
                    MediaItemTypes.Series, "Failed to find series in TvDb")));

            var loader = new TvDbSeriesFromTvDbEpisode(this.sources);

            var result = await loader.LoadFrom(this.mediaItem, null);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("Failed to find series in TvDb"));
        }
    }
}
using System.Threading.Tasks;
using Jellyfin.AniDbMetaStructure.Process;
using Jellyfin.AniDbMetaStructure.Process.Sources;
using Jellyfin.AniDbMetaStructure.SourceDataLoaders;
using Jellyfin.AniDbMetaStructure.Tests.TestData;
using Jellyfin.AniDbMetaStructure.TvDb;
using FluentAssertions;
using LanguageExt;
using NSubstitute;
using NUnit.Framework;

namespace Jellyfin.AniDbMetaStructure.Tests.SourceDataLoaders
{
    [TestFixture]
    public class TvDbSeriesFromJellyfinDataTests
    {
        [SetUp]
        public void Setup()
        {
            var tvDbSource = Substitute.For<ITvDbSource>();

            this.sources = Substitute.For<ISources>();
            this.sources.TvDb.Returns(tvDbSource);

            this.tvDbClient = Substitute.For<ITvDbClient>();

            this.JellyfinItemData = Substitute.For<IJellyfinItemData>();
            this.JellyfinItemData.Identifier.Returns(new ItemIdentifier(2, 1, "SeriesName"));
        }

        private ITvDbClient tvDbClient;
        private ISources sources;
        private IJellyfinItemData JellyfinItemData;

        [Test]
        public void CanLoadFrom_CorrectItemType_IsTrue()
        {
            var loader = new TvDbSeriesFromJellyfinData(this.tvDbClient, this.sources);

            loader.CanLoadFrom(MediaItemTypes.Series).Should().BeTrue();
        }

        [Test]
        public void CanLoadFrom_Null_IsFalse()
        {
            var loader = new TvDbSeriesFromJellyfinData(this.tvDbClient, this.sources);

            loader.CanLoadFrom(null).Should().BeFalse();
        }

        [Test]
        public void CanLoadFrom_WrongItemType_IsFalse()
        {
            var loader = new TvDbSeriesFromJellyfinData(this.tvDbClient, this.sources);

            loader.CanLoadFrom(MediaItemTypes.Season).Should().BeFalse();
        }

        [Test]
        public async Task LoadFrom_CreatesSourceData()
        {
            var tvDbSeriesData = TvDbTestData.Series(22, "SeriesName");
            this.tvDbClient.FindSeriesAsync("SeriesName").Returns(tvDbSeriesData);

            var loader = new TvDbSeriesFromJellyfinData(this.tvDbClient, this.sources);

            var result = await loader.LoadFrom(this.JellyfinItemData);

            result.IsRight.Should().BeTrue();

            result.IfRight(sd => sd.Data.Should().Be(tvDbSeriesData));
            result.IfRight(sd => sd.Source.Should().Be(this.sources.TvDb));
            result.IfRight(sd =>
                sd.Identifier.Should().BeEquivalentTo(new ItemIdentifier(2, Option<int>.None, "SeriesName")));
        }

        [Test]
        public async Task LoadFrom_NoFoundSeries_Fails()
        {
            var loader = new TvDbSeriesFromJellyfinData(this.tvDbClient, this.sources);

            var result = await loader.LoadFrom(this.JellyfinItemData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("Failed to find series in TvDb"));
        }
    }
}
using System.Threading.Tasks;
using Jellyfin.AniDbMetaStructure.AniDb.SeriesData;
using Jellyfin.AniDbMetaStructure.Process;
using Jellyfin.AniDbMetaStructure.Process.Sources;
using Jellyfin.AniDbMetaStructure.SourceDataLoaders;
using FluentAssertions;
using LanguageExt;
using NSubstitute;
using NUnit.Framework;

namespace Jellyfin.AniDbMetaStructure.Tests.SourceDataLoaders
{
    [TestFixture]
    public class AniDbSeasonFromJellyfinDataTests
    {
        [SetUp]
        public void Setup()
        {
            this.aniDbSource = Substitute.For<IAniDbSource>();

            this.sources = Substitute.For<ISources>();
            this.sources.AniDb.Returns(this.aniDbSource);

            this.JellyfinItemData = Substitute.For<IJellyfinItemData>();
            this.JellyfinItemData.Language.Returns("en");

            this.aniDbSeriesTitles = new ItemTitleData[] { };
            var aniDbSeriesData = new AniDbSeriesData
            {
                Titles = this.aniDbSeriesTitles
            };

            this.JellyfinItemData.Identifier.Returns(new ItemIdentifier(67, Option<int>.None, "Name"));
            this.aniDbSource.GetSeriesData(this.JellyfinItemData, Arg.Any<ProcessResultContext>())
                .Returns(aniDbSeriesData);
        }

        private ISources sources;
        private IJellyfinItemData JellyfinItemData;
        private IAniDbSource aniDbSource;
        private ItemTitleData[] aniDbSeriesTitles;

        [Test]
        public void CanLoadFrom_CorrectItemType_IsTrue()
        {
            var loader = new AniDbSeasonFromJellyfinData(this.sources);

            loader.CanLoadFrom(MediaItemTypes.Season).Should().BeTrue();
        }

        [Test]
        public void CanLoadFrom_Null_IsFalse()
        {
            var loader = new AniDbSeasonFromJellyfinData(this.sources);

            loader.CanLoadFrom(null).Should().BeFalse();
        }

        [Test]
        public void CanLoadFrom_WrongItemType_IsFalse()
        {
            var loader = new AniDbSeasonFromJellyfinData(this.sources);

            loader.CanLoadFrom(MediaItemTypes.Series).Should().BeFalse();
        }

        [Test]
        public async Task LoadFrom_NoIndex_SetsIndexToOne()
        {
            var selectedSeriesTitle = "SeriesTitle";
            this.aniDbSource.SelectTitle(this.aniDbSeriesTitles, "en", Arg.Any<ProcessResultContext>())
                .Returns(selectedSeriesTitle);

            this.JellyfinItemData.Identifier.Returns(new ItemIdentifier(Option<int>.None, Option<int>.None, "Name"));

            var loader = new AniDbSeasonFromJellyfinData(this.sources);

            var result = await loader.LoadFrom(this.JellyfinItemData);

            result.IsRight.Should().BeTrue();
            result.IfRight(r => r.Data.Should().Be(r));
            result.IfRight(r => r.Source.Should().Be(this.sources.AniDb));
            result.IfRight(sd =>
                sd.Identifier.Should().BeEquivalentTo(new ItemIdentifier(1, Option<int>.None, "SeriesTitle")));
        }

        [Test]
        public async Task LoadFrom_ReturnsIdentifierOnlySourceDataWithSeriesName()
        {
            var selectedSeriesTitle = "SeriesTitle";
            this.aniDbSource.SelectTitle(this.aniDbSeriesTitles, "en", Arg.Any<ProcessResultContext>())
                .Returns(selectedSeriesTitle);

            var loader = new AniDbSeasonFromJellyfinData(this.sources);

            var result = await loader.LoadFrom(this.JellyfinItemData);

            result.IsRight.Should().BeTrue();
            result.IfRight(r => r.Data.Should().Be(r));
            result.IfRight(r => r.Source.Should().Be(this.sources.AniDb));
            result.IfRight(sd =>
                sd.Identifier.Should().BeEquivalentTo(new ItemIdentifier(67, Option<int>.None, "SeriesTitle")));
        }
    }
}
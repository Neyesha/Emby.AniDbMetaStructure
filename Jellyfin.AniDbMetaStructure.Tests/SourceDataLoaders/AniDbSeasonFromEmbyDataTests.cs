using System.Threading.Tasks;
using Emby.AniDbMetaStructure.AniDb.SeriesData;
using Emby.AniDbMetaStructure.Process;
using Emby.AniDbMetaStructure.Process.Sources;
using Emby.AniDbMetaStructure.SourceDataLoaders;
using FluentAssertions;
using LanguageExt;
using NSubstitute;
using NUnit.Framework;

namespace Emby.AniDbMetaStructure.Tests.SourceDataLoaders
{
    [TestFixture]
    public class AniDbSeasonFromEmbyDataTests
    {
        [SetUp]
        public void Setup()
        {
            this.aniDbSource = Substitute.For<IAniDbSource>();

            this.sources = Substitute.For<ISources>();
            this.sources.AniDb.Returns(this.aniDbSource);

            this.embyItemData = Substitute.For<IEmbyItemData>();
            this.embyItemData.Language.Returns("en");

            this.aniDbSeriesTitles = new ItemTitleData[] { };
            var aniDbSeriesData = new AniDbSeriesData
            {
                Titles = this.aniDbSeriesTitles
            };

            this.embyItemData.Identifier.Returns(new ItemIdentifier(67, Option<int>.None, "Name"));
            this.aniDbSource.GetSeriesData(this.embyItemData, Arg.Any<ProcessResultContext>())
                .Returns(aniDbSeriesData);
        }

        private ISources sources;
        private IEmbyItemData embyItemData;
        private IAniDbSource aniDbSource;
        private ItemTitleData[] aniDbSeriesTitles;

        [Test]
        public void CanLoadFrom_CorrectItemType_IsTrue()
        {
            var loader = new AniDbSeasonFromEmbyData(this.sources);

            loader.CanLoadFrom(MediaItemTypes.Season).Should().BeTrue();
        }

        [Test]
        public void CanLoadFrom_Null_IsFalse()
        {
            var loader = new AniDbSeasonFromEmbyData(this.sources);

            loader.CanLoadFrom(null).Should().BeFalse();
        }

        [Test]
        public void CanLoadFrom_WrongItemType_IsFalse()
        {
            var loader = new AniDbSeasonFromEmbyData(this.sources);

            loader.CanLoadFrom(MediaItemTypes.Series).Should().BeFalse();
        }

        [Test]
        public async Task LoadFrom_NoIndex_SetsIndexToOne()
        {
            var selectedSeriesTitle = "SeriesTitle";
            this.aniDbSource.SelectTitle(this.aniDbSeriesTitles, "en", Arg.Any<ProcessResultContext>())
                .Returns(selectedSeriesTitle);

            this.embyItemData.Identifier.Returns(new ItemIdentifier(Option<int>.None, Option<int>.None, "Name"));

            var loader = new AniDbSeasonFromEmbyData(this.sources);

            var result = await loader.LoadFrom(this.embyItemData);

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

            var loader = new AniDbSeasonFromEmbyData(this.sources);

            var result = await loader.LoadFrom(this.embyItemData);

            result.IsRight.Should().BeTrue();
            result.IfRight(r => r.Data.Should().Be(r));
            result.IfRight(r => r.Source.Should().Be(this.sources.AniDb));
            result.IfRight(sd =>
                sd.Identifier.Should().BeEquivalentTo(new ItemIdentifier(67, Option<int>.None, "SeriesTitle")));
        }
    }
}
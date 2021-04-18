using System.Linq;
using System.Threading.Tasks;
using Emby.AniDbMetaStructure.AniDb.Titles;
using Emby.AniDbMetaStructure.Process;
using Emby.AniDbMetaStructure.Process.Sources;
using Emby.AniDbMetaStructure.SourceDataLoaders;
using Emby.AniDbMetaStructure.Tests.TestData;
using Emby.AniDbMetaStructure.TvDb;
using FluentAssertions;
using LanguageExt;
using NSubstitute;
using NUnit.Framework;

namespace Emby.AniDbMetaStructure.Tests.SourceDataLoaders
{
    [TestFixture]
    public class TvDbEpisodeFromEmbyDataTests
    {
        [SetUp]
        public void Setup()
        {
            this.sources = Substitute.For<ISources>();

            var tvDbSource = Substitute.For<ITvDbSource>();
            this.sources.TvDb.Returns(tvDbSource);

            this.embyItemData = Substitute.For<IEmbyItemData>();
            this.embyItemData.Language.Returns("en");
            this.embyItemData.GetParentId(MediaItemTypes.Series, this.sources.TvDb).Returns(22);

            this.mediaItem = Substitute.For<IMediaItem>();
            this.mediaItem.EmbyData.Returns(this.embyItemData);
            this.mediaItem.ItemType.Returns(MediaItemTypes.Episode);

            this.tvDbClient = Substitute.For<ITvDbClient>();
            this.titleNormaliser = new TitleNormaliser();
        }

        private ISources sources;
        private IMediaItem mediaItem;
        private IEmbyItemData embyItemData;
        private ITvDbClient tvDbClient;
        private ITitleNormaliser titleNormaliser;

        [Test]
        public void CanLoadFrom_CorrectItemType_IsTrue()
        {
            var loader = new TvDbEpisodeFromEmbyData(this.sources, this.tvDbClient, this.titleNormaliser);

            loader.CanLoadFrom(MediaItemTypes.Episode).Should().BeTrue();
        }

        [Test]
        public void CanLoadFrom_Null_IsFalse()
        {
            var loader = new TvDbEpisodeFromEmbyData(this.sources, this.tvDbClient, this.titleNormaliser);

            loader.CanLoadFrom(null).Should().BeFalse();
        }

        [Test]
        public void CanLoadFrom_WrongItemType_IsFalse()
        {
            var loader = new TvDbEpisodeFromEmbyData(this.sources, this.tvDbClient, this.titleNormaliser);

            loader.CanLoadFrom(MediaItemTypes.Season).Should().BeFalse();
        }
        
        [Test]
        public async Task LoadFrom_NoSeriesId_Fails()
        {
            this.embyItemData.GetParentId(MediaItemTypes.Series, this.sources.TvDb).Returns(Option<int>.None);

            var loader = new TvDbEpisodeFromEmbyData(this.sources, this.tvDbClient, this.titleNormaliser);

            var result = await loader.LoadFrom(this.embyItemData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("No TvDb Id found on parent series"));
        }

        [Test]
        public async Task LoadFrom_EpisodeLoadFail_Fails()
        {
            this.embyItemData.Identifier.Returns(new ItemIdentifier(4, 1, "Name"));
            
            var loader = new TvDbEpisodeFromEmbyData(this.sources, this.tvDbClient, this.titleNormaliser);

            var result = await loader.LoadFrom(this.embyItemData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("Failed to load parent series with TvDb Id '22'"));
        }

        [Test]
        public async Task LoadFrom_NoMatchingEpisode_Fails()
        {
            this.embyItemData.Identifier.Returns(new ItemIdentifier(4, 1, "Name"));

            this.tvDbClient.GetEpisodesAsync(22)
                .Returns(new[]
                {
                    TvDbTestData.Episode(1, 1, 1, name: "NonMatch1"),
                    TvDbTestData.Episode(1, 4, 2, name: "NonMatch2")
                }.ToList());

            var loader = new TvDbEpisodeFromEmbyData(this.sources, this.tvDbClient, this.titleNormaliser);

            var result = await loader.LoadFrom(this.embyItemData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("Failed to find TvDb episode"));
        }

        [Test]
        public async Task LoadFrom_MatchOnEpisodeAndSeasonIndex_CreatesSourceData()
        {
            var expected = TvDbTestData.Episode(1, 4, 2, name: "Match");

            this.embyItemData.Identifier.Returns(new ItemIdentifier(4, 2, "Name"));

            this.tvDbClient.GetEpisodesAsync(22)
                .Returns(new[]
                {
                    TvDbTestData.Episode(1, 1, 1, name: "NonMatch"),
                    expected
                }.ToList());

            var loader = new TvDbEpisodeFromEmbyData(this.sources, this.tvDbClient, this.titleNormaliser);

            var result = await loader.LoadFrom(this.embyItemData);

            result.IsRight.Should().BeTrue();
            result.IfRight(sd => sd.Data.Should().Be(expected));
            result.IfRight(sd => sd.Source.Should().Be(this.sources.TvDb));
            result.IfRight(sd => sd.Identifier.Should().BeEquivalentTo(new ItemIdentifier(4, 2, "Match")));
        }

        [Test]
        public async Task LoadFrom_MatchOnEpisodeAndDefaultSeasonIndex_CreatesSourceData()
        {
            var expected = TvDbTestData.Episode(1, 4, 1, name: "Match");

            this.embyItemData.Identifier.Returns(new ItemIdentifier(4, Option<int>.None, "Name"));

            this.tvDbClient.GetEpisodesAsync(22)
                .Returns(new[]
                {
                    TvDbTestData.Episode(1, 1, 2, name: "NonMatch"),
                    expected
                }.ToList());

            var loader = new TvDbEpisodeFromEmbyData(this.sources, this.tvDbClient, this.titleNormaliser);

            var result = await loader.LoadFrom(this.embyItemData);

            result.IsRight.Should().BeTrue();
            result.IfRight(sd => sd.Data.Should().Be(expected));
            result.IfRight(sd => sd.Source.Should().Be(this.sources.TvDb));
            result.IfRight(sd => sd.Identifier.Should().BeEquivalentTo(new ItemIdentifier(4, 1, "Match")));
        }

        [Test]
        public async Task LoadFrom_MatchOnTitle_CreatesSourceData()
        {
            var expected = TvDbTestData.Episode(1, 6, 1, name: "Match");

            this.embyItemData.Identifier.Returns(new ItemIdentifier(4, 2, "Match"));

            this.tvDbClient.GetEpisodesAsync(22)
                .Returns(new[]
                {
                    TvDbTestData.Episode(1, 1, 1, name: "NonMatch"),
                    expected
                }.ToList());

            var loader = new TvDbEpisodeFromEmbyData(this.sources, this.tvDbClient, this.titleNormaliser);

            var result = await loader.LoadFrom(this.embyItemData);

            result.IsRight.Should().BeTrue();
            result.IfRight(sd => sd.Data.Should().Be(expected));
            result.IfRight(sd => sd.Source.Should().Be(this.sources.TvDb));
            result.IfRight(sd => sd.Identifier.Should().BeEquivalentTo(new ItemIdentifier(6, 1, "Match")));
        }

    }
}
using System.Threading.Tasks;
using Jellyfin.AniDbMetaStructure.AniDb.SeriesData;
using Jellyfin.AniDbMetaStructure.Mapping;
using Jellyfin.AniDbMetaStructure.Process;
using Jellyfin.AniDbMetaStructure.Process.Sources;
using Jellyfin.AniDbMetaStructure.Providers.AniDb;
using Jellyfin.AniDbMetaStructure.SourceDataLoaders;
using Jellyfin.AniDbMetaStructure.Tests.TestData;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace Jellyfin.AniDbMetaStructure.Tests.SourceDataLoaders
{
    [TestFixture]
    public class AniDbEpisodeFromJellyfinDataTets
    {
        [SetUp]
        public void Setup()
        {
            this.sources = Substitute.For<ISources>();

            this.JellyfinItemData = Substitute.For<IJellyfinItemData>();
            this.JellyfinItemData.Identifier.Returns(new ItemIdentifier(67, 1, "Name"));
            this.JellyfinItemData.Language.Returns("en");

            var aniDbSource = Substitute.For<IAniDbSource>();
            this.sources.AniDb.Returns(aniDbSource);

            this.mediaItem = Substitute.For<IMediaItem>();
            this.mediaItem.JellyfinData.Returns(this.JellyfinItemData);
            this.mediaItem.ItemType.Returns(MediaItemTypes.Episode);

            this.aniDbEpisodeMatcher = Substitute.For<IAniDbEpisodeMatcher>();

            this.aniDbSeriesData = new AniDbSeriesData().WithStandardData();
            this.aniDbEpisodeData = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = "67",
                    RawType = 1
                },
                Titles = new[]
                {
                    new EpisodeTitleData
                    {
                        Title = "Title"
                    }
                }
            };

            this.mappingList = Substitute.For<IMappingList>();
        }

        private ISources sources;
        private IAniDbEpisodeMatcher aniDbEpisodeMatcher;
        private IMediaItem mediaItem;
        private IJellyfinItemData JellyfinItemData;
        private AniDbSeriesData aniDbSeriesData;
        private AniDbEpisodeData aniDbEpisodeData;
        private IMappingList mappingList;

        [Test]
        public void CanLoadFrom_CorrectItemType_IsTrue()
        {
            var loader = new AniDbEpisodeFromJellyfinData(this.sources, this.aniDbEpisodeMatcher, this.mappingList);

            loader.CanLoadFrom(MediaItemTypes.Episode).Should().BeTrue();
        }

        [Test]
        public void CanLoadFrom_Null_IsFalse()
        {
            var loader = new AniDbEpisodeFromJellyfinData(this.sources, this.aniDbEpisodeMatcher, this.mappingList);

            loader.CanLoadFrom(null).Should().BeFalse();
        }

        [Test]
        public void CanLoadFrom_WrongItemType_IsFalse()
        {
            var loader = new AniDbEpisodeFromJellyfinData(this.sources, this.aniDbEpisodeMatcher, this.mappingList);

            loader.CanLoadFrom(MediaItemTypes.Season).Should().BeFalse();
        }

        [Test]
        public async Task LoadFrom_CreatesSourceData()
        {
            this.sources.AniDb.GetSeriesData(this.mediaItem.JellyfinData, Arg.Any<ProcessResultContext>())
                .Returns(this.aniDbSeriesData);

            this.aniDbEpisodeMatcher.FindEpisode(this.aniDbSeriesData.Episodes, 1, 67, "Name")
                .Returns(this.aniDbEpisodeData);

            this.sources.AniDb.SelectTitle(this.aniDbEpisodeData.Titles, "en", Arg.Any<ProcessResultContext>())
                .Returns("Title");

            var loader = new AniDbEpisodeFromJellyfinData(this.sources, this.aniDbEpisodeMatcher, this.mappingList);

            var result = await loader.LoadFrom(this.JellyfinItemData);

            result.IsRight.Should().BeTrue();
            result.IfRight(sd => sd.Data.Should().Be(this.aniDbEpisodeData));
            result.IfRight(sd => sd.Source.Should().Be(this.sources.AniDb));
            result.IfRight(sd => sd.Identifier.Should().BeEquivalentTo(new ItemIdentifier(67, 1, "Title")));
        }

        [Test]
        public async Task LoadFrom_NoFoundEpisode_Fails()
        {
            this.sources.AniDb.GetSeriesData(this.mediaItem.JellyfinData, Arg.Any<ProcessResultContext>())
                .Returns(this.aniDbSeriesData);

            var loader = new AniDbEpisodeFromJellyfinData(this.sources, this.aniDbEpisodeMatcher, this.mappingList);

            var result = await loader.LoadFrom(this.JellyfinItemData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("Failed to find episode in AniDb"));
        }

        [Test]
        public async Task LoadFrom_NoSeriesData_Fails()
        {
            this.sources.AniDb.GetSeriesData(this.mediaItem.JellyfinData, Arg.Any<ProcessResultContext>())
                .Returns(new ProcessFailedResult(string.Empty, string.Empty, MediaItemTypes.Episode, "FailedSeriesData"));

            var loader = new AniDbEpisodeFromJellyfinData(this.sources, this.aniDbEpisodeMatcher, this.mappingList);

            var result = await loader.LoadFrom(this.JellyfinItemData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("FailedSeriesData"));
        }

        [Test]
        public async Task LoadFrom_NoTitle_Fails()
        {
            this.sources.AniDb.GetSeriesData(this.mediaItem.JellyfinData, Arg.Any<ProcessResultContext>())
                .Returns(this.aniDbSeriesData);

            this.aniDbEpisodeMatcher.FindEpisode(this.aniDbSeriesData.Episodes, 1, 67, "Name")
                .Returns(this.aniDbEpisodeData);

            this.sources.AniDb.SelectTitle(this.aniDbEpisodeData.Titles, "en", Arg.Any<ProcessResultContext>())
                .Returns(new ProcessFailedResult(string.Empty, string.Empty, MediaItemTypes.Episode, "FailedTitle"));

            var loader = new AniDbEpisodeFromJellyfinData(this.sources, this.aniDbEpisodeMatcher, this.mappingList);

            var result = await loader.LoadFrom(this.JellyfinItemData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("FailedTitle"));
        }
    }
}
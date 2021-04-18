using System.Collections.Generic;
using System.Threading.Tasks;
using Jellyfin.AniDbMetaStructure.AniDb.SeriesData;
using Jellyfin.AniDbMetaStructure.AniDb.Titles;
using Jellyfin.AniDbMetaStructure.AniList;
using Jellyfin.AniDbMetaStructure.AniList.Data;
using Jellyfin.AniDbMetaStructure.Process;
using Jellyfin.AniDbMetaStructure.Process.Sources;
using Jellyfin.AniDbMetaStructure.SourceDataLoaders;
using Jellyfin.AniDbMetaStructure.Tests.TestHelpers;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using static LanguageExt.Prelude;

namespace Emby.AniDbMetaStructure.Tests.SourceDataLoaders
{
    [TestFixture]
    public class AniListSeriesFromAniDbTests
    {
        [SetUp]
        public void Setup()
        {
            this.aniListClient = Substitute.For<IAniListClient>();
            this.aniListClient.FindSeriesAsync(null, null)
                .ReturnsForAnyArgs(
                    Left<ProcessFailedResult, IEnumerable<AniListSeriesData>>(
                        TestProcessResultContext.Failed("Failed")));

            this.sources = new TestSources();

            this.titleNormaliser = Substitute.For<ITitleNormaliser>();
            this.titleNormaliser.GetNormalisedTitle(Arg.Any<string>()).Returns(x => $"Normalised{x.Arg<string>()}");

            this.embyData = Substitute.For<IEmbyItemData>();
            this.embyData.Identifier.Returns(new ItemIdentifier(0, 0, "Name"));
            this.embyData.Language.Returns("en");

            this.mediaItem = Substitute.For<IMediaItem>();
            this.mediaItem.EmbyData.Returns(this.embyData);
            this.mediaItem.ItemType.Returns(MediaItemTypes.Series);

            this.aniDbTitles = new ItemTitleData[] { };
            this.aniDbSourceData = Substitute.For<ISourceData<AniDbSeriesData>>();
            this.aniDbSourceData.Data.Returns(x => new AniDbSeriesData
            {
                Titles = this.aniDbTitles
            });

            this.aniListConfiguration = Substitute.For<IAnilistConfiguration>();
            this.aniListConfiguration.IsLinked.Returns(true);

            this.loader = new AniListSeriesFromAniDb(this.aniListClient, this.sources, this.titleNormaliser, this.aniListConfiguration);
        }

        private IAniListClient aniListClient;
        private ISources sources;
        private ITitleNormaliser titleNormaliser;
        private AniListSeriesFromAniDb loader;
        private IMediaItem mediaItem;
        private IEmbyItemData embyData;
        private ISourceData<AniDbSeriesData> aniDbSourceData;
        private ItemTitleData[] aniDbTitles;
        private IAnilistConfiguration aniListConfiguration;

        [Test]
        public void CanLoadFrom_Null_IsFalse()
        {
            this.loader.CanLoadFrom(null).Should().BeFalse();
        }

        [Test]
        public void CanLoadFrom_TypeMatch_IsTrue()
        {
            this.loader.CanLoadFrom(Substitute.For<ISourceData<AniDbSeriesData>>()).Should().BeTrue();
        }

        [Test]
        public void CanLoadFrom_TypeMisMatch_IsFalse()
        {
            this.loader.CanLoadFrom(new object()).Should().BeFalse();
        }

        [Test]
        public void CanLoadFrom_NotLinked_IsFalse()
        {
            this.aniListConfiguration.IsLinked.Returns(false);
            this.loader.CanLoadFrom(Substitute.For<ISourceData<AniDbSeriesData>>()).Should().BeFalse();
        }

        [Test]
        public async Task LoadFrom_NoAniListSeriesFoundForFirstAniDbTitles_QueriesForSecondTitle()
        {
            this.aniDbTitles = new[]
            {
                new ItemTitleData
                {
                    Title = "TitleA",
                    Language = "en"
                },
                new ItemTitleData
                {
                    Title = "TitleB",
                    Language = "en"
                }
            };

            this.aniListClient.FindSeriesAsync("NormalisedTitleA", Arg.Any<ProcessResultContext>())
                .Returns(new AniListSeriesData[] { });

            this.aniListClient.FindSeriesAsync("NormalisedTitleB", Arg.Any<ProcessResultContext>())
                .Returns(new AniListSeriesData[] { });

            await this.loader.LoadFrom(this.mediaItem, this.aniDbSourceData);

            await this.aniListClient.Received(1).FindSeriesAsync("NormalisedTitleA", Arg.Any<ProcessResultContext>());
            await this.aniListClient.Received(1).FindSeriesAsync("NormalisedTitleB", Arg.Any<ProcessResultContext>());
        }

        [Test]
        public async Task LoadFrom_MultipleDistinctSeriesForOneTitle_Fails()
        {
            this.aniDbTitles = new[]
            {
                new ItemTitleData
                {
                    Title = "TitleA",
                    Language = "en"
                }
            };

            this.aniListClient.FindSeriesAsync("NormalisedTitleA", Arg.Any<ProcessResultContext>())
                .Returns(new[]
                {
                    new AniListSeriesData
                    {
                        Id = 1
                    },
                    new AniListSeriesData
                    {
                        Id = 2
                    }
                });

            var result = await this.loader.LoadFrom(this.mediaItem, this.aniDbSourceData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(r => r.Reason.Should().Be("Found too many (2) matching AniList series"));
        }

        [Test]
        public async Task LoadFrom_SingleDistinctSeriesFound_ReturnsSourceData()
        {
            var aniListSeriesData = new AniListSeriesData
            {
                Id = 1,
                Title = new AniListTitleData("english", string.Empty, string.Empty)
            };

            this.aniDbTitles = new[]
            {
                new ItemTitleData
                {
                    Title = "TitleA",
                    Language = "en"
                }
            };

            this.aniListClient.FindSeriesAsync("NormalisedTitleA", Arg.Any<ProcessResultContext>())
                .Returns(new[]
                {
                    aniListSeriesData
                });

            this.sources.AniList.SelectTitle(aniListSeriesData.Title, "en", Arg.Any<ProcessResultContext>())
                .Returns(Right<ProcessFailedResult, string>("SelectedTitle"));

            var result = await this.loader.LoadFrom(this.mediaItem, this.aniDbSourceData);

            result.IsRight.Should().BeTrue();
            result.IfRight(r => r.Should()
                .BeEquivalentTo(new SourceData<AniListSeriesData>(
                    TestSources.AniListSource,
                    1,
                    new ItemIdentifier(None, None, "SelectedTitle"),
                    aniListSeriesData
                )));
        }

        [Test]
        public void SourceName_ReturnsAniList()
        {
            this.loader.SourceName.Should().Be(SourceNames.AniList);
        }
    }
}
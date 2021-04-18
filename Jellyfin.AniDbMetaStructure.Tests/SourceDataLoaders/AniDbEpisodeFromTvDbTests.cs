using System.Collections.Generic;
using System.Threading.Tasks;
using Emby.AniDbMetaStructure.AniDb.SeriesData;
using Emby.AniDbMetaStructure.Mapping;
using Emby.AniDbMetaStructure.Process;
using Emby.AniDbMetaStructure.Process.Sources;
using Emby.AniDbMetaStructure.SourceDataLoaders;
using Emby.AniDbMetaStructure.Tests.TestData;
using Emby.AniDbMetaStructure.TvDb.Data;
using FluentAssertions;
using LanguageExt;
using NSubstitute;
using NUnit.Framework;
using static LanguageExt.Prelude;

namespace Emby.AniDbMetaStructure.Tests.SourceDataLoaders
{
    [TestFixture]
    public class AniDbEpisodeFromTvDbTests
    {
        private IEpisodeMapper episodeMapper;
        private ISources sources;
        private IMappingList mappingList;
        private IEmbyItemData embyData;
        private IMediaItem mediaItem;
        private ISourceData<TvDbEpisodeData> tvDbSourceData;
        private ProcessFailedResult noMappingResult;

        [SetUp]
        public void Setup()
        {
            this.episodeMapper = Substitute.For<IEpisodeMapper>();
            this.sources = Substitute.For<ISources>();
            this.mappingList = Substitute.For<IMappingList>();

            this.embyData = Substitute.For<IEmbyItemData>();
            this.embyData.Identifier.Returns(new ItemIdentifier(67, 53, "Name"));
            this.embyData.Language.Returns("en");

            var aniDbSource = Substitute.For<IAniDbSource>();
            this.sources.AniDb.Returns(aniDbSource);

            var tvDbSource = Substitute.For<ITvDbSource>();
            this.sources.TvDb.Returns(tvDbSource);

            this.tvDbSourceData = Substitute.For<ISourceData<TvDbEpisodeData>>();

            this.mediaItem = Substitute.For<IMediaItem>();
            this.mediaItem.EmbyData.Returns(this.embyData);
            this.mediaItem.ItemType.Returns(MediaItemTypes.Episode);

            this.SetUpTvDbEpisodeData(56, 67, 53);
            
            this.noMappingResult = new ProcessFailedResult(string.Empty, string.Empty, null, string.Empty);
            this.mappingList.GetSeriesMappingsFromTvDb(Arg.Any<int>(), Arg.Any<ProcessResultContext>())
                .Returns(Left<ProcessFailedResult, IEnumerable<ISeriesMapping>>(this.noMappingResult));
        }

        private TvDbSeriesData SetUpTvDbSeriesData(int id)
        {
            var seriesData = TvDbTestData.Series(id);
            
            this.embyData.GetParentId(MediaItemTypes.Series, this.sources.TvDb).Returns(id);
            this.sources.TvDb.GetSeriesData(this.embyData, Arg.Any<ProcessResultContext>()).Returns(seriesData);

            return seriesData;
        }

        private TvDbEpisodeData SetUpTvDbEpisodeData(int id, int tvDbEpisodeIndex, int tvDbSeasonIndex)
        {
            var episodeData = TvDbTestData.Episode(id, tvDbEpisodeIndex, tvDbSeasonIndex);

            this.embyData.GetParentId(MediaItemTypes.Series, this.sources.TvDb).Returns(id);
            this.tvDbSourceData.Data.Returns(episodeData);

            return episodeData;
        }

        private ISeriesMapping CreateSeriesMapping(int tvDbSeriesId, int aniDbSeriesId)
        {
            var mapping = Substitute.For<ISeriesMapping>();

            mapping.Ids.Returns(new SeriesIds(aniDbSeriesId, tvDbSeriesId, Option<int>.None, Option<int>.None));

            return mapping;
        }

        private AniDbEpisodeData SetUpEpisodeMapping(int tvDbEpisodeIndex, int aniDbEpisodeIndex, ISeriesMapping seriesMapping, string title)
        {
            var aniDbEpisodeData = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = aniDbEpisodeIndex.ToString(),
                    RawType = 1
                },
                Titles = new []
                {
                    new EpisodeTitleData
                    {
                        Title = title
                    }
                }
            };

            this.sources.AniDb.SelectTitle(aniDbEpisodeData.Titles, "en", Arg.Any<ProcessResultContext>())
                .Returns(title);

            this.episodeMapper.MapTvDbEpisodeAsync(tvDbEpisodeIndex, seriesMapping, Option<EpisodeGroupMapping>.None)
                .Returns(aniDbEpisodeData);

            return aniDbEpisodeData;
        }

        [Test]
        public void CanLoadFrom_Null_IsFalse()
        {
            var loader = new AniDbEpisodeFromTvDb(this.sources, this.mappingList, this.episodeMapper);

            loader.CanLoadFrom(null).Should().BeFalse();
        }

        [Test]
        public void CanLoadFrom_TypeMatch_IsTrue()
        {
            var loader = new AniDbEpisodeFromTvDb(this.sources, this.mappingList, this.episodeMapper);

            loader.CanLoadFrom(Substitute.For<ISourceData<TvDbEpisodeData>>()).Should().BeTrue();
        }

        [Test]
        public void CanLoadFrom_TypeMisMatch_IsFalse()
        {
            var loader = new AniDbEpisodeFromTvDb(this.sources, this.mappingList, this.episodeMapper);

            loader.CanLoadFrom(new object()).Should().BeFalse();
        }

        [Test]
        public async Task LoadFrom_NoTvDbSeriesData_Fails()
        {
            this.sources.TvDb.GetSeriesData(this.embyData, Arg.Any<ProcessResultContext>())
                .Returns(new ProcessFailedResult(string.Empty, string.Empty, MediaItemTypes.Episode, "Failed"));

            var loader = new AniDbEpisodeFromTvDb(this.sources, this.mappingList, this.episodeMapper);

            var result = await loader.LoadFrom(this.mediaItem, this.tvDbSourceData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("Failed"));
        }

        [Test]
        public async Task LoadFrom_NoAniDbSeriesId_Fails()
        {
            this.SetUpTvDbSeriesData(53);

            var loader = new AniDbEpisodeFromTvDb(this.sources, this.mappingList, this.episodeMapper);

            var result = await loader.LoadFrom(this.mediaItem, this.tvDbSourceData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("Failed to find AniDb series Id"));
        }

        [Test]
        public async Task LoadFrom_NoSeriesMapping_Fails()
        {
            this.SetUpTvDbSeriesData(53);
            this.embyData.GetParentId(MediaItemTypes.Series, this.sources.AniDb).Returns(19);

            var loader = new AniDbEpisodeFromTvDb(this.sources, this.mappingList, this.episodeMapper);

            var result = await loader.LoadFrom(this.mediaItem, this.tvDbSourceData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Should().Be(this.noMappingResult));
        }

        [Test]
        public async Task LoadFrom_NoMatchingSeriesMappings_Fails()
        {
            this.SetUpTvDbSeriesData(53);
            this.embyData.GetParentId(MediaItemTypes.Series, this.sources.AniDb).Returns(19);

            var seriesMappings = new[]
            {
                this.CreateSeriesMapping(53, 55)
            };

            this.mappingList.GetSeriesMappingsFromTvDb(53, Arg.Any<ProcessResultContext>()).Returns(seriesMappings);

            var loader = new AniDbEpisodeFromTvDb(this.sources, this.mappingList, this.episodeMapper);

            var result = await loader.LoadFrom(this.mediaItem, this.tvDbSourceData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("No series mapping between TvDb series Id '53' and AniDb series id '19'"));
        }

        [Test]
        public async Task LoadFrom_MultipleSeriesMappings_Fails()
        {
            this.SetUpTvDbSeriesData(53);
            this.embyData.GetParentId(MediaItemTypes.Series, this.sources.AniDb).Returns(19);

            var seriesMappings = new[]
            {
                this.CreateSeriesMapping(53, 19),
                this.CreateSeriesMapping(53, 19)
            };

            this.mappingList.GetSeriesMappingsFromTvDb(53, Arg.Any<ProcessResultContext>()).Returns(seriesMappings);

            var loader = new AniDbEpisodeFromTvDb(this.sources, this.mappingList, this.episodeMapper);

            var result = await loader.LoadFrom(this.mediaItem, this.tvDbSourceData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("Multiple series mappings found between TvDb series Id '53' and AniDb series Id '19'"));
        }

        [Test]
        public async Task LoadFrom_NoCorrespondingAniDbEpisode_Fails()
        {
            this.SetUpTvDbSeriesData(53);
            this.embyData.GetParentId(MediaItemTypes.Series, this.sources.AniDb).Returns(19);

            var seriesMapping = this.CreateSeriesMapping(53, 19);

            this.mappingList.GetSeriesMappingsFromTvDb(53, Arg.Any<ProcessResultContext>()).Returns(new[]
            {
                seriesMapping
            });

            var loader = new AniDbEpisodeFromTvDb(this.sources, this.mappingList, this.episodeMapper);

            var result = await loader.LoadFrom(this.mediaItem, this.tvDbSourceData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("Failed to find a corresponding AniDb episode in AniDb series id '19'"));
        }

        [Test]
        public async Task LoadFrom_NoSelectableTitle_Fails()
        {
            this.SetUpTvDbSeriesData(53);
            this.embyData.GetParentId(MediaItemTypes.Series, this.sources.AniDb).Returns(19);

            var seriesMapping = this.CreateSeriesMapping(53, 19);

            this.mappingList.GetSeriesMappingsFromTvDb(53, Arg.Any<ProcessResultContext>()).Returns(new[]
            {
                seriesMapping
            });

            var aniDbEpisodeData =  this.SetUpEpisodeMapping(67, 92, seriesMapping, string.Empty);

            this.sources.AniDb.SelectTitle(aniDbEpisodeData.Titles, "en", Arg.Any<ProcessResultContext>())
                .Returns(new ProcessFailedResult(string.Empty, string.Empty, MediaItemTypes.Episode, "Failed to find a title"));

            var loader = new AniDbEpisodeFromTvDb(this.sources, this.mappingList, this.episodeMapper);

            var result = await loader.LoadFrom(this.mediaItem, this.tvDbSourceData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("Failed to find a title"));
        }

        [Test]
        public async Task LoadFrom_ReturnsSourceDataWithSelectedTitle()
        {
            this.SetUpTvDbSeriesData(53);
            this.embyData.GetParentId(MediaItemTypes.Series, this.sources.AniDb).Returns(19);

            var seriesMapping = this.CreateSeriesMapping(53, 19);

            this.mappingList.GetSeriesMappingsFromTvDb(53, Arg.Any<ProcessResultContext>()).Returns(new[]
            {
                seriesMapping
            });

            var aniDbEpisodeData = this.SetUpEpisodeMapping(67, 92, seriesMapping, "Title");
            
            var loader = new AniDbEpisodeFromTvDb(this.sources, this.mappingList, this.episodeMapper);

            var result = await loader.LoadFrom(this.mediaItem, this.tvDbSourceData);

            result.IsRight.Should().BeTrue();
            result.IfRight(sd => sd.Data.Should().Be(aniDbEpisodeData));
            result.IfRight(sd => sd.Source.Should().Be(this.sources.AniDb));
            result.IfRight(sd => sd.Identifier.Should().BeEquivalentTo(new ItemIdentifier(92, 1, "Title")));
        }
    }
}

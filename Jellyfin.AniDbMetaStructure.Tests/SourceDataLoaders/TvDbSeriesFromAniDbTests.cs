using System.Threading.Tasks;
using Jellyfin.AniDbMetaStructure.AniDb.SeriesData;
using Jellyfin.AniDbMetaStructure.Mapping;
using Jellyfin.AniDbMetaStructure.Process;
using Jellyfin.AniDbMetaStructure.Process.Sources;
using Jellyfin.AniDbMetaStructure.SourceDataLoaders;
using Jellyfin.AniDbMetaStructure.Tests.TestData;
using FluentAssertions;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using NSubstitute;
using NUnit.Framework;
using static LanguageExt.Prelude;

namespace Jellyfin.AniDbMetaStructure.Tests.SourceDataLoaders
{
    [TestFixture]
    public class TvDbSeriesFromAniDbTests
    {
        [SetUp]
        public void Setup()
        {
            this.embyData = Substitute.For<IEmbyItemData>();
            this.embyData.Identifier.Returns(new ItemIdentifier(0, 0, "Name"));

            this.mediaItem = Substitute.For<IMediaItem>();
            this.mediaItem.EmbyData.Returns(this.embyData);
            this.mediaItem.ItemType.Returns(MediaItemTypes.Episode);

            this.mappingList = Substitute.For<IMappingList>();

            this.sources = Substitute.For<ISources>();

            this.aniDbSource = Substitute.For<IAniDbSource>();
            this.sources.AniDb.Returns(this.aniDbSource);

            var tvDbSource = Substitute.For<ITvDbSource>();
            this.sources.TvDb.Returns(tvDbSource);

            this.aniDbSourceData = Substitute.For<ISourceData<AniDbSeriesData>>();

            this.embyData.GetParentId(MediaItemTypes.Series, this.aniDbSource).Returns(Option<int>.Some(3));

            this.noMappingResult = new ProcessFailedResult(string.Empty, string.Empty, null, string.Empty);
            this.mappingList.GetSeriesMappingFromAniDb(Arg.Any<int>(), Arg.Any<ProcessResultContext>())
                .Returns(Left<ProcessFailedResult, ISeriesMapping>(this.noMappingResult));
        }

        private IMappingList mappingList;
        private ISources sources;
        private IMediaItem mediaItem;
        private IAniDbSource aniDbSource;
        private ISourceData<AniDbSeriesData> aniDbSourceData;
        private IEmbyItemData embyData;
        private ProcessFailedResult noMappingResult;

        private AniDbSeriesData CreateAniDbSeries(int id, string name)
        {
            var seriesData = new AniDbSeriesData
            {
                Id = id,
                Titles = new[]
                {
                    new ItemTitleData
                    {
                        Title = name
                    }
                }
            };

            this.aniDbSourceData.Id.Returns(id);
            this.aniDbSourceData.Data.Returns(seriesData);

            return seriesData;
        }

        private ISeriesMapping SetUpSeriesMapping(int aniDbSeriesId, int? tvDbSeriesId)
        {
            var seriesMapping = Substitute.For<ISeriesMapping>();
            seriesMapping.Ids.Returns(new SeriesIds(aniDbSeriesId, tvDbSeriesId.ToOption(), Option<int>.None,
                Option<int>.None));

            this.mappingList.GetSeriesMappingFromAniDb(aniDbSeriesId, Arg.Any<ProcessResultContext>())
                .Returns(Right<ProcessFailedResult, ISeriesMapping>(seriesMapping));

            return seriesMapping;
        }

        [Test]
        public void CanLoadFrom_Null_IsFalse()
        {
            var loader = new TvDbSeriesFromAniDb(this.sources, this.mappingList);

            loader.CanLoadFrom(null).Should().BeFalse();
        }

        [Test]
        public void CanLoadFrom_TypeMatch_IsTrue()
        {
            var loader = new TvDbSeriesFromAniDb(this.sources, this.mappingList);

            loader.CanLoadFrom(Substitute.For<ISourceData<AniDbSeriesData>>()).Should().BeTrue();
        }

        [Test]
        public void CanLoadFrom_TypeMisMatch_IsFalse()
        {
            var loader = new TvDbSeriesFromAniDb(this.sources, this.mappingList);

            loader.CanLoadFrom(new object()).Should().BeFalse();
        }

        [Test]
        public async Task LoadFrom_FailsToLoadTvDbSeries_Fails()
        {
            this.CreateAniDbSeries(342, "SeriesName");
            this.SetUpSeriesMapping(342, 66);

            this.sources.TvDb.GetSeriesData(66, Arg.Any<ProcessResultContext>())
                .Returns(new ProcessFailedResult(string.Empty, string.Empty, MediaItemTypes.Series, "Failed"));

            var loader = new TvDbSeriesFromAniDb(this.sources, this.mappingList);

            var result = await loader.LoadFrom(this.mediaItem, this.aniDbSourceData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("Failed"));
        }

        [Test]
        public async Task LoadFrom_MapsToTvDbSeries()
        {
            this.CreateAniDbSeries(342, "SeriesName");
            this.SetUpSeriesMapping(342, 66);

            var tvDbSeriesData = TvDbTestData.Series(66);

            this.sources.TvDb.GetSeriesData(66, Arg.Any<ProcessResultContext>())
                .Returns(tvDbSeriesData);

            var loader = new TvDbSeriesFromAniDb(this.sources, this.mappingList);

            var result = await loader.LoadFrom(this.mediaItem, this.aniDbSourceData);

            result.IsRight.Should().BeTrue();
            result.IfRight(sd => sd.Data.Should().Be(tvDbSeriesData));
            result.IfRight(sd => sd.Source.Should().Be(this.sources.TvDb));
            result.IfRight(sd => sd.Id.ValueUnsafe().Should().Be(66));
            result.IfRight(sd => sd.Identifier.Should().BeEquivalentTo(new ItemIdentifier(Option<int>.None, Option<int>.None, string.Empty)));
        }

        [Test]
        public async Task LoadFrom_NoAniDbIdOnSourceData_Fails()
        {
            this.aniDbSourceData.Id.Returns(Option<int>.None);

            var loader = new TvDbSeriesFromAniDb(this.sources, this.mappingList);

            var result = await loader.LoadFrom(this.mediaItem, this.aniDbSourceData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f =>
                f.Reason.Should().Be("No AniDb Id found on the AniDb data associated with this media item"));
        }

        [Test]
        public async Task LoadFrom_NoSeriesMapping_Fails()
        {
            this.CreateAniDbSeries(342, "SeriesName");

            var loader = new TvDbSeriesFromAniDb(this.sources, this.mappingList);

            var result = await loader.LoadFrom(this.mediaItem, this.aniDbSourceData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Should().Be(this.noMappingResult));
        }

        [Test]
        public async Task LoadFrom_NoTvDbIdOnMapping_Fails()
        {
            this.CreateAniDbSeries(342, "SeriesName");
            this.SetUpSeriesMapping(342, null);

            var loader = new TvDbSeriesFromAniDb(this.sources, this.mappingList);

            var result = await loader.LoadFrom(this.mediaItem, this.aniDbSourceData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("No TvDb Id found on matching mapping"));
        }
    }
}
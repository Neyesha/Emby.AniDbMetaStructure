﻿using FluentAssertions;
using Jellyfin.AniDbMetaStructure.AniDb.SeriesData;
using Jellyfin.AniDbMetaStructure.Mapping;
using Jellyfin.AniDbMetaStructure.Process;
using Jellyfin.AniDbMetaStructure.Process.Sources;
using Jellyfin.AniDbMetaStructure.SourceDataLoaders;
using Jellyfin.AniDbMetaStructure.Tests.TestData;
using Jellyfin.AniDbMetaStructure.TvDb.Data;
using LanguageExt;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace Jellyfin.AniDbMetaStructure.Tests.SourceDataLoaders
{
    [TestFixture]
    public class TvDbEpisodeFromAniDbTests
    {
        [SetUp]
        public void Setup()
        {
            this.JellyfinData = Substitute.For<IJellyfinItemData>();
            this.JellyfinData.Identifier.Returns(new ItemIdentifier(0, 0, "Name"));

            this.mediaItem = Substitute.For<IMediaItem>();
            this.mediaItem.JellyfinData.Returns(this.JellyfinData);
            this.mediaItem.GetDataFromSource(this.aniDbSource).Returns(Option<ISourceData>.Some(this.aniDbSourceData));
            this.mediaItem.ItemType.Returns(MediaItemTypes.Episode);

            this.mappingList = Substitute.For<IMappingList>();

            this.sources = Substitute.For<ISources>();

            this.aniDbSource = Substitute.For<IAniDbSource>();
            this.sources.AniDb.Returns(this.aniDbSource);

            var tvDbSource = Substitute.For<ITvDbSource>();
            this.sources.TvDb.Returns(tvDbSource);

            this.aniDbSourceData = Substitute.For<ISourceData<AniDbEpisodeData>>();
            this.aniDbSourceData.Id.Returns(Option<int>.Some(3));

            this.JellyfinData.GetParentId(MediaItemTypes.Series, this.aniDbSource).Returns(Option<int>.Some(3));

            this.episodeMapper = Substitute.For<IEpisodeMapper>();

            this.mappingList.GetSeriesMappingFromAniDb(Arg.Any<int>(), Arg.Any<ProcessResultContext>())
                .Returns(Left<ProcessFailedResult, ISeriesMapping>(new ProcessFailedResult(string.Empty, string.Empty, null, string.Empty)));
        }

        private IMappingList mappingList;
        private ISources sources;
        private IMediaItem mediaItem;
        private IAniDbSource aniDbSource;
        private ISourceData<AniDbEpisodeData> aniDbSourceData;
        private IJellyfinItemData JellyfinData;
        private IEpisodeMapper episodeMapper;

        private ISeriesMapping SetUpSeriesMapping(int aniDbSeriesId, int tvDbSeriesId)
        {
            var seriesMapping = Substitute.For<ISeriesMapping>();
            seriesMapping.Ids.Returns(new SeriesIds(aniDbSeriesId, tvDbSeriesId, Option<int>.None, Option<int>.None));

            this.mappingList.GetSeriesMappingFromAniDb(aniDbSeriesId, Arg.Any<ProcessResultContext>())
                .Returns(Right<ProcessFailedResult, ISeriesMapping>(seriesMapping));

            return seriesMapping;
        }

        private AniDbSeriesData SetUpAniDbSeriesData(int id)
        {
            var seriesData = new AniDbSeriesData().WithStandardData();

            seriesData.Id = id;

            this.JellyfinData.GetParentId(MediaItemTypes.Series, this.aniDbSource).Returns(id);
            this.aniDbSource.GetSeriesData(this.mediaItem.JellyfinData, Arg.Any<ProcessResultContext>()).Returns(seriesData);

            return seriesData;
        }

        private AniDbEpisodeData SetUpAniDbEpisodeData(int episodeIndex, int season = 1)
        {
            if (season < 0 || season > 1)
            {
                throw new ArgumentOutOfRangeException("Invalid season number");
            }

            var episodeData = new AniDbEpisodeData
            {
                RawEpisodeNumber = new EpisodeNumberData
                {
                    RawNumber = episodeIndex.ToString(),
                    RawType = season == 0 ? 2 : season
                }
            };

            this.aniDbSourceData.Data.Returns(episodeData);

            return episodeData;
        }

        private void SetUpEpisodeMapping(AniDbEpisodeData from, TvDbEpisodeData to, ISeriesMapping seriesMapping)
        {
            this.episodeMapper
                .MapAniDbEpisodeAsync(from.EpisodeNumber.Number, seriesMapping, Option<EpisodeGroupMapping>.None)
                .Returns(to);
        }

        private TvDbEpisodeData CreateTvDbEpisodeData(int episodeIndex, int season = 1, string name = "TvDbEpisodeName")
        {
            return TvDbTestData.Episode(3, episodeIndex, season, name: name);
        }

        [Test]
        public void CanLoadFrom_Null_IsFalse()
        {
            var loader = new TvDbEpisodeFromAniDb(this.sources, this.mappingList, this.episodeMapper);

            loader.CanLoadFrom(null).Should().BeFalse();
        }

        [Test]
        public void CanLoadFrom_TypeMatch_IsTrue()
        {
            var loader = new TvDbEpisodeFromAniDb(this.sources, this.mappingList, this.episodeMapper);

            loader.CanLoadFrom(Substitute.For<ISourceData<AniDbEpisodeData>>()).Should().BeTrue();
        }

        [Test]
        public void CanLoadFrom_TypeMisMatch_IsFalse()
        {
            var loader = new TvDbEpisodeFromAniDb(this.sources, this.mappingList, this.episodeMapper);

            loader.CanLoadFrom(new object()).Should().BeFalse();
        }

        [Test]
        public async Task LoadFrom_EpisodeMappingFails_Fails()
        {
            SetUpSeriesMapping(324, 142);

            SetUpAniDbSeriesData(324);

            SetUpAniDbEpisodeData(33);

            this.episodeMapper.ClearSubstitute();

            var loader = new TvDbEpisodeFromAniDb(this.sources, this.mappingList, this.episodeMapper);

            var result = await loader.LoadFrom(this.mediaItem, this.aniDbSourceData);

            result.IsLeft.Should().BeTrue();
        }

        [Test]
        public async Task LoadFrom_HasMappedEpisodeData_ReturnsMappedEpisodeData()
        {
            SetUpAniDbSeriesData(324);

            var seriesMapping = SetUpSeriesMapping(324, 142);

            var aniDbEpisodeData = SetUpAniDbEpisodeData(33);
            var tvDbEpisodeData = CreateTvDbEpisodeData(55, 6);

            SetUpEpisodeMapping(aniDbEpisodeData, tvDbEpisodeData, seriesMapping);

            var loader = new TvDbEpisodeFromAniDb(this.sources, this.mappingList, this.episodeMapper);

            var result = await loader.LoadFrom(this.mediaItem, this.aniDbSourceData);

            result.IsRight.Should().BeTrue();

            result.IfRight(sd => sd.Data.Should().Be(tvDbEpisodeData));
            result.IfRight(sd => sd.Source.Should().Be(this.sources.TvDb));
            result.IfRight(sd => sd.Identifier.Should().BeEquivalentTo(new ItemIdentifier(55, 6, "TvDbEpisodeName")));
        }

        [Test]
        public async Task LoadFrom_HasSeriesMapping_GetsEpisodeGroupMapping()
        {
            var seriesMapping = SetUpSeriesMapping(324, 142);

            SetUpAniDbSeriesData(324);

            var aniDbEpisodeData = SetUpAniDbEpisodeData(33);

            var loader = new TvDbEpisodeFromAniDb(this.sources, this.mappingList, this.episodeMapper);

            await loader.LoadFrom(this.mediaItem, this.aniDbSourceData);

            seriesMapping.Received(1).GetEpisodeGroupMapping(aniDbEpisodeData.EpisodeNumber);
        }

        [Test]
        public async Task LoadFrom_NoAniDbSeriesData_Fails()
        {
            SetUpSeriesMapping(324, 142);

            SetUpAniDbSeriesData(324);

            SetUpAniDbEpisodeData(33);

            this.aniDbSource.ClearSubstitute();
            this.aniDbSource.GetSeriesData(this.mediaItem.JellyfinData, Arg.Any<ProcessResultContext>())
                .Returns(Left<ProcessFailedResult, AniDbSeriesData>(new ProcessFailedResult(string.Empty, string.Empty,
                    MediaItemTypes.Series,
                    "Failed")));

            var loader = new TvDbEpisodeFromAniDb(this.sources, this.mappingList, this.episodeMapper);

            var result = await loader.LoadFrom(this.mediaItem, this.aniDbSourceData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(r => r.Reason.Should().Be("Failed"));
        }

        [Test]
        public async Task LoadFrom_NoMappedEpisodeData_Fails()
        {
            SetUpSeriesMapping(324, 142);

            SetUpAniDbSeriesData(324);

            SetUpAniDbEpisodeData(33);

            var loader = new TvDbEpisodeFromAniDb(this.sources, this.mappingList, this.episodeMapper);

            var result = await loader.LoadFrom(this.mediaItem, this.aniDbSourceData);

            result.IsLeft.Should().BeTrue();
        }

        [Test]
        public async Task LoadFrom_NoSeriesMapping_Fails()
        {
            SetUpAniDbSeriesData(324);

            SetUpAniDbEpisodeData(33);

            var loader = new TvDbEpisodeFromAniDb(this.sources, this.mappingList, this.episodeMapper);

            var result = await loader.LoadFrom(this.mediaItem, this.aniDbSourceData);

            result.IsLeft.Should().BeTrue();
        }
    }
}
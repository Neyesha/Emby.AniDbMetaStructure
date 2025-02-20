﻿using System.Linq;
using System.Threading.Tasks;
using Jellyfin.AniDbMetaStructure.AniDb.Titles;
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
    public class TvDbEpisodeFromJellyfinDataTests
    {
        [SetUp]
        public void Setup()
        {
            this.sources = Substitute.For<ISources>();

            var tvDbSource = Substitute.For<ITvDbSource>();
            this.sources.TvDb.Returns(tvDbSource);

            this.JellyfinItemData = Substitute.For<IJellyfinItemData>();
            this.JellyfinItemData.Language.Returns("en");
            this.JellyfinItemData.GetParentId(MediaItemTypes.Series, this.sources.TvDb).Returns(22);

            this.mediaItem = Substitute.For<IMediaItem>();
            this.mediaItem.JellyfinData.Returns(this.JellyfinItemData);
            this.mediaItem.ItemType.Returns(MediaItemTypes.Episode);

            this.tvDbClient = Substitute.For<ITvDbClient>();
            this.titleNormaliser = new TitleNormaliser();
        }

        private ISources sources;
        private IMediaItem mediaItem;
        private IJellyfinItemData JellyfinItemData;
        private ITvDbClient tvDbClient;
        private ITitleNormaliser titleNormaliser;

        [Test]
        public void CanLoadFrom_CorrectItemType_IsTrue()
        {
            var loader = new TvDbEpisodeFromJellyfinData(this.sources, this.tvDbClient, this.titleNormaliser);

            loader.CanLoadFrom(MediaItemTypes.Episode).Should().BeTrue();
        }

        [Test]
        public void CanLoadFrom_Null_IsFalse()
        {
            var loader = new TvDbEpisodeFromJellyfinData(this.sources, this.tvDbClient, this.titleNormaliser);

            loader.CanLoadFrom(null).Should().BeFalse();
        }

        [Test]
        public void CanLoadFrom_WrongItemType_IsFalse()
        {
            var loader = new TvDbEpisodeFromJellyfinData(this.sources, this.tvDbClient, this.titleNormaliser);

            loader.CanLoadFrom(MediaItemTypes.Season).Should().BeFalse();
        }
        
        [Test]
        public async Task LoadFrom_NoSeriesId_Fails()
        {
            this.JellyfinItemData.GetParentId(MediaItemTypes.Series, this.sources.TvDb).Returns(Option<int>.None);

            var loader = new TvDbEpisodeFromJellyfinData(this.sources, this.tvDbClient, this.titleNormaliser);

            var result = await loader.LoadFrom(this.JellyfinItemData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("No TvDb Id found on parent series"));
        }

        [Test]
        public async Task LoadFrom_EpisodeLoadFail_Fails()
        {
            this.JellyfinItemData.Identifier.Returns(new ItemIdentifier(4, 1, "Name"));
            
            var loader = new TvDbEpisodeFromJellyfinData(this.sources, this.tvDbClient, this.titleNormaliser);

            var result = await loader.LoadFrom(this.JellyfinItemData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("Failed to load parent series with TvDb Id '22'"));
        }

        [Test]
        public async Task LoadFrom_NoMatchingEpisode_Fails()
        {
            this.JellyfinItemData.Identifier.Returns(new ItemIdentifier(4, 1, "Name"));

            this.tvDbClient.GetEpisodesAsync(22)
                .Returns(new[]
                {
                    TvDbTestData.Episode(1, 1, 1, name: "NonMatch1"),
                    TvDbTestData.Episode(1, 4, 2, name: "NonMatch2")
                }.ToList());

            var loader = new TvDbEpisodeFromJellyfinData(this.sources, this.tvDbClient, this.titleNormaliser);

            var result = await loader.LoadFrom(this.JellyfinItemData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("Failed to find TvDb episode"));
        }

        [Test]
        public async Task LoadFrom_MatchOnEpisodeAndSeasonIndex_CreatesSourceData()
        {
            var expected = TvDbTestData.Episode(1, 4, 2, name: "Match");

            this.JellyfinItemData.Identifier.Returns(new ItemIdentifier(4, 2, "Name"));

            this.tvDbClient.GetEpisodesAsync(22)
                .Returns(new[]
                {
                    TvDbTestData.Episode(1, 1, 1, name: "NonMatch"),
                    expected
                }.ToList());

            var loader = new TvDbEpisodeFromJellyfinData(this.sources, this.tvDbClient, this.titleNormaliser);

            var result = await loader.LoadFrom(this.JellyfinItemData);

            result.IsRight.Should().BeTrue();
            result.IfRight(sd => sd.Data.Should().Be(expected));
            result.IfRight(sd => sd.Source.Should().Be(this.sources.TvDb));
            result.IfRight(sd => sd.Identifier.Should().BeEquivalentTo(new ItemIdentifier(4, 2, "Match")));
        }

        [Test]
        public async Task LoadFrom_MatchOnEpisodeAndDefaultSeasonIndex_CreatesSourceData()
        {
            var expected = TvDbTestData.Episode(1, 4, 1, name: "Match");

            this.JellyfinItemData.Identifier.Returns(new ItemIdentifier(4, Option<int>.None, "Name"));

            this.tvDbClient.GetEpisodesAsync(22)
                .Returns(new[]
                {
                    TvDbTestData.Episode(1, 1, 2, name: "NonMatch"),
                    expected
                }.ToList());

            var loader = new TvDbEpisodeFromJellyfinData(this.sources, this.tvDbClient, this.titleNormaliser);

            var result = await loader.LoadFrom(this.JellyfinItemData);

            result.IsRight.Should().BeTrue();
            result.IfRight(sd => sd.Data.Should().Be(expected));
            result.IfRight(sd => sd.Source.Should().Be(this.sources.TvDb));
            result.IfRight(sd => sd.Identifier.Should().BeEquivalentTo(new ItemIdentifier(4, 1, "Match")));
        }

        [Test]
        public async Task LoadFrom_MatchOnTitle_CreatesSourceData()
        {
            var expected = TvDbTestData.Episode(1, 6, 1, name: "Match");

            this.JellyfinItemData.Identifier.Returns(new ItemIdentifier(4, 2, "Match"));

            this.tvDbClient.GetEpisodesAsync(22)
                .Returns(new[]
                {
                    TvDbTestData.Episode(1, 1, 1, name: "NonMatch"),
                    expected
                }.ToList());

            var loader = new TvDbEpisodeFromJellyfinData(this.sources, this.tvDbClient, this.titleNormaliser);

            var result = await loader.LoadFrom(this.JellyfinItemData);

            result.IsRight.Should().BeTrue();
            result.IfRight(sd => sd.Data.Should().Be(expected));
            result.IfRight(sd => sd.Source.Should().Be(this.sources.TvDb));
            result.IfRight(sd => sd.Identifier.Should().BeEquivalentTo(new ItemIdentifier(6, 1, "Match")));
        }

    }
}
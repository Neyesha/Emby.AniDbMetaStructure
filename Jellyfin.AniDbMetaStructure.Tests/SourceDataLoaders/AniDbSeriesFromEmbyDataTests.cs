﻿using System.Threading.Tasks;
using Jellyfin.AniDbMetaStructure.AniDb;
using Jellyfin.AniDbMetaStructure.AniDb.SeriesData;
using Jellyfin.AniDbMetaStructure.Process;
using Jellyfin.AniDbMetaStructure.Process.Sources;
using Jellyfin.AniDbMetaStructure.SourceDataLoaders;
using Jellyfin.AniDbMetaStructure.Tests.TestData;
using FluentAssertions;
using LanguageExt;
using NSubstitute;
using NUnit.Framework;

namespace Jellyfin.AniDbMetaStructure.Tests.SourceDataLoaders
{
    [TestFixture]
    public class AniDbSeriesFromJellyfinDataTests
    {
        [SetUp]
        public void Setup()
        {
            this.aniDbSource = Substitute.For<IAniDbSource>();
            this.aniDbSource.GetSeriesData(this.JellyfinItemData, Arg.Any<ProcessResultContext>())
                .Returns(x => this.aniDbSeriesData);

            this.sources = Substitute.For<ISources>();
            this.sources.AniDb.Returns(this.aniDbSource);

            this.aniDbClient = Substitute.For<IAniDbClient>();

            this.JellyfinItemData = Substitute.For<IJellyfinItemData>();
            this.JellyfinItemData.Identifier.Returns(new ItemIdentifier(67, 1, "Name"));
            this.JellyfinItemData.Language.Returns("en");

            this.aniDbSeriesData = new AniDbSeriesData().WithStandardData();
        }

        private ISources sources;
        private IAniDbClient aniDbClient;
        private IJellyfinItemData JellyfinItemData;
        private AniDbSeriesData aniDbSeriesData;
        private IAniDbSource aniDbSource;

        [Test]
        public void CanLoadFrom_CorrectItemType_IsTrue()
        {
            var loader = new AniDbSeriesFromJellyfinData(this.aniDbClient, this.sources);

            loader.CanLoadFrom(MediaItemTypes.Series).Should().BeTrue();
        }

        [Test]
        public void CanLoadFrom_Null_IsFalse()
        {
            var loader = new AniDbSeriesFromJellyfinData(this.aniDbClient, this.sources);

            loader.CanLoadFrom(null).Should().BeFalse();
        }

        [Test]
        public void CanLoadFrom_WrongItemType_IsFalse()
        {
            var loader = new AniDbSeriesFromJellyfinData(this.aniDbClient, this.sources);

            loader.CanLoadFrom(MediaItemTypes.Season).Should().BeFalse();
        }

        [Test]
        public async Task LoadFrom_CreatesSourceData()
        {
            this.aniDbClient.FindSeriesAsync("Name").Returns(this.aniDbSeriesData);
            this.sources.AniDb.SelectTitle(this.aniDbSeriesData.Titles, "en", Arg.Any<ProcessResultContext>())
                .Returns("Title");

            var loader = new AniDbSeriesFromJellyfinData(this.aniDbClient, this.sources);

            var result = await loader.LoadFrom(this.JellyfinItemData);

            result.IsRight.Should().BeTrue();
            result.IfRight(sd => sd.Data.Should().Be(this.aniDbSeriesData));
            result.IfRight(sd => sd.Source.Should().Be(this.sources.AniDb));
            result.IfRight(sd =>
                sd.Identifier.Should().BeEquivalentTo(new ItemIdentifier(67, Option<int>.None, "Title")));
        }

        [Test]
        public async Task LoadFrom_NoMatchingSeries_Fails()
        {
            var loader = new AniDbSeriesFromJellyfinData(this.aniDbClient, this.sources);

            var result = await loader.LoadFrom(this.JellyfinItemData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("Failed to find series in AniDb"));
        }

        [Test]
        public async Task LoadFrom_NoTitle_Fails()
        {
            this.aniDbClient.FindSeriesAsync("Name").Returns(this.aniDbSeriesData);
            this.sources.AniDb.SelectTitle(this.aniDbSeriesData.Titles, "en", Arg.Any<ProcessResultContext>())
                .Returns(new ProcessFailedResult(string.Empty, string.Empty, MediaItemTypes.Series, "FailedTitle"));

            var loader = new AniDbSeriesFromJellyfinData(this.aniDbClient, this.sources);

            var result = await loader.LoadFrom(this.JellyfinItemData);

            result.IsLeft.Should().BeTrue();
            result.IfLeft(f => f.Reason.Should().Be("FailedTitle"));
        }
    }
}
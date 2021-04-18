using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Jellyfin.AniDbMetaStructure.Configuration;
using Jellyfin.AniDbMetaStructure.Files;
using Jellyfin.AniDbMetaStructure.Infrastructure;
using Jellyfin.AniDbMetaStructure.JsonApi;
using Jellyfin.AniDbMetaStructure.Tests.TestHelpers;
using Jellyfin.AniDbMetaStructure.TvDb;
using Jellyfin.AniDbMetaStructure.TvDb.Data;
using Jellyfin.AniDbMetaStructure.TvDb.Requests;
using FluentAssertions;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using MediaBrowser.Common.Configuration;
using NSubstitute;
using NUnit.Framework;

namespace Jellyfin.AniDbMetaStructure.Tests
{
    [TestFixture]
    public class TvDbClientTests
    {
        [SetUp]
        public void Setup()
        {
            this.jsonConnection = Substitute.For<IJsonConnection>();
            this.fileCache = Substitute.For<IFileCache>();
            this.applicationPaths = Substitute.For<IApplicationPaths>();
            this.logger = new ConsoleLogger();

            this.jsonConnection.PostAsync(Arg.Any<LoginRequest>(), Option<string>.None)
                .ReturnsForAnyArgs(new Response<LoginRequest.Response>(new LoginRequest.Response("TOKEN")));
        }

        private IJsonConnection jsonConnection;
        private IFileCache fileCache;
        private IApplicationPaths applicationPaths;
        private ConsoleLogger logger;

        [Test]
        public async Task GetEpisodeAsync_NoLocalEpisodeData_RequestsEpisodeData()
        {
            var episode = new TvDbEpisodeSummaryData(1, "Test", 1L, 2, 3, 4, new DateTime(2017, 1, 2, 3, 4, 5), "Overview");
            var episodeDetail = new TvDbEpisodeData(1, "Test", 1L, 2, 3, 4, new DateTime(2017, 1, 2, 3, 4, 5), "Overview", 33.4f, 12);

            this.jsonConnection.GetAsync(Arg.Any<GetEpisodesRequest>(), Arg.Any<Option<string>>())
                .Returns(new Response<GetEpisodesRequest.Response>(new GetEpisodesRequest.Response(new[] { episode },
                    new GetEpisodesRequest.PageLinks(1, 1, Option<int>.None, Option<int>.None))));
            this.jsonConnection.GetAsync(Arg.Any<GetEpisodeDetailsRequest>(), Arg.Any<Option<string>>())
                .Returns(new Response<GetEpisodeDetailsRequest.Response>(
                    new GetEpisodeDetailsRequest.Response(episodeDetail)));

            var tvDbClient = new TvDbClientV2(this.jsonConnection, this.fileCache, this.applicationPaths, this.logger,
                new JsonSerialiser(), new PluginConfiguration());

            await tvDbClient.GetEpisodesAsync(4);

            await this.jsonConnection.ReceivedWithAnyArgs(1)
                .GetAsync<GetEpisodesRequest.Response>(null, Option<string>.None);
            await this.jsonConnection.ReceivedWithAnyArgs(1)
                .GetAsync<GetEpisodeDetailsRequest.Response>(null, Option<string>.None);
        }

        [Test]
        public async Task GetEpisodeAsync_NoLocalEpisodeData_ReturnsNewEpisodeData()
        {
            var episode = new TvDbEpisodeSummaryData(1, "Test", 1L, 2, 3, 4, new DateTime(2017, 1, 2, 3, 4, 5), "Overview");
            var episodeDetail = new TvDbEpisodeData(1, "Test", 1L, 2, 3, 4, new DateTime(2017, 1, 2, 3, 4, 5), "Overview", 33.4f, 12);

            this.jsonConnection.GetAsync(Arg.Any<GetEpisodesRequest>(), Arg.Any<Option<string>>())
                .Returns(new Response<GetEpisodesRequest.Response>(new GetEpisodesRequest.Response(new[] { episode },
                    new GetEpisodesRequest.PageLinks(1, 1, Option<int>.None, Option<int>.None))));
            this.jsonConnection.GetAsync(Arg.Any<GetEpisodeDetailsRequest>(), Arg.Any<Option<string>>())
                .Returns(new Response<GetEpisodeDetailsRequest.Response>(
                    new GetEpisodeDetailsRequest.Response(episodeDetail)));

            var tvDbClient = new TvDbClientV2(this.jsonConnection, this.fileCache, this.applicationPaths, this.logger,
                new JsonSerialiser(), new PluginConfiguration());

            var episodes = await tvDbClient.GetEpisodesAsync(4);

            episodes.IsSome.Should().BeTrue();
            episodes.ValueUnsafe().Should().BeEquivalentTo(episodeDetail);
        }

        [Test]
        public async Task GetEpisodeAsync_NoLocalEpisodeData_SavesNewEpisodeData()
        {
            var episode = new TvDbEpisodeSummaryData(1, "Test", 1L, 2, 3, 4, new DateTime(2017, 1, 2, 3, 4, 5), "Overview");
            var episodeDetail = new TvDbEpisodeData(1, "Test", 1L, 2, 3, 4, new DateTime(2017, 1, 2, 3, 4, 5), "Overview", 33.4f, 12);

            this.jsonConnection.GetAsync(Arg.Any<GetEpisodesRequest>(), Arg.Any<Option<string>>())
                .Returns(new Response<GetEpisodesRequest.Response>(new GetEpisodesRequest.Response(new[] { episode },
                    new GetEpisodesRequest.PageLinks(1, 1, Option<int>.None, Option<int>.None))));
            this.jsonConnection.GetAsync(Arg.Any<GetEpisodeDetailsRequest>(), Arg.Any<Option<string>>())
                .Returns(new Response<GetEpisodeDetailsRequest.Response>(
                    new GetEpisodeDetailsRequest.Response(episodeDetail)));

            var tvDbClient = new TvDbClientV2(this.jsonConnection, this.fileCache, this.applicationPaths, this.logger,
                new JsonSerialiser(), new PluginConfiguration());

            await tvDbClient.GetEpisodesAsync(4);

            this.fileCache.Received(1)
                .SaveFile(Arg.Any<TvDbSeriesEpisodesFileSpec>(),
                    Arg.Is<TvDbEpisodeCollection>(d => d.Episodes.Single() == episodeDetail));
        }

        [Test]
        public async Task GetEpisodesAsync_FailedResponse_ReturnsNone()
        {
            this.jsonConnection.GetAsync(Arg.Any<GetEpisodesRequest>(), Arg.Any<Option<string>>())
                .Returns(new FailedRequest(HttpStatusCode.BadRequest, "Failed"));

            var tvDbClient = new TvDbClientV2(this.jsonConnection, this.fileCache, this.applicationPaths, this.logger,
                new JsonSerialiser(), new PluginConfiguration());

            var episodes = await tvDbClient.GetEpisodesAsync(4);

            episodes.IsSome.Should().BeFalse();
        }

        [Test]
        public async Task GetEpisodesAsync_LocalEpisodeData_DoesNotRequestEpisodeData()
        {
            var episode = new TvDbEpisodeData(1, "Test", 1L, 2, 3, 4, new DateTime(2017, 1, 2, 3, 4, 5), "Overview", 3.77f, 12);

            this.fileCache.GetFileContent(Arg.Any<TvDbSeriesEpisodesFileSpec>())
                .Returns(new TvDbEpisodeCollection(new[] { episode }));

            var tvDbClient = new TvDbClientV2(this.jsonConnection, this.fileCache, this.applicationPaths, this.logger,
                new JsonSerialiser(), new PluginConfiguration());

            await tvDbClient.GetEpisodesAsync(4);

            await this.jsonConnection.DidNotReceiveWithAnyArgs()
                .GetAsync<GetEpisodesRequest.Response>(null, Option<string>.None);
        }

        [Test]
        public async Task GetEpisodesAsync_LocalEpisodeData_ReturnsLocalEpisodeData()
        {
            var episode = new TvDbEpisodeData(1, "Test", 1L, 2, 3, 4, new DateTime(2017, 1, 2, 3, 4, 5), "Overview", 3.77f, 12);

            this.fileCache.GetFileContent(Arg.Any<TvDbSeriesEpisodesFileSpec>())
                .Returns(new TvDbEpisodeCollection(new[] { episode }));

            var tvDbClient = new TvDbClientV2(this.jsonConnection, this.fileCache, this.applicationPaths, this.logger,
                new JsonSerialiser(), new PluginConfiguration());

            var episodes = await tvDbClient.GetEpisodesAsync(4);

            episodes.IsSome.Should().BeTrue();
            episodes.ValueUnsafe().Should().BeEquivalentTo(episode);
        }

        [Test]
        public async Task GetEpisodesAsync_MultiPageResponse_RequestsAllPages()
        {
            var page1Episode = new TvDbEpisodeSummaryData(1, "Test1", 1L, 2, 3, 4, new DateTime(2017, 1, 2, 3, 4, 5), "Overview");
            var page1EpisodeDetail = new TvDbEpisodeData(1, "Test", 1L, 2, 3, 4, new DateTime(2017, 1, 2, 3, 4, 5), "Overview", 33.4f, 12);

            var page2Episode = new TvDbEpisodeSummaryData(2, "Test2", 5L, 6, 7, 8, new DateTime(2017, 1, 2, 3, 4, 5), "Overview");
            var page2EpisodeDetail = new TvDbEpisodeData(2, "Test2", 5L, 6, 7, 8, new DateTime(2017, 1, 2, 3, 4, 5), "Overview", 33.4f, 12);

            this.jsonConnection
                .GetAsync(Arg.Is<GetEpisodesRequest>(r => r.Url == "https://api.thetvdb.com/series/4/episodes?page=1"),
                    Arg.Any<Option<string>>())
                .Returns(new Response<GetEpisodesRequest.Response>(new GetEpisodesRequest.Response(
                    new[] { page1Episode },
                    new GetEpisodesRequest.PageLinks(1, 2, 2, Option<int>.None))));
            this.jsonConnection.GetAsync(Arg.Is<GetEpisodeDetailsRequest>(r => r.Url.EndsWith("1")), Arg.Any<Option<string>>())
                .Returns(new Response<GetEpisodeDetailsRequest.Response>(
                    new GetEpisodeDetailsRequest.Response(page1EpisodeDetail)));

            this.jsonConnection
                .GetAsync(Arg.Is<GetEpisodesRequest>(r => r.Url == "https://api.thetvdb.com/series/4/episodes?page=2"),
                    Arg.Any<Option<string>>())
                .Returns(new Response<GetEpisodesRequest.Response>(new GetEpisodesRequest.Response(
                    new[] { page2Episode },
                    new GetEpisodesRequest.PageLinks(2, 2, Option<int>.None, 1))));
            this.jsonConnection.GetAsync(Arg.Is<GetEpisodeDetailsRequest>(r => r.Url.EndsWith("2")), Arg.Any<Option<string>>())
                .Returns(new Response<GetEpisodeDetailsRequest.Response>(
                    new GetEpisodeDetailsRequest.Response(page2EpisodeDetail)));

            var tvDbClient = new TvDbClientV2(this.jsonConnection, this.fileCache, this.applicationPaths, this.logger,
                new JsonSerialiser(), new PluginConfiguration());

            await tvDbClient.GetEpisodesAsync(4);

            await this.jsonConnection.Received(1)
                .GetAsync(Arg.Is<GetEpisodesRequest>(r => r.Url == "https://api.thetvdb.com/series/4/episodes?page=1"),
                    Arg.Any<Option<string>>());
            await this.jsonConnection.Received(1)
                .GetAsync(Arg.Is<GetEpisodesRequest>(r => r.Url == "https://api.thetvdb.com/series/4/episodes?page=2"),
                    Arg.Any<Option<string>>());
            await this.jsonConnection.ReceivedWithAnyArgs(2)
                .GetAsync<GetEpisodeDetailsRequest.Response>(null, Option<string>.None);
        }

        [Test]
        public async Task GetEpisodesAsync_MultiPageResponse_ReturnsAllPagesConcatenated()
        {
            var page1Episode = new TvDbEpisodeSummaryData(1, "Test1", 1L, 2, 3, 4, new DateTime(2017, 1, 2, 3, 4, 5), "Overview");
            var page1EpisodeDetail = new TvDbEpisodeData(1, "Test", 1L, 2, 3, 4, new DateTime(2017, 1, 2, 3, 4, 5), "Overview", 33.4f, 12);

            var page2Episode = new TvDbEpisodeSummaryData(2, "Test2", 5L, 6, 7, 8, new DateTime(2017, 1, 2, 3, 4, 5), "Overview");
            var page2EpisodeDetail = new TvDbEpisodeData(2, "Test2", 5L, 6, 7, 8, new DateTime(2017, 1, 2, 3, 4, 5), "Overview", 33.4f, 12);

            this.jsonConnection
                .GetAsync(Arg.Is<GetEpisodesRequest>(r => r.Url == "https://api.thetvdb.com/series/4/episodes?page=1"),
                    Arg.Any<Option<string>>())
                .Returns(new Response<GetEpisodesRequest.Response>(new GetEpisodesRequest.Response(
                    new[] { page1Episode },
                    new GetEpisodesRequest.PageLinks(1, 2, 2, Option<int>.None))));
            this.jsonConnection.GetAsync(Arg.Is<GetEpisodeDetailsRequest>(r => r.Url.EndsWith("1")), Arg.Any<Option<string>>())
                .Returns(new Response<GetEpisodeDetailsRequest.Response>(
                    new GetEpisodeDetailsRequest.Response(page1EpisodeDetail)));

            this.jsonConnection
                .GetAsync(Arg.Is<GetEpisodesRequest>(r => r.Url == "https://api.thetvdb.com/series/4/episodes?page=2"),
                    Arg.Any<Option<string>>())
                .Returns(new Response<GetEpisodesRequest.Response>(new GetEpisodesRequest.Response(
                    new[] { page2Episode },
                    new GetEpisodesRequest.PageLinks(2, 2, Option<int>.None, 1))));
            this.jsonConnection.GetAsync(Arg.Is<GetEpisodeDetailsRequest>(r => r.Url.EndsWith("2")), Arg.Any<Option<string>>())
                .Returns(new Response<GetEpisodeDetailsRequest.Response>(
                    new GetEpisodeDetailsRequest.Response(page2EpisodeDetail)));

            var tvDbClient = new TvDbClientV2(this.jsonConnection, this.fileCache, this.applicationPaths, this.logger,
                new JsonSerialiser(), new PluginConfiguration());

            var episodes = await tvDbClient.GetEpisodesAsync(4);

            episodes.IsSome.Should().BeTrue();
            episodes.ValueUnsafe().Should().BeEquivalentTo(page1EpisodeDetail, page2EpisodeDetail);
        }

        [Test]
        public async Task GetEpisodesAsync_MultiPageResponse_SavesAllPagesConcatenated()
        {
            var page1Episode = new TvDbEpisodeSummaryData(1, "Test1", 1L, 2, 3, 4, new DateTime(2017, 1, 2, 3, 4, 5), "Overview");
            var page1EpisodeDetail = new TvDbEpisodeData(1, "Test", 1L, 2, 3, 4, new DateTime(2017, 1, 2, 3, 4, 5), "Overview", 33.4f, 12);

            var page2Episode = new TvDbEpisodeSummaryData(2, "Test2", 5L, 6, 7, 8, new DateTime(2017, 1, 2, 3, 4, 5), "Overview");
            var page2EpisodeDetail = new TvDbEpisodeData(2, "Test2", 5L, 6, 7, 8, new DateTime(2017, 1, 2, 3, 4, 5), "Overview", 33.4f, 12);

            this.jsonConnection
                .GetAsync(Arg.Is<GetEpisodesRequest>(r => r.Url == "https://api.thetvdb.com/series/4/episodes?page=1"),
                    Arg.Any<Option<string>>())
                .Returns(new Response<GetEpisodesRequest.Response>(new GetEpisodesRequest.Response(
                    new[] { page1Episode },
                    new GetEpisodesRequest.PageLinks(1, 2, 2, Option<int>.None))));
            this.jsonConnection.GetAsync(Arg.Is<GetEpisodeDetailsRequest>(r => r.Url.EndsWith("1")), Arg.Any<Option<string>>())
                .Returns(new Response<GetEpisodeDetailsRequest.Response>(
                    new GetEpisodeDetailsRequest.Response(page1EpisodeDetail)));

            this.jsonConnection
                .GetAsync(Arg.Is<GetEpisodesRequest>(r => r.Url == "https://api.thetvdb.com/series/4/episodes?page=2"),
                    Arg.Any<Option<string>>())
                .Returns(new Response<GetEpisodesRequest.Response>(new GetEpisodesRequest.Response(
                    new[] { page2Episode },
                    new GetEpisodesRequest.PageLinks(2, 2, Option<int>.None, 1))));
            this.jsonConnection.GetAsync(Arg.Is<GetEpisodeDetailsRequest>(r => r.Url.EndsWith("2")), Arg.Any<Option<string>>())
                .Returns(new Response<GetEpisodeDetailsRequest.Response>(
                    new GetEpisodeDetailsRequest.Response(page2EpisodeDetail)));

            var tvDbClient = new TvDbClientV2(this.jsonConnection, this.fileCache, this.applicationPaths, this.logger,
                new JsonSerialiser(), new PluginConfiguration());

            await tvDbClient.GetEpisodesAsync(4);

            this.fileCache.Received(1)
                .SaveFile(Arg.Any<TvDbSeriesEpisodesFileSpec>(),
                    Arg.Is<TvDbEpisodeCollection>(d => d.Episodes.SequenceEqual(new[] { page1EpisodeDetail, page2EpisodeDetail })));
        }

        [Test]
        public async Task GetSeriesAsync_FailedResponse_ReturnsNone()
        {
            this.jsonConnection.GetAsync(Arg.Any<GetSeriesRequest>(), Arg.Any<Option<string>>())
                .Returns(new FailedRequest(HttpStatusCode.BadRequest, "Failed"));

            var tvDbClient = new TvDbClientV2(this.jsonConnection, this.fileCache, this.applicationPaths, this.logger,
                new JsonSerialiser(), new PluginConfiguration());

            var seriesResult = await tvDbClient.GetSeriesAsync(4);

            seriesResult.IsSome.Should().BeFalse();
        }

        [Test]
        public async Task GetSeriesAsync_LocalSeriesData_DoesNotRequestSeriesData()
        {
            var series = new TvDbSeriesData(4, "TestSeries", new DateTime(2017, 1, 1, 1, 1, 1), string.Empty, 2,
                AirDay.Monday, string.Empty, 4f, new[] { "Alias1", "Alias2" }, new[] { "Genre1", "Genre2" },
                "Overview");

            this.fileCache.GetFileContent(Arg.Any<TvDbSeriesFileSpec>())
                .Returns(series);

            this.jsonConnection.GetAsync(Arg.Any<GetSeriesRequest>(), Arg.Any<Option<string>>())
                .Returns(new Response<GetSeriesRequest.Response>(new GetSeriesRequest.Response(series)));

            var tvDbClient = new TvDbClientV2(this.jsonConnection, this.fileCache, this.applicationPaths, this.logger,
                new JsonSerialiser(), new PluginConfiguration());

            await tvDbClient.GetSeriesAsync(4);

            await this.jsonConnection.DidNotReceiveWithAnyArgs()
                .GetAsync<GetSeriesRequest.Response>(null, Option<string>.None);
        }

        [Test]
        public async Task GetSeriesAsync_LocalSeriesData_ReturnsLocalSeriesData()
        {
            var series = new TvDbSeriesData(4, "TestSeries", new DateTime(2017, 1, 1, 1, 1, 1), string.Empty, 2,
                AirDay.Monday, string.Empty, 4f, new[] { "Alias1", "Alias2" }, new[] { "Genre1", "Genre2" },
                "Overview");

            this.fileCache.GetFileContent(Arg.Any<TvDbSeriesFileSpec>())
                .Returns(series);

            this.jsonConnection.GetAsync(Arg.Any<GetSeriesRequest>(), Arg.Any<Option<string>>())
                .Returns(new Response<GetSeriesRequest.Response>(new GetSeriesRequest.Response(series)));

            var tvDbClient = new TvDbClientV2(this.jsonConnection, this.fileCache, this.applicationPaths, this.logger,
                new JsonSerialiser(), new PluginConfiguration());

            var seriesResult = await tvDbClient.GetSeriesAsync(4);

            seriesResult.IsSome.Should().BeTrue();
            seriesResult.ValueUnsafe().Should().BeEquivalentTo(series);
        }

        [Test]
        public async Task GetSeriesAsync_NoLocalSeriesData_RequestsSeriesData()
        {
            var series = new TvDbSeriesData(4, "TestSeries", new DateTime(2017, 1, 1, 1, 1, 1), string.Empty, 2,
                AirDay.Monday, string.Empty, 4f, new[] { "Alias1", "Alias2" }, new[] { "Genre1", "Genre2" },
                "Overview");

            this.jsonConnection.GetAsync(Arg.Any<GetSeriesRequest>(), Arg.Any<Option<string>>())
                .Returns(new Response<GetSeriesRequest.Response>(new GetSeriesRequest.Response(series)));

            var tvDbClient = new TvDbClientV2(this.jsonConnection, this.fileCache, this.applicationPaths, this.logger,
                new JsonSerialiser(), new PluginConfiguration());

            await tvDbClient.GetSeriesAsync(4);

            await this.jsonConnection.ReceivedWithAnyArgs(1)
                .GetAsync<GetSeriesRequest.Response>(null, Option<string>.None);
        }

        [Test]
        public async Task GetSeriesAsync_NoLocalSeriesData_ReturnsNewSeriesData()
        {
            var series = new TvDbSeriesData(4, "TestSeries", new DateTime(2017, 1, 1, 1, 1, 1), string.Empty, 2,
                AirDay.Monday, string.Empty, 4f, new[] { "Alias1", "Alias2" }, new[] { "Genre1", "Genre2" },
                "Overview");

            this.jsonConnection.GetAsync(Arg.Any<GetSeriesRequest>(), Arg.Any<Option<string>>())
                .Returns(new Response<GetSeriesRequest.Response>(new GetSeriesRequest.Response(series)));

            var tvDbClient = new TvDbClientV2(this.jsonConnection, this.fileCache, this.applicationPaths, this.logger,
                new JsonSerialiser(), new PluginConfiguration());

            var seriesResult = await tvDbClient.GetSeriesAsync(4);
            seriesResult.IsSome.Should().BeTrue();
            seriesResult.ValueUnsafe().Should().Be(series);
        }

        [Test]
        public async Task GetSeriesAsync_NoLocalSeriesData_SavesNewSeriesData()
        {
            var series = new TvDbSeriesData(4, "TestSeries", new DateTime(2017, 1, 1, 1, 1, 1), string.Empty, 2,
                AirDay.Monday, string.Empty, 4f, new[] { "Alias1", "Alias2" }, new[] { "Genre1", "Genre2" },
                "Overview");

            this.jsonConnection.GetAsync(Arg.Any<GetSeriesRequest>(), Arg.Any<Option<string>>())
                .Returns(new Response<GetSeriesRequest.Response>(new GetSeriesRequest.Response(series)));

            var tvDbClient = new TvDbClientV2(this.jsonConnection, this.fileCache, this.applicationPaths, this.logger,
                new JsonSerialiser(), new PluginConfiguration());

            await tvDbClient.GetSeriesAsync(4);

            this.fileCache.Received(1)
                .SaveFile(Arg.Any<TvDbSeriesFileSpec>(),
                    Arg.Is<TvDbSeriesData>(d => d == series));
        }
    }
}
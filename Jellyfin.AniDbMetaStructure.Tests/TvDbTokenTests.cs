using System.Net;
using System.Threading.Tasks;
using Jellyfin.AniDbMetaStructure.JsonApi;
using Jellyfin.AniDbMetaStructure.Tests.TestHelpers;
using Jellyfin.AniDbMetaStructure.TvDb;
using Jellyfin.AniDbMetaStructure.TvDb.Requests;
using FluentAssertions;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using NSubstitute;
using NUnit.Framework;

namespace Emby.AniDbMetaStructure.Tests
{
    [TestFixture]
    internal class TvDbTokenTests
    {
        [SetUp]
        public void Setup()
        {
            this.logManager = new ConsoleLogManager();
        }

        private ConsoleLogManager logManager;

        [Test]
        public async Task GetToken_ExistingToken_DoesNotRequestNewToken()
        {
            var jsonConnection = Substitute.For<IJsonConnection>();
            jsonConnection.PostAsync(Arg.Is<LoginRequest>(r =>
                    r.Url == "https://api.thetvdb.com/login" &&
                    (r.Data as LoginRequest.RequestData).ApiKey == "apiKey"), Option<string>.None)
                .Returns(new Response<LoginRequest.Response>(new LoginRequest.Response("TOKEN")));

            var token = new TvDbToken(jsonConnection, "apiKey", this.logManager);

            await token.GetTokenAsync();

            var returnedToken = await token.GetTokenAsync();

            returnedToken.IsSome.Should().BeTrue();
            returnedToken.ValueUnsafe().Should().Be("TOKEN");

            await jsonConnection.ReceivedWithAnyArgs(1).PostAsync<LoginRequest.Response>(null, Option<string>.None);
        }

        [Test]
        public async Task GetToken_FailedRequest_ReturnsNone()
        {
            var jsonConnection = Substitute.For<IJsonConnection>();
            jsonConnection.PostAsync(Arg.Is<LoginRequest>(r =>
                    r.Url == "https://api.thetvdb.com/login" &&
                    (r.Data as LoginRequest.RequestData).ApiKey == "apiKey"), Option<string>.None)
                .Returns(new FailedRequest(HttpStatusCode.BadRequest, "Failed"));

            var token = new TvDbToken(jsonConnection, "apiKey", this.logManager);

            var returnedToken = await token.GetTokenAsync();

            returnedToken.IsSome.Should().BeFalse();
        }

        [Test]
        public async Task GetToken_NoExistingToken_RequestsToken()
        {
            var jsonConnection = Substitute.For<IJsonConnection>();
            jsonConnection.PostAsync(Arg.Is<LoginRequest>(r =>
                    r.Url == "https://api.thetvdb.com/login" &&
                    (r.Data as LoginRequest.RequestData).ApiKey == "apiKey"), Option<string>.None)
                .Returns(new Response<LoginRequest.Response>(new LoginRequest.Response("TOKEN")));

            var token = new TvDbToken(jsonConnection, "apiKey", this.logManager);

            var returnedToken = await token.GetTokenAsync();

            returnedToken.IsSome.Should().BeTrue();
            returnedToken.ValueUnsafe().Should().Be("TOKEN");
        }
    }
}
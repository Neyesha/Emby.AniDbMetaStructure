using System.Threading.Tasks;
using Jellyfin.AniDbMetaStructure.Infrastructure;
using Jellyfin.AniDbMetaStructure.JsonApi;
using Jellyfin.AniDbMetaStructure.Tests.TestHelpers;
using Jellyfin.AniDbMetaStructure.TvDb;
using FluentAssertions;
using LanguageExt.UnsafeValueAccess;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Jellyfin.AniDbMetaStructure.Tests.IntegrationTests
{
    [TestFixture]
    [Explicit]
    internal class TvDbTokenIntegrationTests
    {
        [SetUp]
        public void Setup()
        {
            this.logger = new ConsoleLogger();
        }

        private ILogger logger;

        [Test]
        public async Task GetToken_ExistingToken_DoesNotRequestNewToken()
        {
            var tvDbConnection = new JsonConnection(new JsonSerialiser(), this.logger);

            var token = new TvDbToken(tvDbConnection, Secrets.TvDbApiKey, this.logger);

            var token1 = await token.GetTokenAsync();

            var token2 = await token.GetTokenAsync();

            token2.IsSome.Should().BeTrue();
            token2.ValueUnsafe().Should().Be(token1.ValueUnsafe());
        }

        [Test]
        public async Task GetToken_FailedRequest_ReturnsNone()
        {
            var tvDbConnection = new JsonConnection(new JsonSerialiser(), this.logger);

            var token = new TvDbToken(tvDbConnection, "NotValid", this.logger);

            var returnedToken = await token.GetTokenAsync();

            returnedToken.IsSome.Should().BeFalse();
        }

        [Test]
        public async Task GetToken_NoExistingToken_GetsNewToken()
        {
            var tvDbConnection = new JsonConnection(new JsonSerialiser(), this.logger);

            var token = new TvDbToken(tvDbConnection, Secrets.TvDbApiKey, this.logger);

            var returnedToken = await token.GetTokenAsync();

            returnedToken.IsSome.Should().BeTrue();
            returnedToken.ValueUnsafe().Should().NotBeNullOrWhiteSpace();
        }
    }
}
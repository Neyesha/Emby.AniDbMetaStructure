using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Emby.AniDbMetaStructure.AniList;
using Emby.AniDbMetaStructure.AniList.Requests;
using Emby.AniDbMetaStructure.JsonApi;
using Emby.AniDbMetaStructure.Process;
using Emby.AniDbMetaStructure.Tests.TestHelpers;
using FluentAssertions;
using LanguageExt;
using NSubstitute;
using NUnit.Framework;
using static LanguageExt.Prelude;

namespace Emby.AniDbMetaStructure.Tests.AniList
{
    [TestFixture]
    public class AniListTokenTests
    {
        [SetUp]
        public void Setup()
        {
            this.resultContext = TestProcessResultContext.Instance;
            this.jsonConnection = Substitute.For<IJsonConnection>();
            this.aniListConfiguration = Substitute.For<IAnilistConfiguration>();

            this.token = new AniListToken();
        }

        private IJsonConnection jsonConnection;
        private IAnilistConfiguration aniListConfiguration;
        private AniListToken token;
        private ProcessResultContext resultContext;

        [Test]
        public async Task GetToken_FailedRequest_ReturnsNone()
        {
            this.jsonConnection.PostAsync<GetTokenRequest.TokenData>(null, null)
                .ReturnsForAnyArgs(new FailedRequest(HttpStatusCode.NotFound, "NotFound"));

            var result = await this.token.GetToken(this.jsonConnection, this.aniListConfiguration, this.resultContext);

            result.IsRight.Should().BeFalse();
        }

        [Test]
        public async Task GetToken_MultipleSimultaneousCalls_MakesOneRequest()
        {
            this.jsonConnection.PostAsync<GetTokenRequest.TokenData>(null, null)
                .ReturnsForAnyArgs(x => Task.Delay(1000)
                    .ContinueWith(t => Right<FailedRequest, Response<GetTokenRequest.TokenData>>(
                        new Response<GetTokenRequest.TokenData>(
                            new GetTokenRequest.TokenData("AccessToken", 3, "RefreshToken")))));

            await Task.WhenAll(
                this.token.GetToken(this.jsonConnection, this.aniListConfiguration, this.resultContext),
                this.token.GetToken(this.jsonConnection, this.aniListConfiguration, this.resultContext),
                this.token.GetToken(this.jsonConnection, this.aniListConfiguration, this.resultContext),
                this.token.GetToken(this.jsonConnection, this.aniListConfiguration, this.resultContext),
                this.token.GetToken(this.jsonConnection, this.aniListConfiguration, this.resultContext),
                this.token.GetToken(this.jsonConnection, this.aniListConfiguration, this.resultContext));

            await this.jsonConnection.Received(1)
                .PostAsync(Arg.Any<GetTokenRequest>(), Arg.Is(Option<string>.None));
        }
        
        [Test]
        public async Task GetToken_PostsAuthorisationCode()
        {
            this.jsonConnection.PostAsync<GetTokenRequest.TokenData>(null, null)
                .ReturnsForAnyArgs(new Response<GetTokenRequest.TokenData>(
                    new GetTokenRequest.TokenData("AccessToken", 3, "RefreshToken")));

            this.aniListConfiguration.AuthorizationCode.Returns("AuthCode");

            await this.token.GetToken(this.jsonConnection, this.aniListConfiguration, this.resultContext);

            await this.jsonConnection.Received(1)
                .PostAsync(Arg.Any<GetTokenRequest>(), Arg.Is(Option<string>.None));

            var receivedCall = this.jsonConnection.ReceivedCalls().Single();

            var receivedRequest = (GetTokenRequest)receivedCall.GetArguments()[0];
            receivedRequest.Url.Should().Be("https://anilist.co/api/v2/oauth/token");
            receivedRequest.Data.Should()
                .BeEquivalentTo(new
                {
                    grant_type = "authorization_code",
                    client_id = "362",
                    client_secret = "NSjmeTEekFlV9OZuZo9iR0BERNe3KS83iaIiI7EQ",
                    redirect_uri = "http://localhost:8096/web/configurationpage?name=AniMetadata",
                    code = "AuthCode"
                });
        }

        [Test]
        public async Task GetToken_SuccessfulRequest_ReturnsAccessToken()
        {
            this.jsonConnection.PostAsync<GetTokenRequest.TokenData>(null, null)
                .ReturnsForAnyArgs(new Response<GetTokenRequest.TokenData>(
                    new GetTokenRequest.TokenData("AccessToken", 3, "RefreshToken")));

            var result = await this.token.GetToken(this.jsonConnection, this.aniListConfiguration, this.resultContext);

            result.IsRight.Should().BeTrue();
            result.IfRight(r => r.Should().Be("AccessToken"));
        }
    }
}
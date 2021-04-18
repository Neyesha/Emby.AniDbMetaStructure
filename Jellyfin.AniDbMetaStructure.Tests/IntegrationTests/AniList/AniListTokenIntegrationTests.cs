using System.Threading.Tasks;
using Emby.AniDbMetaStructure.AniList;
using Emby.AniDbMetaStructure.Infrastructure;
using Emby.AniDbMetaStructure.JsonApi;
using Emby.AniDbMetaStructure.Process;
using Emby.AniDbMetaStructure.Tests.TestHelpers;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace Emby.AniDbMetaStructure.Tests.IntegrationTests.AniList
{
    [TestFixture]
    [Ignore("Anilist support is not working.")]
    public class AniListTokenIntegrationTests
    {
        private IAnilistConfiguration aniListConfiguration;
        private AniListToken token;
        private ProcessResultContext resultContext;
        private JsonConnection jsonConnection;

        [SetUp]
        public void Setup()
        {
            this.resultContext = TestProcessResultContext.Instance;

            this.jsonConnection =
                new JsonConnection(new TestHttpClient(), new JsonSerialiser(), new ConsoleLogManager());
            this.aniListConfiguration = Substitute.For<IAnilistConfiguration>();

            this.token = new AniListToken();
        }

        [Test]
        public async Task GetToken_InvalidConfiguration_ReturnsNone()
        {
            this.aniListConfiguration.AuthorizationCode.Returns("InvalidCode");

            var result = await this.token.GetToken(this.jsonConnection,this.aniListConfiguration, this.resultContext);

            result.IsRight.Should().BeFalse();
        }

        [Test]
        [Explicit("New authorisation code must be generated once it expires")]
        public async Task GetToken_ValidConfiguration_ReturnsAccessToken()
        {
            this.aniListConfiguration.AuthorizationCode.Returns("def502000219c60df846b8239c4edaa0fb46bb202ce5082b3d8b499f2772d6a659d473083b6fa9823ac1273bd1cfff6e0c94773b46517a8b5997ed1e2c0148266e7b97722bea71005709d6e57cec60e5c9de33d6a1fdeab2e83bf6d8ce56c4b594d80e1cfaef7ae6613966532010b28dd8d5601714d54d4bc2267c7d0da2e5c44e7b3212e07686cd6c242f071b15ec9334d289430fc60f4471922f66775066cf802b486dd5976de82da2cadd8ab944c5c9027f37511908d1aee9fc50dd35b149f6d24ffd1d16b7ecf1304d5871b5a747b4eb4cf0b8e19cf8abd5d6b26e8963755ee25c5edcd8ad1a0445cf7fc11e7bc9893bb703140b17c3b3f4694fe82035372b80042652a877859e3479b8709d7a666dc072033e117880d7fcf7d74248acffe558c6f33ba500554e8038c58e3a1452f51a71952868e4e595a7e4c0f39d25c44cc4ca49c83411f0ebabc18fd81d239a5faaa394dd60b249a7a002c85819d4df64f7aa23591383349ed024b9d502df19a6f6a274ed1736933db41568628baf61e58a216aa88a");
            var result = await this.token.GetToken(this.jsonConnection, this.aniListConfiguration,this.resultContext);

            result.IsRight.Should().BeTrue();
            result.IfRight(r => r.Length.Should().BeGreaterThan(0));
        }
    }
}
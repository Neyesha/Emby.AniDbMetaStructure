using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jellyfin.AniDbMetaStructure.AniList;
using Jellyfin.AniDbMetaStructure.AniList.Data;
using Jellyfin.AniDbMetaStructure.Infrastructure;
using Jellyfin.AniDbMetaStructure.JsonApi;
using Jellyfin.AniDbMetaStructure.Process;
using Jellyfin.AniDbMetaStructure.Tests.TestHelpers;
using FluentAssertions;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;

namespace Emby.AniDbMetaStructure.Tests.IntegrationTests.AniList
{
    [TestFixture]
    [Ignore("Anilist support is not working.")]
    public class AniListClientIntegrationTests
    {
        [SetUp]
        public void Setup()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new OptionJsonConverter() }
            };

            this.resultContext = TestProcessResultContext.Instance;

            var jsonConnection =
                new JsonConnection(new TestHttpClient(), new JsonSerialiser(), new ConsoleLogManager());
            var aniListToken = Substitute.For<IAniListToken>();
            var aniListConfiguration = Substitute.For<IAnilistConfiguration>();

            aniListToken.GetToken(jsonConnection, aniListConfiguration, this.resultContext)
                .Returns(
"eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImp0aSI6IjA1YzRjZDMyNWY1MGNhMTllMThjMzU5MmI3YmQ3NzczNjUzYzVkNzdkNGY3NmYzZDcyNDY5ZDVkNmFhMGE3YmFmYjg2MjM3YjcxM2M2ZjAxIn0.eyJhdWQiOiIzNjIiLCJqdGkiOiIwNWM0Y2QzMjVmNTBjYTE5ZTE4YzM1OTJiN2JkNzc3MzY1M2M1ZDc3ZDRmNzZmM2Q3MjQ2OWQ1ZDZhYTBhN2JhZmI4NjIzN2I3MTNjNmYwMSIsImlhdCI6MTUyMzExMDA5NywibmJmIjoxNTIzMTEwMDk3LCJleHAiOjE1NTQ2NDYwOTcsInN1YiI6IjExMjA4NCIsInNjb3BlcyI6W119.eXUQ1VrEQdinxvuphdPxTmNgISnBf2sYUOdi3bhsR6Rp0_Tohh3PzKXEDZKt6Deu3NZieZ_ET5sMb1iYAeTX5K_XHhYOQwcZzGSwstBT84HkyPl6FL6ONrCxO94z4arfnpriNM3eVPhGQee9CT5jEpMxYAtTgN8-9MsDD5pyc_AvRT_AuC2ugqw81dgPCgNDjSAiOSBNG1XWpXI2jV1jF5TKaOVlfedJqCL-scL7j4XBiq3v-2WdPaV5oqw2kvEfH5A5pReIU_m-SAFduAgvPNPdgGSh7izx14WSzdWpuiYLc_ly8VhxptwWnlHifLrAeu0t2UjmCy5Ssh1op2Bmo2qXJPlx9xcdTyW2yqTxxH-V_VbsPH2Omvmda_PFsi6sLKhCEF1qGhAJ0aSGIpbTl8V6tJ4-JxbhU2GjyR13LOHTOIU7sM_OO9ketgKGZ6L2wI4LQGbm6BIop96QweRjT19hCwkwHS-Tq1d0HRtCJ_tPHuupZKARDrMQgkVHTJ1lPsIKyf92KnUJ1azn6AxTSwvxQSVnaiqM_1CMsC0ht0bs9RgnqjAKRv734qt7yibItu7UzzV3JIOYX2duG8KW_VLyM78V4DqIEhui7K9MARavlqEs2umOkLHb1aaLz9zVvgrNQrzwUXeXPQm_myWgieWEP5RIQH7Gv_YC-W1pB0o");

            this.client = new AniListClient(jsonConnection, aniListToken, aniListConfiguration);
        }

        private AniListClient client;
        private ProcessResultContext resultContext;

        [Test]
        public async Task FindSeriesAsync_ReturnsSeriesData()
        {
            var result = await this.client.FindSeriesAsync("Fumoffu", this.resultContext);

            result.IsRight.Should().BeTrue();
            result.IfRight(r => r.Single().Should()
                .BeEquivalentTo(
                    new AniListSeriesData
                    {
                        Id = 72,
                        SeriesType = AniListSeriesType.Anime,
                        Title = new AniListTitleData("Full Metal Panic? Fumoffu", "Full Metal Panic? Fumoffu",
                            "\u30d5\u30eb\u30e1\u30bf\u30eb\u30fb\u30d1\u30cb\u30c3\u30af? \u3075\u3082\u3063\u3075"),
                        StartDate = new AniListFuzzyDate(2003, 8, 26),
                        EndDate = new AniListFuzzyDate(2003, 10, 18),
                        Description =
                            "It's back-to-school mayhem with Kaname Chidori and her war-freak classmate Sousuke Sagara as they encounter more misadventures in and out of Jindai High School. But when Kaname gets into some serious trouble, Sousuke takes the guise of Bonta-kun - the gun-wielding, butt-kicking mascot. And while he struggles to continue living as a normal teenager, Sousuke also has to deal with protecting his superior officer Teletha Testarossa, who has decided to take a vacation from Mithril and spend a couple of weeks as his and Kaname's classmate.<br><br>\n(Source: ANN)",
                        MyAnimeListId = 72,
                        Genres = new[] { "Action", "Comedy" },
                        AverageScore = 78,
                        AniListUrl = @"https://anilist.co/anime/72",
                        CoverImage = new AniListImageUrlData(@"https://cdn.anilist.co/img/dir/anime/reg/72.jpg",
                            "https://cdn.anilist.co/img/dir/anime/med/72.jpg"),
                        BannerImage = null,
                        EpisodeDurationMinutes = 24,
                        AiringStatus = AniListAiringStatus.Finished,
                        Studios = new GraphQlEdge<AniListStudioData>(new[]
                        {
                            new AniListStudioData(false, new AniListStudioData.StudioName("Kyoto Animation")),
                            new AniListStudioData(false, new AniListStudioData.StudioName("ADV Films")),
                            new AniListStudioData(false, new AniListStudioData.StudioName("FUNimation Entertainment"))
                        }),
                        Staff = new GraphQlEdge<AniListStaffData>(new[]
                        {
                            new AniListStaffData(
                                new AniListStaffData.InnerStaffData(
                                    new AniListPersonNameData("Mikuni", "Shimokawa", "\u307f\u304f\u306b \u4e0b\u5ddd"),
                                    new AniListImageUrlData(@"https://cdn.anilist.co/img/dir/person/reg/1565.jpg",
                                        @"https://cdn.anilist.co/img/dir/person/med/1565.jpg")),
                                "Theme Song Performance  (Sore Ga, Ai Deshou, Opening Theme)"),
                            new AniListStaffData(
                                new AniListStaffData.InnerStaffData(
                                    new AniListPersonNameData("Shoji", "Gatoh", "\u8cc0\u6771 \u62db\u4e8c "),
                                    new AniListImageUrlData(
                                        @"https://cdn.anilist.co/img/dir/person/reg/2622.jpg",
                                        @"https://cdn.anilist.co/img/dir/person/med/2622.jpg")),
                                "Original Creator"),
                            new AniListStaffData(
                                new AniListStaffData.InnerStaffData(
                                    new AniListPersonNameData("Tatsuya", "Ishihara", "\u77f3\u539f\u7acb\u4e5f"),
                                    new AniListImageUrlData(
                                        @"https://cdn.anilist.co/img/dir/person/reg/5055.jpg",
                                        @"https://cdn.anilist.co/img/dir/person/med/5055.jpg")),
                                "Storyboard"),
                            new AniListStaffData(
                                new AniListStaffData.InnerStaffData(
                                    new AniListPersonNameData("Toshihiko", "Sahashi",
                                        "俊彦 佐橋"),
                                    new AniListImageUrlData(
                                        @"https://cdn.anilist.co/img/dir/person/reg/5397.jpg",
                                        @"https://cdn.anilist.co/img/dir/person/med/5397.jpg")),
                                "Music"),
                            new AniListStaffData(
                                new AniListStaffData.InnerStaffData(
                                    new AniListPersonNameData("Douji", "Shiki", "\u7ae5\u5b50 \u56db\u5b63"),
                                    new AniListImageUrlData(
                                        @"https://cdn.anilist.co/img/dir/person/reg/6520.jpg",
                                        @"https://cdn.anilist.co/img/dir/person/med/6520.jpg")),
                                "Original Character Design"),
                            new AniListStaffData(
                                new AniListStaffData.InnerStaffData(
                                    new AniListPersonNameData("Yutaka", "Yamamoto", "\u5c71\u672c\u5bdb"),
                                    new AniListImageUrlData(
                                        @"https://cdn.anilist.co/img/dir/person/reg/6655.jpg",
                                        @"https://cdn.anilist.co/img/dir/person/med/6655.jpg")),
                                "Storyboard  (2)"),
                            new AniListStaffData(
                                new AniListStaffData.InnerStaffData(
                                    new AniListPersonNameData("Yutaka", "Yamamoto", "\u5c71\u672c\u5bdb"),
                                    new AniListImageUrlData(
                                        @"https://cdn.anilist.co/img/dir/person/reg/6655.jpg",
                                        @"https://cdn.anilist.co/img/dir/person/med/6655.jpg")),
                                "Episode Director  (2, 9)"),
                            new AniListStaffData(
                                new AniListStaffData.InnerStaffData(
                                    new AniListPersonNameData("Yasuhiro", "Takemoto", "\u5eb7\u5f18 \u6b66\u672c"),
                                    new AniListImageUrlData(
                                        @"https://cdn.anilist.co/img/dir/person/reg/6771.jpg",
                                        @"https://cdn.anilist.co/img/dir/person/med/6771.jpg")),
                                "Script  (5)"),
                            new AniListStaffData(
                                new AniListStaffData.InnerStaffData(
                                    new AniListPersonNameData("Yasuhiro", "Takemoto", "\u5eb7\u5f18 \u6b66\u672c"),
                                    new AniListImageUrlData(
                                        @"https://cdn.anilist.co/img/dir/person/reg/6771.jpg",
                                        @"https://cdn.anilist.co/img/dir/person/med/6771.jpg")),
                                "Director"),
                            new AniListStaffData(
                                new AniListStaffData.InnerStaffData(
                                    new AniListPersonNameData("Yasuhiro", "Takemoto", "\u5eb7\u5f18 \u6b66\u672c"),
                                    new AniListImageUrlData(
                                        @"https://cdn.anilist.co/img/dir/person/reg/6771.jpg",
                                        @"https://cdn.anilist.co/img/dir/person/med/6771.jpg")),
                                "Episode Director  (12, OP, ED)"),
                            new AniListStaffData(
                                new AniListStaffData.InnerStaffData(
                                    new AniListPersonNameData("Yasuhiro", "Takemoto", "\u5eb7\u5f18 \u6b66\u672c"),
                                    new AniListImageUrlData(
                                        @"https://cdn.anilist.co/img/dir/person/reg/6771.jpg",
                                        @"https://cdn.anilist.co/img/dir/person/med/6771.jpg")),
                                "Storyboard  (1, 9, 12, OP, ED)"),
                            new AniListStaffData(
                                new AniListStaffData.InnerStaffData(
                                    new AniListPersonNameData("Akira", "Takata", "\u6643 \u9ad8\u7530"),
                                    new AniListImageUrlData(
                                        @"https://cdn.anilist.co/img/dir/person/reg/101885-JXiMlg2bXLOB.jpg",
                                        @"https://cdn.anilist.co/img/dir/person/med/101885-JXiMlg2bXLOB.jpg")),
                                "Animation Director  (ep 9, 10)"),
                            new AniListStaffData(
                                new AniListStaffData.InnerStaffData(
                                    new AniListPersonNameData("Yoshiji", "Kigami", "\u6728\u4e0a\u76ca\u6cbb"),
                                    new AniListImageUrlData(
                                        @"https://cdn.anilist.co/img/dir/person/reg/102025-RZBRhBscIWih.jpg",
                                        @"https://cdn.anilist.co/img/dir/person/med/102025-RZBRhBscIWih.jpg")),
                                "Storyboard  (eps 3, 7)"),
                            new AniListStaffData(
                                new AniListStaffData.InnerStaffData(
                                    new AniListPersonNameData("Yoshiji", "Kigami", "\u6728\u4e0a\u76ca\u6cbb"),
                                    new AniListImageUrlData(
                                        @"https://cdn.anilist.co/img/dir/person/reg/102025-RZBRhBscIWih.jpg",
                                        @"https://cdn.anilist.co/img/dir/person/med/102025-RZBRhBscIWih.jpg")),
                                "Episode Director  (eps 3, 7)"),
                            new AniListStaffData(
                                new AniListStaffData.InnerStaffData(
                                    new AniListPersonNameData("Youta", "Tsuruoka", "\u9db4\u5ca1\u967d\u592a"),
                                    new AniListImageUrlData(
                                        @"https://cdn.anilist.co/img/dir/person/reg/7194.jpg",
                                        @"https://cdn.anilist.co/img/dir/person/med/7194.jpg")),
                                "Sound Director"),
                            new AniListStaffData(
                                new AniListStaffData.InnerStaffData(
                                    new AniListPersonNameData("Kazumi", "Ikeda", "\u548c\u7f8e \u6c60\u7530"),
                                    new AniListImageUrlData(
                                        @"https://cdn.anilist.co/img/dir/person/reg/102724-J9l7AWBCqIMK.jpg",
                                        @"https://cdn.anilist.co/img/dir/person/med/102724-J9l7AWBCqIMK.jpg")),
                                "Animation Director  (ep 2)"),
                            new AniListStaffData(
                                new AniListStaffData.InnerStaffData(
                                    new AniListPersonNameData("Kazumi", "Ikeda", "\u548c\u7f8e \u6c60\u7530"),
                                    new AniListImageUrlData(
                                        @"https://cdn.anilist.co/img/dir/person/reg/102724-J9l7AWBCqIMK.jpg",
                                        @"https://cdn.anilist.co/img/dir/person/med/102724-J9l7AWBCqIMK.jpg")),
                                "Key Animation  (OP)"),
                            new AniListStaffData(
                                new AniListStaffData.InnerStaffData(
                                    new AniListPersonNameData("Shoko", "Ikeda", "\u6676\u5b50 \u6c60\u7530"),
                                    new AniListImageUrlData(
                                        @"https://cdn.anilist.co/img/dir/person/reg/7736.jpg",
                                        @"https://cdn.anilist.co/img/dir/person/med/7736.jpg")),
                                "Key Animation  (OP)"),
                            new AniListStaffData(
                                new AniListStaffData.InnerStaffData(
                                    new AniListPersonNameData("Shoko", "Ikeda", "\u6676\u5b50 \u6c60\u7530"),
                                    new AniListImageUrlData(
                                        @"https://cdn.anilist.co/img/dir/person/reg/7736.jpg",
                                        @"https://cdn.anilist.co/img/dir/person/med/7736.jpg")),
                                "Animation Director  (ep 8)"),
                            new AniListStaffData(
                                new AniListStaffData.InnerStaffData(
                                    new AniListPersonNameData("Fumihiko", "Shimo", "\u6587\u5f66 \u5fd7\u8302"),
                                    new AniListImageUrlData(
                                        @"https://cdn.anilist.co/img/dir/person/reg/103067-5PoLOlb221Rm.jpg",
                                        @"https://cdn.anilist.co/img/dir/person/med/103067-5PoLOlb221Rm.jpg")),
                                "Screenplay  (6 eps)"),
                            new AniListStaffData(
                                new AniListStaffData.InnerStaffData(
                                    new AniListPersonNameData("Satoshi", "Kadowaki", "\u8061 \u9580\u8107"),
                                    new AniListImageUrlData(
                                        @"https://cdn.anilist.co/img/dir/person/reg/default.jpg",
                                        @"https://cdn.anilist.co/img/dir/person/med/default.jpg")),
                                "Assistant Animation Director  (eps 3, 7)"),
                            new AniListStaffData(
                                new AniListStaffData.InnerStaffData(
                                    new AniListPersonNameData("Yoko", "Hatta", "\u967d\u5b50 \u516b\u7530"),
                                    new AniListImageUrlData(
                                        @"https://cdn.anilist.co/img/dir/person/reg/default.jpg",
                                        @"https://cdn.anilist.co/img/dir/person/med/default.jpg")),
                                "Producer"),
                            new AniListStaffData(
                                new AniListStaffData.InnerStaffData(
                                    new AniListPersonNameData("Masaya", "Makita", "\u660c\u4e5f \u7267\u7530"),
                                    new AniListImageUrlData(
                                        @"https://cdn.anilist.co/img/dir/person/reg/default.jpg",
                                        @"https://cdn.anilist.co/img/dir/person/med/default.jpg")),
                                "Key Animation  (eps 2, 9)"),
                            new AniListStaffData(
                                new AniListStaffData.InnerStaffData(
                                    new AniListPersonNameData("Ryouhei", "Muta", "\u4eae\u5e73 \u725f\u7530"),
                                    new AniListImageUrlData(
                                        @"https://cdn.anilist.co/img/dir/person/reg/default.jpg",
                                        @"https://cdn.anilist.co/img/dir/person/med/default.jpg")),
                                "Key Animation  (eps 2, 9)"),
                            new AniListStaffData(
                                new AniListStaffData.InnerStaffData(
                                    new AniListPersonNameData("Taichi", "Ishidate", "\u77f3\u7acb \u592a\u4e00"),
                                    new AniListImageUrlData(
                                        @"https://cdn.anilist.co/img/dir/person/reg/8129.jpg",
                                        @"https://cdn.anilist.co/img/dir/person/med/8129.jpg")),
                                "Key Animation  (ep 8)")
                        }),
                        Characters = new GraphQlEdge<AniListCharacterData>(new List<AniListCharacterData>(
                            new[]
                            {
                                new AniListCharacterData(
                                    new AniListCharacterData.InnerCharacterData(
                                        new AniListPersonNameData("Sousuke", "Sagara", "相良宗介"),
                                        new AniListImageUrlData(
                                            @"https://cdn.anilist.co/img/dir/character/reg/168.jpg",
                                            @"https://cdn.anilist.co/img/dir/character/med/168.jpg")),
                                    new[]
                                    {
                                        new AniListCharacterData.VoiceActorData("JAPANESE",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/1.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/1.jpg"),
                                            new AniListPersonNameData("Tomokazu", "Seki", "関智一")),
                                        new AniListCharacterData.VoiceActorData("ENGLISH",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/190.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/190.jpg"),
                                            new AniListPersonNameData("Chris", "Patton", " ")),
                                        new AniListCharacterData.VoiceActorData("ITALIAN",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/774.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/774.jpg"),
                                            new AniListPersonNameData("Simone", "D'Andrea", " ")),
                                        new AniListCharacterData.VoiceActorData("PORTUGUESE",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/1830.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/1830.jpg"),
                                            new AniListPersonNameData("Wendel", "Bezerra", " ")),
                                        new AniListCharacterData.VoiceActorData("HUNGARIAN",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/7800.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/7800.jpg"),
                                            new AniListPersonNameData("Tamás", "Markovics", " ")),
                                        new AniListCharacterData.VoiceActorData("GERMAN",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/12230.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/12230.jpg"),
                                            new AniListPersonNameData("Marius", "Claren", " ")),
                                        new AniListCharacterData.VoiceActorData("KOREAN",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/14757.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/14757.jpg"),
                                            new AniListPersonNameData("Won Hyeong", "Choi", "원형 최"))
                                    }),
                                new AniListCharacterData(
                                    new AniListCharacterData.InnerCharacterData(
                                        new AniListPersonNameData("Kaname", "Chidori", "千鳥かなめ"),
                                        new AniListImageUrlData(
                                            @"https://cdn.anilist.co/img/dir/character/reg/272.jpg",
                                            @"https://cdn.anilist.co/img/dir/character/med/272.jpg")),
                                    new[]
                                    {
                                        new AniListCharacterData.VoiceActorData("JAPANESE",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/3.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/3.jpg"),
                                            new AniListPersonNameData("Satsuki", "Yukino", "五月 雪野")),
                                        new AniListCharacterData.VoiceActorData("ENGLISH",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/189.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/189.jpg"),
                                            new AniListPersonNameData("Luci", "Christian", " ")),
                                        new AniListCharacterData.VoiceActorData("ITALIAN",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/768.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/768.jpg"),
                                            new AniListPersonNameData("Perla", "Liberatori", " ")),
                                        new AniListCharacterData.VoiceActorData("PORTUGUESE",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/1442.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/1442.jpg"),
                                            new AniListPersonNameData("Tatiane", "Keplermaier", " ")),
                                        new AniListCharacterData.VoiceActorData("HUNGARIAN",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/7805.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/7805.jpg"),
                                            new AniListPersonNameData("Ilona", "Molnár", " ")),
                                        new AniListCharacterData.VoiceActorData("GERMAN",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/10699.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/10699.jpg"),
                                            new AniListPersonNameData("Melanie", "Hinze", " ")),
                                        new AniListCharacterData.VoiceActorData("SPANISH",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/22333.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/22333.jpg"),
                                            new AniListPersonNameData("Mireya", "Mendoza", " "))
                                    }),
                                new AniListCharacterData(
                                    new AniListCharacterData.InnerCharacterData(
                                        new AniListPersonNameData("Teletha", "Testarossa", "テレサ・テスタロッサ"),
                                        new AniListImageUrlData(
                                            @"https://cdn.anilist.co/img/dir/character/reg/273.jpg",
                                            @"https://cdn.anilist.co/img/dir/character/med/273.jpg")),
                                    new[]
                                    {
                                        new AniListCharacterData.VoiceActorData("JAPANESE",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/140.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/140.jpg"),
                                            new AniListPersonNameData(string.Empty, "Yukana", " ゆかな")),
                                        new AniListCharacterData.VoiceActorData("ENGLISH",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/151.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/151.jpg"),
                                            new AniListPersonNameData("Hilary", "Haag", " ")),
                                        new AniListCharacterData.VoiceActorData("ITALIAN",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/777.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/777.jpg"),
                                            new AniListPersonNameData("Letizia", "Ciampa", " ")),
                                        new AniListCharacterData.VoiceActorData("GERMAN",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/6101.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/6101.jpg"),
                                            new AniListPersonNameData("Magdalena", "Turba", " ")),
                                        new AniListCharacterData.VoiceActorData("HUNGARIAN",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/7816.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/7816.jpg"),
                                            new AniListPersonNameData("Andrea", "Roatis", " ")),
                                        new AniListCharacterData.VoiceActorData("PORTUGUESE",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/13325.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/13325.jpg"),
                                            new AniListPersonNameData("Rita", "Almeida", " ")),
                                        new AniListCharacterData.VoiceActorData("KOREAN",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/14993.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/14993.jpg"),
                                            new AniListPersonNameData("Ji Yeong", "Lee", "지영 이"))
                                    }),
                                new AniListCharacterData(
                                    new AniListCharacterData.InnerCharacterData(
                                        new AniListPersonNameData("Melissa", "Mao", "メリッサ・マオ"),
                                        new AniListImageUrlData(
                                            @"https://cdn.anilist.co/img/dir/character/reg/781.jpg",
                                            @"https://cdn.anilist.co/img/dir/character/med/781.jpg")),
                                    new[]
                                    {
                                        new AniListCharacterData.VoiceActorData("JAPANESE",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/176.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/176.jpg"),
                                            new AniListPersonNameData("Michiko", "Neya", "美智子 根谷")),
                                        new AniListCharacterData.VoiceActorData("ENGLISH",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/194.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/194.jpg"),
                                            new AniListPersonNameData("Allison", "Keith", " ")),
                                        new AniListCharacterData.VoiceActorData("ITALIAN",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/763.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/763.jpg"),
                                            new AniListPersonNameData("Barbara", "De Bortoli", " ")),
                                        new AniListCharacterData.VoiceActorData("HUNGARIAN",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/7810.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/7810.jpg"),
                                            new AniListPersonNameData("Eszter", "Nyírő", " ")),
                                        new AniListCharacterData.VoiceActorData("PORTUGUESE",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/11518.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/11518.jpg"),
                                            new AniListPersonNameData("Fátima", "Noya", " ")),
                                        new AniListCharacterData.VoiceActorData("KOREAN",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/15135.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/15135.jpg"),
                                            new AniListPersonNameData("Do Yeong", "Song", "도영 송"))
                                    }),
                                new AniListCharacterData(
                                    new AniListCharacterData.InnerCharacterData(
                                        new AniListPersonNameData("Kurz", "Weber", null),
                                        new AniListImageUrlData(
                                            @"https://cdn.anilist.co/img/dir/character/reg/2168.jpg",
                                            @"https://cdn.anilist.co/img/dir/character/med/2168.jpg")),
                                    new[]
                                    {
                                        new AniListCharacterData.VoiceActorData("JAPANESE",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/22.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/22.jpg"),
                                            new AniListPersonNameData("Shinichiro", "Miki", "眞一郎三木")),
                                        new AniListCharacterData.VoiceActorData("ENGLISH",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/43.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/43.jpg"),
                                            new AniListPersonNameData("Vic", "Mignogna", " ")),
                                        new AniListCharacterData.VoiceActorData("ITALIAN",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/764.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/764.jpg"),
                                            new AniListPersonNameData("Massimiliano", "Alto", " ")),
                                        new AniListCharacterData.VoiceActorData("GERMAN",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/6112.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/6112.jpg"),
                                            new AniListPersonNameData("Julien", "Haggége", " ")),
                                        new AniListCharacterData.VoiceActorData("PORTUGUESE",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/7350.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/7350.jpg"),
                                            new AniListPersonNameData("Alfredo", "Rollo", " ")),
                                        new AniListCharacterData.VoiceActorData("HUNGARIAN",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/8384.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/8384.jpg"),
                                            new AniListPersonNameData("Gábor", "Varga", " ")),
                                        new AniListCharacterData.VoiceActorData("KOREAN",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/14829.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/14829.jpg"),
                                            new AniListPersonNameData("Il", "Kim", "일 김"))
                                    }),
                                new AniListCharacterData(
                                    new AniListCharacterData.InnerCharacterData(
                                        new AniListPersonNameData("Kyoko", "Tokiwa", "常盤恭子"),
                                        new AniListImageUrlData(
                                            @"https://cdn.anilist.co/img/dir/character/reg/2745.jpg",
                                            @"https://cdn.anilist.co/img/dir/character/med/2745.jpg")),
                                    new[]
                                    {
                                        new AniListCharacterData.VoiceActorData("ENGLISH",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/159.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/159.jpg"),
                                            new AniListPersonNameData("Monica", "Rial", " ")),
                                        new AniListCharacterData.VoiceActorData("ITALIAN",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/767.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/767.jpg"),
                                            new AniListPersonNameData("Federica", "De Bortoli", " ")),
                                        new AniListCharacterData.VoiceActorData("JAPANESE",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/1688.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/1688.jpg"),
                                            new AniListPersonNameData("Ikue", "Kimura", "郁絵 木村")),
                                        new AniListCharacterData.VoiceActorData("PORTUGUESE",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/5178.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/5178.jpg"),
                                            new AniListPersonNameData("Samira", "Fernandes", " "))
                                    }),
                                new AniListCharacterData(
                                    new AniListCharacterData.InnerCharacterData(
                                        new AniListPersonNameData("Shinji", "Kazama", null),
                                        new AniListImageUrlData(
                                            @"https://cdn.anilist.co/img/dir/character/reg/2746.jpg",
                                            @"https://cdn.anilist.co/img/dir/character/med/2746.jpg")),
                                    new[]
                                    {
                                        new AniListCharacterData.VoiceActorData("JAPANESE",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/40.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/40.jpg"),
                                            new AniListPersonNameData("Mamiko", "Noto", "能登麻美子")),
                                        new AniListCharacterData.VoiceActorData("ENGLISH",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/192.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/192.jpg"),
                                            new AniListPersonNameData("Greg", "Ayres", " ")),
                                        new AniListCharacterData.VoiceActorData("PORTUGUESE",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/1108.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/1108.jpg"),
                                            new AniListPersonNameData("Fábio", "Lucindo", " ")),
                                        new AniListCharacterData.VoiceActorData("ITALIAN",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/1110.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/1110.jpg"),
                                            new AniListPersonNameData("Alessio", "De Filippis", " ")),
                                        new AniListCharacterData.VoiceActorData("HUNGARIAN",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/7822.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/7822.jpg"),
                                            new AniListPersonNameData("Kristóf", "Steiner", " ")),
                                        new AniListCharacterData.VoiceActorData("KOREAN",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/14933.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/14933.jpg"),
                                            new AniListPersonNameData("Myeong Jun", "Jeong", "명준 정"))
                                    }),
                                new AniListCharacterData(
                                    new AniListCharacterData.InnerCharacterData(
                                        new AniListPersonNameData("Andrei Sergeivich", "Kalinin", null),
                                        new AniListImageUrlData(
                                            @"https://cdn.anilist.co/img/dir/character/reg/2747.jpg",
                                            @"https://cdn.anilist.co/img/dir/character/med/2747.jpg")),
                                    new[]
                                    {
                                        new AniListCharacterData.VoiceActorData("JAPANESE",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/146.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/146.jpg"),
                                            new AniListPersonNameData("Akio", "Ohtsuka", "明夫 大塚")),
                                        new AniListCharacterData.VoiceActorData("ENGLISH",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/1604.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/1604.jpg"),
                                            new AniListPersonNameData("Mike", "Kleinhenz", " ")),
                                        new AniListCharacterData.VoiceActorData("KOREAN",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/14779.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/14779.jpg"),
                                            new AniListPersonNameData("Gi hyeon", "Kim", "기현 김"))
                                    }),
                                new AniListCharacterData(
                                    new AniListCharacterData.InnerCharacterData(
                                        new AniListPersonNameData("Bonta-kun", null, null),
                                        new AniListImageUrlData(
                                            @"https://cdn.anilist.co/img/dir/character/reg/2921.jpg",
                                            @"https://cdn.anilist.co/img/dir/character/med/2921.jpg")),
                                    new[]
                                    {
                                        new AniListCharacterData.VoiceActorData("ENGLISH",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/145.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/145.jpg"),
                                            new AniListPersonNameData("Tiffany", "Grant", " ")),
                                        new AniListCharacterData.VoiceActorData("JAPANESE",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/272.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/272.jpg"),
                                            new AniListPersonNameData("Tomoko", "Kaneda", "朋子 金田")),
                                        new AniListCharacterData.VoiceActorData("ITALIAN",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/1415.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/1415.jpg"),
                                            new AniListPersonNameData("Eva", "Padoan", " ")),
                                        new AniListCharacterData.VoiceActorData("PORTUGUESE",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/1830.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/1830.jpg"),
                                            new AniListPersonNameData("Wendel", "Bezerra", " ")),
                                        new AniListCharacterData.VoiceActorData("GERMAN",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/6923.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/6923.jpg"),
                                            new AniListPersonNameData("Julius", "Jellinek", " ")),
                                        new AniListCharacterData.VoiceActorData("HUNGARIAN",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/7800.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/7800.jpg"),
                                            new AniListPersonNameData("Tamás", "Markovics", " "))
                                    }),
                                new AniListCharacterData(
                                    new AniListCharacterData.InnerCharacterData(
                                        new AniListPersonNameData("Mr.", "Mizuhoshi", "水星庵"),
                                        new AniListImageUrlData(
                                            @"https://cdn.anilist.co/img/dir/character/reg/5102.jpg",
                                            @"https://cdn.anilist.co/img/dir/character/med/5102.jpg")),
                                    new[]
                                    {
                                        new AniListCharacterData.VoiceActorData("ENGLISH",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/39.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/39.jpg"),
                                            new AniListPersonNameData("Jason", "Douglas", " ")),
                                        new AniListCharacterData.VoiceActorData("JAPANESE",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/1469.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/1469.jpg"),
                                            new AniListPersonNameData("Mitsuru", "Miyamoto", "充 宮本")),
                                        new AniListCharacterData.VoiceActorData("PORTUGUESE",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/7974.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/7974.jpg"),
                                            new AniListPersonNameData("Wellington", "Lima", " "))
                                    }),
                                new AniListCharacterData(
                                    new AniListCharacterData.InnerCharacterData(
                                        new AniListPersonNameData("Atsunobu", "Hayashimizu", "林水 敦信"),
                                        new AniListImageUrlData(
                                            @"https://cdn.anilist.co/img/dir/character/reg/5766.jpg",
                                            @"https://cdn.anilist.co/img/dir/character/med/5766.jpg")),
                                    new[]
                                    {
                                        new AniListCharacterData.VoiceActorData("JAPANESE",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/6.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/6.jpg"),
                                            new AniListPersonNameData("Toshiyuki", "Morikawa", "森川智之")),
                                        new AniListCharacterData.VoiceActorData("ENGLISH",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/191.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/191.jpg"),
                                            new AniListPersonNameData("Chris", "Ayres", " ")),
                                        new AniListCharacterData.VoiceActorData("KOREAN",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/14819.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/14819.jpg"),
                                            new AniListPersonNameData("Seung jun", "Kim", "승준 김"))
                                    }),
                                new AniListCharacterData(
                                    new AniListCharacterData.InnerCharacterData(
                                        new AniListPersonNameData("Ren", "Mikihara", "美樹原 蓮"),
                                        new AniListImageUrlData(
                                            @"https://cdn.anilist.co/img/dir/character/reg/5924.jpg",
                                            @"https://cdn.anilist.co/img/dir/character/med/5924.jpg")),
                                    new[]
                                    {
                                        new AniListCharacterData.VoiceActorData("JAPANESE",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/60.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/60.jpg"),
                                            new AniListPersonNameData("Rie", "Tanaka", "理恵 田中")),
                                        new AniListCharacterData.VoiceActorData("ENGLISH",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/928.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/928.jpg"),
                                            new AniListPersonNameData("Nancy", "Novotny", " ")),
                                        new AniListCharacterData.VoiceActorData("PORTUGUESE",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/11454.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/11454.jpg"),
                                            new AniListPersonNameData("Márcia", "Regina", " ")),
                                        new AniListCharacterData.VoiceActorData("KOREAN",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/15291.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/15291.jpg"),
                                            new AniListPersonNameData("Chea Eun", "Han", "채언 한"))
                                    }),
                                new AniListCharacterData(
                                    new AniListCharacterData.InnerCharacterData(
                                        new AniListPersonNameData("Richard", "Mardukas", null),
                                        new AniListImageUrlData(
                                            @"https://cdn.anilist.co/img/dir/character/reg/6255.jpg",
                                            @"https://cdn.anilist.co/img/dir/character/med/6255.jpg")),
                                    new[]
                                    {
                                        new AniListCharacterData.VoiceActorData("ENGLISH",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/153.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/153.jpg"),
                                            new AniListPersonNameData("Andy", "McAvin", " ")),
                                        new AniListCharacterData.VoiceActorData("JAPANESE",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/6175.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/6175.jpg"),
                                            new AniListPersonNameData("Tomomichi", "Nishimura", "知道 西村")),
                                        new AniListCharacterData.VoiceActorData("KOREAN",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/14861.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/14861.jpg"),
                                            new AniListPersonNameData("Won Je", "Tak", "원제 탁")),
                                        new AniListCharacterData.VoiceActorData("PORTUGUESE",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/15323.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/15323.jpg"),
                                            new AniListPersonNameData("Gileno", "Santoro", " "))
                                    }),
                                new AniListCharacterData(
                                    new AniListCharacterData.InnerCharacterData(
                                        new AniListPersonNameData("Mizuki", "Inaba", null),
                                        new AniListImageUrlData(
                                            @"https://cdn.anilist.co/img/dir/character/reg/8493.jpg",
                                            @"https://cdn.anilist.co/img/dir/character/med/8493.jpg")),
                                    new[]
                                    {
                                        new AniListCharacterData.VoiceActorData("ENGLISH",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/199.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/199.jpg"),
                                            new AniListPersonNameData("Jessica", "Boone", " ")),
                                        new AniListCharacterData.VoiceActorData("JAPANESE",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/5246.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/5246.jpg"),
                                            new AniListPersonNameData("Sayuri", "Yoshida", "小百合 吉田")),
                                        new AniListCharacterData.VoiceActorData("PORTUGUESE",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/8053.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/8053.jpg"),
                                            new AniListPersonNameData("Melissa", "Garcia", " ")),
                                        new AniListCharacterData.VoiceActorData("HUNGARIAN",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/8401.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/8401.jpg"),
                                            new AniListPersonNameData("Zsófia", "Mánya", " "))
                                    }),
                                new AniListCharacterData(
                                    new AniListCharacterData.InnerCharacterData(
                                        new AniListPersonNameData("Eri", "Kagurazaka", "神楽坂恵理"),
                                        new AniListImageUrlData(
                                            @"https://cdn.anilist.co/img/dir/character/reg/10429.jpg",
                                            @"https://cdn.anilist.co/img/dir/character/med/10429.jpg")),
                                    new[]
                                    {
                                        new AniListCharacterData.VoiceActorData("JAPANESE",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/728.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/728.jpg"),
                                            new AniListPersonNameData("Rio", "Natsuki", "リオ 夏樹")),
                                        new AniListCharacterData.VoiceActorData("ENGLISH",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/1606.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/1606.jpg"),
                                            new AniListPersonNameData("Allison", "Sumrall", " ")),
                                        new AniListCharacterData.VoiceActorData("PORTUGUESE",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/11637.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/11637.jpg"),
                                            new AniListPersonNameData("Angelica", "Santos", " "))
                                    }),
                                new AniListCharacterData(
                                    new AniListCharacterData.InnerCharacterData(
                                        new AniListPersonNameData("Issei", "Tsubaki", "椿 一成"),
                                        new AniListImageUrlData(
                                            @"https://cdn.anilist.co/img/dir/character/reg/10431.jpg",
                                            @"https://cdn.anilist.co/img/dir/character/med/10431.jpg")),
                                    new[]
                                    {
                                        new AniListCharacterData.VoiceActorData("JAPANESE",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/86.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/86.jpg"),
                                            new AniListPersonNameData("Jun", "Fukuyama", "福山潤")),
                                        new AniListCharacterData.VoiceActorData("ENGLISH",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/default.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/default.jpg"),
                                            new AniListPersonNameData("Nomed", "Kaerf", " ")),
                                        new AniListCharacterData.VoiceActorData("PORTUGUESE",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/5069.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/5069.jpg"),
                                            new AniListPersonNameData("Hermes", "Baroli", " ")),
                                        new AniListCharacterData.VoiceActorData("HUNGARIAN",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/7780.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/7780.jpg"),
                                            new AniListPersonNameData("Botond", "Előd", " ")),
                                        new AniListCharacterData.VoiceActorData("KOREAN",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/14933.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/14933.jpg"),
                                            new AniListPersonNameData("Myeong Jun", "Jeong", "명준 정")),
                                        new AniListCharacterData.VoiceActorData("GERMAN",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/22365.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/22365.jpg"),
                                            new AniListPersonNameData("Jesco", "Wirthgen", " "))
                                    }),
                                new AniListCharacterData(
                                    new AniListCharacterData.InnerCharacterData(
                                        new AniListPersonNameData("Masatami", "Hyuga", "日向柾民"),
                                        new AniListImageUrlData(
                                            @"https://cdn.anilist.co/img/dir/character/reg/20092.jpg",
                                            @"https://cdn.anilist.co/img/dir/character/med/20092.jpg")),
                                    new[]
                                    {
                                        new AniListCharacterData.VoiceActorData("JAPANESE",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/277.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/277.jpg"),
                                            new AniListPersonNameData("Junko", "Noda", "順子 野田")),
                                        new AniListCharacterData.VoiceActorData("ENGLISH",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/1536.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/1536.jpg"),
                                            new AniListPersonNameData("Kevin", "Corn", " ")),
                                        new AniListCharacterData.VoiceActorData("HUNGARIAN",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/7827.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/7827.jpg"),
                                            new AniListPersonNameData("Csongor", "Szalay", " ")),
                                        new AniListCharacterData.VoiceActorData("PORTUGUESE",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/11521.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/11521.jpg"),
                                            new AniListPersonNameData("Yuri", "Chesman", " ")),
                                        new AniListCharacterData.VoiceActorData("KOREAN",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/14777.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/14777.jpg"),
                                            new AniListPersonNameData("Sang Hyeon", "Eom", "상현 엄"))
                                    }),
                                new AniListCharacterData(
                                    new AniListCharacterData.InnerCharacterData(
                                        new AniListPersonNameData("Pony-Man", null, "ぽに男"),
                                        new AniListImageUrlData(
                                            @"https://cdn.anilist.co/img/dir/character/reg/27165.jpg",
                                            @"https://cdn.anilist.co/img/dir/character/med/27165.jpg")),
                                    new[]
                                    {
                                        new AniListCharacterData.VoiceActorData("ENGLISH",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/43.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/43.jpg"),
                                            new AniListPersonNameData("Vic", "Mignogna", " ")),
                                        new AniListCharacterData.VoiceActorData("JAPANESE",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/109.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/109.jpg"),
                                            new AniListPersonNameData("Ken", "Narita", "剣 成田")),
                                        new AniListCharacterData.VoiceActorData("KOREAN",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/14777.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/14777.jpg"),
                                            new AniListPersonNameData("Sang Hyeon", "Eom", "상현 엄"))
                                    }),
                                new AniListCharacterData(
                                    new AniListCharacterData.InnerCharacterData(
                                        new AniListPersonNameData("Yoko", "Wakana", "若菜陽子"),
                                        new AniListImageUrlData(
                                            @"https://cdn.anilist.co/img/dir/character/reg/39978.jpg",
                                            @"https://cdn.anilist.co/img/dir/character/med/39978.jpg")),
                                    new[]
                                    {
                                        new AniListCharacterData.VoiceActorData("JAPANESE",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/66.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/66.jpg"),
                                            new AniListPersonNameData("Akiko", "Hiramatsu", "晶子 平松")),
                                        new AniListCharacterData.VoiceActorData("ENGLISH",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/6435.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/6435.jpg"),
                                            new AniListPersonNameData("Kaytha", "Coker", " "))
                                    }),
                                new AniListCharacterData(
                                    new AniListCharacterData.InnerCharacterData(
                                        new AniListPersonNameData("Ena", "Saeki", "佐伯恵那"),
                                        new AniListImageUrlData(
                                            @"https://cdn.anilist.co/img/dir/character/reg/55463.jpg",
                                            @"https://cdn.anilist.co/img/dir/character/med/55463.jpg")),
                                    new[]
                                    {
                                        new AniListCharacterData.VoiceActorData("ENGLISH",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/162.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/162.jpg"),
                                            new AniListPersonNameData("Kira", "Vincent-Davis", " ")),
                                        new AniListCharacterData.VoiceActorData("JAPANESE",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/1858.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/1858.jpg"),
                                            new AniListPersonNameData("Shiho", "Kikuchi", "志穂 菊池")),
                                        new AniListCharacterData.VoiceActorData("ITALIAN",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/20328.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/20328.jpg"),
                                            new AniListPersonNameData("Eleonora", "Reti", " "))
                                    }),
                                new AniListCharacterData(
                                    new AniListCharacterData.InnerCharacterData(
                                        new AniListPersonNameData("Mari", "Akutsu", null),
                                        new AniListImageUrlData(
                                            @"https://cdn.anilist.co/img/dir/character/reg/55465.jpg",
                                            @"https://cdn.anilist.co/img/dir/character/med/55465.jpg")),
                                    new[]
                                    {
                                        new AniListCharacterData.VoiceActorData("JAPANESE",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/400.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/400.jpg"),
                                            new AniListPersonNameData("Mayumi", "Asano", "まゆみ 浅野")),
                                        new AniListCharacterData.VoiceActorData("ENGLISH",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/509.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/509.jpg"),
                                            new AniListPersonNameData("Christine", "Auten", " "))
                                    }),
                                new AniListCharacterData(
                                    new AniListCharacterData.InnerCharacterData(
                                        new AniListPersonNameData("Shiori", "Kudou", "工藤詩織"),
                                        new AniListImageUrlData(
                                            @"https://cdn.anilist.co/img/dir/character/reg/default.jpg",
                                            @"https://cdn.anilist.co/img/dir/character/med/default.jpg")),
                                    new[]
                                    {
                                        new AniListCharacterData.VoiceActorData("JAPANESE",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/56.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/56.jpg"),
                                            new AniListPersonNameData("Hiromi", "Konno", "宏美 今野"))
                                    }),
                                new AniListCharacterData(
                                    new AniListCharacterData.InnerCharacterData(
                                        new AniListPersonNameData("Kozue", "Nishino", "西野こずえ"),
                                        new AniListImageUrlData(
                                            @"https://cdn.anilist.co/img/dir/character/reg/default.jpg",
                                            @"https://cdn.anilist.co/img/dir/character/med/default.jpg")),
                                    new[]
                                    {
                                        new AniListCharacterData.VoiceActorData("JAPANESE",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/56.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/56.jpg"),
                                            new AniListPersonNameData("Hiromi", "Konno", "宏美 今野")),
                                        new AniListCharacterData.VoiceActorData("ENGLISH",
                                            new AniListImageUrlData(
                                                @"https://cdn.anilist.co/img/dir/person/reg/1606.jpg",
                                                @"https://cdn.anilist.co/img/dir/person/med/1606.jpg"),
                                            new AniListPersonNameData("Allison", "Sumrall", " "))
                                    })
                            }))
                    }
                , o => o.Excluding(d => d.Popularity)));
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.AniDbMetaStructure.Configuration;
using Jellyfin.AniDbMetaStructure.EntryPoints;
using Jellyfin.AniDbMetaStructure.Process.Sources;
using Jellyfin.AniDbMetaStructure.Tests.TestHelpers;
using FluentAssertions;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Serialization;
using NUnit.Framework;

namespace Emby.AniDbMetaStructure.Tests.IntegrationTests
{
    [TestFixture]
    public class SeriesProviderIntegrationTests
    {
        [SetUp]
        public void Setup()
        {
            this.applicationHost = new TestApplicationHost();
            var applicationPaths = this.applicationHost.Resolve<IApplicationPaths>();

            var plugin = new Plugin(applicationPaths,
                this.applicationHost.Resolve<IXmlSerializer>());

            plugin.SetConfiguration(new PluginConfiguration
            {
                TvDbApiKey = Secrets.TvDbApiKey
            });

            // pre-populate the cache to avoid spamming AniDb with requests when the tests run
            // (and for static test data)
            FileCacheHelper.SetupCachedFile(applicationPaths.CachePath, @"\anidb\titles.xml",
                @"\anidb\titles\titles.xml");

            FileCacheHelper.SetupCachedFile(applicationPaths.CachePath, @"\anidb\959.xml",
                @"\anidb\series\959\series.xml");

            FileCacheHelper.SetupCachedFile(applicationPaths.CachePath, @"\TvDb\78914.json", @"\anidb\tvdb\78914.json");

            FileCacheHelper.SetupCachedFile(applicationPaths.CachePath, @"\Mappings\anime-list.xml",
                @"\anime-list.xml");

            FileCacheHelper.SetupCachedFile(applicationPaths.CachePath, @"\anidb\10145.xml",
                @"\anidb\season\10145\season.xml");
            FileCacheHelper.SetupCachedFile(applicationPaths.CachePath, @"\anidb\10145.xml",
                @"\anidb\series\10145\series.xml");

            FileCacheHelper.SetupCachedFile(applicationPaths.CachePath, @"\TvDb\278157.json", @"\anidb\tvdb\278157.json");
        }

        private TestApplicationHost applicationHost;

        [Test]
        [TestCase("AniDb")]
        [TestCase("Tvdb")]
        public async Task GetMetadata_AniDbLibraryStructure_UsesNameFromLibraryStructureSource(
            string fileStructureSourceName)
        {
            Plugin.Instance.Configuration.LibraryStructureSourceName = SourceNames.AniDb;
            Plugin.Instance.Configuration.FileStructureSourceName = fileStructureSourceName;

            var seriesInfo = new SeriesInfo
            {
                Name = "Full Metal Panic Fumoffu"
            };

            var seriesEntryPoint = new SeriesProviderEntryPoint(this.applicationHost);

            var result = await seriesEntryPoint.GetMetadata(seriesInfo, CancellationToken.None);

            result.HasMetadata.Should().BeTrue();             
            result.Item.Name.Should().BeEquivalentTo("Fullmetal Panic? Fumoffu");
            result.Item.AirDays.Should().BeEquivalentTo(new[] { DayOfWeek.Tuesday });
            result.Item.AirTime.Should().BeEquivalentTo("18:30");
            result.Item.PremiereDate.Should().Be(new DateTime(2003, 08, 26));
            result.Item.EndDate.Should().Be(new DateTime(2003, 11, 18));
            result.Item.Overview.Should().BeEquivalentTo(@"It is back-to-school mayhem with Chidori Kaname and her battle-hardened classmate Sagara Sousuke as they encounter more misadventures in and out of Jindai High School. But when Kaname gets into some serious trouble, Sousuke takes the guise of Bonta-kun — the gun-wielding, butt-kicking mascot. And while he struggles to continue living as a normal teenager, Sousuke also has to deal with protecting his superior officer Teletha Testarossa, who has decided to take a vacation from Mithril and spend a couple of weeks as his and Kaname`s classmate.
Source: ANN
Note: Because of a then current kidnapping event, TV Tokyo did not broadcast what were supposed to be part 2 of episode 1 (A Hostage with No Compromises) and part 1 of episode 2 (Hostility Passing-By). A Hostage with No Compromises was replaced by part 2 of episode 2 (A Fruitless Lunchtime) and thus, episode 2 was skipped, leaving the episode count at 11. The DVD release contained all the episodes in the intended order.");
            result.Item.Studios.Should().BeEquivalentTo(new[] { "Kyoto Animation" });
            result.Item.Genres.Should().BeEquivalentTo(new[] { "Anime", "Present", "Earth", "Slapstick", "Japan" });
            result.Item.Tags.Should().BeEquivalentTo(new[] { "Asia", "Comedy", "High School", "School Life", "Action" });
            result.Item.CommunityRating.Should().Be(8.22f);
            result.Item.ProviderIds.Should().BeEquivalentTo(
                            new Dictionary<string, string>
                            {
                                { SourceNames.AniDb, "959" },
                                { SourceNames.TvDb, "78914" }
                            });
            result.People.Should().HaveCount(55);
        }

        [Test]
        [TestCase("AniDb")]
        [TestCase("Tvdb")]
        [Ignore("Anilist support is not working.")]
        public async Task GetMetadata_AniListMappings_MapsAniListData(string fileStructureSourceName)
        {
            Plugin.Instance.Configuration.AniListAccessToken =
                "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImp0aSI6IjA1YzRjZDMyNWY1MGNhMTllMThjMzU5MmI3YmQ3NzczNjUzYzVkNzdkNGY3NmYzZDcyNDY5ZDVkNmFhMGE3YmFmYjg2MjM3YjcxM2M2ZjAxIn0.eyJhdWQiOiIzNjIiLCJqdGkiOiIwNWM0Y2QzMjVmNTBjYTE5ZTE4YzM1OTJiN2JkNzc3MzY1M2M1ZDc3ZDRmNzZmM2Q3MjQ2OWQ1ZDZhYTBhN2JhZmI4NjIzN2I3MTNjNmYwMSIsImlhdCI6MTUyMzExMDA5NywibmJmIjoxNTIzMTEwMDk3LCJleHAiOjE1NTQ2NDYwOTcsInN1YiI6IjExMjA4NCIsInNjb3BlcyI6W119.eXUQ1VrEQdinxvuphdPxTmNgISnBf2sYUOdi3bhsR6Rp0_Tohh3PzKXEDZKt6Deu3NZieZ_ET5sMb1iYAeTX5K_XHhYOQwcZzGSwstBT84HkyPl6FL6ONrCxO94z4arfnpriNM3eVPhGQee9CT5jEpMxYAtTgN8-9MsDD5pyc_AvRT_AuC2ugqw81dgPCgNDjSAiOSBNG1XWpXI2jV1jF5TKaOVlfedJqCL-scL7j4XBiq3v-2WdPaV5oqw2kvEfH5A5pReIU_m-SAFduAgvPNPdgGSh7izx14WSzdWpuiYLc_ly8VhxptwWnlHifLrAeu0t2UjmCy5Ssh1op2Bmo2qXJPlx9xcdTyW2yqTxxH-V_VbsPH2Omvmda_PFsi6sLKhCEF1qGhAJ0aSGIpbTl8V6tJ4-JxbhU2GjyR13LOHTOIU7sM_OO9ketgKGZ6L2wI4LQGbm6BIop96QweRjT19hCwkwHS-Tq1d0HRtCJ_tPHuupZKARDrMQgkVHTJ1lPsIKyf92KnUJ1azn6AxTSwvxQSVnaiqM_1CMsC0ht0bs9RgnqjAKRv734qt7yibItu7UzzV3JIOYX2duG8KW_VLyM78V4DqIEhui7K9MARavlqEs2umOkLHb1aaLz9zVvgrNQrzwUXeXPQm_myWgieWEP5RIQH7Gv_YC-W1pB0o";

            Plugin.Instance.Configuration.LibraryStructureSourceName = SourceNames.AniDb;
            Plugin.Instance.Configuration.SeriesMappings.Iter(sm =>
            {
                if (sm.Mappings.Any(m => m.SourceName == SourceNames.AniList))
                {
                    sm.Mappings = sm.Mappings.Where(m =>
                            m.SourceName == SourceNames.AniList || m.TargetPropertyName == nameof(Series.Name))
                        .ToArray();
                }
            });

            var seriesInfo = new SeriesInfo
            {
                Name = "Full Metal Panic Fumoffu"
            };

            var seriesEntryPoint = new SeriesProviderEntryPoint(this.applicationHost);

            var result = await seriesEntryPoint.GetMetadata(seriesInfo, CancellationToken.None);

            result.HasMetadata.Should().BeTrue();
            result.Item.Name.Should().BeEquivalentTo("Fullmetal Panic? Fumoffu");
            result.Item.AirDays.Should().BeEquivalentTo(new[] { DayOfWeek.Tuesday });
            result.Item.AirTime.Should().BeEquivalentTo("18:30");
            result.Item.PremiereDate.Should().Be(new DateTime(2003, 08, 26));
            result.Item.EndDate.Should().Be(new DateTime(2003, 11, 18));
            result.Item.Overview.Should().BeEquivalentTo(
                "It's back-to-school mayhem with Kaname Chidori and her war-freak classmate Sousuke Sagara as they encounter more misadventures in and out of Jindai High School. But when Kaname gets into some serious trouble, Sousuke takes the guise of Bonta-kun - the gun-wielding, butt-kicking mascot. And while he struggles to continue living as a normal teenager, Sousuke also has to deal with protecting his superior officer Teletha Testarossa, who has decided to take a vacation from Mithril and spend a couple of weeks as his and Kaname's classmate.< br >< br >\n(Source: ANN)");
            result.Item.Studios.Should().BeEquivalentTo(new[] { "Kyoto Animation", "ADV Films", "FUNimation Entertainment" });
            result.Item.Genres.Should().BeEquivalentTo(new[] { "Action", "Comedy" });
            result.Item.Tags.Should().BeEquivalentTo(new string[] { });
            result.Item.CommunityRating.Should().Be(78f);
            result.Item.ProviderIds.Should().BeEquivalentTo(
                            new Dictionary<string, string>
                            {
                                { SourceNames.AniDb, "959" },
                                { SourceNames.TvDb, "78914" },
                                { SourceNames.AniList, "72" }
                            });
            result.People.Should().HaveCount(48);
        }

        [Test]
        [TestCase("AniDb")]
        [TestCase("Tvdb")]
        public async Task GetMetadata_TvDbLibraryStructure_UsesNameFromLibraryStructureSource(
            string fileStructureSourceName)
        {
            Plugin.Instance.Configuration.LibraryStructureSourceName = SourceNames.TvDb;

            var seriesInfo = new SeriesInfo
            {
                Name = "Full Metal Panic Fumoffu"
            };

            var seriesEntryPoint = new SeriesProviderEntryPoint(this.applicationHost);

            var result = await seriesEntryPoint.GetMetadata(seriesInfo, CancellationToken.None);

            result.HasMetadata.Should().BeTrue();
            result.Item.Name.Should().BeEquivalentTo("Full Metal Panic!");
            result.Item.AirDays.Should().BeEquivalentTo(new[] { DayOfWeek.Tuesday });
            result.Item.AirTime.Should().BeEquivalentTo("18:30");
            result.Item.PremiereDate.Should().Be(new DateTime(2003, 08, 26));
            result.Item.EndDate.Should().Be(new DateTime(2003, 11, 18));
            result.Item.Overview.Should().BeEquivalentTo(@"It is back-to-school mayhem with Chidori Kaname and her battle-hardened classmate Sagara Sousuke as they encounter more misadventures in and out of Jindai High School. But when Kaname gets into some serious trouble, Sousuke takes the guise of Bonta-kun — the gun-wielding, butt-kicking mascot. And while he struggles to continue living as a normal teenager, Sousuke also has to deal with protecting his superior officer Teletha Testarossa, who has decided to take a vacation from Mithril and spend a couple of weeks as his and Kaname`s classmate.
Source: ANN
Note: Because of a then current kidnapping event, TV Tokyo did not broadcast what were supposed to be part 2 of episode 1 (A Hostage with No Compromises) and part 1 of episode 2 (Hostility Passing-By). A Hostage with No Compromises was replaced by part 2 of episode 2 (A Fruitless Lunchtime) and thus, episode 2 was skipped, leaving the episode count at 11. The DVD release contained all the episodes in the intended order.");
            result.Item.Studios.Should().BeEquivalentTo(new[] { "Kyoto Animation" });
            result.Item.Genres.Should().BeEquivalentTo(new[] { "Anime", "Present", "Earth", "Slapstick", "Japan" });
            result.Item.Tags.Should().BeEquivalentTo(new[] { "Asia", "Comedy", "High School", "School Life", "Action" });
            result.Item.CommunityRating.Should().Be(8.22f);
            result.Item.ProviderIds.Should().BeEquivalentTo(
                            new Dictionary<string, string>
                            {
                                { SourceNames.AniDb, "959" },
                                { SourceNames.TvDb, "78914" }
                            });
            result.People.Should().HaveCount(55);
        }

        [Test]
        public async Task GetMetadata_AniDbLibraryStructure_UsesNameFromTvDBFileStructureSource()
        {
            Plugin.Instance.Configuration.LibraryStructureSourceName = SourceNames.AniDb;
            Plugin.Instance.Configuration.FileStructureSourceName = SourceNames.TvDb;

           var seriesInfo = new SeriesInfo
            {
                Name = "Haikyu!!"
            };

            var seriesEntryPoint = new SeriesProviderEntryPoint(this.applicationHost);

            var resultSeries = await seriesEntryPoint.GetMetadata(seriesInfo, CancellationToken.None);

            var seasonInfo = new SeasonInfo
            {
                Name = "Season 01",
                IndexNumber = 1,
                SeriesProviderIds = { { resultSeries.Item.ProviderIds.First().Key, resultSeries.Item.ProviderIds.First().Value } }
            };
            var seasonInfo2 = new SeasonInfo
            {
                Name = "Season 02",
                IndexNumber = 2,
                SeriesProviderIds = { { resultSeries.Item.ProviderIds.First().Key, resultSeries.Item.ProviderIds.First().Value } }
            };


            var seasonEntryPoint = new SeasonProviderEntryPoint(this.applicationHost);

            var resultSeason1 = await seasonEntryPoint.GetMetadata(seasonInfo, CancellationToken.None);

            var resultSeason2 = await seasonEntryPoint.GetMetadata(seasonInfo2, CancellationToken.None);

            var episodeEntryPoint = new EpisodeProviderEntryPoint(this.applicationHost);

            var episodeInfo = new EpisodeInfo
            {
                Name = "Haikyu.S02E02.1080p.HDTV-HorribleSubs",
                IndexNumber = 2,
                ParentIndexNumber = 2,
                SeriesProviderIds = { 
                    { resultSeries.Item.ProviderIds.Last().Key, resultSeries.Item.ProviderIds.Last().Value }
                }
            };
            var resultEpisode11 = await episodeEntryPoint.GetMetadata(episodeInfo, CancellationToken.None);

            resultSeries.HasMetadata.Should().BeTrue();
            resultSeries.Item.Name.Should().BeEquivalentTo("Haikyuu!!");
            resultSeries.Item.AirDays.Should().BeEquivalentTo(new[] { DayOfWeek.Saturday });
            resultSeries.Item.AirTime.Should().BeEquivalentTo("");
            resultSeries.Item.PremiereDate.Should().Be(new DateTime(2003, 08, 26));
            resultSeries.Item.EndDate.Should().Be(new DateTime(2003, 11, 18));
            resultSeries.Item.Overview.Should().BeEquivalentTo(@"It is back-to-school mayhem with Chidori Kaname and her battle-hardened classmate Sagara Sousuke as they encounter more misadventures in and out of Jindai High School. But when Kaname gets into some serious trouble, Sousuke takes the guise of Bonta-kun — the gun-wielding, butt-kicking mascot. And while he struggles to continue living as a normal teenager, Sousuke also has to deal with protecting his superior officer Teletha Testarossa, who has decided to take a vacation from Mithril and spend a couple of weeks as his and Kaname`s classmate.
Source: ANN
Note: Because of a then current kidnapping event, TV Tokyo did not broadcast what were supposed to be part 2 of episode 1 (A Hostage with No Compromises) and part 1 of episode 2 (Hostility Passing-By). A Hostage with No Compromises was replaced by part 2 of episode 2 (A Fruitless Lunchtime) and thus, episode 2 was skipped, leaving the episode count at 11. The DVD release contained all the episodes in the intended order.");
            resultSeries.Item.Studios.Should().BeEquivalentTo(new[] { "Kyoto Animation" });
            resultSeries.Item.Genres.Should().BeEquivalentTo(new[] { "Anime", "Present", "Earth", "Slapstick", "Japan" });
            resultSeries.Item.Tags.Should().BeEquivalentTo(new[] { "Asia", "Comedy", "High School", "School Life", "Action" });
            resultSeries.Item.CommunityRating.Should().Be(8.22f);
            resultSeries.Item.ProviderIds.Should().BeEquivalentTo(
                            new Dictionary<string, string>
                            {
                                { SourceNames.AniDb, "959" },
                                { SourceNames.TvDb, "78914" }
                            });
            resultSeries.People.Should().HaveCount(55);
        }
    }
}
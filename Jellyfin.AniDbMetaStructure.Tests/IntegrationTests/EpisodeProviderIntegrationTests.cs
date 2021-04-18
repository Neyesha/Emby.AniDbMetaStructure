using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.AniDbMetaStructure.Configuration;
using Jellyfin.AniDbMetaStructure.EntryPoints;
using Jellyfin.AniDbMetaStructure.Process.Sources;
using Jellyfin.AniDbMetaStructure.Tests.TestHelpers;
using FluentAssertions;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Serialization;
using NUnit.Framework;

namespace Emby.AniDbMetaStructure.Tests.IntegrationTests
{
    [TestFixture]
    public class EpisodeProviderIntegrationTests
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

            // pre-populate the cache to avoid spamming sources with requests when the tests run
            // (and for static test data)
            FileCacheHelper.SetupCachedFile(applicationPaths.CachePath, @"\AniDb\titles.xml",
                @"\anidb\titles\titles.xml");

            FileCacheHelper.SetupCachedFile(applicationPaths.CachePath, @"\AniDb\959.xml",
                @"\anidb\series\959\series.xml");

            FileCacheHelper.SetupCachedFile(applicationPaths.CachePath, @"\Mappings\anime-list.xml",
                @"\anime-list.xml");

            FileCacheHelper.SetupCachedFile(applicationPaths.CachePath, @"\TvDb\78914.json", @"\anidb\tvdb\78914.json");

            FileCacheHelper.SetupCachedFile(applicationPaths.CachePath, @"\TvDb\78914_Episodes.json",
                @"\anidb\tvdb\78914_Episodes.json");
        }

        private class AniDbFileStructureCases
        {
            public static object[] FumoffuEpisodeOneCases => new[]
            {
                new EpisodeInfo
                {
                    Name = "The Man from the South A Fruitless Lunchtime",
                    SeriesProviderIds = { { SourceNames.TvDb, "78914" }, { SourceNames.AniDb, "959" } },
                    MetadataLanguage = "en"
                },
                new EpisodeInfo
                {
                    Name = "NotFound",
                    IndexNumber = 1,
                    SeriesProviderIds = { { SourceNames.TvDb, "78914" }, { SourceNames.AniDb, "959" } },
                    MetadataLanguage = "en"
                },
                new EpisodeInfo
                {
                    Name = "NotFound",
                    IndexNumber = 1,
                    ParentIndexNumber = 1,
                    SeriesProviderIds = { { SourceNames.TvDb, "78914" }, { SourceNames.AniDb, "959" } },
                    MetadataLanguage = "en"
                }
            };
        }

        private class TvDbFileStructureCases
        {
            public static object[] FumoffuEpisodeOneCases => new[]
            {
                new EpisodeInfo
                {
                    Name = "The Man from the South A Fruitless Lunchtime",
                    SeriesProviderIds = { { SourceNames.TvDb, "78914" }, { SourceNames.AniDb, "959" } },
                    MetadataLanguage = "en"
                },
                new EpisodeInfo
                {
                    Name = "NotFound",
                    IndexNumber = 1,
                    ParentIndexNumber = 0,
                    SeriesProviderIds = { { SourceNames.TvDb, "78914" }, { SourceNames.AniDb, "959" } },
                    MetadataLanguage = "en"
                }
            };
        }

        private TestApplicationHost applicationHost;

        [Test]
        [TestCaseSource(typeof(AniDbFileStructureCases), nameof(AniDbFileStructureCases.FumoffuEpisodeOneCases))]
        public async Task GetMetadata_AniDbLibraryStructure_AniDbFileStructure_GetsMetadata(EpisodeInfo episodeInfo)
        {
            Plugin.Instance.Configuration.LibraryStructureSourceName = SourceNames.AniDb;
            Plugin.Instance.Configuration.FileStructureSourceName = SourceNames.AniDb;

            var episodeEntryPoint = new EpisodeProviderEntryPoint(this.applicationHost);

            var result = await episodeEntryPoint.GetMetadata(episodeInfo, CancellationToken.None);

            result.HasMetadata.Should().BeTrue();
            result.Item.Name.Should().BeEquivalentTo("The Man from the South / A Fruitless Lunchtime (AniDb)");
            result.Item.PremiereDate.Should().Be(new DateTime(2003, 08, 26));
            result.Item.Overview.Should().BeEquivalentTo(@"The Man From The South:
A secret admirer leaves a love letter in Sousuke's shoe locker. Instead of finding the letter, he deduces that his locker was tampered with and promptly blows it up. From its fragments, Sousuke misinterprets the letter as a death threat and confronts his ""stalker"".

A Fruitless Lunchtime:
After Sousuke causes a panic at the school's bread stand, he and Kaname have to fill in for the stand's lunchtime duties, which raises the ire of Mr. Kogure, the physical education teacher. He attempts to sabotage their bread, but falls for Sousuke's booby traps, expecting that someone would tamper the bread."
);
            result.Item.CommunityRating.Should().Be(8.99f);
            result.Item.ProviderIds.Should().BeEquivalentTo(new Dictionary<string, string>
                        {
                            { SourceNames.AniDb, "10407" },
                            { SourceNames.TvDb, "1973471" }
                        });
            result.Item.RunTimeTicks.Should().Be(15000000000L);
            result.Item.IndexNumber.Should().Be(1);
            result.Item.ParentIndexNumber.Should().Be(1);
            result.Item.Studios.Should().BeEquivalentTo(new[] { "Kyoto Animation" });
            result.Item.Genres.Should().BeEquivalentTo(
                new[]
                {
                    "Anime",
                    "Present",
                    "Earth",
                    "Slapstick",
                    "Japan"
                });
            result.Item.Tags.Should().BeEquivalentTo(
                new[]
                {
                    "Asia",
                    "Comedy",
                    "High School",
                    "School Life",
                    "Action"
                });
        }

        [Test]
        [TestCaseSource(typeof(TvDbFileStructureCases), nameof(TvDbFileStructureCases.FumoffuEpisodeOneCases))]
        public async Task GetMetadata_AniDbLibraryStructure_TvDbFileStructure_GetsMetadata(EpisodeInfo episodeInfo)
        {
            Plugin.Instance.Configuration.LibraryStructureSourceName = SourceNames.AniDb;
            Plugin.Instance.Configuration.FileStructureSourceName = SourceNames.TvDb;

            var episodeEntryPoint = new EpisodeProviderEntryPoint(this.applicationHost);

            var result = await episodeEntryPoint.GetMetadata(episodeInfo, CancellationToken.None);

            result.HasMetadata.Should().BeTrue();
            result.Item.Name.Should().BeEquivalentTo("The Man from the South / A Fruitless Lunchtime (AniDb)");
            result.Item.PremiereDate.Should().Be(new DateTime(2003, 08, 26));
            result.Item.Overview.Should().BeEquivalentTo(@"The Man From The South:
A secret admirer leaves a love letter in Sousuke's shoe locker. Instead of finding the letter, he deduces that his locker was tampered with and promptly blows it up. From its fragments, Sousuke misinterprets the letter as a death threat and confronts his ""stalker"".

A Fruitless Lunchtime:
After Sousuke causes a panic at the school's bread stand, he and Kaname have to fill in for the stand's lunchtime duties, which raises the ire of Mr. Kogure, the physical education teacher. He attempts to sabotage their bread, but falls for Sousuke's booby traps, expecting that someone would tamper the bread."
);

            result.Item.ProviderIds.Should().BeEquivalentTo(new Dictionary<string, string>
                        {
                            { SourceNames.AniDb, "10407" },
                            { SourceNames.TvDb, "1973471" }
                        });
            result.Item.RunTimeTicks.Should().Be(15000000000L);
            result.Item.IndexNumber.Should().Be(1);
            result.Item.ParentIndexNumber.Should().Be(1);
            result.Item.Studios.Should().BeEquivalentTo(new[] { "Kyoto Animation" });
            result.Item.Genres.Should().BeEquivalentTo(
                new[]
                {
                    "Anime",
                    "Present",
                    "Earth",
                    "Slapstick",
                    "Japan"
                });
            result.Item.Tags.Should().BeEquivalentTo(
                new[]
                {
                    "Asia",
                    "Comedy",
                    "High School",
                    "School Life",
                    "Action"
                });
        }

        [Test]
        [TestCaseSource(typeof(AniDbFileStructureCases), nameof(AniDbFileStructureCases.FumoffuEpisodeOneCases))]
        public async Task GetMetadata_TvDbLibraryStructure_AniDbFileStructure_GetsMetadata(EpisodeInfo episodeInfo)
        {
            Plugin.Instance.Configuration.LibraryStructureSourceName = SourceNames.TvDb;
            Plugin.Instance.Configuration.FileStructureSourceName = SourceNames.AniDb;

            var episodeEntryPoint = new EpisodeProviderEntryPoint(this.applicationHost);

            var result = await episodeEntryPoint.GetMetadata(episodeInfo, CancellationToken.None);

            result.HasMetadata.Should().BeTrue();
            result.Item.Name.Should().BeEquivalentTo("The Man From The South / A Fruitless Lunchtime");
            result.Item.PremiereDate.Should().Be(new DateTime(2003, 08, 26));
            result.Item.Overview.Should().BeEquivalentTo(@"The Man From The South:
A secret admirer leaves a love letter in Sousuke's shoe locker. Instead of finding the letter, he deduces that his locker was tampered with and promptly blows it up. From its fragments, Sousuke misinterprets the letter as a death threat and confronts his ""stalker"".

A Fruitless Lunchtime:
After Sousuke causes a panic at the school's bread stand, he and Kaname have to fill in for the stand's lunchtime duties, which raises the ire of Mr. Kogure, the physical education teacher. He attempts to sabotage their bread, but falls for Sousuke's booby traps, expecting that someone would tamper the bread."
);

            result.Item.ProviderIds.Should().BeEquivalentTo(new Dictionary<string, string>
                        {
                            { SourceNames.AniDb, "10407" },
                            { SourceNames.TvDb, "1973471" }
                        });
            result.Item.RunTimeTicks.Should().Be(15000000000L);
            result.Item.IndexNumber.Should().Be(1);
            result.Item.ParentIndexNumber.Should().Be(0);
            result.Item.Studios.Should().BeEquivalentTo(new[] { "Kyoto Animation" });
            result.Item.Genres.Should().BeEquivalentTo(
                new[]
                {
                    "Anime",
                    "Present",
                    "Earth",
                    "Slapstick",
                    "Japan"
                });
            result.Item.Tags.Should().BeEquivalentTo(
                new[]
                {
                    "Asia",
                    "Comedy",
                    "High School",
                    "School Life",
                    "Action"
                });
        }

        [Test]
        [TestCaseSource(typeof(TvDbFileStructureCases), nameof(TvDbFileStructureCases.FumoffuEpisodeOneCases))]
        public async Task GetMetadata_TvDbLibraryStructure_TvDbFileStructure_GetsMetadata(EpisodeInfo episodeInfo)
        {
            Plugin.Instance.Configuration.LibraryStructureSourceName = SourceNames.TvDb;
            Plugin.Instance.Configuration.FileStructureSourceName = SourceNames.TvDb;

            var episodeEntryPoint = new EpisodeProviderEntryPoint(this.applicationHost);

            var result = await episodeEntryPoint.GetMetadata(episodeInfo, CancellationToken.None);

            result.HasMetadata.Should().BeTrue();
            result.Item.Name.Should().BeEquivalentTo("The Man From The South / A Fruitless Lunchtime");
            result.Item.PremiereDate.Should().Be(new DateTime(2003, 08, 26));
            result.Item.Overview.Should().BeEquivalentTo(@"The Man From The South:
A secret admirer leaves a love letter in Sousuke's shoe locker. Instead of finding the letter, he deduces that his locker was tampered with and promptly blows it up. From its fragments, Sousuke misinterprets the letter as a death threat and confronts his ""stalker"".

A Fruitless Lunchtime:
After Sousuke causes a panic at the school's bread stand, he and Kaname have to fill in for the stand's lunchtime duties, which raises the ire of Mr. Kogure, the physical education teacher. He attempts to sabotage their bread, but falls for Sousuke's booby traps, expecting that someone would tamper the bread."
);

            result.Item.ProviderIds.Should().BeEquivalentTo(new Dictionary<string, string>
                        {
                            { SourceNames.AniDb, "10407" },
                            { SourceNames.TvDb, "1973471" }
                        });
            result.Item.RunTimeTicks.Should().Be(15000000000L);
            result.Item.IndexNumber.Should().Be(1);
            result.Item.ParentIndexNumber.Should().Be(0);
            result.Item.Studios.Should().BeEquivalentTo(new[] { "Kyoto Animation" });
            result.Item.Genres.Should().BeEquivalentTo(
                new[]
                {
                    "Anime",
                    "Present",
                    "Earth",
                    "Slapstick",
                    "Japan"
                });
            result.Item.Tags.Should().BeEquivalentTo(
                new[]
                {
                    "Asia",
                    "Comedy",
                    "High School",
                    "School Life",
                    "Action"
                });
        }
    }
}
﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Plugins.AniMetadata.Configuration;
using MediaBrowser.Plugins.AniMetadata.EntryPoints;
using MediaBrowser.Plugins.AniMetadata.Process.Sources;
using MediaBrowser.Plugins.AniMetadata.Tests.TestHelpers;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests.IntegrationTests
{
    [TestFixture]
    public class SeasonProviderIntegrationTests
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
                @"\anidb\season\959\season.xml");
            FileCacheHelper.SetupCachedFile(applicationPaths.CachePath, @"\anidb\959.xml",
                @"\anidb\series\959\series.xml");

            FileCacheHelper.SetupCachedFile(applicationPaths.CachePath, @"\TvDb\78914.json", @"\anidb\tvdb\78914.json");

            FileCacheHelper.SetupCachedFile(applicationPaths.CachePath, @"\Mappings\anime-list.xml",
                @"\anime-list.xml");
        }

        private TestApplicationHost applicationHost;

        [Test]
        //[TestCase("AniDb")]
        [TestCase("Tvdb")]
        public async Task GetMetadata_AniDbLibraryStructure_UsesNameFromLibraryStructureSource(
            string fileStructureSourceName)
        {
            Plugin.Instance.Configuration.LibraryStructureSourceName = SourceNames.AniDb;
            Plugin.Instance.Configuration.FileStructureSourceName = fileStructureSourceName;

            var seasonInfo = new SeasonInfo
            {
                Name = "Season Unknown",
                SeriesProviderIds = new Dictionary<string, string>
                {
                    { SourceNames.AniDb, "959" },
                    { SourceNames.TvDb, "78914" },
                    { SourceNames.AniList, "72" }
                }
            };

            var seasonEntryPoint = new SeasonProviderEntryPoint(this.applicationHost);

            var result = await seasonEntryPoint.GetMetadata(seasonInfo, CancellationToken.None);

            result.HasMetadata.Should().BeTrue();
            result.Item.Name.Should().BeEquivalentTo("Fullmetal Panic? Fumoffu");
        }

        [Test]
        [TestCase("AniDb")]
        [TestCase("Tvdb")]
        public async Task GetMetadata_TvDbLibraryStructure_UsesNameFromLibraryStructureSource(
            string fileStructureSourceName)
        {
            Plugin.Instance.Configuration.LibraryStructureSourceName = SourceNames.TvDb;

            var seasonInfo = new SeasonInfo
            {
                Name = "Season Unknown",
                IndexNumber = 2,
                SeriesProviderIds = new Dictionary<string, string>
                {
                    { SourceNames.AniDb, "959" },
                    { SourceNames.TvDb, "78914" },
                    { SourceNames.AniList, "72" }
                }
            };

            var seasonEntryPoint = new SeasonProviderEntryPoint(this.applicationHost);

            var result = await seasonEntryPoint.GetMetadata(seasonInfo, CancellationToken.None);

            result.HasMetadata.Should().BeTrue();
            result.Item.Name.Should().BeEquivalentTo("Season 2");
        }
    }
}
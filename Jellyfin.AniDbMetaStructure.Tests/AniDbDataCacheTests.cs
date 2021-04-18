using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.AniDbMetaStructure.AniDb;
using Jellyfin.AniDbMetaStructure.AniDb.Seiyuu;
using Jellyfin.AniDbMetaStructure.AniDb.SeriesData;
using Jellyfin.AniDbMetaStructure.Files;
using Jellyfin.AniDbMetaStructure.Tests.TestData;
using Jellyfin.AniDbMetaStructure.Tests.TestHelpers;
using FluentAssertions;
using MediaBrowser.Common.Configuration;
using NSubstitute;
using NUnit.Framework;

namespace Emby.AniDbMetaStructure.Tests
{
    [TestFixture]
    public class AniDbDataCacheTests
    {
        [Test]
        public void GetSeiyuu_LoadsSeiyuuFile()
        {
            var rootPath = AppDomain.CurrentDomain.BaseDirectory + @"\" + Guid.NewGuid();
            var expectedFileLocation = rootPath + @"\anidb\seiyuu.xml";
            var applicationPaths = Substitute.For<IApplicationPaths>();
            var fileCache = Substitute.For<IFileCache>();

            applicationPaths.CachePath.Returns(rootPath);

            var aniDbDataCache = new AniDbDataCache(applicationPaths, fileCache, new ConsoleLogManager());

            aniDbDataCache.GetSeiyuu();

            fileCache.Received(1).GetFileContent(Arg.Is<SeiyuuFileSpec>(f => f.LocalPath == expectedFileLocation));
        }

        [Test]
        public async Task GetSeries_AddsSeiyuuToFile()
        {
            var rootPath = AppDomain.CurrentDomain.BaseDirectory + @"\" + Guid.NewGuid();
            var expectedFileLocation = rootPath + @"\anidb\seiyuu.xml";
            var applicationPaths = Substitute.For<IApplicationPaths>();
            var fileCache = Substitute.For<IFileCache>();

            var seiyuu1 = new SeiyuuData
            {
                Id = 3,
                Name = "Test",
                PictureFileName = "132"
            };

            var seiyuu2 = new SeiyuuData
            {
                Id = 4,
                Name = "Test2",
                PictureFileName = "422"
            };

            var series = new AniDbSeriesData().WithStandardData();
            var seriesWithExtraSeiyuu = new AniDbSeriesData().WithStandardData();

            seriesWithExtraSeiyuu.Characters = seriesWithExtraSeiyuu.Characters.Concat(new[]
                {
                    new CharacterData
                    {
                        Seiyuu = seiyuu1
                    },
                    new CharacterData
                    {
                        Seiyuu = seiyuu2
                    }
                })
                .ToArray();

            applicationPaths.CachePath.Returns(rootPath);

            fileCache.GetFileContentAsync(Arg.Is<SeriesFileSpec>(s => s.Url.EndsWith("1")),
                    Arg.Any<CancellationToken>())
                .Returns(series);

            fileCache.GetFileContentAsync(Arg.Is<SeriesFileSpec>(s => s.Url.EndsWith("2")),
                    Arg.Any<CancellationToken>())
                .Returns(seriesWithExtraSeiyuu);

            var aniDbDataCache = new AniDbDataCache(applicationPaths, fileCache, new ConsoleLogManager());

            aniDbDataCache.GetSeiyuu().Should().BeEmpty();

            await aniDbDataCache.GetSeriesAsync(1, CancellationToken.None);

            fileCache.Received(1)
                .SaveFile(Arg.Is<SeiyuuFileSpec>(f => f.LocalPath == expectedFileLocation),
                    Arg.Is<SeiyuuListData>(s => s.Seiyuu.Length == 1));

            fileCache.GetFileContent<SeiyuuListData>(null)
                .ReturnsForAnyArgs(new SeiyuuListData { Seiyuu = new[] { series.Characters[0].Seiyuu } });

            await aniDbDataCache.GetSeriesAsync(2, CancellationToken.None);

            fileCache.Received(1)
                .SaveFile(Arg.Is<SeiyuuFileSpec>(f => f.LocalPath == expectedFileLocation),
                    Arg.Is<SeiyuuListData>(s =>
                        s.Seiyuu.SequenceEqual(new[] { series.Characters[0].Seiyuu, seiyuu1, seiyuu2 })));
        }

        [Test]
        public async Task GetSeries_SavesSeiyuuFile()
        {
            var rootPath = AppDomain.CurrentDomain.BaseDirectory + @"\" + Guid.NewGuid();
            var expectedFileLocation = rootPath + @"\anidb\seiyuu.xml";
            var applicationPaths = Substitute.For<IApplicationPaths>();
            var fileCache = Substitute.For<IFileCache>();

            applicationPaths.CachePath.Returns(rootPath);

            fileCache.GetFileContentAsync(Arg.Is<SeriesFileSpec>(s => s.Url.EndsWith("1")),
                    Arg.Any<CancellationToken>())
                .Returns(new AniDbSeriesData().WithStandardData());

            var aniDbDataCache = new AniDbDataCache(applicationPaths, fileCache, new ConsoleLogManager());

            await aniDbDataCache.GetSeriesAsync(1, CancellationToken.None);

            fileCache.Received(1)
                .SaveFile(Arg.Is<SeiyuuFileSpec>(f => f.LocalPath == expectedFileLocation),
                    Arg.Is<SeiyuuListData>(s => s.Seiyuu.Length == 1));
        }
    }
}
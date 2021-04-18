using System.Threading;
using System.Threading.Tasks;
using Emby.AniDbMetaStructure.AniDb.Titles;
using Emby.AniDbMetaStructure.Files;
using Emby.AniDbMetaStructure.Tests.TestHelpers;
using NUnit.Framework;

namespace Emby.AniDbMetaStructure.Tests.Files
{
    [TestFixture]
    public class FileDownloadderTests
    {
        private IFileDownloader fileDownloader;

        [SetUp]
        public void Setup()
        {
            this.fileDownloader = DependencyConfiguration.Resolve<IFileDownloader>(new TestApplicationHost());
        }

        [Test]
        [Ignore("Anidb ip ban")]
        public async Task DownloadFileAsync_AnidbTitleListDowloaded()
        {
            await this.fileDownloader.DownloadFileAsync(new TitlesFileSpec(@"D:\GitHub\MediaBrowser.Plugins.AniDbForTvDb\MediaBrowser.Plugins.AniMetadata\bin\Debug\netstandard2.0"), new CancellationToken());
        }
    }
}

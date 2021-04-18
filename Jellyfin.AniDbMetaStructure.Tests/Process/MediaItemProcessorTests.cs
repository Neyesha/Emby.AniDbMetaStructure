using System.Linq;
using System.Threading.Tasks;
using Emby.AniDbMetaStructure.Configuration;
using Emby.AniDbMetaStructure.Process;
using Emby.AniDbMetaStructure.Tests.TestHelpers;
using FluentAssertions;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;
using NSubstitute;
using NUnit.Framework;
using static LanguageExt.Prelude;

namespace Emby.AniDbMetaStructure.Tests.Process
{
    [TestFixture]
    public class MediaItemProcessorTests
    {
        [SetUp]
        public virtual void Setup()
        {
            this.PluginConfiguration = Substitute.For<IPluginConfiguration>();

            this.MediaItemBuilder = Substitute.For<IMediaItemBuilder>();
            this.MediaItemBuilder.BuildMediaItem(Arg.Any<IMediaItem>())
                .Returns(x => Right<ProcessFailedResult, IMediaItem>(x.Arg<IMediaItem>()));

            this.MediaItemType = Substitute.For<IMediaItemType<Series>>();
            this.MediaItemType.CreateMetadataFoundResult(this.PluginConfiguration, Arg.Any<IMediaItem>(), Arg.Any<ILogManager>())
                .Returns(x => Right<ProcessFailedResult, IMetadataFoundResult<Series>>(new MetadataFoundResult<Series>(
                    x.Arg<IMediaItem>(), new MetadataResult<Series>
                    {
                        Item = new Series()
                    })));

            this.Processor = new MediaItemProcessor(this.PluginConfiguration, this.MediaItemBuilder, new ConsoleLogManager());
        }

        internal static class Data
        {
            public static IMediaItem MediaItem()
            {
                return Substitute.For<IMediaItem>();
            }

            public static ItemLookupInfo EmbyInfo()
            {
                return new ItemLookupInfo
                {
                    IndexNumber = 1,
                    ParentIndexNumber = 2,
                    Name = "name"
                };
            }
        }

        internal IMediaItemType<Series> MediaItemType;
        internal IPluginConfiguration PluginConfiguration;
        internal IMediaItemBuilder MediaItemBuilder;
        internal MediaItemProcessor Processor;

        [TestFixture]
        public class GetResultAsync : MediaItemProcessorTests
        {
            [SetUp]
            public override void Setup()
            {
                base.Setup();

                this.mediaItem = Data.MediaItem();
                this.embyInfo = Data.EmbyInfo();

                this.MediaItemBuilder.Identify(Arg.Any<EmbyItemData>(), this.MediaItemType)
                    .Returns(Right<ProcessFailedResult, IMediaItem>(this.mediaItem));
            }

            private ItemLookupInfo embyInfo;
            private IMediaItem mediaItem;

            [Test]
            public async Task BuildsMediaItem()
            {
                this.MediaItemBuilder.Identify(Arg.Any<EmbyItemData>(), this.MediaItemType)
                    .Returns(Right<ProcessFailedResult, IMediaItem>(this.mediaItem));

                var result = await this.Processor.GetResultAsync(this.embyInfo, this.MediaItemType, Enumerable.Empty<EmbyItemId>());

                result.IsRight.Should().BeTrue();
                await this.MediaItemBuilder.Received(1).BuildMediaItem(this.mediaItem);
            }

            [Test]
            public async Task CreatesResult()
            {
                var embyInfo = Data.EmbyInfo();
                var mediaItem = Data.MediaItem();
                var builtMediaItem = Data.MediaItem();

                this.MediaItemBuilder.Identify(Arg.Any<EmbyItemData>(), this.MediaItemType)
                    .Returns(Right<ProcessFailedResult, IMediaItem>(mediaItem));
                this.MediaItemBuilder.BuildMediaItem(mediaItem)
                    .Returns(Right<ProcessFailedResult, IMediaItem>(builtMediaItem));

                var result = await this.Processor.GetResultAsync(embyInfo, this.MediaItemType, Enumerable.Empty<EmbyItemId>());

                result.IsRight.Should().BeTrue();
                result.IfRight(r => r.MediaItem.Should().Be(builtMediaItem));
            }

            [Test]
            public async Task IdentifiesItem()
            {
                var embyInfo = Data.EmbyInfo();
                var mediaItem = Data.MediaItem();

                this.MediaItemBuilder.Identify(Arg.Any<EmbyItemData>(), this.MediaItemType)
                    .Returns(Right<ProcessFailedResult, IMediaItem>(mediaItem));

                var result = await this.Processor.GetResultAsync(embyInfo, this.MediaItemType, Enumerable.Empty<EmbyItemId>());

                result.IsRight.Should().BeTrue();

                await this.MediaItemBuilder.Received(1)
                    .Identify(Arg.Is<EmbyItemData>(d => d.Identifier.Index == 1 &&
                                                             d.Identifier.ParentIndex == 2 &&
                                                             d.Identifier.Name == "name"), this.MediaItemType);
            }
        }
    }
}
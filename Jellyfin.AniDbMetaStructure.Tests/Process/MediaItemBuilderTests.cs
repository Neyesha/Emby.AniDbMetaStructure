using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jellyfin.AniDbMetaStructure.Configuration;
using Jellyfin.AniDbMetaStructure.Process;
using Jellyfin.AniDbMetaStructure.Process.Sources;
using Jellyfin.AniDbMetaStructure.SourceDataLoaders;
using Jellyfin.AniDbMetaStructure.Tests.TestHelpers;
using FluentAssertions;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NUnit.Framework;
using static LanguageExt.Prelude;

namespace Jellyfin.AniDbMetaStructure.Tests.Process
{
    [TestFixture]
    public class MediaItemBuilderTests
    {
        [SetUp]
        public virtual void Setup()
        {
            this.PluginConfiguration = Substitute.For<IPluginConfiguration>();

            this.Builder = new MediaItemBuilder(this.PluginConfiguration, null, new ConsoleLogger());
        }

        internal IPluginConfiguration PluginConfiguration;
        internal MediaItemBuilder Builder;

        internal static class Data
        {
            public static JellyfinItemData FileJellyfinItemData()
            {
                return new JellyfinItemData(MediaItemTypes.Series, new ItemIdentifier(1, 2, "name"), null, "en",
                    Enumerable.Empty<JellyfinItemId>());
            }

            public static JellyfinItemData LibraryJellyfinItemData()
            {
                return new JellyfinItemData(MediaItemTypes.Series, new ItemIdentifier(1, 2, "name"),
                    new Dictionary<string, int> { { "Key", 1 } }, "en", Enumerable.Empty<JellyfinItemId>());
            }

            public static ISourceDataLoader SourceDataLoader(IMediaItem mediaItem, ISourceData dependencySourceData,
                string sourceName)
            {
                var sourceDataLoader = Substitute.For<ISourceDataLoader>();
                var producedSourceData = SourceData(sourceName);

                sourceDataLoader.CanLoadFrom(dependencySourceData).Returns(true);
                sourceDataLoader.CanLoadFrom(Arg.Is<object>(o => o != dependencySourceData)).Returns(false);
                sourceDataLoader.LoadFrom(mediaItem, dependencySourceData)
                    .ReturnsForAnyArgs(Right<ProcessFailedResult, ISourceData>(producedSourceData));

                return sourceDataLoader;
            }

            public static ISourceData SourceData(string sourceName)
            {
                var sourceData = Substitute.For<ISourceData>();
                var source = Substitute.For<ISource>();
                source.Name.Returns(new SourceName(sourceName));

                sourceData.Source.Returns(source);

                return sourceData;
            }
        }

        public class Identify : MediaItemBuilderTests
        {
            [Test]
            public async Task FileData_UsesFileSourceLoader()
            {
                var data = Data.FileJellyfinItemData();
                var sourceData = Substitute.For<ISourceData>();
                var source = TestSources.AniDbSource;
                sourceData.Source.Returns(source);
                var JellyfinSourceDataLoader = Substitute.For<IJellyfinSourceDataLoader>();
                JellyfinSourceDataLoader.LoadFrom(data).Returns(Right<ProcessFailedResult, ISourceData>(sourceData));

                var fileStructureSource = Substitute.For<ISource>();
                fileStructureSource.GetJellyfinSourceDataLoader(MediaItemTypes.Series)
                    .Returns(Right<ProcessFailedResult, IJellyfinSourceDataLoader>(JellyfinSourceDataLoader));

                this.PluginConfiguration.FileStructureSource(MediaItemTypes.Series).Returns(fileStructureSource);

                var result = await this.Builder.Identify(data, MediaItemTypes.Series);

                result.IsRight.Should().BeTrue();
                fileStructureSource.Received(1).GetJellyfinSourceDataLoader(MediaItemTypes.Series);
                await JellyfinSourceDataLoader.Received(1).LoadFrom(data);
            }

            [Test]
            public async Task LibraryData_UsesLibrarySourceLoader()
            {
                var data = Data.LibraryJellyfinItemData();
                var sourceData = Substitute.For<ISourceData>();
                var source = TestSources.AniDbSource;
                sourceData.Source.Returns(source);
                var JellyfinSourceDataLoader = Substitute.For<IJellyfinSourceDataLoader>();
                JellyfinSourceDataLoader.LoadFrom(data).Returns(Right<ProcessFailedResult, ISourceData>(sourceData));

                var libraryStructureSource = Substitute.For<ISource>();
                libraryStructureSource.GetJellyfinSourceDataLoader(MediaItemTypes.Series)
                    .Returns(Right<ProcessFailedResult, IJellyfinSourceDataLoader>(JellyfinSourceDataLoader));

                this.PluginConfiguration.LibraryStructureSource(MediaItemTypes.Series).Returns(libraryStructureSource);

                var result = await this.Builder.Identify(data, MediaItemTypes.Series);

                result.IsRight.Should().BeTrue();
                libraryStructureSource.Received(1).GetJellyfinSourceDataLoader(MediaItemTypes.Series);
                await JellyfinSourceDataLoader.Received(1).LoadFrom(data);
            }
        }
    }

    [TestFixture]
    public class BuildMediaItem : MediaItemBuilderTests
    {
        [SetUp]
        public override void Setup()
        {
            this.initialSourceData = Data.SourceData("InitialSource");
            this.mediaItem = new MediaItem(Substitute.For<IJellyfinItemData>(), MediaItemTypes.Series,
                this.initialSourceData);

            this.sourceDataLoaders = new[]
            {
                Data.SourceDataLoader(this.mediaItem, this.initialSourceData, "SourceA"),
                Data.SourceDataLoader(this.mediaItem, this.initialSourceData, "SourceB"),
                Data.SourceDataLoader(this.mediaItem, this.initialSourceData, "SourceC")
            }.ToList();

            this.Builder = new MediaItemBuilder(this.PluginConfiguration, this.sourceDataLoaders, new ConsoleLogger());
        }

        private IList<ISourceDataLoader> sourceDataLoaders;
        private IMediaItem mediaItem;
        private ISourceData initialSourceData;

        [Test]
        public async Task CallsEveryLoaderThatCanLoadFromExistingData()
        {
            this.mediaItem = Substitute.For<IMediaItem>();

            this.sourceDataLoaders = new[]
            {
                Data.SourceDataLoader(this.mediaItem, this.initialSourceData, "SourceA"),
                Data.SourceDataLoader(this.mediaItem, this.initialSourceData, "SourceB"),
                Data.SourceDataLoader(this.mediaItem, this.initialSourceData, "SourceC")
            }.ToList();

            var existingLoader = Substitute.For<ISourceDataLoader>();
            var existingSourceData = Data.SourceData("ExistingSource");
            var newLoaders = this.sourceDataLoaders.ToList();

            this.sourceDataLoaders.Add(existingLoader);

            this.mediaItem.GetAllSourceData().Returns(Option<ISourceData>.Some(existingSourceData));
            this.mediaItem.AddData(Arg.Any<ISourceData>()).Returns(Right<ProcessFailedResult, IMediaItem>(this.mediaItem));

            existingLoader.CanLoadFrom(existingSourceData).Returns(false);
            newLoaders.Iter(l => l.CanLoadFrom(existingSourceData).Returns(true));

            this.Builder = new MediaItemBuilder(this.PluginConfiguration, this.sourceDataLoaders, new ConsoleLogger());

            await this.Builder.BuildMediaItem(this.mediaItem);

            newLoaders.Iter(s => s.Received(1).LoadFrom(this.mediaItem, existingSourceData));
            await existingLoader.DidNotReceive().LoadFrom(this.mediaItem, existingSourceData);
        }

        [Test]
        public async Task CombinesOutputFromAllLoaders()
        {
            var expected = new List<ISourceData>();

            this.sourceDataLoaders.Iter((i, l) =>
            {
                var loaderSourceData = Data.SourceData("LoaderSource" + i.ToString());

                expected.Add(loaderSourceData);

                l.ClearSubstitute();

                l.CanLoadFrom(this.initialSourceData).Returns(true);
                l.CanLoadFrom(Arg.Is<object>(o => o != this.initialSourceData)).Returns(false);

                l.LoadFrom(Arg.Any<IMediaItem>(), this.initialSourceData)
                    .Returns(Right<ProcessFailedResult, ISourceData>(loaderSourceData));
            });

            var builtMediaItem = await this.Builder.BuildMediaItem(this.mediaItem);

            builtMediaItem.IsRight.Should().BeTrue();
            builtMediaItem.IfRight(mi => this.sourceDataLoaders.Iter((index, l) =>
                mi.GetAllSourceData().ElementAt(index).Should().BeSameAs(expected[index])));
        }

        [Test]
        public async Task CombinesOutputFromLoadersThatDependOnOtherLoaders()
        {
            var dependentLoader = Substitute.For<ISourceDataLoader>();
            var dependentSourceData = Data.SourceData("DependentSource");

            var dependencySourceData = Data.SourceData("DependencySource");
            this.sourceDataLoaders.Last()
                .LoadFrom(Arg.Any<IMediaItem>(), this.initialSourceData)
                .Returns(Right<ProcessFailedResult, ISourceData>(dependencySourceData));

            dependentLoader.CanLoadFrom(dependencySourceData).Returns(true);
            dependentLoader.CanLoadFrom(Arg.Is<object>(o => o != dependencySourceData)).Returns(false);

            dependentLoader.LoadFrom(Arg.Any<IMediaItem>(), dependencySourceData)
                .Returns(x => Right<ProcessFailedResult, ISourceData>(dependentSourceData));

            this.sourceDataLoaders.Insert(0, dependentLoader);

            var builtMediaItem = await this.Builder.BuildMediaItem(this.mediaItem);

            builtMediaItem.IsRight.Should().BeTrue();
            builtMediaItem.ValueUnsafe().GetAllSourceData().Should().Contain(dependencySourceData);
        }
    }
}
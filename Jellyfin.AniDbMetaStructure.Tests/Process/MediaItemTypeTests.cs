using System;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.AniDbMetaStructure.AniDb.SeriesData;
using Jellyfin.AniDbMetaStructure.Configuration;
using Jellyfin.AniDbMetaStructure.Process;
using Jellyfin.AniDbMetaStructure.Process.Sources;
using Jellyfin.AniDbMetaStructure.PropertyMapping;
using Jellyfin.AniDbMetaStructure.Tests.TestHelpers;
using FluentAssertions;
using LanguageExt;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using NSubstitute;
using NUnit.Framework;
using Jellyfin.AniDbMetaStructure.Infrastructure;

namespace Jellyfin.AniDbMetaStructure.Tests.Process
{
    [TestFixture]
    internal class MediaItemTypeTests
    {
        [SetUp]
        public void Setup()
        {
            this.PropertyMappings = Substitute.For<IPropertyMappingCollection>();
            this.PropertyMappings.Apply(Arg.Any<IEnumerable<object>>(), Arg.Any<MetadataResult<Series>>(),
                    Arg.Any<Action<string>>())
                .Returns(x => x.Arg<MetadataResult<Series>>());
            this.PropertyMappings.GetEnumerator()
                .Returns(new IPropertyMapping[]
                {
                    new PropertyMapping<AniDbSeriesData, MetadataResult<Series>, string>("Name", s => s.Item.Name,
                        (s, t) => t.Item.Name = "Name", SourceNames.AniDb)
                }.AsEnumerable().GetEnumerator());

            this.PluginConfiguration = Substitute.For<IPluginConfiguration>();

            var embyItemData = Substitute.For<IEmbyItemData>();
            embyItemData.Language.Returns("en");

            this.Sources = new TestSources();
            var aniDbSourceData = new SourceData<AniDbSeriesData>(this.Sources.AniDb, 33, new ItemIdentifier(33, 1, "Name"),
                new AniDbSeriesData());

            this.MediaItem = Substitute.For<IMediaItem>();
            this.MediaItem.GetAllSourceData().Returns(new ISourceData[] { aniDbSourceData });
            this.MediaItem.GetDataFromSource(null).ReturnsForAnyArgs(aniDbSourceData);
            this.MediaItem.EmbyData.Returns(embyItemData);

            this.MediaItemType = new MediaItemType<Series>("Series", (c, l) => this.PropertyMappings);
        }

        internal IPluginConfiguration PluginConfiguration;
        internal IMediaItem MediaItem;
        internal MediaItemType<Series> MediaItemType;
        internal IPropertyMappingCollection PropertyMappings;
        internal TestSources Sources;

        [TestFixture]
        internal class CreateMetadataFoundResult : MediaItemTypeTests
        {
            [Test]
            public void AddsProviderIdsForNonAdditionalSources()
            {
                var source = Substitute.For<ISource>();
                source.Name.Returns(new SourceName("SourceName"));

                var additionalSource = source.ForAdditionalData();

                var sourceData = Substitute.For<ISourceData>();
                sourceData.Id.Returns(3);
                sourceData.Source.Returns(source);

                var additionalSourceData = Substitute.For<ISourceData>();
                additionalSourceData.Id.Returns(3);
                additionalSourceData.Source.Returns(additionalSource);

                this.MediaItem.GetAllSourceData().Returns(new[] { sourceData });

                this.PropertyMappings.Apply(Arg.Any<IEnumerable<object>>(), Arg.Any<MetadataResult<Series>>(),
                        Arg.Any<Action<string>>())
                    .Returns(m =>
                    {
                        var r = m.Arg<MetadataResult<Series>>();

                        r.Item.Name = "Name";

                        return r;
                    });

                var result = this.MediaItemType.CreateMetadataFoundResult(this.PluginConfiguration, this.MediaItem, new ConsoleLogger());

                result.IsRight.Should().BeTrue();
                result.IfRight(r =>
                {
                    r.EmbyMetadataResult.Item.ProviderIds.Should().ContainKey("SourceName");
                    r.EmbyMetadataResult.Item.ProviderIds.Should().ContainValue("3");
                });
            }

            [Test]
            public void AppliesPropertyMappings()
            {
                this.MediaItemType.CreateMetadataFoundResult(this.PluginConfiguration, this.MediaItem, new ConsoleLogger());

                this.PropertyMappings.Received(1)
                    .Apply(Arg.Any<IEnumerable<object>>(), Arg.Any<MetadataResult<Series>>(),
                        Arg.Any<Action<string>>());
            }

            [Test]
            public void AppliesPropertyMappingsForIdentifierOnlySourceData()
            {
                var identifierOnlySourceData = new IdentifierOnlySourceData(this.Sources.AniDb, 33, new ItemIdentifier(33, 1, "Name"), this.MediaItemType);

                this.MediaItem = Substitute.For<IMediaItem>();
                this.MediaItem.GetAllSourceData().Returns(new ISourceData[] { identifierOnlySourceData });

                this.MediaItemType.CreateMetadataFoundResult(this.PluginConfiguration, this.MediaItem, new ConsoleLogger());

                this.PropertyMappings.Received(1)
                    .Apply(Arg.Is<IEnumerable<object>>(e => e.Contains(identifierOnlySourceData)), Arg.Any<MetadataResult<Series>>(),
                        Arg.Any<Action<string>>());
            }

            [Test]
            public void NameIsMapped_ReturnsResultWithHasMetadataToTrue()
            {
                this.PropertyMappings.Apply(Arg.Any<IEnumerable<object>>(), Arg.Any<MetadataResult<Series>>(),
                        Arg.Any<Action<string>>())
                    .Returns(m =>
                    {
                        var r = m.Arg<MetadataResult<Series>>();

                        r.Item.Name = "Name";

                        return r;
                    });

                var result = this.MediaItemType.CreateMetadataFoundResult(this.PluginConfiguration, this.MediaItem, new ConsoleLogger());

                result.IsRight.Should().BeTrue();
                result.IfRight(r => r.EmbyMetadataResult.HasMetadata.Should().BeTrue());
            }

            [Test]
            [TestCase("")]
            [TestCase(null)]
            [TestCase("   ")]
            public void NameNotMapped_ReturnsFailure(string name)
            {
                this.PropertyMappings.GetEnumerator()
                .Returns(new IPropertyMapping[]
                {
                    new PropertyMapping<AniDbSeriesData, MetadataResult<Series>, string>("Name", s => s.Item.Name,
                        (s, t) => t.Item.Name = name, SourceNames.AniDb)
                }.AsEnumerable().GetEnumerator());

                var result = this.MediaItemType.CreateMetadataFoundResult(this.PluginConfiguration, this.MediaItem, new ConsoleLogger());

                result.IsLeft.Should().BeTrue();
                result.IfLeft(r => r.Reason.Should().Be("Property mapping failed for the Name property"));
            }

            [Test]
            public void NoLibrarySourceData_ReturnsFailure()
            {
                this.MediaItem.GetAllSourceData().Returns(new ISourceData[] { });
                this.MediaItem.GetDataFromSource(null).ReturnsForAnyArgs(Option<ISourceData>.None);

                var result = this.MediaItemType.CreateMetadataFoundResult(this.PluginConfiguration, this.MediaItem, new ConsoleLogger());

                result.IsLeft.Should().BeTrue();
                result.IfLeft(r => r.Reason.Should().Be("No data returned by library structure source"));
            }

            [Test]
            public void NoNameMappingForLibrarySourceData_ReturnsFailure()
            {
                this.PropertyMappings.GetEnumerator()
                    .Returns(Enumerable.Empty<IPropertyMapping>().GetEnumerator());

                var result = this.MediaItemType.CreateMetadataFoundResult(this.PluginConfiguration, this.MediaItem, new ConsoleLogger());

                result.IsLeft.Should().BeTrue();
                result.IfLeft(r => r.Reason.Should().Be("No value for Name property mapped from library source"));
            }

            [Test]
            public void RemovesIdsThatNoLongerExist()
            {
                var source = Substitute.For<ISource>();
                source.Name.Returns(new SourceName("SourceName"));

                var sourceData = Substitute.For<ISourceData>();
                sourceData.Source.Returns(source);

                this.MediaItem.GetAllSourceData().Returns(new[] { sourceData });

                this.PropertyMappings.Apply(Arg.Any<IEnumerable<object>>(), Arg.Any<MetadataResult<Series>>(),
                        Arg.Any<Action<string>>())
                    .Returns(m =>
                    {
                        var r = m.Arg<MetadataResult<Series>>();

                        r.Item.Name = "Name";
                        r.Item.ProviderIds = new Dictionary<string, string> { { "SourceName", "3" } };

                        return r;
                    });

                var result = this.MediaItemType.CreateMetadataFoundResult(this.PluginConfiguration, this.MediaItem, new ConsoleLogger());

                result.IsRight.Should().BeTrue();
                result.IfRight(r =>
                {
                    r.EmbyMetadataResult.Item.ProviderIds.Should().NotContainKey("SourceName");
                    r.EmbyMetadataResult.Item.ProviderIds.Should().NotContainValue("3");
                });
            }
        }
    }
}
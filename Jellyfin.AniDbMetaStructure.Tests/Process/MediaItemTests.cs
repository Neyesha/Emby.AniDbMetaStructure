using System;
using Jellyfin.AniDbMetaStructure.Process;
using Jellyfin.AniDbMetaStructure.Process.Sources;
using FluentAssertions;
using LanguageExt.UnsafeValueAccess;
using NSubstitute;
using NUnit.Framework;

namespace Jellyfin.AniDbMetaStructure.Tests.Process
{
    [TestFixture]
    public class MediaItemTests
    {
        [SetUp]
        public void Setup()
        {
            this.Source = Substitute.For<ISource>();
            this.Source.Name.Returns(new SourceName("Source"));
            this.SourceData = Substitute.For<ISourceData>();

            this.SourceData.Source.Returns(this.Source);

            this.Source2 = Substitute.For<ISource>();
            this.Source2.Name.Returns(new SourceName("Source2"));
            this.SourceData2 = Substitute.For<ISourceData>();

            this.SourceData2.Source.Returns(this.Source2);
        }

        internal ISource Source;
        internal ISourceData SourceData;
        internal ISource Source2;
        internal ISourceData SourceData2;

        [TestFixture]
        public class Constructor : MediaItemTests
        {
            [Test]
            public void AddsInitialData()
            {
                var mediaItem = new MediaItem(Substitute.For<IEmbyItemData>(), MediaItemTypes.Series, this.SourceData);

                mediaItem.GetDataFromSource(this.Source).ValueUnsafe().Should().Be(this.SourceData);
            }

            [Test]
            public void InitialisesItemType()
            {
                var mediaItem = new MediaItem(Substitute.For<IEmbyItemData>(), MediaItemTypes.Series, this.SourceData);

                mediaItem.ItemType.Should().Be(MediaItemTypes.Series);
            }

            [Test]
            public void NullData_ThrowsArgumentNullException()
            {
                Action action = () => new MediaItem(Substitute.For<IEmbyItemData>(), MediaItemTypes.Series, null);

                action.Should().Throw<ArgumentNullException>();
            }
        }

        [TestFixture]
        public class AddData : MediaItemTests
        {
            [Test]
            public void DoesNotModifyInstanceCalledOn()
            {
                var mediaItem = new MediaItem(Substitute.For<IEmbyItemData>(), MediaItemTypes.Series, this.SourceData);

                mediaItem.AddData(this.SourceData2);

                mediaItem.GetDataFromSource(this.Source).ValueUnsafe().Should().Be(this.SourceData);
                mediaItem.GetDataFromSource(this.Source2).IsNone.Should().BeTrue();
            }

            [Test]
            public void ExistingDataFromSource_ReturnsFailedResult()
            {
                var mediaItem = new MediaItem(Substitute.For<IEmbyItemData>(), MediaItemTypes.Series, this.SourceData);

                var result = mediaItem.AddData(this.SourceData);

                result.IsLeft.Should().BeTrue();
            }

            [Test]
            public void ReturnsMediaItemWithSourceDataAdded()
            {
                var mediaItem = new MediaItem(Substitute.For<IEmbyItemData>(), MediaItemTypes.Series, this.SourceData);

                var mediaItem2 = mediaItem.AddData(this.SourceData2);

                mediaItem2.IsRight.Should().BeTrue();
                mediaItem2.ValueUnsafe().GetDataFromSource(this.Source2).ValueUnsafe().Should().Be(this.SourceData2);
            }
        }

        [TestFixture]
        public class GetDataFromSource : MediaItemTests
        {
            [Test]
            public void NoDataWithMatchingSource_ReturnsNone()
            {
                var mediaItem = new MediaItem(Substitute.For<IEmbyItemData>(), MediaItemTypes.Series, this.SourceData);

                mediaItem.GetDataFromSource(this.Source2).IsNone.Should().BeTrue();
            }
        }
    }
}
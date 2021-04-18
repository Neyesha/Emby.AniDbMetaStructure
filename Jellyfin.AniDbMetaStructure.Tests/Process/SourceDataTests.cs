using Jellyfin.AniDbMetaStructure.Process;
using FluentAssertions;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using NSubstitute;
using NUnit.Framework;

namespace Jellyfin.AniDbMetaStructure.Tests.Process
{
    [TestFixture]
    public class SourceDataTests
    {
        [TestFixture]
        public class GetData : SourceDataTests
        {
            [SetUp]
            public void Setup()
            {
                this.data = new ChildData();
                this.sourceData =
                    new SourceData<ChildData>(Substitute.For<ISource>(), Option<int>.None,
                        Substitute.For<IItemIdentifier>(), this.data);
            }

            private ISourceData sourceData;
            private ChildData data;

            private class ParentData
            {
            }

            private class ChildData : ParentData
            {
            }

            [Test]
            public void DataCastableToType_ReturnsDataAsType()
            {
                this.sourceData.GetData<ParentData>().ValueUnsafe().Should().Be(this.data);
            }

            [Test]
            public void DataDoesNotMatchType_ReturnsNone()
            {
                this.sourceData.GetData<ISourceData>().IsNone.Should().BeTrue();
            }

            [Test]
            public void DataMatchesType_ReturnsData()
            {
                this.sourceData.GetData<ChildData>().ValueUnsafe().Should().Be(this.data);
            }
        }

        [TestFixture]
        public class Constructor : SourceDataTests
        {
            [Test]
            public void SetsIdentifier()
            {
                var identifier = Substitute.For<IItemIdentifier>();

                var sourceData = new SourceData<object>(Substitute.For<ISource>(), Option<int>.None, identifier,
                    new object());

                sourceData.Identifier.Should().Be(identifier);
            }

            [Test]
            public void SetsSource()
            {
                var source = Substitute.For<ISource>();

                var sourceData = new SourceData<object>(source, Option<int>.None, Substitute.For<IItemIdentifier>(),
                    new object());

                sourceData.Source.Should().Be(source);
            }
        }
    }
}
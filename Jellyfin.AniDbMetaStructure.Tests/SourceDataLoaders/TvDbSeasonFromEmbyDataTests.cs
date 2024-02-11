using System.Threading.Tasks;
using Jellyfin.AniDbMetaStructure.Process;
using Jellyfin.AniDbMetaStructure.Process.Sources;
using Jellyfin.AniDbMetaStructure.SourceDataLoaders;
using FluentAssertions;
using LanguageExt;
using NSubstitute;
using NUnit.Framework;

namespace Jellyfin.AniDbMetaStructure.Tests.SourceDataLoaders
{
    [TestFixture]
    public class TvDbSeasonFromJellyfinDataTests
    {
        [SetUp]
        public void Setup()
        {
            var tvDbSource = Substitute.For<ITvDbSource>();

            this.sources = Substitute.For<ISources>();
            this.sources.TvDb.Returns(tvDbSource);

            this.JellyfinItemData = Substitute.For<IJellyfinItemData>();
            this.JellyfinItemData.Language.Returns("en");
        }

        private ISources sources;
        private IJellyfinItemData JellyfinItemData;

        [Test]
        public void CanLoadFrom_CorrectItemType_IsTrue()
        {
            var loader = new TvDbSeasonFromJellyfinData(this.sources);

            loader.CanLoadFrom(MediaItemTypes.Season).Should().BeTrue();
        }

        [Test]
        public void CanLoadFrom_Null_IsFalse()
        {
            var loader = new TvDbSeasonFromJellyfinData(this.sources);

            loader.CanLoadFrom(null).Should().BeFalse();
        }

        [Test]
        public void CanLoadFrom_WrongItemType_IsFalse()
        {
            var loader = new TvDbSeasonFromJellyfinData(this.sources);

            loader.CanLoadFrom(MediaItemTypes.Series).Should().BeFalse();
        }

        [Test]
        public async Task LoadFrom_ReturnsIdentifierOnlySourceData()
        {
            this.JellyfinItemData.Identifier.Returns(new ItemIdentifier(67, Option<int>.None, "Name"));

            var loader = new TvDbSeasonFromJellyfinData(this.sources);

            var result = await loader.LoadFrom(this.JellyfinItemData);

            result.IsRight.Should().BeTrue();
            result.IfRight(r => r.Data.Should().Be(r));
            result.IfRight(r => r.Source.Should().Be(this.sources.TvDb));
            result.IfRight(sd =>
                sd.Identifier.Should().BeEquivalentTo(new ItemIdentifier(67, Option<int>.None, "Name")));
        }
    }
}
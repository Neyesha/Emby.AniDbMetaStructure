using Jellyfin.AniDbMetaStructure.AniList;
using Jellyfin.AniDbMetaStructure.AniList.Data;
using Jellyfin.AniDbMetaStructure.Configuration;
using Jellyfin.AniDbMetaStructure.Tests.TestHelpers;
using FluentAssertions;
using NUnit.Framework;

namespace Jellyfin.AniDbMetaStructure.Tests.AniList
{
    [TestFixture]
    public class AniListNameSelectorTests
    {
        [SetUp]
        public void Setup()
        {
            this.nameSelector = new AniListNameSelector(new ConsoleLogger());
        }

        private AniListNameSelector nameSelector;

        [Test]
        public void SelectName_JapanesePreferred_ReturnsNative()
        {
            var nameData = new AniListPersonNameData("First", "Last", "Native");

            var result = this.nameSelector.SelectName(nameData, TitleType.Japanese, "en");

            result.IsSome.Should().BeTrue();
            result.IfSome(r => r.Should().Be("Native"));
        }

        [Test]
        public void SelectName_NoNameData_ReturnsNone()
        {
            var result = this.nameSelector.SelectName(null, TitleType.Japanese, "en");

            result.IsSome.Should().BeFalse();
        }

        [Test]
        [TestCase(TitleType.JapaneseRomaji)]
        [TestCase(TitleType.Localized)]
        public void SelectName_NonJapanesePreferred_ReturnsFirstNameFollowedByLastName(TitleType preferredType)
        {
            var nameData = new AniListPersonNameData("First", "Last", "Native");

            var result = this.nameSelector.SelectName(nameData, preferredType, "en");

            result.IsSome.Should().BeTrue();
            result.IfSome(r => r.Should().Be("First Last"));
        }

        [Test]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase(null)]
        public void SelectTitle_JapanesePreferred_NoNative_ReturnsRomaji(string emptyValue)
        {
            var titleData = new AniListTitleData("English", "Romaji", emptyValue);

            var result = this.nameSelector.SelectTitle(titleData, TitleType.Japanese, "en");

            result.IsSome.Should().BeTrue();
            result.IfSome(r => r.Should().Be("Romaji"));
        }

        [Test]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase(null)]
        public void SelectTitle_JapanesePreferred_NoNativeOrRomaji_ReturnsEnglish(string emptyValue)
        {
            var titleData = new AniListTitleData("English", emptyValue, emptyValue);

            var result = this.nameSelector.SelectTitle(titleData, TitleType.Japanese, "en");

            result.IsSome.Should().BeTrue();
            result.IfSome(r => r.Should().Be("English"));
        }

        [Test]
        public void SelectTitle_JapanesePreferred_ReturnsNative()
        {
            var titleData = new AniListTitleData("English", "Romaji", "Native");

            var result = this.nameSelector.SelectTitle(titleData, TitleType.Japanese, "en");

            result.IsSome.Should().BeTrue();
            result.IfSome(r => r.Should().Be("Native"));
        }

        [Test]
        public void SelectTitle_LocalisedPreferred_JapaneseMetadataLanguage_ReturnsNative()
        {
            var titleData = new AniListTitleData("English", "Romaji", "Native");

            var result = this.nameSelector.SelectTitle(titleData, TitleType.Localized, "ja");

            result.IsSome.Should().BeTrue();
            result.IfSome(r => r.Should().Be("Native"));
        }

        [Test]
        public void SelectTitle_LocalisedPreferred_NonJapaneseMetadataLanguage_ReturnsEnglish()
        {
            var titleData = new AniListTitleData("English", "Romaji", "Native");

            var result = this.nameSelector.SelectTitle(titleData, TitleType.Localized, "en");

            result.IsSome.Should().BeTrue();
            result.IfSome(r => r.Should().Be("English"));
        }

        [Test]
        public void SelectTitle_NoNameData_ReturnsNone()
        {
            var result = this.nameSelector.SelectTitle(null, TitleType.Japanese, "en");

            result.IsSome.Should().BeFalse();
        }

        [Test]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase(null)]
        public void SelectTitle_RomajiPreferred_NoRomaji_ReturnsEnglish(string emptyValue)
        {
            var titleData = new AniListTitleData("English", emptyValue, "Native");

            var result = this.nameSelector.SelectTitle(titleData, TitleType.JapaneseRomaji, "en");

            result.IsSome.Should().BeTrue();
            result.IfSome(r => r.Should().Be("English"));
        }

        [Test]
        public void SelectTitle_RomajiPreferred_ReturnsRomaji()
        {
            var titleData = new AniListTitleData("English", "Romaji", "Native");

            var result = this.nameSelector.SelectTitle(titleData, TitleType.JapaneseRomaji, "en");

            result.IsSome.Should().BeTrue();
            result.IfSome(r => r.Should().Be("Romaji"));
        }
    }
}
using Jellyfin.AniDbMetaStructure.AniDb.Titles;
using FluentAssertions;
using NUnit.Framework;

namespace Jellyfin.AniDbMetaStructure.Tests
{
    [TestFixture]
    public class TitleNormaliserTests
    {
        [Test]
        [TestCase("A Goddess Comes to Japan (Part 1: The Suffering)",
            "A Goddess Comes to Japan (Part 1 - The Suffering)")]
        [TestCase("A Goddess Comes to Japan (Part 1: The Suffering)",
            "07 - A Goddess Comes to Japan (Part 1 - The Suffering)")]
        [TestCase("The Man from the South / A Fruitless Lunchtime ",
            "The Man from the South A Fruitless Lunchtime")]
        [TestCase("The Man from the South / A Fruitless Lunchtime ",
            "01 - The Man from the South A Fruitless Lunchtime")]
        public void GetNormalisedTitle_NormalisesSimilarTitles(string titleA, string titleB)
        {
            var titleNormaliser = new TitleNormaliser();

            var normalisedA = titleNormaliser.GetNormalisedTitle(titleA);
            var normalisedB = titleNormaliser.GetNormalisedTitle(titleB);

            normalisedA.Should().Be(normalisedB);
        }

        [Test]
        public void GetNormalisedTitle_NullTitle_ReturnsFalse()
        {
            var titleNormaliser = new TitleNormaliser();

            var normalised = titleNormaliser.GetNormalisedTitle(null);

            normalised.Should().BeNull();
        }
    }
}
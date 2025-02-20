﻿using System.Collections.Generic;
using Jellyfin.AniDbMetaStructure.AniDb;
using Jellyfin.AniDbMetaStructure.AniDb.SeriesData;
using Jellyfin.AniDbMetaStructure.AniDb.Titles;
using FluentAssertions;
using LanguageExt.UnsafeValueAccess;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace Jellyfin.AniDbMetaStructure.Tests
{
    [TestFixture]
    public class SeriesTitleCacheTests
    {
        [Test]
        [TestCase(@"/")]
        [TestCase(@",")]
        [TestCase(@".")]
        [TestCase(@":")]
        [TestCase(@";")]
        [TestCase(@"\")]
        [TestCase(@"(")]
        [TestCase(@")")]
        [TestCase(@"{")]
        [TestCase(@"}")]
        [TestCase(@"[")]
        [TestCase(@"]")]
        [TestCase(@"+")]
        [TestCase(@"-")]
        [TestCase(@"_")]
        [TestCase(@"=")]
        [TestCase(@"–")]
        [TestCase(@"*")]
        [TestCase(@"""")]
        [TestCase(@"'")]
        [TestCase(@"!")]
        [TestCase(@"`")]
        [TestCase(@"?")]
        public void FindSeriesByTitle_ComparableTitleMatch_ReturnsSeries(string replacedCharacter)
        {
            var dataCache = Substitute.For<IAniDbDataCache>();
            var logger = Substitute.For<ILogger>();

            dataCache.TitleList.Returns(new List<TitleListItemData>
            {
                new TitleListItemData
                {
                    AniDbId = 123,
                    Titles = new[]
                    {
                        new ItemTitleData
                        {
                            Title = "Test - ComparableMatch"
                        }
                    }
                }
            });

            var seriesTitleCache = new SeriesTitleCache(dataCache, new TitleNormaliser(), logger);

            var foundTitle = seriesTitleCache.FindSeriesByTitle($"Test{replacedCharacter} ComparableMatch");

            foundTitle.IsSome.Should().BeTrue();
            foundTitle.ValueUnsafe().AniDbId.Should().Be(123);
        }

        [Test]
        public void FindSeriesByTitle_YearSuffix_ReturnsCorrectSeries()
        {
            var dataCache = Substitute.For<IAniDbDataCache>();
            var logger = Substitute.For<ILogger>();

            dataCache.TitleList.Returns(new List<TitleListItemData>
            {
                new TitleListItemData
                {
                    AniDbId = 123,
                    Titles = new[]
                    {
                        new ItemTitleData
                        {
                            Title = "Bakuman."
                        }
                    }
                },
                new TitleListItemData
                {
                    AniDbId = 456,
                    Titles = new[]
                    {
                        new ItemTitleData
                        {
                            Title = "Bakuman. (2012)"
                        }
                    }
                }
            });

            var seriesTitleCache = new SeriesTitleCache(dataCache, new TitleNormaliser(), logger);

            var foundTitle = seriesTitleCache.FindSeriesByTitle("Bakuman (2012)");

            foundTitle.IsSome.Should().BeTrue();
            foundTitle.ValueUnsafe().AniDbId.Should().Be(456);
        }
    }
}
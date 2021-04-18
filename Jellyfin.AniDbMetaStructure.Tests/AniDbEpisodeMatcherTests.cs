using Jellyfin.AniDbMetaStructure.AniDb.SeriesData;
using Jellyfin.AniDbMetaStructure.AniDb.Titles;
using Jellyfin.AniDbMetaStructure.Providers.AniDb;
using FluentAssertions;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace Jellyfin.AniDbMetaStructure.Tests
{
    [TestFixture]
    public class AniDbEpisodeMatcherTests
    {
        [SetUp]
        public void Setup()
        {
            this.logger = Substitute.For<ILogger>();
            this.titleNormaliser = new TitleNormaliser();
        }

        private ILogger logger;
        private ITitleNormaliser titleNormaliser;

        [Test]
        public void FindEpisode_NoSeasonIndexProvided_MatchesOnTitle()
        {
            var episodes = new[]
            {
                new AniDbEpisodeData
                {
                    Id = 122,
                    RawEpisodeNumber = new EpisodeNumberData
                    {
                        RawNumber = "88",
                        RawType = 1
                    },
                    Titles = new[]
                    {
                        new EpisodeTitleData
                        {
                            Language = "en",
                            Title = "OtherEpisode",
                            Type = "Official"
                        }
                    }
                },
                new AniDbEpisodeData
                {
                    Id = 442,
                    RawEpisodeNumber = new EpisodeNumberData
                    {
                        RawNumber = "55",
                        RawType = 1
                    },
                    Titles = new[]
                    {
                        new EpisodeTitleData
                        {
                            Language = "en",
                            Title = "EpisodeTitle",
                            Type = "Official"
                        }
                    }
                }
            };

            var episodeMatcher = new AniDbEpisodeMatcher(this.titleNormaliser, this.logger);

            var foundEpisode =
                episodeMatcher.FindEpisode(episodes, Option<int>.None, 3, "EpisodeTitle");

            foundEpisode.ValueUnsafe().Should().Be(episodes[1]);
        }

        [Test]
        public void FindEpisode_NoTitleMatch_ReturnsNone()
        {
            var episodes = new[]
            {
                new AniDbEpisodeData
                {
                    Id = 122,
                    RawEpisodeNumber = new EpisodeNumberData
                    {
                        RawNumber = "88",
                        RawType = 1
                    },
                    Titles = new[]
                    {
                        new EpisodeTitleData
                        {
                            Language = "en",
                            Title = "OtherEpisode",
                            Type = "Official"
                        }
                    }
                },
                new AniDbEpisodeData
                {
                    Id = 442,
                    RawEpisodeNumber = new EpisodeNumberData
                    {
                        RawNumber = "55",
                        RawType = 1
                    },
                    Titles = new[]
                    {
                        new EpisodeTitleData
                        {
                            Language = "en",
                            Title = "EpisodeTitle",
                            Type = "Official"
                        }
                    }
                }
            };

            var episodeMatcher = new AniDbEpisodeMatcher(this.titleNormaliser, this.logger);

            var foundEpisode = episodeMatcher.FindEpisode(episodes, Option<int>.None, 3, "Title");

            foundEpisode.IsSome.Should().BeFalse();
        }

        [Test]
        public void FindEpisode_SeasonAndEpisodeIndexesProvided_MatchesOnIndexes()
        {
            var episodes = new[]
            {
                new AniDbEpisodeData
                {
                    Id = 122,
                    RawEpisodeNumber = new EpisodeNumberData
                    {
                        RawNumber = "88",
                        RawType = 1
                    },
                    Titles = new[]
                    {
                        new EpisodeTitleData
                        {
                            Language = "en",
                            Title = "OtherEpisode",
                            Type = "Official"
                        }
                    }
                },
                new AniDbEpisodeData
                {
                    Id = 442,
                    RawEpisodeNumber = new EpisodeNumberData
                    {
                        RawNumber = "55",
                        RawType = 1
                    },
                    Titles = new[]
                    {
                        new EpisodeTitleData
                        {
                            Language = "en",
                            Title = "EpisodeTitle",
                            Type = "Official"
                        }
                    }
                }
            };

            var episodeMatcher = new AniDbEpisodeMatcher(this.titleNormaliser, this.logger);

            var foundEpisode = episodeMatcher.FindEpisode(episodes, 1, 55, Option<string>.None);

            foundEpisode.ValueUnsafe().Should().Be(episodes[1]);
        }
    }
}
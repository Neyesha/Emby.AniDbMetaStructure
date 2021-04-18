using Emby.AniDbMetaStructure.AniDb.SeriesData;
using Emby.AniDbMetaStructure.AniDb.Titles;
using Emby.AniDbMetaStructure.Providers.AniDb;
using FluentAssertions;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using MediaBrowser.Model.Logging;
using NSubstitute;
using NUnit.Framework;

namespace Emby.AniDbMetaStructure.Tests
{
    [TestFixture]
    public class AniDbEpisodeMatcherTests
    {
        [SetUp]
        public void Setup()
        {
            this.logManager = Substitute.For<ILogManager>();
            this.titleNormaliser = new TitleNormaliser();
        }

        private ILogManager logManager;
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

            var episodeMatcher = new AniDbEpisodeMatcher(this.titleNormaliser, this.logManager);

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

            var episodeMatcher = new AniDbEpisodeMatcher(this.titleNormaliser, this.logManager);

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

            var episodeMatcher = new AniDbEpisodeMatcher(this.titleNormaliser, this.logManager);

            var foundEpisode = episodeMatcher.FindEpisode(episodes, 1, 55, Option<string>.None);

            foundEpisode.ValueUnsafe().Should().Be(episodes[1]);
        }
    }
}
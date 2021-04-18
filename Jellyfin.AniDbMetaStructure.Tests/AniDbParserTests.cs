using System;
using System.Linq;
using Emby.AniDbMetaStructure.AniDb;
using Emby.AniDbMetaStructure.AniDb.Seiyuu;
using Emby.AniDbMetaStructure.AniDb.SeriesData;
using Emby.AniDbMetaStructure.Tests.TestData;
using FluentAssertions;
using MediaBrowser.Model.Entities;
using NUnit.Framework;

namespace Emby.AniDbMetaStructure.Tests
{
    [TestFixture]
    public class AniDbParserTests
    {
        [Test]
        public void FormatDescription_NoDescription_DoesNotThrow()
        {
            var series = new AniDbSeriesData().WithStandardData();

            series.Description = null;

            var aniDbParser = new AniDbParser();

            Action action = () => aniDbParser.FormatDescription(series.Description);

            action.Should().NotThrow();
        }

        [Test]
        public void GetGenres_AddAnimeGenreIsFalse_DoesNotAddAnimeGenre()
        {
            var series = new AniDbSeriesData().WithStandardData();
            series.Tags = new TagData[0];

            var aniDbParser = new AniDbParser();

            var genres = aniDbParser.GetGenres(series, 2, false);

            genres.Should().BeEmpty();
        }

        [Test]
        public void GetGenres_AddAnimeGenreIsTrue_AddsAnimeGenre()
        {
            var series = new AniDbSeriesData().WithStandardData();
            series.Tags = new TagData[0];

            var aniDbParser = new AniDbParser();

            var genres = aniDbParser.GetGenres(series, 2, true);

            genres.Should().BeEquivalentTo("Anime");
        }

        [Test]
        public void GetGenres_HasTags_ReturnsTagNames()
        {
            var series = new AniDbSeriesData().WithStandardData();

            series.Tags = new[]
            {
                new TagData
                {
                    Id = 55,
                    Weight = 400,
                    Name = "Tag1"
                },
                new TagData
                {
                    Id = 46,
                    Weight = 400,
                    Name = "Tag2"
                }
            };

            var aniDbParser = new AniDbParser();

            var genres = aniDbParser.GetGenres(series, 2, false);

            genres.Should().BeEquivalentTo("Tag1", "Tag2");
        }

        [Test]
        public void GetGenres_HasTags_ReturnsTagNamesProperCased()
        {
            var series = new AniDbSeriesData().WithStandardData();

            series.Tags = new[]
            {
                new TagData
                {
                    Id = 55,
                    Weight = 400,
                    Name = "tag name A"
                },
                new TagData
                {
                    Id = 46,
                    Weight = 400,
                    Name = "aNotheR tag name"
                }
            };

            var aniDbParser = new AniDbParser();

            var genres = aniDbParser.GetGenres(series, 2, false);

            genres.Should().BeEquivalentTo("Tag Name A", "Another Tag Name");
        }

        [Test]
        [TestCase(6)]
        [TestCase(22)]
        [TestCase(23)]
        [TestCase(60)]
        [TestCase(128)]
        [TestCase(129)]
        [TestCase(185)]
        [TestCase(216)]
        [TestCase(242)]
        [TestCase(255)]
        [TestCase(268)]
        [TestCase(269)]
        [TestCase(289)]
        public void GetGenres_IgnoresSpecificTags(int id)
        {
            var series = new AniDbSeriesData().WithStandardData();

            series.Tags = new[]
            {
                new TagData
                {
                    Id = id,
                    Weight = 600,
                    Name = "Tag1"
                }
            };

            var aniDbParser = new AniDbParser();

            var genres = aniDbParser.GetGenres(series, 1, false);

            genres.Should().BeEmpty();
        }

        [Test]
        public void GetGenres_MoreTagsThanMaxGenres_TakesHighestWeighted()
        {
            var series = new AniDbSeriesData().WithStandardData();

            series.Tags = new[]
            {
                new TagData
                {
                    Id = 55,
                    Weight = 400,
                    Name = "Tag1"
                },
                new TagData
                {
                    Id = 46,
                    Weight = 500,
                    Name = "Tag2"
                }
            };

            var aniDbParser = new AniDbParser();

            var genres = aniDbParser.GetGenres(series, 1, false);

            genres.Should().BeEquivalentTo("Tag2");
        }

        [Test]
        public void GetGenres_NoTags_ReturnsEmpty()
        {
            var series = new AniDbSeriesData().WithoutTags();

            var aniDbParser = new AniDbParser();

            var genres = aniDbParser.GetGenres(series, 1, false);

            genres.Should().BeNullOrEmpty();
        }

        [Test]
        public void GetGenres_TagWeightUnder400_IgnoresTags()
        {
            var series = new AniDbSeriesData().WithStandardData();

            series.Tags = new[]
            {
                new TagData
                {
                    Id = 55,
                    Weight = 100,
                    Name = "Tag1"
                },
                new TagData
                {
                    Id = 46,
                    Weight = 399,
                    Name = "Tag2"
                }
            };

            var aniDbParser = new AniDbParser();

            var genres = aniDbParser.GetGenres(series, 2, false);

            genres.Should().BeEmpty();
        }

        [Test]
        public void GetGenres_TooManyTags_IncludesAnimeGenre()
        {
            var series = new AniDbSeriesData().WithStandardData();

            series.Tags = new[]
            {
                new TagData
                {
                    Id = 55,
                    Weight = 500,
                    Name = "Tag1"
                },
                new TagData
                {
                    Id = 46,
                    Weight = 400,
                    Name = "Tag2"
                }
            };

            var aniDbParser = new AniDbParser();

            var genres = aniDbParser.GetGenres(series, 2, true);

            genres.Should().BeEquivalentTo("Anime", "Tag1");
        }

        [Test]
        public void GetPeople_CharactersWithoutSeiyuu_ReturnsEmpty()
        {
            var series = new AniDbSeriesData
            {
                Characters = new[]
                {
                    new CharacterData()
                }
            };

            var aniDbParser = new AniDbParser();

            aniDbParser.GetPeople(series).Should().BeEmpty();
        }

        [Test]
        public void GetPeople_DirectionType_MapsTypeToDirector()
        {
            var series = new AniDbSeriesData
            {
                Creators = new[]
                {
                    new CreatorData
                    {
                        Name = "Reverse Name",
                        Type = "Direction"
                    }
                }
            };

            var aniDbParser = new AniDbParser();

            var person = aniDbParser.GetPeople(series).Single();

            person.Type.Should().Be(PersonType.Director);
        }

        [Test]
        public void GetPeople_HasCharacters_ReturnsPersonInfo()
        {
            var series = new AniDbSeriesData
            {
                Characters = new[]
                {
                    new CharacterData
                    {
                        Name = "CharacterName",
                        Seiyuu = new SeiyuuData
                        {
                            PictureFileName = "Picture.jpg",
                            Name = "Reverse Name"
                        }
                    }
                }
            };

            var aniDbParser = new AniDbParser();

            var person = aniDbParser.GetPeople(series).Single();

            person.ImageUrl.Should().Be("http://img7.anidb.net/pics/anime/Picture.jpg");
            person.Type.Should().Be(PersonType.Actor);
            person.Role.Should().Be("CharacterName");
        }

        [Test]
        public void GetPeople_HasCharacters_ReturnsReversedNames()
        {
            var series = new AniDbSeriesData
            {
                Characters = new[]
                {
                    new CharacterData
                    {
                        Seiyuu = new SeiyuuData
                        {
                            Name = "Reverse Name"
                        }
                    }
                }
            };

            var aniDbParser = new AniDbParser();

            aniDbParser.GetPeople(series).Single().Name.Should().Be("Name Reverse");
        }

        [Test]
        public void GetPeople_HasCreators_ReturnsPersonInfo()
        {
            var series = new AniDbSeriesData
            {
                Creators = new[]
                {
                    new CreatorData
                    {
                        Name = "Reverse Name",
                        Type = "Direction"
                    }
                }
            };

            var aniDbParser = new AniDbParser();

            var person = aniDbParser.GetPeople(series).Single();

            person.Type.Should().Be(PersonType.Director);
        }

        [Test]
        public void GetPeople_HasCreators_ReturnsReversedNames()
        {
            var series = new AniDbSeriesData
            {
                Creators = new[]
                {
                    new CreatorData
                    {
                        Name = "Reverse Name",
                        Type = "Music"
                    }
                }
            };

            var aniDbParser = new AniDbParser();

            aniDbParser.GetPeople(series).Single().Name.Should().Be("Name Reverse");
        }

        [Test]
        public void GetPeople_HasCreatorsAndCharacters_ReturnsBoth()
        {
            var series = new AniDbSeriesData
            {
                Characters = new[]
                {
                    new CharacterData
                    {
                        Name = "CharacterName1",
                        Seiyuu = new SeiyuuData
                        {
                            PictureFileName = "Picture1.jpg",
                            Name = "NameA"
                        }
                    },
                    new CharacterData
                    {
                        Name = "CharacterName2",
                        Seiyuu = new SeiyuuData
                        {
                            PictureFileName = "Picture2.jpg",
                            Name = "NameB"
                        }
                    }
                },
                Creators = new[]
                {
                    new CreatorData
                    {
                        Name = "Name1",
                        Type = "Music"
                    },
                    new CreatorData
                    {
                        Name = "Name2",
                        Type = "Direction"
                    }
                }
            };

            var aniDbParser = new AniDbParser();

            var people = aniDbParser.GetPeople(series);

            people.Select(p => p.Name).Should().BeEquivalentTo("NameA", "NameB", "Name1", "Name2");
        }

        [Test]
        public void GetPeople_MusicType_MapsTypeToComposer()
        {
            var series = new AniDbSeriesData
            {
                Creators = new[]
                {
                    new CreatorData
                    {
                        Name = "Reverse Name",
                        Type = "Music"
                    }
                }
            };

            var aniDbParser = new AniDbParser();

            var person = aniDbParser.GetPeople(series).Single();

            person.Type.Should().Be(PersonType.Composer);
        }

        [Test]
        public void GetPeople_NullCharacters_ReturnsEmpty()
        {
            var series = new AniDbSeriesData
            {
                Creators = new CreatorData[0]
            };

            var aniDbParser = new AniDbParser();

            aniDbParser.GetPeople(series).Should().BeEmpty();
        }

        [Test]
        public void GetPeople_NullCreators_ReturnsEmpty()
        {
            var series = new AniDbSeriesData
            {
                Characters = new CharacterData[0]
            };

            var aniDbParser = new AniDbParser();

            aniDbParser.GetPeople(series).Should().BeEmpty();
        }

        [Test]
        public void GetTags_ConfiguredForExtraGenresToTags_ReturnsExcessGenres()
        {
            var series = new AniDbSeriesData().WithStandardData();

            series.Tags = new[]
            {
                new TagData
                {
                    Id = 55,
                    Weight = 400,
                    Name = "Tag1"
                },
                new TagData
                {
                    Id = 46,
                    Weight = 500,
                    Name = "Tag2"
                }
            };

            var aniDbParser = new AniDbParser();

            var tags = aniDbParser.GetTags(series, 2, true);

            tags.Should().BeEquivalentTo("Tag1");
        }

        [Test]
        [TestCase(6)]
        [TestCase(22)]
        [TestCase(23)]
        [TestCase(60)]
        [TestCase(128)]
        [TestCase(129)]
        [TestCase(185)]
        [TestCase(216)]
        [TestCase(242)]
        [TestCase(255)]
        [TestCase(268)]
        [TestCase(269)]
        [TestCase(289)]
        public void GetTags_IgnoresSpecificTags(int id)
        {
            var series = new AniDbSeriesData().WithStandardData();

            series.Tags = new[]
            {
                new TagData
                {
                    Id = id,
                    Weight = 600,
                    Name = "Tag1"
                }
            };

            var aniDbParser = new AniDbParser();

            var tags = aniDbParser.GetTags(series, 0, false);

            tags.Should().BeEmpty();
        }
    }
}
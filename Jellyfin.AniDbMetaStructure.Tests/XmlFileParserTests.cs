using System;
using System.IO;
using Jellyfin.AniDbMetaStructure.AniDb.Seiyuu;
using Jellyfin.AniDbMetaStructure.AniDb.SeriesData;
using Jellyfin.AniDbMetaStructure.AniDb.Titles;
using Jellyfin.AniDbMetaStructure.Files;
using Jellyfin.AniDbMetaStructure.Tests.TestHelpers;
using FluentAssertions;
using NUnit.Framework;

namespace Jellyfin.AniDbMetaStructure.Tests
{
    [TestFixture]
    public class XmlFileParserTests
    {
        [Test]
        public void ParseSeriesFile_ValidXml_ReturnsDeserialised()
        {
            var fileContent = File.ReadAllText(TestContext.CurrentContext.TestDirectory + @"\TestData\anidb\series.xml");

            var xmlFileParser = new XmlSerialiser(new ConsoleLogger());

            var series = xmlFileParser.Deserialise<AniDbSeriesData>(fileContent);

            series.Id.Should().Be(1);
            series.Restricted.Should().BeFalse();

            series.EpisodeCount.Should().Be(13);
            series.StartDate.Should().Be(new DateTime(1999, 1, 3));
            series.EndDate.Should().Be(new DateTime(1999, 3, 28));

            series.Titles.Should().BeEquivalentTo(new[]
            {
                new ItemTitleData
                {
                    Language = "x-jat",
                    Type = "main",
                    Title = "Seikai no Monshou"
                },
                new ItemTitleData
                {
                    Language = "cs",
                    Type = "synonym",
                    Title = "Hvězdný erb"
                },
                new ItemTitleData
                {
                    Language = "zh-Hans",
                    Type = "synonym",
                    Title = "星界之纹章"
                },
                new ItemTitleData
                {
                    Language = "en",
                    Type = "short",
                    Title = "CotS"
                },
                new ItemTitleData
                {
                    Language = "x-jat",
                    Type = "short",
                    Title = "SnM"
                },
                new ItemTitleData
                {
                    Language = "ja",
                    Type = "official",
                    Title = "星界の紋章"
                },
                new ItemTitleData
                {
                    Language = "en",
                    Type = "official",
                    Title = "Crest of the Stars"
                },
                new ItemTitleData
                {
                    Language = "fr",
                    Type = "official",
                    Title = "Crest of the Stars"
                },
                new ItemTitleData
                {
                    Language = "pl",
                    Type = "official",
                    Title = "Crest of the Stars"
                }
            });

            series.RelatedSeries.Should().BeEquivalentTo(new[]
            {
                new RelatedSeriesData
                {
                    Id = 4,
                    Type = "Sequel",
                    Title = "Seikai no Senki"
                },
                new RelatedSeriesData
                {
                    Id = 6,
                    Type = "Prequel",
                    Title = "Seikai no Danshou: Tanjou"
                },
                new RelatedSeriesData
                {
                    Id = 1623,
                    Type = "Summary",
                    Title = "Seikai no Monshou Tokubetsu Hen"
                }
            });

            series.SimilarSeries.Should().BeEquivalentTo(new[]
            {
                new SimilarSeriesData
                {
                    Id = 584,
                    Approval = 70,
                    Total = 84,
                    Title = "Ginga Eiyuu Densetsu"
                },
                new SimilarSeriesData
                {
                    Id = 2745,
                    Approval = 51,
                    Total = 61,
                    Title = "Starship Operators"
                },
                new SimilarSeriesData
                {
                    Id = 6005,
                    Approval = 34,
                    Total = 49,
                    Title = "Tytania"
                },
                new SimilarSeriesData
                {
                    Id = 192,
                    Approval = 18,
                    Total = 40,
                    Title = "Mugen no Ryvius"
                },
                new SimilarSeriesData
                {
                    Id = 630,
                    Approval = 13,
                    Total = 27,
                    Title = "Uchuu no Stellvia"
                },
                new SimilarSeriesData
                {
                    Id = 5406,
                    Approval = 3,
                    Total = 15,
                    Title = "Ookami to Koushinryou"
                },
                new SimilarSeriesData
                {
                    Id = 18,
                    Approval = 2,
                    Total = 10,
                    Title = "Musekinin Kanchou Tylor"
                }
            });

            series.Url.Should().Be("http://www.sunrise-inc.co.jp/seikai/");

            series.Creators.Should().BeEquivalentTo(new[]
            {
                new CreatorData
                {
                    Id = 4303,
                    Type = "Music",
                    Name = "Hattori Katsuhisa"
                },
                new CreatorData
                {
                    Id = 4234,
                    Type = "Direction",
                    Name = "Nagaoka Yasuchika"
                },
                new CreatorData
                {
                    Id = 4516,
                    Type = "Character Design",
                    Name = "Watabe Keisuke"
                },
                new CreatorData
                {
                    Id = 8924,
                    Type = "Series Composition",
                    Name = "Yoshinaga Aya"
                },
                new CreatorData
                {
                    Id = 4495,
                    Type = "Original Work",
                    Name = "Morioka Hiroyuki"
                }
            });

            series.Description.Should()
                .Be(
                    "* Based on the sci-fi novel series by http://anidb.net/cr4495 [Morioka Hiroyuki].\nhttp://anidb.net/ch4081 [Linn Jinto]`s life changes forever when the http://anidb.net/ch7514 [Humankind Empire Abh] takes over his home planet of Martine without firing a single shot. He is soon sent off to study the http://anidb.net/t2324 [Abh] language and culture and to prepare himself for his future as a nobleman — a future he never dreamed of, asked for, or even wanted.\nNow, Jinto is entering the next phase of his training, and he is about to meet the first Abh in his life, the lovely http://anidb.net/ch28 [Lafiel]. However, Jinto is about to learn that there is more to her than meets the eye, and together they will have to fight for their very lives.");

            series.Ratings.Should().BeEquivalentTo(new RatingData[]
            {
                new PermanentRatingData
                {
                    VoteCount = 4303,
                    Value = 8.17f
                },
                new TemporaryRatingData
                {
                    VoteCount = 4333,
                    Value = 8.26f
                },
                new ReviewRatingData
                {
                    VoteCount = 12,
                    Value = 8.70f
                }
            });

            series.Tags.Length.Should().Be(51);
            series.Tags[0]
                .Should().BeEquivalentTo(new TagData
                {
                    Id = 36,
                    ParentId = 2607,
                    Weight = 300,
                    LocalSpoiler = false,
                    GlobalSpoiler = false,
                    Verified = true,
                    LastUpdated = new DateTime(2017, 4, 17),
                    Name = "military",
                    Description =
                        "The military, also known as the armed forces, are forces authorized and legally entitled to use deadly force so as to support the interests of the state and its citizens. The task of the military is usually defined as defence of the state and its citizens and the prosecution of war against foreign powers. The military may also have additional functions within a society, including construction, emergency services, social ceremonies, and guarding critical areas.\nSource: Wikipedia"
                });

            series.Characters.Length.Should().Be(29);
            series.Characters[0]
                .Should().BeEquivalentTo(new CharacterData
                {
                    Id = 28,
                    Role = "main character in",
                    Rating = new CharacterRatingData
                    {
                        Value = 9.31f,
                        VoteCount = 934
                    },
                    Name = "Abriel Nei Debrusc Borl Paryun Lafiel",
                    LastUpdated = new DateTime(2012, 7, 25),
                    Gender = "female",
                    Type = new CharacterType
                    {
                        Id = 1,
                        Name = "Character"
                    },
                    Description =
                        "Ablïarsec néïc Dubreuscr Bœrh Parhynr Lamhirh (a.k.a., Viscountess Paryunu Abriel Nei Dobrusk Lafiel) is the main female protagonist in the anime Crest of the Stars, Banner of the Stars, and Banner of the Stars II, as well as all the novels written by Morioka Hiroyuki on which the shows were based. She is a strong-willed Abh princess (granddaughter of the Abh empress) who has a steely exterior, but ends up befriending Ghintec Linn (Jinto Lynn in the Martinh tongue). Like all Abh, she has bluish hued hair, and has a natural lifespan of over 200 years. Lamhirh also has lapis lazuli colored eyes. As an Ablïarsec, she has pointed ears, yet hers are markedly less so than other Ablïarsec. This is because half her genes (those not from her father) are from someone outside the Abriel clan and her father chose not to make any unnecessary alterations in her genes. She is deemed \"child of love\" (an Abh child with the genes of the parent, and the one the parent loves). Her full name can be roughly translated to Lamhirh (néïc Dubleuscr) Ablïarsec, Viscountess of Parhyn.\nDespite being a princess, she rarely acts like one and hates being treated as one. One of the reasons she took a liking towards Ghintec is because when they first met, he neither recognized her as a princess nor treated her as one. Their relationship is so close that she freely allows him to use her real name of Lamhirh when addressing her, something that is very uncommon when addressing those of nobility or royalty.\nShe acts remarkably older than her age (at her introduction in Crest of the Stars, she is 16 years old) and can, in most cases, logically think her way out of most situations. However, her headstrong nature sometimes clouds her judgement and can lead her to become impulsive. An example of this is when she is reprimanded by Laicch for wishing to stay behind on the Gothlauth instead of continuing her mission of escorting Ghintec to the capital. She believes that she would have been of more use fighting with the crew rather than abandoning them. She is quickly shown how wrong her line of reasoning is and how much more disgraceful it would have been to abandon Ghintec and her mission. She is a remarkably good shot and although she sometimes doubts herself, she proves to be a worthy ship captain (deca-commander) in Banner of the Stars. She shows little emotion throughout Crest of the Stars, but as time goes by became very close friend with Ghintec through Banner of the Stars. This is especially true in later installments, where she more frequently questions how their friendship will last due to the doubt of Ghintec`s lifespan.\nShe is one of the candidates for the Abh Imperial Throne and, as indicated by her full name, she is the Viscountess of Parhynh, the so-called \"Country, or Nation, of Roses.\"",
                    PictureFileName = "14304.jpg",
                    Seiyuu = new SeiyuuData
                    {
                        Id = 12,
                        PictureFileName = "184301.jpg",
                        Name = "Kawasumi Ayako"
                    }
                });

            series.Episodes.Length.Should().Be(16);
            series.Episodes[0]
                .Should().BeEquivalentTo(new AniDbEpisodeData
                {
                    Id = 1,
                    LastUpdated = new DateTime(2011, 7, 1),
                    RawEpisodeNumber = new EpisodeNumberData
                    {
                        RawType = 1,
                        RawNumber = "1"
                    },
                    TotalMinutes = 25,
                    AirDate = new DateTime(1999, 1, 3),
                    Rating = new EpisodeRatingData
                    {
                        VoteCount = 28,
                        Rating = 3.16f
                    },
                    Titles = new[]
                    {
                        new EpisodeTitleData
                        {
                            Language = "ja",
                            Title = "侵略"
                        },
                        new EpisodeTitleData
                        {
                            Language = "en",
                            Title = "Invasion"
                        },
                        new EpisodeTitleData
                        {
                            Language = "fr",
                            Title = "Invasion"
                        },
                        new EpisodeTitleData
                        {
                            Language = "x-jat",
                            Title = "Shinryaku"
                        }
                    }
                });
        }

        [Test]
        public void ParseSeriesFile_ValidXml_ReturnsPictureFileName()
        {
            var fileContent = File.ReadAllText(TestContext.CurrentContext.TestDirectory + @"\TestData\anidb\series2.xml");

            var xmlFileParser = new XmlSerialiser(new ConsoleLogger());

            var series = xmlFileParser.Deserialise<AniDbSeriesData>(fileContent);

            series.PictureFileName.Should().Be("64304.jpg");
        }

        [Test]
        public void ParseTitlesFile_ValidXml_ReturnsDeserialised()
        {
            var fileContent = File.ReadAllText(TestContext.CurrentContext.TestDirectory + @"\TestData\anidb\titles.xml");

            var xmlFileParser = new XmlSerialiser(new ConsoleLogger());

            var titleList = xmlFileParser.Deserialise<TitleListData>(fileContent);

            titleList.Titles.Length.Should().Be(12221);

            titleList.Titles[0]
                .Should().BeEquivalentTo(new TitleListItemData
                {
                    AniDbId = 1,
                    Titles = new[]
                    {
                        new ItemTitleData
                        {
                            Type = "short",
                            Language = "en",
                            Title = "CotS"
                        },
                        new ItemTitleData
                        {
                            Type = "official",
                            Language = "en",
                            Title = "Crest of the Stars"
                        },
                        new ItemTitleData
                        {
                            Type = "official",
                            Language = "pl",
                            Title = "Crest of the Stars"
                        },
                        new ItemTitleData
                        {
                            Type = "official",
                            Language = "fr",
                            Title = "Crest of the Stars"
                        },
                        new ItemTitleData
                        {
                            Type = "syn",
                            Language = "cs",
                            Title = "Hvězdný erb"
                        },
                        new ItemTitleData
                        {
                            Type = "main",
                            Language = "x-jat",
                            Title = "Seikai no Monshou"
                        },
                        new ItemTitleData
                        {
                            Type = "short",
                            Language = "x-jat",
                            Title = "SnM"
                        },
                        new ItemTitleData
                        {
                            Type = "syn",
                            Language = "zh-Hans",
                            Title = "星界之纹章"
                        },
                        new ItemTitleData
                        {
                            Type = "official",
                            Language = "ja",
                            Title = "星界の紋章"
                        }
                    }
                });
        }
    }
}
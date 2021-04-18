using System.IO;
using Jellyfin.AniDbMetaStructure.Files;
using Jellyfin.AniDbMetaStructure.Mapping.Data;
using Jellyfin.AniDbMetaStructure.Tests.TestHelpers;
using FluentAssertions;
using NUnit.Framework;

namespace Emby.AniDbMetaStructure.Tests
{
    [TestFixture]
    public class AnimeMappingListDataTests
    {
        private static string MappingsFilePath => TestContext.CurrentContext.TestDirectory +
            @"\TestData\Mappings\anime-list.xml";

        [Test]
        public void CreateMappingListAsync_ParsesFileCorrectly()
        {
            var fileParser = new XmlSerialiser(new ConsoleLogManager());
            var fileContent = File.ReadAllText(MappingsFilePath);

            var mappingList = fileParser.Deserialise<AnimeMappingListData>(fileContent);

            mappingList.AnimeSeriesMapping.Length.Should().Be(7427);
            mappingList.AnimeSeriesMapping[22]
                .Should().BeEquivalentTo(new AniDbSeriesMappingData
                {
                    AnidbId = "23",
                    Name = "Cowboy Bebop",
                    TvDbId = "76885",
                    DefaultTvDbSeason = "1",
                    GroupMappingList = new[]
                    {
                        new AnimeEpisodeGroupMappingData
                        {
                            EpisodeMappingString = ";1-2;",
                            AnidbSeason = 0,
                            TvDbSeason = 0
                        }
                    },
                    SupplementalInfo = new[]
                    {
                        new AnimeSeriesSupplementalInfoData
                        {
                            Items = new object[]
                            {
                                "Sunrise"
                            },
                            ItemsElementName = new[]
                            {
                                ItemsChoiceType.studio
                            }
                        }
                    }
                });

            mappingList.AnimeSeriesMapping[101]
                .Should().BeEquivalentTo(new AniDbSeriesMappingData
                {
                    AnidbId = "107",
                    Name = "Chikyuu Shoujo Arjuna",
                    TvDbId = "80113",
                    DefaultTvDbSeason = "1",
                    GroupMappingList = new[]
                    {
                        new AnimeEpisodeGroupMappingData
                        {
                            EpisodeMappingString = ";1-9;",
                            AnidbSeason = 0,
                            TvDbSeason = 1
                        },
                        new AnimeEpisodeGroupMappingData
                        {
                            EpisodeMappingString = null,
                            AnidbSeason = 1,
                            TvDbSeason = 1,
                            Start = 9,
                            End = 12,
                            Offset = 1,
                            StartSpecified = true,
                            EndSpecified = true,
                            OffsetSpecified = true
                        }
                    },
                    SpecialEpisodePositionsString = ";1-9;"
                });
        }
    }
}
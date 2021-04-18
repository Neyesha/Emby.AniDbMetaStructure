using Jellyfin.AniDbMetaStructure.AniDb.Seiyuu;
using Jellyfin.AniDbMetaStructure.AniDb.SeriesData;

namespace Emby.AniDbMetaStructure.Tests.TestData
{
    internal static class AniDbSeriesTestData
    {
        public static AniDbSeriesData WithStandardData(this AniDbSeriesData aniDbSeriesData)
        {
            return new AniDbSeriesData
            {
                Id = 324,
                Description = "Series description",
                Titles = new[]
                {
                    new ItemTitleData
                    {
                        Language = "en",
                        Title = "EnTitle",
                        Type = "official"
                    }
                },
                Ratings = new RatingData[]
                {
                    new PermanentRatingData
                    {
                        Value = 6.53f,
                        VoteCount = 41
                    }
                },
                Creators = new CreatorData[0],
                Characters = new[]
                {
                    new CharacterData
                    {
                        Id = 54,
                        Description = "Character",
                        Name = "CharacterName",
                        Gender = "unknown",
                        Seiyuu = new SeiyuuData().WithStandardData()
                    }
                }
            };
        }

        public static AniDbSeriesData WithoutTags(this AniDbSeriesData aniDbSeriesData)
        {
            aniDbSeriesData = aniDbSeriesData.WithStandardData();

            aniDbSeriesData.Tags = null;

            return aniDbSeriesData;
        }
    }

    internal static class SeiyuuTestData
    {
        public static SeiyuuData WithStandardData(this SeiyuuData seiyuuData)
        {
            return new SeiyuuData
            {
                Id = 1,
                Name = "SeiyuuName",
                PictureFileName = "FileName"
            };
        }
    }
}
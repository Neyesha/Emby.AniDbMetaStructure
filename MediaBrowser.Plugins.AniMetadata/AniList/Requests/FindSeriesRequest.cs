using System.Collections.Generic;
using Jellyfin.AniDbMetaStructure.AniList.Data;

namespace Jellyfin.AniDbMetaStructure.AniList.Requests
{
    internal class FindSeriesRequest : AniListQueryRequest<AniListGraphQlPage<FindSeriesRequest.FindSeriesResponse>>
    {
        private const string FindSeriesQuery = @"query($maxResults: Int, $seriesName: String){
  Page(page: 1, perPage: $maxResults) {
    media(type: ANIME, search: $seriesName) {
      id
      type
      title {
        romaji
        english
        native
      }
      startDate {
        year
        month
        day
      }
      endDate {
        year
        month
        day
      }
      description
      idMal
      genres
      averageScore
      popularity
      siteUrl
      coverImage {
        large
        medium
      }
      bannerImage
      duration
      status
      studios {
        edges {
          isMain
          node{
            name,
          }
        }
      }
      staff {
        edges{
          node{
            name {
              first
              last
              native
            }
            image {
              large
              medium
            }
          }
          role
        }
      }
      characters{
        edges{
          node{
            name {
              first
              last
              native
            }
            image {
              large
              medium
            }
          }
          voiceActors{
            language
            image {
              large
              medium
            }
            name {
              first
              last
              native
            }
          }
        }
      }
    }
  }
}";

        public FindSeriesRequest(string seriesName, int maxResults = 10) : base(
            FindSeriesQuery,
            new
            {
                seriesName,
                maxResults
            })
        {
        }

        public class FindSeriesResponse
        {
            public FindSeriesResponse(IEnumerable<AniListSeriesData> media)
            {
                Media = media;
            }

            public IEnumerable<AniListSeriesData> Media { get; }
        }
    }
}
using Jellyfin.AniDbMetaStructure.JsonApi;
using Jellyfin.AniDbMetaStructure.TvDb.Data;
using LanguageExt;
using System.Collections.Generic;

namespace Jellyfin.AniDbMetaStructure.TvDb.Requests
{
    internal class GetEpisodesRequest : TvDbRequest<GetEpisodesRequest.Response>, IGetRequest<GetEpisodesRequest.Response>
    {
        public GetEpisodesRequest(int seriesId, int pageIndex) : base($"series/{seriesId}/episodes?page={pageIndex}")
        {
        }

        public class Response
        {
            public Response(IEnumerable<TvDbEpisodeSummaryData> data, PageLinks links)
            {
                Data = data;
                Links = links;
            }

            public IEnumerable<TvDbEpisodeSummaryData> Data { get; }

            public PageLinks Links { get; }
        }

        public class PageLinks
        {
            public PageLinks(int first, int last, Option<int> next, Option<int> previous)
            {
                First = first;
                Last = last;
                Next = next;
                Previous = previous;
            }

            public int First { get; }

            public int Last { get; }

            public Option<int> Next { get; }

            public Option<int> Previous { get; }
        }
    }
}
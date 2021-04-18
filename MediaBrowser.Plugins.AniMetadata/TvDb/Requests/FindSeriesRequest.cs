using Jellyfin.AniDbMetaStructure.JsonApi;
using System.Collections.Generic;

namespace Jellyfin.AniDbMetaStructure.TvDb.Requests
{
    internal class FindSeriesRequest : TvDbRequest<FindSeriesRequest.Response>, IGetRequest<FindSeriesRequest.Response>
    {
        public FindSeriesRequest(string seriesName) : base($"search/series?name={seriesName}")
        {
        }

        public class Response
        {
            public Response(IEnumerable<MatchingSeries> data)
            {
                MatchingSeries = data ?? new List<MatchingSeries>();
            }

            public IEnumerable<MatchingSeries> MatchingSeries { get; }
        }

        public class MatchingSeries
        {
            public MatchingSeries(int id, string seriesName, string[] aliases)
            {
                Id = id;
                SeriesName = seriesName;
                Aliases = aliases;
            }

            public int Id { get; }

            public string SeriesName { get; }

            public string[] Aliases { get; }
        }
    }
}
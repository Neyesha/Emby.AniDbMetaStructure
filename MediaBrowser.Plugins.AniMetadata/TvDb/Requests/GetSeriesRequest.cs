using Jellyfin.AniDbMetaStructure.JsonApi;
using Jellyfin.AniDbMetaStructure.TvDb.Data;

namespace Jellyfin.AniDbMetaStructure.TvDb.Requests
{
    internal class GetSeriesRequest : TvDbRequest<GetSeriesRequest.Response>, IGetRequest<GetSeriesRequest.Response>
    {
        public GetSeriesRequest(int tvDbSeriesId) : base($"series/{tvDbSeriesId}")
        {
        }

        public class Response
        {
            public Response(TvDbSeriesData data)
            {
                Data = data;
            }

            public TvDbSeriesData Data { get; }
        }
    }
}
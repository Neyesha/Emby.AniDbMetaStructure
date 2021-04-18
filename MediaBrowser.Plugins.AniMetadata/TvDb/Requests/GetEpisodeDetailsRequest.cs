using Jellyfin.AniDbMetaStructure.JsonApi;
using Jellyfin.AniDbMetaStructure.TvDb.Data;

namespace Jellyfin.AniDbMetaStructure.TvDb.Requests
{
    internal class GetEpisodeDetailsRequest
        : TvDbRequest<GetEpisodeDetailsRequest.Response>, IGetRequest<GetEpisodeDetailsRequest.Response>
    {
        public GetEpisodeDetailsRequest(int episodeId) : base($"episodes/{episodeId}")
        {
        }

        public class Response
        {
            public Response(TvDbEpisodeData data)
            {
                this.Data = data;
            }

            public TvDbEpisodeData Data { get; }
        }
    }
}
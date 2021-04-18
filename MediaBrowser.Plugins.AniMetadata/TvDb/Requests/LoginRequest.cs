using Jellyfin.AniDbMetaStructure.JsonApi;

namespace Jellyfin.AniDbMetaStructure.TvDb.Requests
{
    internal class LoginRequest : TvDbRequest<GetEpisodesRequest.Response>, IPostRequest<LoginRequest.Response>
    {
        public LoginRequest(string apiKey) : base("login")
        {
            Data = new RequestData(apiKey);
        }

        public object Data { get; }

        public class RequestData
        {
            public RequestData(string apiKey)
            {
                ApiKey = apiKey;
            }

            public string ApiKey { get; }
        }

        public class Response
        {
            public Response(string token)
            {
                Token = token;
            }

            public string Token { get; }
        }
    }
}
using Jellyfin.AniDbMetaStructure.JsonApi;

namespace Jellyfin.AniDbMetaStructure.AniList.Requests
{
    internal class GetTokenRequest : Request<GetTokenRequest.TokenData>, IPostRequest<GetTokenRequest.TokenData>
    {
        public GetTokenRequest(int clientId, string clientSecret, string redirectUrl, string authorisationCode) : base(
            "https://anilist.co/api/v2/oauth/token")
        {
            Data = new
            {
                grant_type = "authorization_code",
                client_id = clientId.ToString(),
                client_secret = clientSecret,
                redirect_uri = redirectUrl,
                code = authorisationCode
            };
        }

        public object Data { get; }

        public class TokenData
        {
            public TokenData(string accessToken, int expiresIn, string refreshToken)
            {
                AccessToken = accessToken;
                ExpiresIn = expiresIn;
                RefreshToken = refreshToken;
            }

            public string AccessToken { get; }

            public int ExpiresIn { get; }

            public string RefreshToken { get; }
        }
    }
}
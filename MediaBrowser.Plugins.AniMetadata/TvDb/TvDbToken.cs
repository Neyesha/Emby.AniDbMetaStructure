using System.Threading.Tasks;
using Jellyfin.AniDbMetaStructure.JsonApi;
using Jellyfin.AniDbMetaStructure.TvDb.Requests;
using LanguageExt;
using Microsoft.Extensions.Logging;

namespace Jellyfin.AniDbMetaStructure.TvDb
{
    internal class TvDbToken
    {
        private readonly string apiKey;
        private readonly ILogger logger;
        private readonly IJsonConnection jsonConnection;
        private bool hasToken;
        private string token;

        public TvDbToken(IJsonConnection jsonConnection, string apiKey, ILogger logger)
        {
            this.jsonConnection = jsonConnection;
            this.apiKey = apiKey;
            this.logger = logger;
        }

        public async Task<Option<string>> GetTokenAsync()
        {
            if (this.hasToken)
            {
                this.logger.LogDebug($"Using existing token '{this.token}'");
                return this.token;
            }

            var request = new LoginRequest(this.apiKey);

            var response = await this.jsonConnection.PostAsync(request, Option<string>.None);

            return response.Match(
                r =>
                {
                    this.hasToken = true;
                    this.token = r.Data.Token;

                    this.logger.LogDebug($"Got new token '{this.token}'");
                    return this.token;
                },
                fr =>
                {
                    this.logger.LogDebug("Failed to get a new token");
                    return Option<string>.None;
                });
        }
    }
}
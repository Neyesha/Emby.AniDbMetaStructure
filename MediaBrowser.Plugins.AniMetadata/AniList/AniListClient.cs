using System.Collections.Generic;
using System.Threading.Tasks;
using Jellyfin.AniDbMetaStructure.AniList.Data;
using Jellyfin.AniDbMetaStructure.AniList.Requests;
using Jellyfin.AniDbMetaStructure.JsonApi;
using Jellyfin.AniDbMetaStructure.Process;
using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.AniList
{
    internal class AniListClient : IAniListClient
    {
        private readonly IAnilistConfiguration anilistConfiguration;
        private readonly IAniListToken aniListToken;
        private readonly IJsonConnection jsonConnection;

        public AniListClient(IJsonConnection jsonConnection, IAniListToken aniListToken,
            IAnilistConfiguration anilistConfiguration)
        {
            this.jsonConnection = jsonConnection;
            this.aniListToken = aniListToken;
            this.anilistConfiguration = anilistConfiguration;
        }

        public Task<Either<ProcessFailedResult, IEnumerable<AniListSeriesData>>> FindSeriesAsync(string title,
            ProcessResultContext resultContext)
        {
            var token = this.aniListToken.GetToken(this.jsonConnection, this.anilistConfiguration, resultContext);

            var request = new FindSeriesRequest(title);

            return token.Map(e => e.MapLeft(FailedRequest.ToFailedResult(resultContext)))
                .BindAsync(t =>
                {
                    return this.jsonConnection.PostAsync(request, t)
                        .MapAsync(r => r.Data.Data.Page.Media)
                        .Map(e => e.MapLeft(FailedRequest.ToFailedResult(resultContext)));
                });
        }
    }
}
using System.IO;
using Jellyfin.AniDbMetaStructure.JsonApi;

namespace Jellyfin.AniDbMetaStructure.TvDb.Requests
{
    internal abstract class TvDbRequest<TResponse> : Request<TResponse>
    {
        private const string TvDbBaseUrl = "https://api.thetvdb.com/";

        protected TvDbRequest(string urlPath) : base(Path.Combine(TvDbBaseUrl, urlPath))
        {
        }
    }
}
using System.Threading.Tasks;
using Jellyfin.AniDbMetaStructure.JsonApi;
using Jellyfin.AniDbMetaStructure.Process;
using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.AniList
{
    internal interface IAniListToken
    {
        Task<Either<FailedRequest, string>> GetToken(IJsonConnection jsonConnection,
            IAnilistConfiguration anilistConfiguration, ProcessResultContext resultContext);
    }
}
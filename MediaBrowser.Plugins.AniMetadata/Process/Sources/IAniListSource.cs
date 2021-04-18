using Jellyfin.AniDbMetaStructure.AniList.Data;
using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.Process.Sources
{
    internal interface IAniListSource : ISource
    {
        Either<ProcessFailedResult, string> SelectTitle(AniListTitleData titleData,
            string metadataLanguage, ProcessResultContext resultContext);
    }
}
using Jellyfin.AniDbMetaStructure.AniList.Data;
using Jellyfin.AniDbMetaStructure.Configuration;
using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.AniList
{
    internal interface IAniListNameSelector
    {
        Option<string> SelectTitle(AniListTitleData titleData, TitleType preferredTitleType,
            string metadataLanguage);

        Option<string> SelectName(AniListPersonNameData nameData, TitleType preferredTitleType,
            string metadataLanguage);
    }
}
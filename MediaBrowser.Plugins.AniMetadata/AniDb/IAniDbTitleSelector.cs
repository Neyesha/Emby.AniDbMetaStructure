using System.Collections.Generic;
using Jellyfin.AniDbMetaStructure.AniDb.SeriesData;
using Jellyfin.AniDbMetaStructure.Configuration;
using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.AniDb
{
    internal interface IAniDbTitleSelector
    {
        Option<ItemTitleData> SelectTitle(IEnumerable<ItemTitleData> titles, TitleType preferredTitleType,
            string metadataLanguage);
    }
}
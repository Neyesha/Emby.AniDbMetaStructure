using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.AniDb.Titles
{
    internal interface ISeriesTitleCache
    {
        Option<TitleListItemData> FindSeriesByTitle(string title);
    }
}
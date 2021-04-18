namespace Jellyfin.AniDbMetaStructure.AniList.Data
{
    internal class AniListGraphQlPage<T>
    {
        public AniListGraphQlPage(T page)
        {
            Page = page;
        }

        public T Page { get; }
    }
}
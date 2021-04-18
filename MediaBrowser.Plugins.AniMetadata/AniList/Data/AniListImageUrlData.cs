namespace Jellyfin.AniDbMetaStructure.AniList.Data
{
    internal class AniListImageUrlData
    {
        public AniListImageUrlData(string large, string medium)
        {
            Large = large;
            Medium = medium;
        }

        public string Large { get; }

        public string Medium { get; }
    }
}
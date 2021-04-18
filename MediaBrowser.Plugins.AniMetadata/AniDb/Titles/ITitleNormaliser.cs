namespace Jellyfin.AniDbMetaStructure.AniDb.Titles
{
    internal interface ITitleNormaliser
    {
        string GetNormalisedTitle(string title);
    }
}
using Jellyfin.AniDbMetaStructure.Process.Sources;

namespace Jellyfin.AniDbMetaStructure.Process
{
    internal interface ISources
    {
        IAniDbSource AniDb { get; }

        ITvDbSource TvDb { get; }

        IAniListSource AniList { get; }

        ISource Get(string sourceName);
    }
}
using Jellyfin.AniDbMetaStructure.Configuration;

namespace Jellyfin.AniDbMetaStructure.Process.Sources
{
    internal interface ITitlePreferenceConfiguration
    {
        TitleType TitlePreference { get; }
    }
}
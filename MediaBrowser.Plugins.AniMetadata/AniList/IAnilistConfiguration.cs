using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.AniList
{
    internal interface IAnilistConfiguration
    {
        bool IsLinked { get; }

        string AuthorizationCode { get; }

        Option<string> AccessToken { get; set; }
    }
}
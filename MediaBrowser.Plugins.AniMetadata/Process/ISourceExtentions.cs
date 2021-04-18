namespace Jellyfin.AniDbMetaStructure.Process
{
    internal static class SourceExtentions
    {
        public static ISource ForAdditionalData(this ISource source)
        {
            return new AdditionalSource(source);
        }

        public static bool IsForAdditionalData(this ISource source)
        {
            return source is AdditionalSource;
        }
    }
}
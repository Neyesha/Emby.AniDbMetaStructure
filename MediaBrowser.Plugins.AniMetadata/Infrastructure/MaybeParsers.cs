using LanguageExt;
using static LanguageExt.Prelude;

namespace Jellyfin.AniDbMetaStructure.Infrastructure
{
    public static class MaybeParsers
    {
        public static Option<int> MaybeInt(this string value)
        {
            return parseInt(value);
        }
    }
}
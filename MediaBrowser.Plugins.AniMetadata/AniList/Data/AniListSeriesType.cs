using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Jellyfin.AniDbMetaStructure.AniList.Data
{
    [JsonConverter(typeof(StringEnumConverter))]
    internal enum AniListSeriesType
    {
        Anime,
        Manga
    }
}
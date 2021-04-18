using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Jellyfin.AniDbMetaStructure.AniList.Data
{
    [JsonConverter(typeof(StringEnumConverter))]
    internal enum AniListMediaFormat
    {
        [EnumMember(Value = "TV")] Tv = 0,

        [EnumMember(Value = "TV_SHORT")] TvShort = 1,

        [EnumMember(Value = "MOVIE")] Movie = 2,

        [EnumMember(Value = "SPECIAL")] Special = 3,

        [EnumMember(Value = "OVA")] Ova = 4,

        [EnumMember(Value = "ONA")] Ona = 5,

        [EnumMember(Value = "MUSIC")] Music = 6,

        [EnumMember(Value = "MANGA")] Manga = 7,

        [EnumMember(Value = "NOVEL")] Novel = 8,

        [EnumMember(Value = "ONE_SHOT")] OneShot = 9
    }
}
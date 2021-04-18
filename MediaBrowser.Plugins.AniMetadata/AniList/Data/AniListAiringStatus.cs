using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Jellyfin.AniDbMetaStructure.AniList.Data
{
    [JsonConverter(typeof(StringEnumConverter))]
    internal enum AniListAiringStatus
    {
        [EnumMember(Value = "FINISHED")] Finished,

        [EnumMember(Value = "RELEASING")] Airing,

        [EnumMember(Value = "NOT_YET_RELEASED")]
        NotYetAired,

        [EnumMember(Value = "CANCELLED")] Cancelled
    }
}
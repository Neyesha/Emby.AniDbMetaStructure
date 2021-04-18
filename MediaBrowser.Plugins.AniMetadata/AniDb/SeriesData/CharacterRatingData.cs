using System.Xml.Serialization;

namespace Jellyfin.AniDbMetaStructure.AniDb.SeriesData
{
    public class CharacterRatingData
    {
        [XmlAttribute("votes")]
        public int VoteCount { get; set; }

        [XmlText]
        public float Value { get; set; }
    }
}
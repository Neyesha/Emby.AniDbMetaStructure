using System.Xml.Serialization;

namespace Jellyfin.AniDbMetaStructure.AniDb.SeriesData
{
    [XmlType(AnonymousType = true)]
    public abstract class RatingData
    {
        [XmlAttribute("count")]
        public int VoteCount { get; set; }

        [XmlText]
        public float Value { get; set; }

        public abstract RatingType Type { get; }
    }
}
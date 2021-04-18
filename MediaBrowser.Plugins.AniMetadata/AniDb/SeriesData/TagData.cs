using System;
using System.Xml.Serialization;

namespace Jellyfin.AniDbMetaStructure.AniDb.SeriesData
{
    public class TagData
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlAttribute("parentid")]
        public int ParentId { get; set; }

        [XmlAttribute("weight")]
        public int Weight { get; set; }

        [XmlAttribute("localspoiler")]
        public bool LocalSpoiler { get; set; }

        [XmlAttribute("globalspoiler")]
        public bool GlobalSpoiler { get; set; }

        [XmlAttribute("verified")]
        public bool Verified { get; set; }

        [XmlAttribute("update")]
        public DateTime LastUpdated { get; set; }

        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("description")]
        public string Description { get; set; }
    }
}
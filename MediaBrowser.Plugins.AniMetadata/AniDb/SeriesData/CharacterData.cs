using System;
using System.Xml.Serialization;
using Jellyfin.AniDbMetaStructure.AniDb.Seiyuu;

namespace Jellyfin.AniDbMetaStructure.AniDb.SeriesData
{
    public class CharacterData
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        /// <summary>
        ///     The role of the character, e.g. 'main character in', 'secondary cast in'
        /// </summary>
        [XmlAttribute("type")]
        public string Role { get; set; }

        [XmlAttribute("update")]
        public DateTime LastUpdated { get; set; }

        [XmlElement("rating")]
        public CharacterRatingData Rating { get; set; }

        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("charactertype")]
        public CharacterType Type { get; set; }

        [XmlElement("description")]
        public string Description { get; set; }

        [XmlElement("gender")]
        public string Gender { get; set; }

        [XmlElement("picture")]
        public string PictureFileName { get; set; }

        [XmlElement("seiyuu")]
        public SeiyuuData Seiyuu { get; set; }
    }
}
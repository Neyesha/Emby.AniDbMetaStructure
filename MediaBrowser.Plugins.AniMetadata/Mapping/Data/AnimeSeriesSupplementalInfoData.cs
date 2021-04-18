using System.Xml.Serialization;

namespace Jellyfin.AniDbMetaStructure.Mapping.Data
{
    /// <summary>
    ///     Additional data about an anime series
    /// </summary>
    [XmlType(AnonymousType = true)]
    public class AnimeSeriesSupplementalInfoData
    {
        /// <summary>
        ///     An array of the values
        /// </summary>
        [XmlElement("credits", typeof(string))]
        [XmlElement("director", typeof(string))]
        [XmlElement("fanart", typeof(AnimeSeriesSupplementalInfoFanartData))]
        [XmlElement("genre", typeof(string))]
        [XmlElement("studio", typeof(string))]
        [XmlChoiceIdentifier("ItemsElementName")]
        public object[] Items { get; set; }

        /// <summary>
        ///     An array of the fields that the values correspond to
        /// </summary>
        [XmlElement("ItemsElementName")]
        [XmlIgnore]
        public ItemsChoiceType[] ItemsElementName { get; set; }

        /// <summary>
        ///     If true, the data should replace the data provided by AniDB
        /// </summary>
        [XmlAttribute("replace")]
        public bool Replace { get; set; }

        [XmlIgnore]
        public bool ReplaceSpecified { get; set; }
    }
}
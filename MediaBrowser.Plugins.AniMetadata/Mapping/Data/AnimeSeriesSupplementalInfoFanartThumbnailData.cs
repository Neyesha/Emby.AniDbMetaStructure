using System.Xml.Serialization;

namespace Jellyfin.AniDbMetaStructure.Mapping.Data
{
    /// <summary>
    ///     Details of a fanart thumbnail specified as supplemental info
    /// </summary>
    [XmlType(AnonymousType = true)]
    public class AnimeSeriesSupplementalInfoFanartThumbnailData
    {
        /// <summary>
        ///     The dimensions of the fanart as a string, e.g. 1280x720
        /// </summary>
        [XmlAttribute("dim")]
        public string Dimensions { get; set; }

        /// <summary>
        ///     The colours of the fanart (usually left blank), e.g. |148,149,153|13,23,22|165,159,137|
        /// </summary>
        [XmlAttribute("colors")]
        public string Colors { get; set; }

        /// <summary>
        ///     A URL of a smaller preview version of the fanart
        /// </summary>
        [XmlAttribute("preview")]
        public string PreviewUrl { get; set; }

        /// <summary>
        ///     The URL of the full-size version of the fanart
        /// </summary>
        [XmlText]
        public string Url { get; set; }
    }
}
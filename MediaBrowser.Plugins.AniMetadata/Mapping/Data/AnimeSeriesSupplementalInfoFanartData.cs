using System.Xml.Serialization;

namespace Jellyfin.AniDbMetaStructure.Mapping.Data
{
    /// <summary>
    ///     Details of additional Fanart specified as supplemental information
    /// </summary>
    [XmlType(AnonymousType = true)]
    public class AnimeSeriesSupplementalInfoFanartData
    {
        /// <summary>
        ///     Details of the thumbnail fanart
        /// </summary>
        [XmlElement("thumb")]
        public AnimeSeriesSupplementalInfoFanartThumbnailData Thumbnail { get; set; }
    }
}
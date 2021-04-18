using System.Xml.Serialization;

namespace Jellyfin.AniDbMetaStructure.Mapping.Data
{
    /// <summary>
    ///     A list of anime mappings between AniDb, theTVDB, and themoviedb
    /// </summary>
    [XmlType(AnonymousType = true)]
    [XmlRoot("anime-list", Namespace = "", IsNullable = false)]
    public class AnimeMappingListData
    {
        /// <remarks />
        [XmlElement("anime")]
        public AniDbSeriesMappingData[] AnimeSeriesMapping { get; set; }
    }
}
using System.Xml.Serialization;

namespace Jellyfin.AniDbMetaStructure.Mapping.Data
{
    /// <summary>
    ///     The different fields of data that can be provided as supplemental info
    /// </summary>
    [XmlType(IncludeInSchema = false)]
    public enum ItemsChoiceType
    {
        credits,
        director,
        fanart,
        genre,
        studio
    }
}
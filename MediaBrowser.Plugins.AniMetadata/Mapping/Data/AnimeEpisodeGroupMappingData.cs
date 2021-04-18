using System.Collections.Generic;
using System.Xml.Serialization;

namespace Jellyfin.AniDbMetaStructure.Mapping.Data
{
    [XmlType(AnonymousType = true)]
    public class AnimeEpisodeGroupMappingData
    {
        /// <summary>
        ///     The AniDB season (either 1 for normal episodes or 0 for specials)
        /// </summary>
        [XmlAttribute("anidbseason")]
        public int AnidbSeason { get; set; }

        /// <summary>
        ///     The corresponding TvDb season
        /// </summary>
        [XmlAttribute("tvdbseason")]
        public int TvDbSeason { get; set; }

        /// <summary>
        ///     The first AniDB episode this mapping applies to.
        /// </summary>
        [XmlAttribute("start")]
        public short Start { get; set; }

        [XmlIgnore]
        public bool StartSpecified { get; set; }

        /// <summary>
        ///     The last AniDB episode this mapping applies to.
        /// </summary>
        [XmlAttribute("end")]
        public short End { get; set; }

        [XmlIgnore]
        public bool EndSpecified { get; set; }

        /// <summary>
        ///     The number to add to each episode this mapping applies to to get the corresponding TvDb episode.
        /// </summary>
        [XmlAttribute("offset")]
        public short Offset { get; set; }

        /// <remarks />
        [XmlIgnore]
        public bool OffsetSpecified { get; set; }

        /// <summary>
        ///     Mappings for individual episodes in the format ;x-y; where x is the AniDB episode and y is the TvDB episode in the
        ///     <see cref="TvDbSeason" />. These override the offset.
        /// </summary>
        [XmlText]
        public string EpisodeMappingString { get; set; }

        /// <summary>
        ///     Typed mappings for individual episodes.
        /// </summary>
        [XmlIgnore]
        public List<AnimeEpisodeMappingData> ParsedMappings { get; set; }
    }
}
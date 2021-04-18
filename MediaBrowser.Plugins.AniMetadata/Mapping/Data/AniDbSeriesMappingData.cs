using System.Xml.Serialization;

namespace Jellyfin.AniDbMetaStructure.Mapping.Data
{
    /// <summary>
    ///     A mapping of a single anime series
    /// </summary>
    [XmlType(AnonymousType = true)]
    public class AniDbSeriesMappingData
    {
        /// <summary>
        ///     The name of the series
        /// </summary>
        [XmlElement("name")]
        public string Name { get; set; }

        /// <summary>
        ///     A list of specific episode mappings to handle complex scenarios.
        /// </summary>
        [XmlArray("mapping-list")]
        [XmlArrayItem("mapping", typeof(AnimeEpisodeGroupMappingData), IsNullable = false)]
        public AnimeEpisodeGroupMappingData[] GroupMappingList { get; set; }

        /// <summary>
        ///     A semi-colon separated string of AniDB special episode numbers and the normal AniDB episode they should be listed
        ///     before (e.g. ;1-4; means special episode 1 should appear before episode 4)
        /// </summary>
        [XmlElement("before")]
        public string SpecialEpisodePositionsString { get; set; }

        /// <summary>
        ///     Additional information that should be used over what AniDB provides where available.
        /// </summary>
        [XmlElement("supplemental-info")]
        public AnimeSeriesSupplementalInfoData[] SupplementalInfo { get; set; }

        /// <summary>
        ///     The Id of the series on AniDB
        /// </summary>
        [XmlAttribute("anidbid")]
        public string AnidbId { get; set; }

        /// <summary>
        ///     The Id of the series on TvDb. 'Unknown' if the series does not appear on TvDb. 'movie', 'OVA', 'hentai', 'tv
        ///     special', 'music video', 'web' or other AniDB type if of a type that will never appear on TvDb.
        /// </summary>
        [XmlAttribute("tvdbid")]
        public string TvDbId { get; set; }

        /// <summary>
        ///     The default TvDb season index for episodes of this series, can be 1 or 0, or 'a' if the absolute episode indexes
        ///     match those on TvDb for series that span multiple TvDb seasons.
        /// </summary>
        [XmlAttribute("defaulttvdbseason")]
        public string DefaultTvDbSeason { get; set; }

        /// <summary>
        ///     The Imdb Id of the series if it is a one-off title. Not always set, can be 'unknown'.
        /// </summary>
        [XmlAttribute("imdbid")]
        public string ImdbId { get; set; }

        /// <summary>
        ///     The themoviedb Id of the series if it is a one-off title. Not always set, can be 'unknown'.
        /// </summary>
        [XmlAttribute("tmdbid")]
        public string TmdbId { get; set; }

        /// <summary>
        ///     The number to add to the AniDB episode numbers to get the corresponding TvDb episode number in the
        ///     <see cref="DefaultTvDbSeason" />.
        /// </summary>
        [XmlAttribute("episodeoffset")]
        public short EpisodeOffset { get; set; }

        /// <remarks />
        [XmlIgnore]
        public bool EpisodeOffsetSpecified { get; set; }
    }
}
using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData
{
    public class EpisodeData
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlAttribute("update")]
        public DateTime LastUpdated { get; set; }

        [XmlElement("epno")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public EpisodeNumberData RawEpisodeNumber { get; set; }

        [XmlIgnore]
        public IAniDbEpisodeNumber EpisodeNumber => RawEpisodeNumber;

        [XmlElement("length")]
        public int TotalMinutes { get; set; }

        [XmlElement("airdate")]
        public DateTime AirDate { get; set; }

        [XmlElement("rating")]
        public EpisodeRatingData Rating { get; set; }

        [XmlElement("title", typeof(EpisodeTitleData))]
        public EpisodeTitleData[] Titles { get; set; }

        [XmlElement("summary")]
        public string Summary { get; set; }
    }
}
using System;
using System.Xml.Serialization;

namespace Jellyfin.AniDbMetaStructure.AniDb.SeriesData
{
    [XmlType(AnonymousType = true)]
    [XmlRoot("anime", Namespace = "", IsNullable = false)]
    public class AniDbSeriesData
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlAttribute("restricted")]
        public bool Restricted { get; set; }

        [XmlElement("type")]
        public string Type { get; set; }

        [XmlElement("episodecount")]
        public int EpisodeCount { get; set; }

        [XmlElement("startdate")]
        public DateTime? StartDate { get; set; }

        [XmlElement("enddate")]
        public DateTime? EndDate { get; set; }

        [XmlArray("titles")]
        [XmlArrayItem("title", typeof(ItemTitleData))]
        public ItemTitleData[] Titles { get; set; }

        [XmlArray("relatedanime")]
        [XmlArrayItem("anime", typeof(RelatedSeriesData))]
        public RelatedSeriesData[] RelatedSeries { get; set; }

        [XmlArray("similaranime")]
        [XmlArrayItem("anime", typeof(SimilarSeriesData))]
        public SimilarSeriesData[] SimilarSeries { get; set; }

        [XmlElement("url")]
        public string Url { get; set; }

        [XmlArray("creators")]
        [XmlArrayItem("name", typeof(CreatorData))]
        public CreatorData[] Creators { get; set; }

        /// <summary>
        ///     Contains links in the form url [link text], e.g. 'http://anidb.net/cr4495 [Morioka Hiroyuki]'
        /// </summary>
        [XmlElement("description")]
        public string Description { get; set; }

        [XmlArray("ratings")]
        [XmlArrayItem("permanent", typeof(PermanentRatingData))]
        [XmlArrayItem("temporary", typeof(TemporaryRatingData))]
        [XmlArrayItem("review", typeof(ReviewRatingData))]
        public RatingData[] Ratings { get; set; }

        [XmlElement("picture")]
        public string PictureFileName { get; set; }

        [XmlArray("tags")]
        [XmlArrayItem("tag", typeof(TagData))]
        public TagData[] Tags { get; set; }

        [XmlArray("episodes")]
        [XmlArrayItem("episode", typeof(AniDbEpisodeData))]
        public AniDbEpisodeData[] Episodes { get; set; }

        [XmlArray("characters")]
        [XmlArrayItem("character", typeof(CharacterData))]
        public CharacterData[] Characters { get; set; }
    }
}
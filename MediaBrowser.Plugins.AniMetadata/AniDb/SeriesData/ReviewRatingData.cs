using System.Xml.Serialization;

namespace Jellyfin.AniDbMetaStructure.AniDb.SeriesData
{
    public class ReviewRatingData : RatingData
    {
        [XmlIgnore]
        public override RatingType Type => RatingType.Review;
    }
}
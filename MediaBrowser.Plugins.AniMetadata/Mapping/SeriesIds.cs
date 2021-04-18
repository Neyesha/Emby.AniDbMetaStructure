using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.Mapping
{
    public class SeriesIds
    {
        public SeriesIds(int aniDbSeriesId, Option<int> tvDbSeriesId, Option<int> imdbSeriesId,
            Option<int> tmDbSeriesId)
        {
            this.AniDbSeriesId = aniDbSeriesId;
            this.TvDbSeriesId = tvDbSeriesId;
            this.ImdbSeriesId = imdbSeriesId;
            this.TmDbSeriesId = tmDbSeriesId;
        }

        public int AniDbSeriesId { get; }

        public Option<int> TvDbSeriesId { get; }

        public Option<int> ImdbSeriesId { get; }

        public Option<int> TmDbSeriesId { get; }
    }
}
namespace Jellyfin.AniDbMetaStructure.TvDb.Data
{
    internal class TvDbSeasonData
    {
        public TvDbSeasonData(int seasonNumber)
        {
            this.SeasonNumber = seasonNumber;
        }

        public int SeasonNumber { get; }
    }
}
namespace Jellyfin.AniDbMetaStructure.Mapping
{
    public class EpisodeMapping
    {
        public EpisodeMapping(int aniDbEpisodeIndex, int tvDbEpisodeIndex)
        {
            this.AniDbEpisodeIndex = aniDbEpisodeIndex;
            this.TvDbEpisodeIndex = tvDbEpisodeIndex;
        }

        public int AniDbEpisodeIndex { get; }

        public int TvDbEpisodeIndex { get; }
    }
}
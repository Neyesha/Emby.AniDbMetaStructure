namespace Jellyfin.AniDbMetaStructure.Mapping.Data
{
    /// <summary>
    ///     A mapping between two specific episode numbers.
    /// </summary>
    public class AnimeEpisodeMappingData
    {
        /// <summary>
        ///     The AniDB episode number
        /// </summary>
        public int AniDb { get; set; }

        /// <summary>
        ///     The TvDb episode number, zero indicates that there is no equivalent TvDb episode and that the AniDb episode
        ///     conflicts with the TvDb episode.
        /// </summary>
        public int TvDb { get; set; }
    }
}
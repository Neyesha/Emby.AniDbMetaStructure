namespace Jellyfin.AniDbMetaStructure.Configuration
{
    public enum LibraryStructure
    { 
        /// <summary>
        ///     Multiple series with one season and absolute episode numbers
        /// </summary>
        AniDb,

        /// <summary>
        ///     Single series with seasons
        /// </summary>
        TvDb
    }
}
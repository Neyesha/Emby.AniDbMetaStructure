using MediaBrowser.Controller.Entities.TV;

namespace Jellyfin.AniDbMetaStructure.Process
{
    internal static class MediaItemTypes
    {
        internal static readonly IMediaItemType<Episode> Episode =
            new MediaItemType<Episode>("Episode", (c, l) => c.GetEpisodeMetadataMapping(l));

        internal static readonly IMediaItemType<Season> Season =
            new MediaItemType<Season>("Season", (c, l) => c.GetSeasonMetadataMapping(l));

        internal static readonly IMediaItemType<Series> Series =
            new MediaItemType<Series>("Series", (c, l) => c.GetSeriesMetadataMapping(l));
    }
}
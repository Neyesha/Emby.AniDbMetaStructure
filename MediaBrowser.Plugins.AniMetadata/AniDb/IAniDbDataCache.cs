using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.AniDbMetaStructure.AniDb.Seiyuu;
using Jellyfin.AniDbMetaStructure.AniDb.SeriesData;
using Jellyfin.AniDbMetaStructure.AniDb.Titles;
using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.AniDb
{
    internal interface IAniDbDataCache
    {
        IEnumerable<TitleListItemData> TitleList { get; }

        Task<Option<AniDbSeriesData>> GetSeriesAsync(int aniDbSeriesId, CancellationToken cancellationToken);

        IEnumerable<SeiyuuData> GetSeiyuu();
    }
}
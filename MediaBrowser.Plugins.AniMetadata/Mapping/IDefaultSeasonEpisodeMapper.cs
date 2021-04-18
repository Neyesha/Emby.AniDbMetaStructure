using System.Threading.Tasks;
using Jellyfin.AniDbMetaStructure.TvDb.Data;
using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.Mapping
{
    internal interface IDefaultSeasonEpisodeMapper
    {
        Task<Option<TvDbEpisodeData>> MapEpisodeAsync(int aniDbEpisodeIndex, ISeriesMapping seriesMapping);
    }
}
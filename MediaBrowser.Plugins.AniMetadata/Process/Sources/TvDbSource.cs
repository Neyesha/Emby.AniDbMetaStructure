using System.Collections.Generic;
using System.Threading.Tasks;
using Jellyfin.AniDbMetaStructure.SourceDataLoaders;
using Jellyfin.AniDbMetaStructure.TvDb;
using Jellyfin.AniDbMetaStructure.TvDb.Data;
using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.Process.Sources
{
    internal class TvDbSource : ITvDbSource
    {
        private readonly IEnumerable<IEmbySourceDataLoader> embySourceDataLoaders;
        private readonly ITvDbClient tvDbClient;

        public TvDbSource(ITvDbClient tvDbClient, IEnumerable<IEmbySourceDataLoader> embySourceDataLoaders)
        {
            this.tvDbClient = tvDbClient;
            this.embySourceDataLoaders = embySourceDataLoaders;
        }

        public SourceName Name => SourceNames.TvDb;

        public Either<ProcessFailedResult, IEmbySourceDataLoader> GetEmbySourceDataLoader(IMediaItemType mediaItemType)
        {
            return this.embySourceDataLoaders.Find(l => l.SourceName == this.Name && l.CanLoadFrom(mediaItemType))
                .ToEither(new ProcessFailedResult(this.Name, string.Empty, mediaItemType,
                    "No Emby source data loader for this source and media item type"));
        }

        public bool ShouldUsePlaceholderSourceData(IMediaItemType mediaItemType)
        {
            return false;
        }

        public Task<Either<ProcessFailedResult, TvDbSeriesData>> GetSeriesData(IEmbyItemData embyItemData,
            ProcessResultContext resultContext)
        {
            Task<Either<ProcessFailedResult, int>> seriesId;

            if (embyItemData.ItemType == MediaItemTypes.Series)
            {
                seriesId = embyItemData.GetExistingId(this.Name)
                    .ToEitherAsync(resultContext.Failed("No TvDb Id found on this series"));
            }
            else
            {
                seriesId = embyItemData.GetParentId(MediaItemTypes.Series, this)
                    .ToEitherAsync(resultContext.Failed("No TvDb Id found on parent series"));
            }

            return seriesId.BindAsync(tvDbSeriesId => this.GetSeriesData(tvDbSeriesId, resultContext));
        }

        public Task<Either<ProcessFailedResult, TvDbSeriesData>> GetSeriesData(int tvDbSeriesId,
            ProcessResultContext resultContext)
        {
            return this.tvDbClient.GetSeriesAsync(tvDbSeriesId)
                .ToEitherAsync(resultContext.Failed($"Failed to load parent series with TvDb Id '{tvDbSeriesId}'"));
        }
    }
}
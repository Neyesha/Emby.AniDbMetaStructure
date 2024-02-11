using Jellyfin.AniDbMetaStructure.Process;
using Jellyfin.AniDbMetaStructure.Process.Sources;
using Jellyfin.AniDbMetaStructure.TvDb;
using Jellyfin.AniDbMetaStructure.TvDb.Data;
using LanguageExt;
using System.Threading.Tasks;

namespace Jellyfin.AniDbMetaStructure.SourceDataLoaders
{
    /// <summary>
    ///     Loads series data from TvDb based on the data provided by Emby
    /// </summary>
    internal class TvDbSeriesFromEmbyData : IJellyfinSourceDataLoader
    {
        private readonly ISources sources;
        private readonly ITvDbClient tvDbClient;

        public TvDbSeriesFromEmbyData(ITvDbClient tvDbClient, ISources sources)
        {
            this.tvDbClient = tvDbClient;
            this.sources = sources;
        }

        public SourceName SourceName => SourceNames.TvDb;

        public bool CanLoadFrom(IMediaItemType mediaItemType)
        {
            return mediaItemType == MediaItemTypes.Series;
        }

        public Task<Either<ProcessFailedResult, ISourceData>> LoadFrom(IJellyfinItemData embyItemData)
        {
            var resultContext = new ProcessResultContext(nameof(TvDbSeriesFromEmbyData), embyItemData.Identifier.Name,
                embyItemData.ItemType);

            return this.tvDbClient.FindSeriesAsync(embyItemData.Identifier.Name)
                .ToEitherAsync(resultContext.Failed("Failed to find series in TvDb"))
                .MapAsync(s => CreateSourceData(s, embyItemData));
        }

        private ISourceData CreateSourceData(TvDbSeriesData seriesData, IJellyfinItemData embyItemData)
        {
            return new SourceData<TvDbSeriesData>(this.sources.TvDb, seriesData.Id,
                new ItemIdentifier(embyItemData.Identifier.Index, Option<int>.None, seriesData.SeriesName), seriesData);
        }
    }
}
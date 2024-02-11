using Jellyfin.AniDbMetaStructure.Process;
using Jellyfin.AniDbMetaStructure.Process.Sources;
using Jellyfin.AniDbMetaStructure.TvDb;
using Jellyfin.AniDbMetaStructure.TvDb.Data;
using LanguageExt;
using System.Threading.Tasks;

namespace Jellyfin.AniDbMetaStructure.SourceDataLoaders
{
    /// <summary>
    ///     Loads series data from TvDb based on the data provided by Jellyfin
    /// </summary>
    internal class TvDbSeriesFromJellyfinData : IJellyfinSourceDataLoader
    {
        private readonly ISources sources;
        private readonly ITvDbClient tvDbClient;

        public TvDbSeriesFromJellyfinData(ITvDbClient tvDbClient, ISources sources)
        {
            this.tvDbClient = tvDbClient;
            this.sources = sources;
        }

        public SourceName SourceName => SourceNames.TvDb;

        public bool CanLoadFrom(IMediaItemType mediaItemType)
        {
            return mediaItemType == MediaItemTypes.Series;
        }

        public Task<Either<ProcessFailedResult, ISourceData>> LoadFrom(IJellyfinItemData JellyfinItemData)
        {
            var resultContext = new ProcessResultContext(nameof(TvDbSeriesFromJellyfinData), JellyfinItemData.Identifier.Name,
                JellyfinItemData.ItemType);

            return this.tvDbClient.FindSeriesAsync(JellyfinItemData.Identifier.Name)
                .ToEitherAsync(resultContext.Failed("Failed to find series in TvDb"))
                .MapAsync(s => CreateSourceData(s, JellyfinItemData));
        }

        private ISourceData CreateSourceData(TvDbSeriesData seriesData, IJellyfinItemData JellyfinItemData)
        {
            return new SourceData<TvDbSeriesData>(this.sources.TvDb, seriesData.Id,
                new ItemIdentifier(JellyfinItemData.Identifier.Index, Option<int>.None, seriesData.SeriesName), seriesData);
        }
    }
}
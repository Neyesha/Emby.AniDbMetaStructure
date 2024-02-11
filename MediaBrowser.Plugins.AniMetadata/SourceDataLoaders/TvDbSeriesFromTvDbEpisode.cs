using System.Threading.Tasks;
using Jellyfin.AniDbMetaStructure.Process;
using Jellyfin.AniDbMetaStructure.TvDb.Data;
using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.SourceDataLoaders
{
    /// <summary>
    ///     Loads series data for an item that already has TvDb episode data loaded
    /// </summary>
    internal class TvDbSeriesFromTvDbEpisode : ISourceDataLoader
    {
        private readonly ISources sources;

        public TvDbSeriesFromTvDbEpisode(ISources sources)
        {
            this.sources = sources;
        }

        public bool CanLoadFrom(object sourceData)
        {
            return sourceData is ISourceData<TvDbEpisodeData>;
        }

        public Task<Either<ProcessFailedResult, ISourceData>> LoadFrom(IMediaItem mediaItem, object sourceData)
        {
            var resultContext = new ProcessResultContext(nameof(TvDbSeriesFromJellyfinData),
                mediaItem.JellyfinData.Identifier.Name,
                mediaItem.JellyfinData.ItemType);

            return this.sources.TvDb.GetSeriesData(mediaItem.JellyfinData, resultContext)
                .MapAsync(s => this.CreateSourceData(s, mediaItem.JellyfinData));
        }

        private ISourceData CreateSourceData(TvDbSeriesData seriesData, IJellyfinItemData JellyfinItemData)
        {
            return new SourceData<TvDbSeriesData>(this.sources.TvDb.ForAdditionalData(), seriesData.Id,
                new ItemIdentifier(JellyfinItemData.Identifier.Index, Option<int>.None, seriesData.SeriesName), seriesData);
        }
    }
}
using Jellyfin.AniDbMetaStructure.AniDb.SeriesData;
using Jellyfin.AniDbMetaStructure.Process;
using LanguageExt;
using System.Threading.Tasks;

namespace Jellyfin.AniDbMetaStructure.SourceDataLoaders
{
    /// <summary>
    ///     Loads series data for an item that already has AniDb episode data loaded
    /// </summary>
    internal class AniDbSeriesFromAniDbEpisode : ISourceDataLoader
    {
        private readonly ISources sources;

        public AniDbSeriesFromAniDbEpisode(ISources sources)
        {
            this.sources = sources;
        }

        public bool CanLoadFrom(object sourceData)
        {
            return sourceData is ISourceData<AniDbEpisodeData>;
        }

        public Task<Either<ProcessFailedResult, ISourceData>> LoadFrom(IMediaItem mediaItem, object sourceData)
        {
            var resultContext = new ProcessResultContext(nameof(AniDbSeriesFromAniDbEpisode),
                mediaItem.EmbyData.Identifier.Name,
                mediaItem.ItemType);

            return this.sources.AniDb.GetSeriesData(mediaItem.EmbyData, resultContext)
                .BindAsync(s =>
                {
                    var title = this.sources.AniDb.SelectTitle(s.Titles, mediaItem.EmbyData.Language, resultContext);

                    return title.Map(t => CreateSourceData(s, mediaItem.EmbyData, t));
                });
        }

        private ISourceData CreateSourceData(AniDbSeriesData seriesData, IJellyfinItemData embyItemData, string title)
        {
            return new SourceData<AniDbSeriesData>(this.sources.AniDb.ForAdditionalData(), seriesData.Id,
                new ItemIdentifier(embyItemData.Identifier.Index, Option<int>.None, title), seriesData);
        }
    }
}
using System.Threading.Tasks;
using Jellyfin.AniDbMetaStructure.Process;
using Jellyfin.AniDbMetaStructure.Process.Sources;
using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.SourceDataLoaders
{
    /// <summary>
    ///     Loads season data from AniDb based on the data provided by Emby
    /// </summary>
    internal class AniDbSeasonFromEmbyData : IEmbySourceDataLoader
    {
        private readonly ISources sources;

        public AniDbSeasonFromEmbyData(ISources sources)
        {
            this.sources = sources;
        }

        public SourceName SourceName => SourceNames.AniDb;

        public bool CanLoadFrom(IMediaItemType mediaItemType)
        {
            return mediaItemType == MediaItemTypes.Season;
        }

        public Task<Either<ProcessFailedResult, ISourceData>> LoadFrom(IEmbyItemData embyItemData)
        {
            var resultContext = new ProcessResultContext(nameof(AniDbSeasonFromEmbyData), embyItemData.Identifier.Name,
                embyItemData.ItemType);

            var aniDbSeries = this.sources.AniDb.GetSeriesData(embyItemData, resultContext);

            return aniDbSeries.BindAsync(series =>
                    this.sources.AniDb.SelectTitle(series.Titles, embyItemData.Language, resultContext))
                .MapAsync(seriesTitle => new ItemIdentifier(embyItemData.Identifier.Index.IfNone(1),
                    embyItemData.Identifier.ParentIndex, seriesTitle))
                .MapAsync(itemIdentifier =>
                    (ISourceData)new IdentifierOnlySourceData(this.sources.AniDb, Option<int>.None, itemIdentifier, embyItemData.ItemType));
        }
    }
}
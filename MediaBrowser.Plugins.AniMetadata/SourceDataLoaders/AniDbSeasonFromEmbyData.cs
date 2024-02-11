using System.Threading.Tasks;
using Jellyfin.AniDbMetaStructure.Process;
using Jellyfin.AniDbMetaStructure.Process.Sources;
using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.SourceDataLoaders
{
    /// <summary>
    ///     Loads season data from AniDb based on the data provided by Jellyfin
    /// </summary>
    internal class AniDbSeasonFromJellyfinData : IJellyfinSourceDataLoader
    {
        private readonly ISources sources;

        public AniDbSeasonFromJellyfinData(ISources sources)
        {
            this.sources = sources;
        }

        public SourceName SourceName => SourceNames.AniDb;

        public bool CanLoadFrom(IMediaItemType mediaItemType)
        {
            return mediaItemType == MediaItemTypes.Season;
        }

        public Task<Either<ProcessFailedResult, ISourceData>> LoadFrom(IJellyfinItemData JellyfinItemData)
        {
            var resultContext = new ProcessResultContext(nameof(AniDbSeasonFromJellyfinData), JellyfinItemData.Identifier.Name,
                JellyfinItemData.ItemType);

            var aniDbSeries = this.sources.AniDb.GetSeriesData(JellyfinItemData, resultContext);

            return aniDbSeries.BindAsync(series =>
                    this.sources.AniDb.SelectTitle(series.Titles, JellyfinItemData.Language, resultContext))
                .MapAsync(seriesTitle => new ItemIdentifier(JellyfinItemData.Identifier.Index.IfNone(1),
                    JellyfinItemData.Identifier.ParentIndex, seriesTitle))
                .MapAsync(itemIdentifier =>
                    (ISourceData)new IdentifierOnlySourceData(this.sources.AniDb, Option<int>.None, itemIdentifier, JellyfinItemData.ItemType));
        }
    }
}
using Jellyfin.AniDbMetaStructure.AniDb.SeriesData;
using Jellyfin.AniDbMetaStructure.Mapping;
using Jellyfin.AniDbMetaStructure.Process;
using Jellyfin.AniDbMetaStructure.Process.Sources;
using LanguageExt;
using System.Linq;
using System.Threading.Tasks;

namespace Jellyfin.AniDbMetaStructure.SourceDataLoaders
{
    internal class AniDbSeriesFromTvDb : ISourceDataLoader
    {
        private readonly ISources sources;
        private readonly IMappingList mappingList;

        public AniDbSeriesFromTvDb(ISources sources, IMappingList mappingList)
        {
            this.sources = sources;
            this.mappingList = mappingList;
        }

        public bool CanLoadFrom(object sourceData)
        {
            return sourceData is ISourceData<IdentifierOnlySourceData> identifierOnlySourceData && identifierOnlySourceData.ItemType == MediaItemTypes.Season && identifierOnlySourceData.Source.Name == SourceNames.TvDb;
        }

        public Task<Either<ProcessFailedResult, ISourceData>> LoadFrom(IMediaItem mediaItem, object sourceData)
        {
            var tvDbIdentifierOnlySourceData = (ISourceData<IdentifierOnlySourceData>)sourceData;
            var IdentifierOnlySourceData = tvDbIdentifierOnlySourceData.Data;

            var resultContext = new ProcessResultContext(nameof(AniDbSeriesFromTvDb),
                mediaItem.EmbyData.Identifier.Name,
                mediaItem.ItemType);

            var tvDbSeriesId = mediaItem.EmbyData.GetParentId(MediaItemTypes.Series, this.sources.TvDb)
                .ToEither(resultContext.Failed("Failed to find TvDb series Id"));

            var aniDbSeriesId = tvDbSeriesId.BindAsync(id => MapSeriesDataAsync(id, IdentifierOnlySourceData, resultContext));

            return aniDbSeriesId
                .BindAsync(id => this.sources.AniDb.GetSeriesData(id, resultContext))
                .BindAsync(s =>
                {
                    var title = this.sources.AniDb.SelectTitle(s.Titles, mediaItem.EmbyData.Language, resultContext);

                    return title.Map(t => CreateSourceData(s, mediaItem.EmbyData, t));
                });
        }

        private Task<Either<ProcessFailedResult, int>> MapSeriesDataAsync(int tvDbSeriesId, IdentifierOnlySourceData identifierData, ProcessResultContext resultContext)
        {
            var seriesMapping = this.mappingList.GetSeriesMappingsFromTvDb(tvDbSeriesId, resultContext)
                .BindAsync(sm => sm.Where(m => m.DefaultTvDbSeason.Exists(s => s.Index == identifierData.Identifier.Index))
                    .Match(
                        () => resultContext.Failed(
                            $"No series mapping between TvDb series Id '{tvDbSeriesId}', season '{identifierData.Identifier.Index}'' and AniDb series"),
                        Prelude.Right<ProcessFailedResult, ISeriesMapping>,
                        (head, tail) =>
                            resultContext.Failed(
                                $"Multiple series mappings found between TvDb series Id '{tvDbSeriesId}', season '{identifierData.Identifier.Index}'' and AniDb series")));

            return seriesMapping.MapAsync(sm => sm.Ids.AniDbSeriesId);
        }

        private ISourceData CreateSourceData(AniDbSeriesData seriesData, IEmbyItemData embyItemData, string title)
        {
            return new SourceData<AniDbSeriesData>(this.sources.AniDb, seriesData.Id,
                new ItemIdentifier(embyItemData.Identifier.Index, Option<int>.None, title), seriesData);
        }
    }
}

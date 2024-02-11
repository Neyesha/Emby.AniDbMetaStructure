using Jellyfin.AniDbMetaStructure.AniDb.SeriesData;
using Jellyfin.AniDbMetaStructure.Mapping;
using Jellyfin.AniDbMetaStructure.Process;
using Jellyfin.AniDbMetaStructure.TvDb.Data;
using LanguageExt;
using System.Threading.Tasks;

namespace Jellyfin.AniDbMetaStructure.SourceDataLoaders
{
    /// <summary>
    ///     Loads TvDb series data based on existing AniDb series information
    /// </summary>
    internal class TvDbSeriesFromAniDb : ISourceDataLoader
    {
        private readonly IMappingList mappingList;
        private readonly ISources sources;

        public TvDbSeriesFromAniDb(ISources sources, IMappingList mappingList)
        {
            this.sources = sources;
            this.mappingList = mappingList;
        }

        public bool CanLoadFrom(object sourceData)
        {
            return sourceData is ISourceData<AniDbSeriesData>;
        }

        public Task<Either<ProcessFailedResult, ISourceData>> LoadFrom(IMediaItem mediaItem, object sourceData)
        {
            var aniDbSourceData = (ISourceData<AniDbSeriesData>)sourceData;

            var resultContext = new ProcessResultContext(nameof(TvDbSeriesFromAniDb),
                mediaItem.JellyfinData.Identifier.Name,
                mediaItem.ItemType);

            return aniDbSourceData.Id.ToEither(
                    resultContext.Failed(
                        "No AniDb Id found on the AniDb data associated with this media item"))
                .BindAsync(aniDbSeriesId => GetMappedTvDbSeriesId(aniDbSeriesId, resultContext))
                .BindAsync(tvDbSeriesId => this.sources.TvDb.GetSeriesData(tvDbSeriesId, resultContext))
                .MapAsync(CreateSourceData);
        }

        private Task<Either<ProcessFailedResult, int>> GetMappedTvDbSeriesId(int aniDbSeriesId,
            ProcessResultContext resultContext)
        {
            return this.mappingList.GetSeriesMappingFromAniDb(aniDbSeriesId, resultContext)
                .BindAsync(sm =>
                    sm.Ids.TvDbSeriesId.ToEither(resultContext.Failed("No TvDb Id found on matching mapping")));
        }

        private ISourceData CreateSourceData(TvDbSeriesData tvDbSeriesData)
        {
            return new SourceData<TvDbSeriesData>(this.sources.TvDb,
                tvDbSeriesData.Id,
                new ItemIdentifier(Option<int>.None, Option<int>.None, tvDbSeriesData.SeriesName),
                tvDbSeriesData);
        }
    }
}
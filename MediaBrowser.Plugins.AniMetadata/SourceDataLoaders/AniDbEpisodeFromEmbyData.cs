using Emby.AniDbMetaStructure.AniDb.SeriesData;
using Emby.AniDbMetaStructure.Mapping;
using Emby.AniDbMetaStructure.Process;
using Emby.AniDbMetaStructure.Process.Sources;
using Emby.AniDbMetaStructure.Providers.AniDb;
using LanguageExt;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Emby.AniDbMetaStructure.SourceDataLoaders
{
    /// <summary>
    ///     Loads episode data from AniDb based on the data provided by Emby
    /// </summary>
    internal class AniDbEpisodeFromEmbyData : IEmbySourceDataLoader
    {
        private readonly IAniDbEpisodeMatcher aniDbEpisodeMatcher;
        private readonly ISources sources;
        private readonly IMappingList mappingList;

        public AniDbEpisodeFromEmbyData(ISources sources, IAniDbEpisodeMatcher aniDbEpisodeMatcher, IMappingList mappingList)
        {
            this.sources = sources;
            this.aniDbEpisodeMatcher = aniDbEpisodeMatcher;
            this.mappingList = mappingList;
        }

        public SourceName SourceName => SourceNames.AniDb;

        public bool CanLoadFrom(IMediaItemType mediaItemType)
        {
            return mediaItemType == MediaItemTypes.Episode;
        }

        public async Task<Either<ProcessFailedResult, ISourceData>> LoadFrom(IEmbyItemData embyItemData)
        {
            var resultContext = new ProcessResultContext(nameof(AniDbEpisodeFromEmbyData), embyItemData.Identifier.Name,
                embyItemData.ItemType);

            if (embyItemData.GetParentId(MediaItemTypes.Series, this.sources.AniDb).IsNone)
            {
                var tvDbSeriesId = embyItemData.GetParentId(MediaItemTypes.Series, this.sources.TvDb)
                    .ToEither(resultContext.Failed("Failed to find TvDb series Id"));

                if (tvDbSeriesId.IsRight && embyItemData.Identifier.ParentIndex.IsSome)
                {
                    var aniDbSeriesId = await tvDbSeriesId.BindAsync(id => MapSeriesDataAsync(id, embyItemData.Identifier.ParentIndex.Single(), resultContext));
                    aniDbSeriesId.IfRight((anidbId) =>
                    {
                        var updatedParentIds = embyItemData.ParentIds.Concat(new List<EmbyItemId> { new EmbyItemId(MediaItemTypes.Series, this.sources.AniDb.Name, anidbId) });
                        embyItemData = new EmbyItemData(embyItemData.ItemType, embyItemData.Identifier, embyItemData.ExistingIds, embyItemData.Language, updatedParentIds);
                    });

                }
            }

            return await this.sources.AniDb.GetSeriesData(embyItemData, resultContext)
                .BindAsync(seriesData => GetAniDbEpisodeData(seriesData, embyItemData, resultContext))
                .BindAsync(episodeData =>
                {
                    var title = this.sources.AniDb.SelectTitle(episodeData.Titles, embyItemData.Language, resultContext);

                    return title.Map(t => CreateSourceData(episodeData, t, embyItemData.Identifier.ParentIndex.Single()));
                });
        }

        private Task<Either<ProcessFailedResult, int>> MapSeriesDataAsync(int tvDbSeriesId, int index, ProcessResultContext resultContext)
        {
            var seriesMapping = this.mappingList.GetSeriesMappingsFromTvDb(tvDbSeriesId, resultContext)
                .BindAsync(sm => sm.Where(m => m.DefaultTvDbSeason.Exists(s => s.Index == index))
                    .Match(
                        () => resultContext.Failed(
                            $"No series mapping between TvDb series Id '{tvDbSeriesId}', season '{index}'' and AniDb series"),
                        Prelude.Right<ProcessFailedResult, ISeriesMapping>,
                        (head, tail) =>
                            resultContext.Failed(
                                $"Multiple series mappings found between TvDb series Id '{tvDbSeriesId}', season '{index}'' and AniDb series")));

            return seriesMapping.MapAsync(sm => sm.Ids.AniDbSeriesId);
        }

        private Either<ProcessFailedResult, AniDbEpisodeData> GetAniDbEpisodeData(AniDbSeriesData aniDbSeriesData,
            IEmbyItemData embyItemData,
            ProcessResultContext resultContext)
        {
            return this.aniDbEpisodeMatcher.FindEpisode(aniDbSeriesData.Episodes,
                    embyItemData.Identifier.ParentIndex,
                    embyItemData.Identifier.Index, embyItemData.Identifier.Name)
                .ToEither(resultContext.Failed("Failed to find episode in AniDb"));
        }

        private ISourceData CreateSourceData(AniDbEpisodeData e, string title, int seasonNumber)
        {
            return new SourceData<AniDbEpisodeData>(this.sources.AniDb, e.Id,
                new ItemIdentifier(e.EpisodeNumber.Number, seasonNumber, title), e);
        }
    }
}
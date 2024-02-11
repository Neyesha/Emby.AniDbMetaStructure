using Jellyfin.AniDbMetaStructure.AniDb.SeriesData;
using Jellyfin.AniDbMetaStructure.Mapping;
using Jellyfin.AniDbMetaStructure.Process;
using Jellyfin.AniDbMetaStructure.Process.Sources;
using Jellyfin.AniDbMetaStructure.Providers.AniDb;
using LanguageExt;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jellyfin.AniDbMetaStructure.SourceDataLoaders
{
    /// <summary>
    ///     Loads episode data from AniDb based on the data provided by Jellyfin
    /// </summary>
    internal class AniDbEpisodeFromJellyfinData : IJellyfinSourceDataLoader
    {
        private readonly IAniDbEpisodeMatcher aniDbEpisodeMatcher;
        private readonly ISources sources;
        private readonly IMappingList mappingList;

        public AniDbEpisodeFromJellyfinData(ISources sources, IAniDbEpisodeMatcher aniDbEpisodeMatcher, IMappingList mappingList)
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

        public async Task<Either<ProcessFailedResult, ISourceData>> LoadFrom(IJellyfinItemData JellyfinItemData)
        {
            var resultContext = new ProcessResultContext(nameof(AniDbEpisodeFromJellyfinData), JellyfinItemData.Identifier.Name,
                JellyfinItemData.ItemType);

            if (JellyfinItemData.GetParentId(MediaItemTypes.Series, this.sources.AniDb).IsNone)
            {
                var tvDbSeriesId = JellyfinItemData.GetParentId(MediaItemTypes.Series, this.sources.TvDb)
                    .ToEither(resultContext.Failed("Failed to find TvDb series Id"));

                if (tvDbSeriesId.IsRight && JellyfinItemData.Identifier.ParentIndex.IsSome)
                {
                    var aniDbSeriesId = await tvDbSeriesId.BindAsync(id => MapSeriesDataAsync(id, JellyfinItemData.Identifier.ParentIndex.Single(), resultContext));
                    aniDbSeriesId.IfRight((anidbId) =>
                    {
                        var updatedParentIds = JellyfinItemData.ParentIds.Concat(new List<JellyfinItemId> { new JellyfinItemId(MediaItemTypes.Series, this.sources.AniDb.Name, anidbId) });
                        JellyfinItemData = new JellyfinItemData(JellyfinItemData.ItemType, JellyfinItemData.Identifier, JellyfinItemData.ExistingIds, JellyfinItemData.Language, updatedParentIds);
                    });

                }
            }

            return await this.sources.AniDb.GetSeriesData(JellyfinItemData, resultContext)
                .BindAsync(seriesData => GetAniDbEpisodeData(seriesData, JellyfinItemData, resultContext))
                .BindAsync(episodeData =>
                {
                    var title = this.sources.AniDb.SelectTitle(episodeData.Titles, JellyfinItemData.Language, resultContext);

                    return title.Map(t => CreateSourceData(episodeData, t, JellyfinItemData.Identifier.ParentIndex.Single()));
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
            IJellyfinItemData JellyfinItemData,
            ProcessResultContext resultContext)
        {
            return this.aniDbEpisodeMatcher.FindEpisode(aniDbSeriesData.Episodes,
                    JellyfinItemData.Identifier.ParentIndex,
                    JellyfinItemData.Identifier.Index, JellyfinItemData.Identifier.Name)
                .ToEither(resultContext.Failed("Failed to find episode in AniDb"));
        }

        private ISourceData CreateSourceData(AniDbEpisodeData e, string title, int seasonNumber)
        {
            return new SourceData<AniDbEpisodeData>(this.sources.AniDb, e.Id,
                new ItemIdentifier(e.EpisodeNumber.Number, seasonNumber, title), e);
        }
    }
}
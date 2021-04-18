using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jellyfin.AniDbMetaStructure.AniDb.Titles;
using Jellyfin.AniDbMetaStructure.Process;
using Jellyfin.AniDbMetaStructure.Process.Sources;
using Jellyfin.AniDbMetaStructure.TvDb;
using Jellyfin.AniDbMetaStructure.TvDb.Data;
using LanguageExt;
using static LanguageExt.Prelude;

namespace Jellyfin.AniDbMetaStructure.SourceDataLoaders
{
    /// <summary>
    ///     Loads episode data from TvDb based on the data provided by Emby
    /// </summary>
    internal class TvDbEpisodeFromEmbyData : IEmbySourceDataLoader
    {
        private readonly ISources sources;
        private readonly ITitleNormaliser titleNormaliser;
        private readonly ITvDbClient tvDbClient;

        public TvDbEpisodeFromEmbyData(ISources sources, ITvDbClient tvDbClient, ITitleNormaliser titleNormaliser)
        {
            this.sources = sources;
            this.tvDbClient = tvDbClient;
            this.titleNormaliser = titleNormaliser;
        }

        public SourceName SourceName => SourceNames.TvDb;

        public bool CanLoadFrom(IMediaItemType mediaItemType)
        {
            return mediaItemType == MediaItemTypes.Episode;
        }

        public Task<Either<ProcessFailedResult, ISourceData>> LoadFrom(IEmbyItemData embyItemData)
        {
            var resultContext = new ProcessResultContext(nameof(TvDbEpisodeFromEmbyData), embyItemData.Identifier.Name,
                embyItemData.ItemType);

            var seriesId = embyItemData.GetParentId(MediaItemTypes.Series, this.sources.TvDb);

            return seriesId.ToEitherAsync(resultContext.Failed("No TvDb Id found on parent series"))
                .BindAsync(id => this.tvDbClient.GetEpisodesAsync(id)
                    .ToEitherAsync(resultContext.Failed($"Failed to load parent series with TvDb Id '{id}'")))
                .BindAsync(episodes => this.FindEpisode(episodes, embyItemData.Identifier.Name,
                    embyItemData.Identifier.Index, embyItemData.Identifier.ParentIndex, resultContext))
                .MapAsync(this.CreateSourceData);
        }

        private Task<Either<ProcessFailedResult, TvDbEpisodeData>> FindEpisode(IEnumerable<TvDbEpisodeData> episodes,
            string title, Option<int> episodeIndex, Option<int> seasonIndex, ProcessResultContext resultContext)
        {
            var normalisedTitle = this.titleNormaliser.GetNormalisedTitle(title);

            var episodeByIndexes = episodeIndex.Bind(i =>
                seasonIndex.Bind(si => episodes.Find(e => e.AiredEpisodeNumber == i && e.AiredSeason == si))
                    .Match(e => e, () => episodes.Find(e => e.AiredEpisodeNumber == i && e.AiredSeason == 1)));

            return episodeByIndexes.Match(Right<ProcessFailedResult, TvDbEpisodeData>,
                    () => Option<TvDbEpisodeData>
                        .Some(episodes.FirstOrDefault(e =>
                            this.titleNormaliser.GetNormalisedTitle(e.EpisodeName) == normalisedTitle))
                        .ToEither(resultContext.Failed(
                            $"Failed to find TvDb episode")))
                .AsTask();
        }

        private ISourceData CreateSourceData(TvDbEpisodeData e)
        {
            return new SourceData<TvDbEpisodeData>(this.sources.TvDb, e.Id,
                new ItemIdentifier(e.AiredEpisodeNumber, e.AiredSeason, e.EpisodeName), e);
        }
    }
}
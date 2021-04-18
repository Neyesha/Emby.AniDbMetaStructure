using Jellyfin.AniDbMetaStructure.AniDb.SeriesData;
using Jellyfin.AniDbMetaStructure.AniDb.Titles;
using Jellyfin.AniDbMetaStructure.AniList;
using Jellyfin.AniDbMetaStructure.AniList.Data;
using Jellyfin.AniDbMetaStructure.Process;
using Jellyfin.AniDbMetaStructure.Process.Sources;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace Jellyfin.AniDbMetaStructure.SourceDataLoaders
{
    /// <summary>
    ///     Loads series data from AniList based on data provided by AniDb
    /// </summary>
    internal class AniListSeriesFromAniDb : ISourceDataLoader
    {
        private readonly IAniListClient aniListClient;
        private readonly IAnilistConfiguration anilistConfiguration;
        private readonly ISources sources;
        private readonly ITitleNormaliser titleNormaliser;

        public AniListSeriesFromAniDb(IAniListClient aniListClient, ISources sources, ITitleNormaliser titleNormaliser,
            IAnilistConfiguration anilistConfiguration)
        {
            this.aniListClient = aniListClient;
            this.sources = sources;
            this.titleNormaliser = titleNormaliser;
            this.anilistConfiguration = anilistConfiguration;
        }

        public SourceName SourceName => SourceNames.AniList;

        public bool CanLoadFrom(object sourceData)
        {
            return this.anilistConfiguration.IsLinked && sourceData is ISourceData<AniDbSeriesData>;
        }

        public Task<Either<ProcessFailedResult, ISourceData>> LoadFrom(IMediaItem mediaItem, object sourceData)
        {
            var resultContext = new ProcessResultContext(nameof(AniListSeriesFromAniDb),
                mediaItem.EmbyData.Identifier.Name,
                mediaItem.ItemType);

            var aniDbSeriesData = (ISourceData<AniDbSeriesData>)sourceData;

            var distinctAniDbTitles = aniDbSeriesData.Data.Titles
                .Where(t => new[] { "en", "ja" }.Contains(t.Language, StringComparer.InvariantCultureIgnoreCase))
                .Select(t => t.Title)
                .Select(this.titleNormaliser.GetNormalisedTitle)
                .Distinct()
                .OrderByDescending(t => t.Length);

            var sourceDataTask = distinctAniDbTitles
                .Fold(Left<ProcessFailedResult, AniListSeriesData>(resultContext.Failed(string.Empty)).AsTask(),
                    (lastResult, currentTitle) =>
                    {
                        return lastResult.Bind(e =>
                            e.IsLeft ? FindSingleMatchingSeries(currentTitle, resultContext) : e.AsTask());
                    });

            return sourceDataTask.BindAsync(sd => CreateSourceDataWithTitle(mediaItem, sd, resultContext));
        }

        private Either<ProcessFailedResult, ISourceData> CreateSourceDataWithTitle(IMediaItem mediaItem,
            AniListSeriesData seriesData, ProcessResultContext resultContext)
        {
            return this.sources.AniList.SelectTitle(seriesData.Title, mediaItem.EmbyData.Language, resultContext)
                .Map(t => CreateSourceData(seriesData, t));
        }

        private Task<Either<ProcessFailedResult, AniListSeriesData>> FindSingleMatchingSeries(string title,
            ProcessResultContext resultContext)
        {
            return this.aniListClient.FindSeriesAsync(title, resultContext)
                .BindAsync(aniListSeriesData => FailUnlessOneResult(aniListSeriesData, resultContext));
        }

        private static Either<ProcessFailedResult, AniListSeriesData> FailUnlessOneResult(
            IEnumerable<AniListSeriesData> aniListSeriesData, ProcessResultContext resultContext)
        {
            return aniListSeriesData.Match(
                () => resultContext.Failed("No matching AniList series"),
                single => Right<ProcessFailedResult, AniListSeriesData>(single),
                (head, tail) =>
                    resultContext.Failed($"Found too many ({SeqExtensions.Count(tail) + 1}) matching AniList series"));
        }

        private ISourceData CreateSourceData(AniListSeriesData seriesData, string title)
        {
            return new SourceData<AniListSeriesData>(this.sources.AniList, seriesData.Id,
                new ItemIdentifier(None, None, title), seriesData);
        }
    }
}
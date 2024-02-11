using Jellyfin.AniDbMetaStructure.AniDb;
using Jellyfin.AniDbMetaStructure.AniDb.SeriesData;
using Jellyfin.AniDbMetaStructure.Process;
using Jellyfin.AniDbMetaStructure.Process.Sources;
using LanguageExt;
using System.Threading.Tasks;

namespace Jellyfin.AniDbMetaStructure.SourceDataLoaders
{
    /// <summary>
    ///     Loads series data from AniDb based on the data provided by Jellyfin
    /// </summary>
    internal class AniDbSeriesFromJellyfinData : IJellyfinSourceDataLoader
    {
        private readonly IAniDbClient aniDbClient;
        private readonly ISources sources;

        public AniDbSeriesFromJellyfinData(IAniDbClient aniDbClient, ISources sources)
        {
            this.aniDbClient = aniDbClient;
            this.sources = sources;
        }

        public SourceName SourceName => SourceNames.AniDb;

        public bool CanLoadFrom(IMediaItemType mediaItemType)
        {
            return mediaItemType == MediaItemTypes.Series;
        }

        public Task<Either<ProcessFailedResult, ISourceData>> LoadFrom(IJellyfinItemData JellyfinItemData)
        {
            var resultContext = new ProcessResultContext(nameof(AniDbSeriesFromJellyfinData), JellyfinItemData.Identifier.Name,
                JellyfinItemData.ItemType);

            return this.aniDbClient.FindSeriesAsync(JellyfinItemData.Identifier.Name)
                .ToEitherAsync(resultContext.Failed("Failed to find series in AniDb"))
                .BindAsync(s =>
                {
                    var title = this.sources.AniDb.SelectTitle(s.Titles, JellyfinItemData.Language, resultContext);

                    return title.Map(t => CreateSourceData(s, JellyfinItemData, t));
                });
        }

        private ISourceData CreateSourceData(AniDbSeriesData seriesData, IJellyfinItemData JellyfinItemData, string title)
        {
            return new SourceData<AniDbSeriesData>(this.sources.AniDb, seriesData.Id,
                new ItemIdentifier(JellyfinItemData.Identifier.Index, Option<int>.None, title), seriesData);
        }
    }
}
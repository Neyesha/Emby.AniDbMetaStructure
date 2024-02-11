using Jellyfin.AniDbMetaStructure.AniDb;
using Jellyfin.AniDbMetaStructure.AniDb.SeriesData;
using Jellyfin.AniDbMetaStructure.SourceDataLoaders;
using LanguageExt;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jellyfin.AniDbMetaStructure.Process.Sources
{
    internal class AniDbSource : IAniDbSource
    {
        private readonly IAniDbClient aniDbClient;
        private readonly IEnumerable<IJellyfinSourceDataLoader> JellyfinSourceDataLoaders;
        private readonly ITitlePreferenceConfiguration titlePreferenceConfiguration;
        private readonly IAniDbTitleSelector titleSelector;

        public AniDbSource(IAniDbClient aniDbClient, ITitlePreferenceConfiguration titlePreferenceConfiguration,
            IAniDbTitleSelector titleSelector, IEnumerable<IJellyfinSourceDataLoader> JellyfinSourceDataLoaders)
        {
            this.aniDbClient = aniDbClient;
            this.titlePreferenceConfiguration = titlePreferenceConfiguration;
            this.titleSelector = titleSelector;
            this.JellyfinSourceDataLoaders = JellyfinSourceDataLoaders;
        }

        public SourceName Name => SourceNames.AniDb;

        public Either<ProcessFailedResult, IJellyfinSourceDataLoader> GetJellyfinSourceDataLoader(IMediaItemType mediaItemType)
        {
            return this.JellyfinSourceDataLoaders.Find(l => l.SourceName == Name && l.CanLoadFrom(mediaItemType))
                .ToEither(new ProcessFailedResult(Name, string.Empty, mediaItemType,
                    "No Jellyfin source data loader for this source and media item type"));
        }

        public bool ShouldUsePlaceholderSourceData(IMediaItemType mediaItemType)
        {
            return mediaItemType == MediaItemTypes.Series;
        }

        public Task<Either<ProcessFailedResult, AniDbSeriesData>> GetSeriesData(IJellyfinItemData JellyfinItemData,
            ProcessResultContext resultContext)
        {
            return JellyfinItemData.GetParentId(MediaItemTypes.Series, this)
                .ToEitherAsync(
                    resultContext.Failed("No AniDb Id found on parent series"))
                .BindAsync(aniDbSeriesId => this.aniDbClient.GetSeriesAsync(aniDbSeriesId)
                    .ToEitherAsync(
                        resultContext.Failed($"Failed to load parent series with AniDb Id '{aniDbSeriesId}'")));
        }

        public Either<ProcessFailedResult, string> SelectTitle(IEnumerable<ItemTitleData> titles,
            string metadataLanguage, ProcessResultContext resultContext)
        {
            return this.titleSelector.SelectTitle(titles, this.titlePreferenceConfiguration.TitlePreference, metadataLanguage)
                .Map(t => t.Title)
                .ToEither(resultContext.Failed("Failed to find a title"));
        }

        public Task<Either<ProcessFailedResult, AniDbSeriesData>> GetSeriesData(int aniDbSeriesId,
            ProcessResultContext resultContext)
        {
            return this.aniDbClient.GetSeriesAsync(aniDbSeriesId)
                .ToEitherAsync(resultContext.Failed($"Failed to load series with AniDb Id '{aniDbSeriesId}'"));
        }
    }
}
﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Emby.AniDbMetaStructure.AniDb;
using Emby.AniDbMetaStructure.AniDb.SeriesData;
using Emby.AniDbMetaStructure.SourceDataLoaders;
using LanguageExt;

namespace Emby.AniDbMetaStructure.Process.Sources
{
    internal class AniDbSource : IAniDbSource
    {
        private readonly IAniDbClient aniDbClient;
        private readonly IEnumerable<IEmbySourceDataLoader> embySourceDataLoaders;
        private readonly ITitlePreferenceConfiguration titlePreferenceConfiguration;
        private readonly IAniDbTitleSelector titleSelector;

        public AniDbSource(IAniDbClient aniDbClient, ITitlePreferenceConfiguration titlePreferenceConfiguration,
            IAniDbTitleSelector titleSelector, IEnumerable<IEmbySourceDataLoader> embySourceDataLoaders)
        {
            this.aniDbClient = aniDbClient;
            this.titlePreferenceConfiguration = titlePreferenceConfiguration;
            this.titleSelector = titleSelector;
            this.embySourceDataLoaders = embySourceDataLoaders;
        }

        public SourceName Name => SourceNames.AniDb;

        public Either<ProcessFailedResult, IEmbySourceDataLoader> GetEmbySourceDataLoader(IMediaItemType mediaItemType)
        {
            return this.embySourceDataLoaders.Find(l => l.SourceName == this.Name && l.CanLoadFrom(mediaItemType))
                .ToEither(new ProcessFailedResult(this.Name, string.Empty, mediaItemType,
                    "No Emby source data loader for this source and media item type"));
        }

        public bool ShouldUsePlaceholderSourceData(IMediaItemType mediaItemType)
        {
            return mediaItemType == MediaItemTypes.Series;
        }

        public Task<Either<ProcessFailedResult, AniDbSeriesData>> GetSeriesData(IEmbyItemData embyItemData,
            ProcessResultContext resultContext)
        {
            return embyItemData.GetParentId(MediaItemTypes.Series, this)
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
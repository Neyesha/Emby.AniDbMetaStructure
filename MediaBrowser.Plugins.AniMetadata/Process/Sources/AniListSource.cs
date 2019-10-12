﻿using System.Collections.Generic;
using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniList;
using MediaBrowser.Plugins.AniMetadata.AniList.Data;
using MediaBrowser.Plugins.AniMetadata.SourceDataLoaders;

namespace MediaBrowser.Plugins.AniMetadata.Process.Sources
{
    internal class AniListSource : IAniListSource
    {
        private readonly IAniListNameSelector aniListNameSelector;
        private readonly IEnumerable<IEmbySourceDataLoader> embySourceDataLoaders;
        private readonly ITitlePreferenceConfiguration titlePreferenceConfiguration;

        public AniListSource(ITitlePreferenceConfiguration titlePreferenceConfiguration,
            IEnumerable<IEmbySourceDataLoader> embySourceDataLoaders, IAniListNameSelector aniListNameSelector)
        {
            this.titlePreferenceConfiguration = titlePreferenceConfiguration;
            this.embySourceDataLoaders = embySourceDataLoaders;
            this.aniListNameSelector = aniListNameSelector;
        }

        public SourceName Name => SourceNames.AniList;

        public Either<ProcessFailedResult, IEmbySourceDataLoader> GetEmbySourceDataLoader(IMediaItemType mediaItemType)
        {
            return this.embySourceDataLoaders.Find(l => l.SourceName == Name && l.CanLoadFrom(mediaItemType))
                .ToEither(new ProcessFailedResult(Name, string.Empty, mediaItemType,
                    "No Emby source data loader for this source and media item type"));
        }

        public Either<ProcessFailedResult, string> SelectTitle(AniListTitleData titleData,
            string metadataLanguage, ProcessResultContext resultContext)
        {
            return this.aniListNameSelector
                .SelectTitle(titleData, this.titlePreferenceConfiguration.TitlePreference, metadataLanguage)
                .ToEither(resultContext.Failed("Failed to find a title"));
        }
    }
}
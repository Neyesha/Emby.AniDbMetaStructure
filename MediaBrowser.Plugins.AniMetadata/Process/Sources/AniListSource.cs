using Jellyfin.AniDbMetaStructure.AniList;
using Jellyfin.AniDbMetaStructure.AniList.Data;
using Jellyfin.AniDbMetaStructure.SourceDataLoaders;
using LanguageExt;
using System.Collections.Generic;

namespace Jellyfin.AniDbMetaStructure.Process.Sources
{
    internal class AniListSource : IAniListSource
    {
        private readonly IAniListNameSelector aniListNameSelector;
        private readonly IEnumerable<IJellyfinSourceDataLoader> embySourceDataLoaders;
        private readonly ITitlePreferenceConfiguration titlePreferenceConfiguration;

        public AniListSource(ITitlePreferenceConfiguration titlePreferenceConfiguration,
            IEnumerable<IJellyfinSourceDataLoader> embySourceDataLoaders, IAniListNameSelector aniListNameSelector)
        {
            this.titlePreferenceConfiguration = titlePreferenceConfiguration;
            this.embySourceDataLoaders = embySourceDataLoaders;
            this.aniListNameSelector = aniListNameSelector;
        }

        public SourceName Name => SourceNames.AniList;

        public Either<ProcessFailedResult, IJellyfinSourceDataLoader> GetJellyfinSourceDataLoader(IMediaItemType mediaItemType)
        {
            return this.embySourceDataLoaders.Find(l => l.SourceName == Name && l.CanLoadFrom(mediaItemType))
                .ToEither(new ProcessFailedResult(Name, string.Empty, mediaItemType,
                    "No Emby source data loader for this source and media item type"));
        }

        public bool ShouldUsePlaceholderSourceData(IMediaItemType mediaItemType)
        {
            return false;
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
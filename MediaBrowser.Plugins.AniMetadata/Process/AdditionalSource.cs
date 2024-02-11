using Jellyfin.AniDbMetaStructure.Process.Sources;
using Jellyfin.AniDbMetaStructure.SourceDataLoaders;
using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.Process
{
    /// <summary>
    ///     A source used as the source of additional data (data related to but not directly about the media item being
    ///     processed)
    ///     E.g. The series of an episode
    /// </summary>
    internal class AdditionalSource : ISource
    {
        private readonly ISource sourceImplementation;

        public AdditionalSource(ISource sourceImplementation)
        {
            this.sourceImplementation = sourceImplementation;
            Name = new SourceName(this.sourceImplementation.Name + "_Additional");
        }

        public SourceName Name { get; }

        public Either<ProcessFailedResult, IJellyfinSourceDataLoader> GetJellyfinSourceDataLoader(IMediaItemType mediaItemType)
        {
            return this.sourceImplementation.GetJellyfinSourceDataLoader(mediaItemType);
        }

        public bool ShouldUsePlaceholderSourceData(IMediaItemType mediaItemType)
        {
            return false;
        }
    }
}
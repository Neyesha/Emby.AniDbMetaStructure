using Jellyfin.AniDbMetaStructure.Configuration;
using LanguageExt;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;

namespace Jellyfin.AniDbMetaStructure.Process
{
    public interface IMediaItemType
    {
    }

    internal interface IMediaItemType<TJellyfinItem> : IMediaItemType where TJellyfinItem : BaseItem
    {
        Either<ProcessFailedResult, IMetadataFoundResult<TJellyfinItem>> CreateMetadataFoundResult(
            IPluginConfiguration pluginConfiguration, IMediaItem mediaItem, ILogger logger);
    }
}
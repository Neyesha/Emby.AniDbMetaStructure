using Jellyfin.AniDbMetaStructure.Configuration;
using LanguageExt;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;

namespace Jellyfin.AniDbMetaStructure.Process
{
    public interface IMediaItemType
    {
    }

    internal interface IMediaItemType<TEmbyItem> : IMediaItemType where TEmbyItem : BaseItem
    {
        Either<ProcessFailedResult, IMetadataFoundResult<TEmbyItem>> CreateMetadataFoundResult(
            IPluginConfiguration pluginConfiguration, IMediaItem mediaItem, ILogger logger);
    }
}
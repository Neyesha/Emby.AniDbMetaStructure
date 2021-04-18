using Jellyfin.AniDbMetaStructure.Configuration;
using LanguageExt;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jellyfin.AniDbMetaStructure.Process
{
    internal class MediaItemProcessor : IMediaItemProcessor
    {
        private readonly ILogger logger;
        private readonly IMediaItemBuilder mediaItemBuilder;
        private readonly IPluginConfiguration pluginConfiguration;

        public MediaItemProcessor(IPluginConfiguration pluginConfiguration, IMediaItemBuilder mediaItemBuilder,
            ILogger logger)
        {
            this.pluginConfiguration = pluginConfiguration;
            this.mediaItemBuilder = mediaItemBuilder;
            this.logger = logger;
        }

        public Task<Either<ProcessFailedResult, IMetadataFoundResult<TEmbyItem>>> GetResultAsync<TEmbyItem>(
            ItemLookupInfo embyInfo, IMediaItemType<TEmbyItem> itemType, IEnumerable<EmbyItemId> parentIds)
            where TEmbyItem : BaseItem
        {
            var embyItemData = ToEmbyItemData(embyInfo, itemType, parentIds);

            this.logger.LogDebug($"Finding metadata for {embyItemData}");

            var mediaItem = this.mediaItemBuilder.Identify(embyItemData, itemType);

            var fullyRecognisedMediaItem = mediaItem.BindAsync(this.mediaItemBuilder.BuildMediaItem);

            return fullyRecognisedMediaItem.BindAsync(
                    mi => itemType.CreateMetadataFoundResult(this.pluginConfiguration, mi, this.logger))
                .MapAsync(r =>
                {
                    this.logger.LogDebug(
                        $"Created metadata with provider Ids: {string.Join(", ", r.EmbyMetadataResult.Item.ProviderIds.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}");
                    return r;
                });
        }

        private EmbyItemData ToEmbyItemData<TEmbyItem>(ItemLookupInfo embyInfo, IMediaItemType<TEmbyItem> itemType,
            IEnumerable<EmbyItemId> parentIds)
            where TEmbyItem : BaseItem
        {
            var existingIds = embyInfo.ProviderIds.Where(v => int.TryParse(v.Value, out _))
                .ToDictionary(k => k.Key, v => int.Parse(v.Value));

            return new EmbyItemData(itemType,
                new ItemIdentifier(embyInfo.IndexNumber.ToOption(), embyInfo.ParentIndexNumber.ToOption(),
                    embyInfo.Name), existingIds, embyInfo.MetadataLanguage, parentIds);
        }
    }
}
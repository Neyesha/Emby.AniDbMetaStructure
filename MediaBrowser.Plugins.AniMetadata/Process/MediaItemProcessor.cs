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

        public Task<Either<ProcessFailedResult, IMetadataFoundResult<TJellyfinItem>>> GetResultAsync<TJellyfinItem>(
            ItemLookupInfo JellyfinInfo, IMediaItemType<TJellyfinItem> itemType, IEnumerable<JellyfinItemId> parentIds)
            where TJellyfinItem : BaseItem
        {
            var JellyfinItemData = ToJellyfinItemData(JellyfinInfo, itemType, parentIds);

            this.logger.LogDebug($"Finding metadata for {JellyfinItemData}");

            var mediaItem = this.mediaItemBuilder.Identify(JellyfinItemData, itemType);

            var fullyRecognisedMediaItem = mediaItem.BindAsync(this.mediaItemBuilder.BuildMediaItem);

            return fullyRecognisedMediaItem.BindAsync(
                    mi => itemType.CreateMetadataFoundResult(this.pluginConfiguration, mi, this.logger))
                .MapAsync(r =>
                {
                    this.logger.LogDebug(
                        $"Created metadata with provider Ids: {string.Join(", ", r.JellyfinMetadataResult.Item.ProviderIds.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}");
                    return r;
                });
        }

        private JellyfinItemData ToJellyfinItemData<TJellyfinItem>(ItemLookupInfo JellyfinInfo, IMediaItemType<TJellyfinItem> itemType,
            IEnumerable<JellyfinItemId> parentIds)
            where TJellyfinItem : BaseItem
        {
            var existingIds = JellyfinInfo.ProviderIds.Where(v => int.TryParse(v.Value, out _))
                .ToDictionary(k => k.Key, v => int.Parse(v.Value));

            return new JellyfinItemData(itemType,
                new ItemIdentifier(JellyfinInfo.IndexNumber.ToOption(), JellyfinInfo.ParentIndexNumber.ToOption(),
                    JellyfinInfo.Name), existingIds, JellyfinInfo.MetadataLanguage, parentIds);
        }
    }
}
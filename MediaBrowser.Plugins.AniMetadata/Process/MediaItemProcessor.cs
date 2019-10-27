﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Emby.AniDbMetaStructure.Configuration;
using LanguageExt;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;

namespace Emby.AniDbMetaStructure.Process
{
    internal class MediaItemProcessor : IMediaItemProcessor
    {
        private readonly ILogger log;
        private readonly IMediaItemBuilder mediaItemBuilder;
        private readonly ILogManager logManager;
        private readonly IPluginConfiguration pluginConfiguration;

        public MediaItemProcessor(IPluginConfiguration pluginConfiguration, IMediaItemBuilder mediaItemBuilder,
            ILogManager logManager)
        {
            this.pluginConfiguration = pluginConfiguration;
            this.mediaItemBuilder = mediaItemBuilder;
            this.logManager = logManager;
            this.log = logManager.GetLogger(nameof(MediaItemProcessor));
        }

        public Task<Either<ProcessFailedResult, IMetadataFoundResult<TEmbyItem>>> GetResultAsync<TEmbyItem>(
            ItemLookupInfo embyInfo, IMediaItemType<TEmbyItem> itemType, IEnumerable<EmbyItemId> parentIds)
            where TEmbyItem : BaseItem
        {
            var embyItemData = this.ToEmbyItemData(embyInfo, itemType, parentIds);

            this.log.Debug($"Finding metadata for {embyItemData}");

            var mediaItem = this.mediaItemBuilder.Identify(embyItemData, itemType);

            var fullyRecognisedMediaItem = mediaItem.BindAsync(this.mediaItemBuilder.BuildMediaItem);

            return fullyRecognisedMediaItem.BindAsync(
                    mi => itemType.CreateMetadataFoundResult(this.pluginConfiguration, mi, this.logManager))
                .MapAsync(r =>
                {
                    this.log.Debug(
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
using Jellyfin.AniDbMetaStructure.Configuration;
using Jellyfin.AniDbMetaStructure.Process.Sources;
using Jellyfin.AniDbMetaStructure.PropertyMapping;
using LanguageExt;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using static LanguageExt.Prelude;

namespace Jellyfin.AniDbMetaStructure.Process
{
    internal class MediaItemType<TEmbyItem> : IMediaItemType<TEmbyItem> where TEmbyItem : BaseItem, new()
    {
        private readonly string name;
        private readonly Func<IPluginConfiguration, string, IPropertyMappingCollection> propertyMappingsFactory;

        internal MediaItemType(string name,
            Func<IPluginConfiguration, string, IPropertyMappingCollection> propertyMappingsFactory)
        {
            this.name = name;
            this.propertyMappingsFactory = propertyMappingsFactory;
        }

        public Either<ProcessFailedResult, IMetadataFoundResult<TEmbyItem>> CreateMetadataFoundResult(
            IPluginConfiguration pluginConfiguration, IMediaItem mediaItem, ILogger logger)
        {
            var resultContext = new ProcessResultContext("PropertyMapping", mediaItem.EmbyData.Identifier.Name,
                mediaItem.ItemType);

            var metadataResult = new MetadataResult<TEmbyItem>
            {
                Item = new TEmbyItem(),
                HasMetadata = true
            };

            var propertyMappings = this.propertyMappingsFactory(pluginConfiguration, mediaItem.EmbyData.Language);
            var sourceData = mediaItem.GetAllSourceData().ToList();

            var mediaItemMetadata = sourceData.Select(sd => sd.GetData<object>()).Somes();

            metadataResult = propertyMappings.Apply(mediaItemMetadata, metadataResult, s => logger.LogDebug(s));

            metadataResult = UpdateProviderIds(metadataResult, sourceData);

            var mappedMetadataResult = Option<MetadataResult<TEmbyItem>>.Some(metadataResult);

            return mappedMetadataResult.ToEither(resultContext.Failed("Property mapping returned no data"))
                .Bind(m => mediaItem.GetDataFromSource(pluginConfiguration.LibraryStructureSource(mediaItem.ItemType))
                    .ToEither(resultContext.Failed("No data returned by library structure source"))
                    .Bind(sd => SetIdentity(sd, m, propertyMappings, pluginConfiguration.LibraryStructureSource(mediaItem.ItemType).Name,
                        resultContext)))
                .Match(r => string.IsNullOrWhiteSpace(r.Item.Name)
                        ? Left<ProcessFailedResult, MetadataResult<TEmbyItem>>(
                            resultContext.Failed("Property mapping failed for the Name property"))
                        : Right<ProcessFailedResult, MetadataResult<TEmbyItem>>(r),
                    failure => failure)
                .Map(r => (IMetadataFoundResult<TEmbyItem>)new MetadataFoundResult<TEmbyItem>(mediaItem, r));
        }

        private MetadataResult<TEmbyItem> UpdateProviderIds(MetadataResult<TEmbyItem> metadataResult,
            IEnumerable<ISourceData> sourceData)
        {
            return sourceData
                .Where(sd => !sd.Source.IsForAdditionalData())
                .Aggregate(metadataResult, (r, sd) =>
                {
                    return sd.Id.Match(id =>
                        {
                            r.Item.SetProviderId(sd.Source.Name, id.ToString());

                            return r;
                        },
                        () =>
                        {
                            if (r.Item.ProviderIds.ContainsKey(sd.Source.Name))
                            {
                                r.Item.ProviderIds.Remove(sd.Source.Name);
                            }

                            return r;
                        });
                });
        }

        private Either<ProcessFailedResult, MetadataResult<TEmbyItem>> SetIdentity(ISourceData librarySourceData,
            MetadataResult<TEmbyItem> target, IPropertyMappingCollection propertyMappings,
            SourceName librarySourceName, ProcessResultContext resultContext)
        {
            return SetIndexes(librarySourceData, target)
                .Bind(r => SetName(librarySourceData.Data, r, propertyMappings, librarySourceName, resultContext));
        }

        private Either<ProcessFailedResult, MetadataResult<TEmbyItem>> SetIndexes(ISourceData librarySourceData,
            MetadataResult<TEmbyItem> target)
        {
            return Right<ProcessFailedResult, MetadataResult<TEmbyItem>>(target)
                .Map(r => librarySourceData.Identifier.Index
                    .Map(index =>
                    {
                        r.Item.IndexNumber = index;
                        return r;
                    })
                    .Match(r2 => librarySourceData.Identifier.ParentIndex.Match(parentIndex =>
                    {
                        r2.Item.ParentIndexNumber = parentIndex;
                        return r2;
                    }, () => r2), () => r));
        }

        private Either<ProcessFailedResult, MetadataResult<TEmbyItem>> SetName(object source,
            MetadataResult<TEmbyItem> target, IPropertyMappingCollection propertyMappings,
            SourceName librarySourceName, ProcessResultContext resultContext)
        {
            return Option<IPropertyMapping>.Some(propertyMappings.FirstOrDefault(m =>
                    m.CanApply(source, target) && m.TargetPropertyName == nameof(target.Item.Name)))
                .Map(m =>
                {
                    m.Apply(source, target);
                    return target;
                })
                .ToEither(resultContext.Failed("No value for Name property mapped from library source"));
        }

        public override string ToString()
        {
            return $"{this.name}";
        }
    }
}
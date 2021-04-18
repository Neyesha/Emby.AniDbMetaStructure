using Jellyfin.AniDbMetaStructure.Configuration;
using Jellyfin.AniDbMetaStructure.SourceDataLoaders;
using LanguageExt;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace Jellyfin.AniDbMetaStructure.Process
{
    internal class MediaItemBuilder : IMediaItemBuilder
    {
        private readonly IPluginConfiguration pluginConfiguration;
        private readonly IEnumerable<ISourceDataLoader> sourceDataLoaders;
        private readonly ILogger logger;

        public MediaItemBuilder(IPluginConfiguration pluginConfiguration,
            IEnumerable<ISourceDataLoader> sourceDataLoaders, ILogger logger)
        {
            this.pluginConfiguration = pluginConfiguration;
            this.sourceDataLoaders = sourceDataLoaders;
            this.logger = logger;
        }

        public Task<Either<ProcessFailedResult, IMediaItem>> Identify(EmbyItemData embyItemData,
            IMediaItemType itemType)
        {
            return IdentifyAsync(embyItemData, itemType).MapAsync(sd => (IMediaItem)new MediaItem(embyItemData, itemType, sd));
        }

        public Task<Either<ProcessFailedResult, IMediaItem>> BuildMediaItem(IMediaItem rootMediaItem)
        {
            return AddDataFromSourcesAsync(Right<ProcessFailedResult, IMediaItem>(rootMediaItem).AsTask(),
                this.sourceDataLoaders.ToImmutableList());

            Task<Either<ProcessFailedResult, IMediaItem>> AddDataFromSourcesAsync(
                Task<Either<ProcessFailedResult, IMediaItem>> mediaItem,
                ImmutableList<ISourceDataLoader> sourceDataLoaders)
            {
                int sourceLoaderCount = sourceDataLoaders.Count;

                var mediaItemTask = sourceDataLoaders.Aggregate(mediaItem,
                    (miTask, l) =>
                        miTask.MapAsync(mi => mi.GetAllSourceData().Find(l.CanLoadFrom)
                            .MatchAsync(sd =>
                                {
                                    this.logger.LogDebug($"Loading source data using {l.GetType().FullName}");
                                    return l.LoadFrom(mi, sd)
                                        .Map(e => e.Match(
                                            newSourceData =>
                                            {
                                                this.logger.LogDebug($"Loaded {sd.Source.Name} source data: {sd.Identifier}");
                                                sourceDataLoaders = sourceDataLoaders.Remove(l);
                                                return mi.AddData(newSourceData).IfLeft(() =>
                                                {
                                                    this.logger.LogWarning($"Failed to add source data: {sd.Identifier}");
                                                    return mi;
                                                });
                                            },
                                            fail =>
                                            {
                                                this.logger.LogDebug($"Failed to load source data: {fail.Reason}");
                                                return mi;
                                            }));
                                },
                                () => mi)));

                return mediaItemTask.BindAsync(mi =>
                {
                    bool wasSourceDataAdded = sourceLoaderCount != sourceDataLoaders.Count;

                    var mediaItemAsEither = Right<ProcessFailedResult, IMediaItem>(mi).AsTask();

                    return wasSourceDataAdded
                        ? AddDataFromSourcesAsync(mediaItemAsEither, sourceDataLoaders)
                        : mediaItemAsEither;
                });
            }
        }

        private Task<Either<ProcessFailedResult, ISourceData>> IdentifyAsync(EmbyItemData embyItemData, IMediaItemType itemType)
        {
            var identifyingSource = this.pluginConfiguration.FileStructureSource(itemType);

            return identifyingSource.GetEmbySourceDataLoader(embyItemData.ItemType)
                .BindAsync(l => l.LoadFrom(embyItemData));
        }
    }
}
using Emby.AniDbMetaStructure.Configuration;
using Emby.AniDbMetaStructure.Infrastructure;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace Emby.AniDbMetaStructure.Process.Providers
{
    internal class SeriesProvider
    {
        private readonly ILogger logger;
        private readonly IMediaItemProcessor mediaItemProcessor;
        private readonly IPluginConfiguration pluginConfiguration;

        public SeriesProvider(ILogger logger, IMediaItemProcessor mediaItemProcessor,
            IPluginConfiguration pluginConfiguration)
        {
            this.mediaItemProcessor = mediaItemProcessor;
            this.pluginConfiguration = pluginConfiguration;
            this.logger = logger;
        }

        public int Order => -1;

        public string Name => "AniDbMetaStructure";

        private MetadataResult<Series> EmptyMetadataResult => new MetadataResult<Series>
        {
            Item = new Series(),
            HasMetadata = false
        };

        public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(SeriesInfo searchInfo,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<MetadataResult<Series>> GetMetadata(SeriesInfo info, CancellationToken cancellationToken)
        {
            var metadataResult = Try(() =>
                {
                    if (this.pluginConfiguration.ExcludedSeriesNames.Contains(info.Name,
                        StringComparer.InvariantCultureIgnoreCase))
                    {
                        this.logger.LogInformation($"Skipping series '{info.Name}' as it is excluded");

                        return EmptyMetadataResult.AsTask();
                    }

                    var result =
                        this.mediaItemProcessor.GetResultAsync(info, MediaItemTypes.Series, Enumerable.Empty<EmbyItemId>());

                    return result.Map(either =>
                        either.Match(r =>
                            {
                                this.logger.LogInformation($"Found data for series '{info.Name}': '{r.EmbyMetadataResult.Item.Name}'");

                                info.IndexNumber = null;
                                info.ParentIndexNumber = null;
                                info.Name = string.Empty;
                                info.ProviderIds = new Dictionary<string, string>().ToProviderIdDictionary();

                                return r.EmbyMetadataResult;
                            },
                            failure =>
                            {
                                this.logger.LogError($"Failed to get data for series '{info.Name}': {failure.Reason}");

                                return EmptyMetadataResult;
                            })
                    );
                })
                .IfFail(e =>
                {
                    this.logger.LogError($"Failed to get data for series '{info.Name}'", e);

                    return EmptyMetadataResult.AsTask();
                });

            return metadataResult;
        }

        public Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}
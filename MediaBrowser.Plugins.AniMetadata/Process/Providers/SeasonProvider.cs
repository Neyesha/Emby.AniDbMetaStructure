using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace Jellyfin.AniDbMetaStructure.Process.Providers
{
    internal class SeasonProvider
    {
        private readonly ILogger logger;
        private readonly IMediaItemProcessor mediaItemProcessor;

        public SeasonProvider(ILogger logger, IMediaItemProcessor mediaItemProcessor)
        {
            this.mediaItemProcessor = mediaItemProcessor;
            this.logger = logger;
        }

        private MetadataResult<Season> EmptyMetadataResult => new MetadataResult<Season>
        {
            Item = new Season(),
            HasMetadata = false
        };

        public string Name => "AniDbMetaStructure";

        public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(SeasonInfo searchInfo,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<MetadataResult<Season>> GetMetadata(SeasonInfo info, CancellationToken cancellationToken)
        {
            var metadataResult = Try(() =>
                {
                    var result =
                        this.mediaItemProcessor.GetResultAsync(info, MediaItemTypes.Season, GetParentIds(info));

                    return result.Map(either =>
                        either.Match(r =>
                            {
                                this.logger.LogInformation($"Found data for season '{info.Name}': '{r.JellyfinMetadataResult.Item.Name}'");

                                info.IndexNumber = null;
                                info.ParentIndexNumber = null;
                                info.Name = string.Empty;
                                info.ProviderIds = new Dictionary<string, string>();

                                return r.JellyfinMetadataResult;
                            },
                            failure =>
                            {
                                this.logger.LogError($"Failed to get data for season '{info.Name}': {failure.Reason}");

                                return EmptyMetadataResult;
                            })
                    );
                })
                .IfFail(e =>
                {
                    this.logger.LogError($"Failed to get data for season '{info.Name}'", e);

                    return EmptyMetadataResult.AsTask();
                });

            return metadataResult;
        }

        public Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        private IEnumerable<JellyfinItemId> GetParentIds(SeasonInfo info)
        {
            return info.SeriesProviderIds.Where(kv => int.TryParse(kv.Value, out _))
                .Select(kv => new JellyfinItemId(MediaItemTypes.Series, kv.Key, int.Parse(kv.Value)));
        }
    }
}
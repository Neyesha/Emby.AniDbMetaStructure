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
    internal class EpisodeProvider
    {
        private readonly ILogger logger;
        private readonly IMediaItemProcessor mediaItemProcessor;

        public EpisodeProvider(ILogger logger, IMediaItemProcessor mediaItemProcessor)
        {
            this.mediaItemProcessor = mediaItemProcessor;
            this.logger = logger;
        }

        public int Order => -1;

        public string Name => "AniDbMetaStructure";

        private MetadataResult<Episode> EmptyMetadataResult => new MetadataResult<Episode>
        {
            Item = new Episode(),
            HasMetadata = false
        };

        public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(EpisodeInfo searchInfo,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<MetadataResult<Episode>> GetMetadata(EpisodeInfo info, CancellationToken cancellationToken)
        {
            var metadataResult = Try(() =>
                {
                    var result = this.mediaItemProcessor.GetResultAsync(info, MediaItemTypes.Episode, GetParentIds(info));

                    return result.Map(either =>
                        either.Match(r =>
                            {
                                this.logger.LogInformation($"Found data for episode '{info.Name}': '{r.EmbyMetadataResult.Item.Name}'");

                                info.IndexNumber = null;
                                info.ParentIndexNumber = null;
                                info.Name = string.Empty;
                                info.ProviderIds = new Dictionary<string, string>();

                                return r.EmbyMetadataResult;
                            },
                            failure =>
                            {
                                this.logger.LogError($"Failed to get data for episode '{info.Name}': {failure.Reason}");

                                return EmptyMetadataResult;
                            })
                    );
                })
                .IfFail(e =>
                {
                    this.logger.LogError($"Failed to get data for episode '{info.Name}'", e);

                    return EmptyMetadataResult.AsTask();
                });

            return metadataResult;
        }

        public Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        private IEnumerable<JellyfinItemId> GetParentIds(EpisodeInfo info)
        {
            this.logger.LogInformation($"ParentIds: '{info.SeriesProviderIds}'");
            return info.SeriesProviderIds.Where(kv => int.TryParse(kv.Value, out _))
                .Select(kv => new JellyfinItemId(MediaItemTypes.Series, kv.Key, int.Parse(kv.Value)));
        }
    }
}
using Jellyfin.AniDbMetaStructure.AniDb;
using Jellyfin.AniDbMetaStructure.AniDb.Seiyuu;
using Jellyfin.AniDbMetaStructure.Infrastructure;
using Jellyfin.AniDbMetaStructure.Process.Sources;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace Jellyfin.AniDbMetaStructure.Providers.AniDb
{
    public class AniDbPersonProvider
    {
        private readonly IAniDbClient aniDbClient;
        private readonly HttpClient httpClient;
        private readonly ILogger logger;
        private readonly IRateLimiter rateLimiter;

        public AniDbPersonProvider(IAniDbClient aniDbClient, IRateLimiters rateLimiters, HttpClient httpClient, ILogger logger)
        {
            this.rateLimiter = rateLimiters.AniDb;
            this.aniDbClient = aniDbClient;
            this.httpClient = httpClient;
            this.logger = logger;
        }

        public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(PersonLookupInfo searchInfo, CancellationToken cancellationToken)
        {
            this.logger.LogDebug(
                $"Searching for person name: '{searchInfo.Name}', id: '{searchInfo.ProviderIds.GetOrDefault(SourceNames.AniDb)}'");

            var result = Enumerable.Empty<RemoteSearchResult>();

            if (!string.IsNullOrWhiteSpace(searchInfo.Name))
            {
                result = this.aniDbClient.FindSeiyuu(searchInfo.Name).Select(ToSearchResult);
            }
            else if (searchInfo.ProviderIds.ContainsKey(SourceNames.AniDb))
            {
                string aniDbPersonIdString = searchInfo.ProviderIds[SourceNames.AniDb];

                parseInt(aniDbPersonIdString)
                    .Iter(aniDbPersonId =>
                    {
                        this.aniDbClient.GetSeiyuu(aniDbPersonId)
                            .Iter(s =>
                                result = new[] { ToSearchResult(s) }
                            );
                    });
            }

            this.logger.LogDebug($"Found {result.Count()} results");

            return Task.FromResult(result);
        }

        public Task<MetadataResult<Person>> GetMetadata(PersonLookupInfo info, CancellationToken cancellationToken)
        {
            this.logger.LogDebug(
                $"Getting metadata for person name: '{info.Name}', id: '{info.ProviderIds.GetOrDefault(SourceNames.AniDb)}'");

            var result = new MetadataResult<Person>();

            if (info.ProviderIds.ContainsKey(SourceNames.AniDb))
            {
                string aniDbPersonIdString = info.ProviderIds[SourceNames.AniDb];

                parseInt(aniDbPersonIdString)
                    .Iter(aniDbPersonId =>
                    {
                        this.aniDbClient.GetSeiyuu(aniDbPersonId)
                            .Match(s =>
                                {
                                    result.Item = new Person
                                    {
                                        Name = s.Name,
                                        ImageInfos =
                                            new[]
                                            {
                                                new ItemImageInfo { Type = ImageType.Primary, Path = s.PictureUrl }
                                            },
                                        ProviderIds =
                                            new Dictionary<string, string> { { SourceNames.AniDb, s.Id.ToString() } }
                                    };

                                    this.logger.LogDebug("Found metadata");
                                },
                                () => this.logger.LogDebug("Failed to find metadata"));
                    });
            }

            return Task.FromResult(result);
        }

        public string Name => SourceNames.AniDb;

        public async Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            this.logger.LogDebug($"Getting image: '{url}'");

            await this.rateLimiter.TickAsync().ConfigureAwait(false);

            return await this.httpClient.GetAsync(url, cancellationToken)
                .ConfigureAwait(false);
        }

        private RemoteSearchResult ToSearchResult(SeiyuuData seiyuuData)
        {
            return new RemoteSearchResult
            {
                Name = seiyuuData.Name,
                SearchProviderName = Name,
                ImageUrl = seiyuuData.PictureUrl,
                ProviderIds = new Dictionary<string, string> { { SourceNames.AniDb, seiyuuData.Id.ToString() } }
            };
        }
    }
}
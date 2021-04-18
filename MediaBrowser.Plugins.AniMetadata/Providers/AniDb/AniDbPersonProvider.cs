using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emby.AniDbMetaStructure.AniDb;
using Emby.AniDbMetaStructure.AniDb.Seiyuu;
using Emby.AniDbMetaStructure.Infrastructure;
using Emby.AniDbMetaStructure.Process.Sources;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;
using MediaBrowser.Model.Providers;
using static LanguageExt.Prelude;

namespace Emby.AniDbMetaStructure.Providers.AniDb
{
    public class AniDbPersonProvider
    {
        private readonly IAniDbClient aniDbClient;
        private readonly IHttpClient httpClient;
        private readonly ILogger logger;
        private readonly IRateLimiter rateLimiter;

        public AniDbPersonProvider(IAniDbClient aniDbClient, IRateLimiters rateLimiters, IHttpClient httpClient,
            ILogger logger)
        {
            this.rateLimiter = rateLimiters.AniDb;
            this.aniDbClient = aniDbClient;
            this.httpClient = httpClient;
            this.logger = logger;
        }

        public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(PersonLookupInfo searchInfo,
            CancellationToken cancellationToken)
        {
            this.logger.LogDebug(
                $"Searching for person name: '{searchInfo.Name}', id: '{searchInfo.ProviderIds.GetOrDefault(SourceNames.AniDb)}'");

            var result = Enumerable.Empty<RemoteSearchResult>();

            if (!string.IsNullOrWhiteSpace(searchInfo.Name))
            {
                result = this.aniDbClient.FindSeiyuu(searchInfo.Name).Select(this.ToSearchResult);
            }
            else if (searchInfo.ProviderIds.ContainsKey(SourceNames.AniDb))
            {
                var aniDbPersonIdString = searchInfo.ProviderIds[SourceNames.AniDb];

                parseInt(aniDbPersonIdString)
                    .Iter(aniDbPersonId =>
                    {
                        this.aniDbClient.GetSeiyuu(aniDbPersonId)
                            .Iter(s =>
                                result = new[] { this.ToSearchResult(s) }
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
                var aniDbPersonIdString = info.ProviderIds[SourceNames.AniDb];

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
                                            new Dictionary<string, string> { { SourceNames.AniDb, s.Id.ToString() } }.ToProviderIdDictionary()
                                    };

                                    this.logger.LogDebug("Found metadata");
                                },
                                () => this.logger.LogDebug("Failed to find metadata"));
                    });
            }

            return Task.FromResult(result);
        }

        public string Name => SourceNames.AniDb;

        public async Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            this.logger.LogDebug($"Getting image: '{url}'");

            await this.rateLimiter.TickAsync().ConfigureAwait(false);

            return await this.httpClient.GetResponse(new HttpRequestOptions
                {
                    CancellationToken = cancellationToken,
                    Url = url
                })
                .ConfigureAwait(false);
        }

        private RemoteSearchResult ToSearchResult(SeiyuuData seiyuuData)
        {
            return new RemoteSearchResult
            {
                Name = seiyuuData.Name,
                SearchProviderName = this.Name,
                ImageUrl = seiyuuData.PictureUrl,
                ProviderIds = new Dictionary<string, string> { { SourceNames.AniDb, seiyuuData.Id.ToString() } }.ToProviderIdDictionary()
            };
        }
    }
}
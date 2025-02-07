﻿using Jellyfin.AniDbMetaStructure.AniDb;
using Jellyfin.AniDbMetaStructure.AniDb.SeriesData;
using Jellyfin.AniDbMetaStructure.Infrastructure;
using Jellyfin.AniDbMetaStructure.Process.Sources;
using LanguageExt;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Jellyfin.AniDbMetaStructure.Providers.AniDb
{
    public class AniDbImageProvider
    {
        private readonly IAniDbClient aniDbClient;
        private readonly HttpClient httpClient;
        private readonly ILogger logger;
        private readonly IRateLimiter rateLimiter;

        public AniDbImageProvider(IAniDbClient aniDbClient, IRateLimiters rateLimiters, HttpClient httpClient,
            ILogger logger)
        {
            this.aniDbClient = aniDbClient;
            this.httpClient = httpClient;
            this.rateLimiter = rateLimiters.AniDb;
            this.logger = logger;
        }

        public bool Supports(BaseItem item)
        {
            return item is Series || item is Season;
        }

        public string Name => SourceNames.AniDb;

        public IEnumerable<ImageType> GetSupportedImages(BaseItem item)
        {
            return new[] { ImageType.Primary };
        }

        public async Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item,
            CancellationToken cancellationToken)
        {
            var imageInfos = new List<RemoteImageInfo>();

            var JellyfinSeries = GetJellyfinSeries(item);

            var aniDbSeries =
                await JellyfinSeries.Match(GetAniDbSeriesAsync, () => Task.FromResult(Option<AniDbSeriesData>.None));

            aniDbSeries
                .Match(s =>
                    {
                        var imageUrl = GetImageUrl(s.PictureFileName);

                        imageUrl.Match(url =>
                            {
                                this.logger.LogDebug($"Adding series image: {url}");

                                imageInfos.Add(new RemoteImageInfo
                                {
                                    ProviderName = SourceNames.AniDb,
                                    Url = url
                                });
                            },
                            () => this.logger.LogDebug($"No image Url specified for '{item.Name}'"));
                    },
                    () => this.logger.LogDebug($"Failed to find AniDb series for '{item.Name}'"));

            return imageInfos;
        }

        public async Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            await this.rateLimiter.TickAsync().ConfigureAwait(false);

            return await this.httpClient.GetAsync(url, cancellationToken)
                .ConfigureAwait(false);
        }

        private Option<Series> GetJellyfinSeries(BaseItem item)
        {
            return item as Series ?? (item as Season)?.Series;
        }

        private Task<Option<AniDbSeriesData>> GetAniDbSeriesAsync(Series JellyfinSeries)
        {
            return this.aniDbClient.GetSeriesAsync(JellyfinSeries.ProviderIds.GetOrDefault(SourceNames.AniDb));
        }

        private Option<string> GetImageUrl(string imageFileName)
        {
            var result = Option<string>.None;

            if (!string.IsNullOrWhiteSpace(imageFileName))
            {
                result = $"http://img7.anidb.net/pics/anime/{imageFileName}";
            }

            return result;
        }
    }
}
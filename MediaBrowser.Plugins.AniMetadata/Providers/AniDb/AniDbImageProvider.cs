﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Emby.AniDbMetaStructure.AniDb;
using Emby.AniDbMetaStructure.AniDb.SeriesData;
using Emby.AniDbMetaStructure.Infrastructure;
using Emby.AniDbMetaStructure.Process.Sources;
using LanguageExt;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Providers;

namespace Emby.AniDbMetaStructure.Providers.AniDb
{
    public class AniDbImageProvider
    {
        private readonly IAniDbClient aniDbClient;
        private readonly IHttpClient httpClient;
        private readonly ILogger log;
        private readonly IRateLimiter rateLimiter;

        public AniDbImageProvider(IAniDbClient aniDbClient, IRateLimiters rateLimiters, IHttpClient httpClient,
            ILogManager logManager)
        {
            this.aniDbClient = aniDbClient;
            this.httpClient = httpClient;
            this.rateLimiter = rateLimiters.AniDb;
            this.log = logManager.GetLogger(nameof(AniDbImageProvider));
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

            var embySeries = this.GetEmbySeries(item);

            var aniDbSeries =
                await embySeries.Match(this.GetAniDbSeriesAsync, () => Task.FromResult(Option<AniDbSeriesData>.None));

            aniDbSeries
                .Match(s =>
                    {
                        var imageUrl = this.GetImageUrl(s.PictureFileName);

                        imageUrl.Match(url =>
                            {
                                this.log.Debug($"Adding series image: {url}");

                                imageInfos.Add(new RemoteImageInfo
                                {
                                    ProviderName = SourceNames.AniDb,
                                    Url = url
                                });
                            },
                            () => this.log.Debug($"No image Url specified for '{item.Name}'"));
                    },
                    () => this.log.Debug($"Failed to find AniDb series for '{item.Name}'"));

            return imageInfos;
        }

        public async Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            await this.rateLimiter.TickAsync().ConfigureAwait(false);

            return await this.httpClient.GetResponse(new HttpRequestOptions
                {
                    CancellationToken = cancellationToken,
                    Url = url
                })
                .ConfigureAwait(false);
        }

        private Option<Series> GetEmbySeries(BaseItem item)
        {
            return item as Series ?? (item as Season)?.Series;
        }

        private Task<Option<AniDbSeriesData>> GetAniDbSeriesAsync(Series embySeries)
        {
            return this.aniDbClient.GetSeriesAsync(embySeries.ProviderIds.GetOrDefault(SourceNames.AniDb));
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
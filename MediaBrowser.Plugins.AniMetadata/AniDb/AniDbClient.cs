using Emby.AniDbMetaStructure.AniDb.Seiyuu;
using Emby.AniDbMetaStructure.AniDb.SeriesData;
using Emby.AniDbMetaStructure.AniDb.Titles;
using LanguageExt;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Emby.AniDbMetaStructure.AniDb
{
    /// <summary>
    ///     Retrieves data from AniDb
    /// </summary>
    internal class AniDbClient : IAniDbClient
    {
        private readonly IAniDbDataCache aniDbDataCache;
        private readonly ILogger logger;
        private readonly ISeriesTitleCache seriesTitleCache;

        public AniDbClient(IAniDbDataCache aniDbDataCache,
            ISeriesTitleCache seriesTitleCache, ILogger logger)
        {
            this.aniDbDataCache = aniDbDataCache;
            this.seriesTitleCache = seriesTitleCache;
            this.logger = logger;
        }

        public Task<Option<AniDbSeriesData>> FindSeriesAsync(string title)
        {
            this.logger.LogDebug($"Finding AniDb series with title '{title}'");

            var matchedTitle = this.seriesTitleCache.FindSeriesByTitle(title);

            var seriesTask = Task.FromResult(Option<AniDbSeriesData>.None);

            matchedTitle.Match(
                t =>
                {
                    this.logger.LogDebug($"Found AniDb series Id '{t.AniDbId}' by title");

                    seriesTask = this.aniDbDataCache.GetSeriesAsync(t.AniDbId, CancellationToken.None);
                },
                () => this.logger.LogDebug("Failed to find AniDb series by title"));

            return seriesTask;
        }

        public async Task<Option<AniDbSeriesData>> GetSeriesAsync(string aniDbSeriesIdString)
        {
            var aniDbSeries = !int.TryParse(aniDbSeriesIdString, out int aniDbSeriesId)
                ? Option<AniDbSeriesData>.None
                : await GetSeriesAsync(aniDbSeriesId);

            return aniDbSeries;
        }

        public Task<Option<AniDbSeriesData>> GetSeriesAsync(int aniDbSeriesId)
        {
            return this.aniDbDataCache.GetSeriesAsync(aniDbSeriesId, CancellationToken.None);
        }

        public IEnumerable<SeiyuuData> FindSeiyuu(string name)
        {
            name = name.ToUpperInvariant();

            return this.aniDbDataCache.GetSeiyuu().Where(s => s.Name.ToUpperInvariant().Contains(name));
        }

        public Option<SeiyuuData> GetSeiyuu(int seiyuuId)
        {
            return this.aniDbDataCache.GetSeiyuu().Find(s => s.Id == seiyuuId);
        }
    }
}
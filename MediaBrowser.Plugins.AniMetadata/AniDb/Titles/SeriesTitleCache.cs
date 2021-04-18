using LanguageExt;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Emby.AniDbMetaStructure.AniDb.Titles
{
    internal class SeriesTitleCache : ISeriesTitleCache
    {
        private readonly IAniDbDataCache aniDbDataCache;
        private readonly ILogger logger;
        private readonly ITitleNormaliser titleNormaliser;
        private readonly Lazy<IDictionary<string, TitleListItemData>> titles;

        public SeriesTitleCache(IAniDbDataCache aniDbDataCache, ITitleNormaliser titleNormaliser, ILogger logger)
        {
            this.aniDbDataCache = aniDbDataCache;
            this.titleNormaliser = titleNormaliser;
            this.logger = logger;
            this.titles = new Lazy<IDictionary<string, TitleListItemData>>(GetTitles);
        }

        public Option<TitleListItemData> FindSeriesByTitle(string title)
        {
            var match = FindExactTitleMatch(title).Match(t => t, () => FindComparableMatch(title));

            return match;
        }

        private Option<TitleListItemData> FindExactTitleMatch(string title)
        {
            this.titles.Value.TryGetValue(title, out var match);

            Option<TitleListItemData> foundTitle = match;

            foundTitle.Match(t => this.logger.LogDebug($"Found exact title match for '{title}'"),
                () => this.logger.LogDebug($"Failed to find exact title match for '{title}'"));

            return foundTitle;
        }

        private Option<TitleListItemData> FindComparableMatch(string title)
        {
            title = this.titleNormaliser.GetNormalisedTitle(title);

            this.titles.Value.TryGetValue(title, out var match);

            Option<TitleListItemData> foundTitle = match;

            foundTitle.Match(t => this.logger.LogDebug($"Found comparable title match for '{title}'"),
                () => this.logger.LogDebug($"Failed to find comparable title match for '{title}'"));

            return foundTitle;
        }

        private IDictionary<string, TitleListItemData> GetTitles()
        {
            var titles = new Dictionary<string, TitleListItemData>(StringComparer.OrdinalIgnoreCase);

            var titlesAgainstItems = this.aniDbDataCache.TitleList.SelectMany(i => i.Titles.Select(t => new
            {
                t.Title,
                ComparableTitle = this.titleNormaliser.GetNormalisedTitle(t.Title),
                Item = i
            }));

            foreach (var titlesAgainstItem in titlesAgainstItems)
            {
                AddIfMissing(titles, titlesAgainstItem.Title, titlesAgainstItem.Item);
                AddIfMissing(titles, titlesAgainstItem.ComparableTitle, titlesAgainstItem.Item);
            }

            return titles;
        }

        private void AddIfMissing(IDictionary<string, TitleListItemData> dictionary, string key,
            TitleListItemData value)
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, value);
            }
        }
    }
}
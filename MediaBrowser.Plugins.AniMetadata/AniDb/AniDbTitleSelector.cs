using Emby.AniDbMetaStructure.AniDb.SeriesData;
using Emby.AniDbMetaStructure.Configuration;
using LanguageExt;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace Emby.AniDbMetaStructure.AniDb
{
    internal class AniDbTitleSelector : IAniDbTitleSelector
    {
        private readonly ILogger logger;

        public AniDbTitleSelector(ILogger logger)
        {
            this.logger = logger;
        }

        public Option<ItemTitleData> SelectTitle(IEnumerable<ItemTitleData> titles, TitleType preferredTitleType,
            string metadataLanguage)
        {
            this.logger.LogDebug(
                $"Selecting title from [{string.Join(", ", titles.Select(t => t.ToString()))}] available, preference for {preferredTitleType}, metadata language '{metadataLanguage}'");

            var preferredTitle = FindPreferredTitle(titles, preferredTitleType, metadataLanguage);

            preferredTitle.Match(
                t => this.logger.LogDebug($"Found preferred title '{t.Title}'"),
                () =>
                {
                    var defaultTitle = FindDefaultTitle(titles);

                    defaultTitle.Match(
                        t => this.logger.LogDebug($"Failed to find preferred title, falling back to default title '{t.Title}'"),
                        () => this.logger.LogDebug("Failed to find any title"));

                    preferredTitle = defaultTitle;
                });

            return preferredTitle;
        }

        private Option<ItemTitleData> FindDefaultTitle(IEnumerable<ItemTitleData> titles)
        {
            var title = FindTitle(titles, "x-jat");

            title.Match(
                t => { },
                () => title = FindMainTitle(titles));

            return title;
        }

        private Option<ItemTitleData> FindPreferredTitle(IEnumerable<ItemTitleData> titles,
            TitleType preferredTitleType, string metadataLanguage)
        {
            switch (preferredTitleType)
            {
                case TitleType.Localized:
                    return FindTitle(titles, metadataLanguage);

                case TitleType.Japanese:
                    return FindTitle(titles, "ja");

                case TitleType.JapaneseRomaji:
                    return FindTitle(titles, "x-jat");
            }

            return Option<ItemTitleData>.None;
        }

        private Option<ItemTitleData> FindTitle(IEnumerable<ItemTitleData> titles, string metadataLanguage)
        {
            var title = titles
                .OrderBy(t => t.Priority)
                .FirstOrDefault(t => t.Language == metadataLanguage);

            return title;
        }

        private Option<ItemTitleData> FindMainTitle(IEnumerable<ItemTitleData> titles)
        {
            return titles.FirstOrDefault(t => t.Type == "main");
        }
    }
}
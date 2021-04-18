using Jellyfin.AniDbMetaStructure.AniList.Data;
using Jellyfin.AniDbMetaStructure.Configuration;
using LanguageExt;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jellyfin.AniDbMetaStructure.AniList
{
    internal class AniListNameSelector : IAniListNameSelector
    {
        private readonly ILogger logger;

        public AniListNameSelector(ILogger logger)
        {
            this.logger = logger;
        }

        public Option<string> SelectTitle(AniListTitleData titleData, TitleType preferredTitleType,
            string metadataLanguage)
        {
            this.logger.LogDebug(
                $"Selecting title from {titleData} available, preference for {preferredTitleType}, metadata language '{metadataLanguage}'");

            var preferredTitle = FindPreferredTitle(titleData, preferredTitleType, metadataLanguage);

            preferredTitle.IfSome(t => this.logger.LogDebug($"Found title '{t}'"));

            if (preferredTitle.IsNone)
            {
                this.logger.LogDebug("No title found");
            }

            return preferredTitle;
        }

        public Option<string> SelectName(AniListPersonNameData nameData, TitleType preferredTitleType,
            string metadataLanguage)
        {
            if (preferredTitleType == TitleType.Japanese)
            {
                return new[] { GetNativeName(nameData), GetFirstLastName(nameData) }.Somes().FirstOrDefault();
            }

            return new[] { GetFirstLastName(nameData), GetNativeName(nameData) }.Somes().FirstOrDefault();
        }

        private Option<string> FindPreferredTitle(AniListTitleData titleData,
            TitleType preferredTitleType, string metadataLanguage)
        {
            if (preferredTitleType == TitleType.Localized &&
                "ja".Equals(metadataLanguage, StringComparison.InvariantCultureIgnoreCase))
            {
                preferredTitleType = TitleType.Japanese;
            }

            switch (preferredTitleType)
            {
                case TitleType.Localized:
                    return SelectTitle(titleData, new[]
                    {
                        TitleType.Localized,
                        TitleType.JapaneseRomaji,
                        TitleType.Japanese
                    });

                case TitleType.Japanese:
                    return SelectTitle(titleData, new[]
                    {
                        TitleType.Japanese,
                        TitleType.JapaneseRomaji,
                        TitleType.Localized
                    });

                case TitleType.JapaneseRomaji:
                    return SelectTitle(titleData, new[]
                    {
                        TitleType.JapaneseRomaji,
                        TitleType.Localized,
                        TitleType.Japanese
                    });
            }

            return Option<string>.None;
        }

        private Option<string> SelectTitle(Option<AniListTitleData> titleData, IEnumerable<TitleType> preferenceOrder)
        {
            return preferenceOrder
                .Select(p => SelectTitle(titleData, p))
                .FirstOrDefault(t => t.IsSome);
        }

        private Option<string> SelectTitle(Option<AniListTitleData> titleData, TitleType preference)
        {
            return titleData.Map(t =>
                {
                    switch (preference)
                    {
                        case TitleType.Localized:
                            return t.English;

                        case TitleType.Japanese:
                            return t.Native;

                        case TitleType.JapaneseRomaji:
                            return t.Romaji;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(preference), preference, null);
                    }
                }).Bind(NoneIfNullOrWhitespace);
        }

        private Option<string> GetFirstLastName(Option<AniListPersonNameData> personNameData)
        {
            return personNameData.Bind(n => NoneIfNullOrWhitespace($"{n.First} {n.Last}"));
        }

        private Option<string> GetNativeName(Option<AniListPersonNameData> personNameData)
        {
            return personNameData.Bind(n => NoneIfNullOrWhitespace(n.Native));
        }

        private Option<string> NoneIfNullOrWhitespace(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? Option<string>.None : value;
        }
    }
}
using System;
using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.AniList.Data
{
    internal class AniListFuzzyDate
    {
        public AniListFuzzyDate(Option<long> year, Option<long> month, Option<long> day)
        {
            Year = year;
            Month = month;
            Day = day;
        }

        public Option<long> Year { get; }

        public Option<long> Month { get; }

        public Option<long> Day { get; }

        public static Option<DateTime> ToDate(Option<AniListFuzzyDate> aniListFuzzyDate)
        {
            return aniListFuzzyDate.Bind(date => date.Year.Bind(y =>
                date.Month.Bind(m => date.Day.Map(d => new DateTime((int)y, (int)m, (int)d)))));
        }
    }
}
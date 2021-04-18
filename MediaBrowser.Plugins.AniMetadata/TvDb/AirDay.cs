using System;
using System.Collections.Generic;

namespace Jellyfin.AniDbMetaStructure.TvDb
{
    public enum AirDay
    {
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday,
        Sunday,
        Daily
    }

    public static class AirDaysExtensions
    {
        public static IEnumerable<DayOfWeek> ToDaysOfWeek(this AirDay airDay)
        {
            var daysOfWeek = new List<DayOfWeek>();

            switch (airDay)
            {
                case AirDay.Monday:
                    daysOfWeek.Add(DayOfWeek.Monday);
                    break;
                case AirDay.Tuesday:
                    daysOfWeek.Add(DayOfWeek.Tuesday);
                    break;
                case AirDay.Wednesday:
                    daysOfWeek.Add(DayOfWeek.Wednesday);
                    break;
                case AirDay.Thursday:
                    daysOfWeek.Add(DayOfWeek.Thursday);
                    break;
                case AirDay.Friday:
                    daysOfWeek.Add(DayOfWeek.Friday);
                    break;
                case AirDay.Saturday:
                    daysOfWeek.Add(DayOfWeek.Saturday);
                    break;
                case AirDay.Sunday:
                    daysOfWeek.Add(DayOfWeek.Sunday);
                    break;
                case AirDay.Daily:
                    daysOfWeek.AddRange(new[]
                    {
                        DayOfWeek.Monday,
                        DayOfWeek.Tuesday,
                        DayOfWeek.Wednesday,
                        DayOfWeek.Thursday,
                        DayOfWeek.Friday,
                        DayOfWeek.Saturday,
                        DayOfWeek.Sunday
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(airDay), airDay, null);
            }

            return daysOfWeek;
        }
    }
}
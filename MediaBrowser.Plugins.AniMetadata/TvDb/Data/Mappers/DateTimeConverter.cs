using AutoMapper;
using LanguageExt;
using System;

namespace Jellyfin.AniDbMetaStructure.TvDb.Data.Mappers
{
    public class DateTimeConverter : ITypeConverter<string, Option<DateTime>>
    {
        public Option<DateTime> Convert(string source, Option<DateTime> destination, ResolutionContext context)
        {
            if (DateTime.TryParse(source, out var result))
            {
                return Option<DateTime>.Some(result);
            }

            return Option<DateTime>.None;
        }
    }
}

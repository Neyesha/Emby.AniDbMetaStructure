using AutoMapper;
using LanguageExt;
using System;

namespace Jellyfin.AniDbMetaStructure.TvDb.Data.Mappers
{
    public class AirDayConverter : ITypeConverter<string, Option<AirDay>>
    {
        public Option<AirDay> Convert(string source, Option<AirDay> destination, ResolutionContext context)
        {
            if (Enum.TryParse(source, out AirDay result))
            {
                return Option<AirDay>.Some(result);
            }

            return Option<AirDay>.None;
        }
    }
}

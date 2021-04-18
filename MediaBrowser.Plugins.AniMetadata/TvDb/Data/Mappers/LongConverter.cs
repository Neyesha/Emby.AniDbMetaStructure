using AutoMapper;
using LanguageExt;
using System;

namespace Jellyfin.AniDbMetaStructure.TvDb.Data.Mappers
{
    public class LongConverter : ITypeConverter<int?, Option<long>>
    {
        public Option<long> Convert(int? source, Option<long> destination, ResolutionContext context)
        {
            
            if (long.TryParse(source.ToString(), out var result))
            {
                return Option<long>.Some(result);
            }

            return Option<long>.None;
        }
    }
}

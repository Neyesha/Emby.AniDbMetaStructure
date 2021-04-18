using AutoMapper;

namespace Jellyfin.AniDbMetaStructure.TvDb.Data.Mappers
{
    public class IntFormatter : IValueConverter<string, int>
    {
        public int Convert(string source, ResolutionContext context)
        {
            if (int.TryParse(source, out var result))
            {
                return result;
            }

            return 0;
        }
    }
}

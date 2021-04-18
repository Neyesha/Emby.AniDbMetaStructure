using System.Collections.Generic;
using Jellyfin.AniDbMetaStructure.Process;
using Jellyfin.AniDbMetaStructure.PropertyMapping;

namespace Jellyfin.AniDbMetaStructure.Configuration
{
    internal interface IPluginConfiguration
    {
        bool AddAnimeGenre { get; set; }

        IEnumerable<string> ExcludedSeriesNames { get; }

        LibraryStructure LibraryStructure { get; }

        int MaxGenres { get; set; }

        bool MoveExcessGenresToTags { get; set; }

        TitleType TitlePreference { get; set; }

        string TvDbApiKey { get; set; }

        //string AniListAuthorisationCode { get; set; }
        //TODO: doesn't work

        /// <summary>
        ///     The source that was used to name the files
        /// </summary>
        ISource FileStructureSource(IMediaItemType itemType);

        IPropertyMappingCollection GetEpisodeMetadataMapping(string metadataLanguage);

        IPropertyMappingCollection GetSeasonMetadataMapping(string metadataLanguage);

        IPropertyMappingCollection GetSeriesMetadataMapping(string metadataLanguage);

        /// <summary>
        ///     The source to use to structure the Emby library
        /// </summary>
        ISource LibraryStructureSource(IMediaItemType itemType);
    }
}
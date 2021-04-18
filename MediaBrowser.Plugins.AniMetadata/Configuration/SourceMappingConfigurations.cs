using System.Collections;
using System.Collections.Generic;
using Jellyfin.AniDbMetaStructure.AniDb;
using Jellyfin.AniDbMetaStructure.AniList;
using Jellyfin.AniDbMetaStructure.TvDb;

namespace Jellyfin.AniDbMetaStructure.Configuration
{
    internal class SourceMappingConfigurations : IEnumerable<ISourceMappingConfiguration>
    {
        private readonly IEnumerable<ISourceMappingConfiguration> sourceMappingConfigurations;

        public SourceMappingConfigurations(AniDbSourceMappingConfiguration aniDbSourceMappingConfiguration,
            TvDbSourceMappingConfiguration tvDbSourceMappingConfiguration,
            AniListSourceMappingConfiguration aniListSourceMappingConfiguration)
        {
            this.sourceMappingConfigurations =
                new ISourceMappingConfiguration[]
                {
                    aniDbSourceMappingConfiguration,
                    tvDbSourceMappingConfiguration,
                    aniListSourceMappingConfiguration
                };
        }

        public IEnumerator<ISourceMappingConfiguration> GetEnumerator()
        {
            return this.sourceMappingConfigurations.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.sourceMappingConfigurations).GetEnumerator();
        }
    }
}
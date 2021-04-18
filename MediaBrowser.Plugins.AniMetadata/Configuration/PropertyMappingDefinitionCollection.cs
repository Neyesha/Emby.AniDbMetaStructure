using System.Collections.Generic;
using System.Linq;

namespace Jellyfin.AniDbMetaStructure.Configuration
{
    public class PropertyMappingDefinitionCollection
    {
        public PropertyMappingDefinitionCollection()
        {
        }

        public PropertyMappingDefinitionCollection(string friendlyName, IEnumerable<PropertyMappingDefinition> mappings)
        {
            this.FriendlyName = friendlyName;
            this.Mappings = mappings.ToArray();
        }

        public string FriendlyName { get; }
        
        public PropertyMappingDefinition[] Mappings { get; set; }
    }
}
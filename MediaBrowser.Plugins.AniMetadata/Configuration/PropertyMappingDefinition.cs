namespace Jellyfin.AniDbMetaStructure.Configuration
{
    public class PropertyMappingDefinition
        {
            public PropertyMappingDefinition()
            {
            }

            public PropertyMappingDefinition(string friendlyName, string sourceName, string targetPropertyName)
            {
                this.FriendlyName = friendlyName;
                this.SourceName = sourceName;
                this.TargetPropertyName = targetPropertyName;
            }

            public string FriendlyName { get; set; }

            public string SourceName { get; set; }

            public string TargetPropertyName { get; set; }
        }
}
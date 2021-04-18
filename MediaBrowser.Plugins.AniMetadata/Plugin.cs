using System;
using System.Collections.Generic;
using Jellyfin.AniDbMetaStructure.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.AniDbMetaStructure
{
    public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
    {
        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer) : base(
            applicationPaths, xmlSerializer)
        {
            Instance = this;
        }

        public override Guid Id => new Guid("77780029-0ab8-4c7a-ad47-4f0187f13301");

        public override string Name => "AniDbMetaStructure";

        public override string Description => "Combines data from AniDb and TvDb to identify anime";

        public static Plugin Instance { get; private set; }

        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = "AniDbMetaStructure",
                    EmbeddedResourcePath = "Emby.AniDbMetaStructure.Configuration.ConfigPage.html"
                }
            };
        }

        internal void SetConfiguration(PluginConfiguration configuration)
        {
            this.Configuration = configuration;
        }
    }
}
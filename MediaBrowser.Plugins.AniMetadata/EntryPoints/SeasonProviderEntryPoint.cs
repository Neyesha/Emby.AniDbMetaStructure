using Emby.AniDbMetaStructure.Process.Providers;
using MediaBrowser.Common;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Providers;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Emby.AniDbMetaStructure.EntryPoints
{
    public class SeasonProviderEntryPoint : IRemoteMetadataProvider<Season, SeasonInfo>
    {
        private readonly SeasonProvider seasonProvider;

        public SeasonProviderEntryPoint(IApplicationHost applicationHost)
        {
            this.seasonProvider = DependencyConfiguration.Resolve<SeasonProvider>(applicationHost);
        }

        public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(SeasonInfo searchInfo,
            CancellationToken cancellationToken)
        {
            return this.seasonProvider.GetSearchResults(searchInfo, cancellationToken);
        }

        public Task<MetadataResult<Season>> GetMetadata(SeasonInfo info, CancellationToken cancellationToken)
        {
            return this.seasonProvider.GetMetadata(info, cancellationToken);
        }

        public string Name => this.seasonProvider.Name;

        public Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            return this.seasonProvider.GetImageResponse(url, cancellationToken);
        }
    }
}
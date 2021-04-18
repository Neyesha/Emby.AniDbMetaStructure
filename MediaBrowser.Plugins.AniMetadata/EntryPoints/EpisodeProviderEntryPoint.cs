using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.AniDbMetaStructure.Process.Providers;
using MediaBrowser.Common;
using System.Net.Http;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Providers;

namespace Jellyfin.AniDbMetaStructure.EntryPoints
{
    public class EpisodeProviderEntryPoint : IRemoteMetadataProvider<Episode, EpisodeInfo>
    {
        private readonly EpisodeProvider episodeProvider;

        public EpisodeProviderEntryPoint(IApplicationHost applicationHost)
        {
            this.episodeProvider = DependencyConfiguration.Resolve<EpisodeProvider>(applicationHost);
        }

        public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(EpisodeInfo searchInfo,
            CancellationToken cancellationToken)
        {
            return this.episodeProvider.GetSearchResults(searchInfo, cancellationToken);
        }

        public Task<MetadataResult<Episode>> GetMetadata(EpisodeInfo info, CancellationToken cancellationToken)
        {
            return this.episodeProvider.GetMetadata(info, cancellationToken);
        }

        public string Name => this.episodeProvider.Name;

        public Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            return this.episodeProvider.GetImageResponse(url, cancellationToken);
        }
    }
}
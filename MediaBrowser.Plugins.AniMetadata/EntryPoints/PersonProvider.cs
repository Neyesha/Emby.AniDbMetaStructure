using Jellyfin.AniDbMetaStructure.Providers.AniDb;
using MediaBrowser.Common;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Providers;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Jellyfin.AniDbMetaStructure.EntryPoints
{
    public class PersonProvider : IRemoteMetadataProvider<Person, PersonLookupInfo>
    {
        private readonly AniDbPersonProvider personProvider;

        public PersonProvider(IApplicationHost applicationHost)
        {
            this.personProvider =
                DependencyConfiguration.Resolve<AniDbPersonProvider>(applicationHost);
        }

        public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(PersonLookupInfo searchInfo,
            CancellationToken cancellationToken)
        {
            return this.personProvider.GetSearchResults(searchInfo, cancellationToken);
        }

        public Task<MetadataResult<Person>> GetMetadata(PersonLookupInfo info, CancellationToken cancellationToken)
        {
            return this.personProvider.GetMetadata(info, cancellationToken);
        }

        public string Name => this.personProvider.Name;

        public Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            return this.personProvider.GetImageResponse(url, cancellationToken);
        }
    }
}
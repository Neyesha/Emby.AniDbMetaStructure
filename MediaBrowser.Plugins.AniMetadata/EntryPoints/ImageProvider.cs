using Jellyfin.AniDbMetaStructure.Providers.AniDb;
using MediaBrowser.Common;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Jellyfin.AniDbMetaStructure.EntryPoints
{
    public class ImageProvider : IRemoteImageProvider
    {
        private readonly AniDbImageProvider imageProvider;

        public ImageProvider(IApplicationHost applicationHost)
        {
            this.imageProvider = DependencyConfiguration.Resolve<AniDbImageProvider>(applicationHost);
        }

        public bool Supports(BaseItem item)
        {
            return this.imageProvider.Supports(item);
        }

        public string Name => this.imageProvider.Name;

        public IEnumerable<ImageType> GetSupportedImages(BaseItem item)
        {
            return this.imageProvider.GetSupportedImages(item);
        }

        public Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, CancellationToken cancellationToken)
        {
            return this.imageProvider.GetImages(item, cancellationToken);
        }

        public Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            return this.imageProvider.GetImageResponse(url, cancellationToken);
        }

        public Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, LibraryOptions libraryOptions, CancellationToken cancellationToken)
        {
            return GetImages(item, cancellationToken);
        }
    }
}
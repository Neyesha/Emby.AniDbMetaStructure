using Jellyfin.AniDbMetaStructure.Files;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xem.Api;
using Xem.Api.Mapping;

namespace Jellyfin.AniDbMetaStructure.Mapping
{
    internal class XemTvDbMappingsFileSpec : IRemoteFileSpec<IDictionary<string, string[]>>, ICustomDownload<IDictionary<string, string[]>>
    {
        private readonly string rootPath;
        private readonly IApiClient xemApiClient;

        public XemTvDbMappingsFileSpec(string rootPath, IApiClient xemApiClient)
        {
            this.rootPath = rootPath;
            this.xemApiClient = xemApiClient;
        }

        public string Url => string.Empty;

        public string LocalPath => Path.Combine(this.rootPath, "xem-tvdb-anime-list.xml");

        public bool IsGZipped => false;

        public async Task<string> DownloadFileAsync(IRemoteFileSpec<IDictionary<string, string[]>> fileSpec, CancellationToken cancellationToken)
        {
            var mappingsFromTvDb = await this.xemApiClient.GetAllNames(new AllNamesQuery(EntityType.TvDb));

            return JsonConvert.SerializeObject(mappingsFromTvDb.NameValues);
        }
    }
}

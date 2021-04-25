using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.AniDbMetaStructure.Files;
using Newtonsoft.Json;
using Xem.Api;
using Xem.Api.Mapping;

namespace Jellyfin.AniDbMetaStructure.Mapping
{
    internal class XemAniDbMappingsFileSpec : IRemoteFileSpec<IDictionary<string, string[]>>, ICustomDownload<IDictionary<string, string[]>>
    {
        private readonly string rootPath;
        private readonly IApiClient xemApiClient;

        public XemAniDbMappingsFileSpec(string rootPath, IApiClient xemApiClient)
        {
            this.rootPath = rootPath;
            this.xemApiClient = xemApiClient;
        }

        public string Url => string.Empty;

        public string LocalPath => Path.Combine(this.rootPath, "xem-anidb-anime-list.json");

        public bool IsGZipped => false;

        public async Task<string> DownloadFileAsync(IRemoteFileSpec<IDictionary<string, string[]>> fileSpec, CancellationToken cancellationToken)
        {
            var mappingsFromAniDb = await this.xemApiClient.GetAllNames(new AllNamesQuery(EntityType.AniDb));

            return JsonConvert.SerializeObject(mappingsFromAniDb.NameValues);
        }
    }
}
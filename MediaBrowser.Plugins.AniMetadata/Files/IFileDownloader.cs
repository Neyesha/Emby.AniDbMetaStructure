using System.Threading;
using System.Threading.Tasks;

namespace Jellyfin.AniDbMetaStructure.Files
{
    internal interface IFileDownloader
    {
        Task DownloadFileAsync<T>(IRemoteFileSpec<T> fileSpec, CancellationToken cancellationToken) where T : class;
    }
}
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.Files
{
    internal interface IFileCache
    {
        Option<T> GetFileContent<T>(ILocalFileSpec<T> fileSpec) where T : class;

        Task<Option<T>> GetFileContentAsync<T>(IRemoteFileSpec<T> fileSpec, CancellationToken cancellationToken)
            where T : class;

        void SaveFile<T>(ILocalFileSpec<T> fileSpec, T data) where T : class;
    }
}
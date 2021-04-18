using Emby.AniDbMetaStructure.Infrastructure;
using Microsoft.Extensions.Logging;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Emby.AniDbMetaStructure.Files
{
    internal class FileDownloader : IFileDownloader
    {
        private readonly HttpClient httpClient;
        private readonly ILogger logger;
        private readonly IRateLimiter requestLimiter;

        public FileDownloader(IRateLimiters rateLimiters, HttpClient httpClient, ILogger logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;
            this.requestLimiter = rateLimiters.AniDb;
        }

        public async Task DownloadFileAsync<T>(IRemoteFileSpec<T> fileSpec, CancellationToken cancellationToken)
            where T : class
        {
            await this.requestLimiter.TickAsync();

            if (fileSpec is ICustomDownload<T> customDownloadFile)
            {
                string content = await customDownloadFile.DownloadFileAsync(fileSpec, cancellationToken);
                await SaveFileContentAsync(content, fileSpec, cancellationToken);
            }
            else
            {
                await DownloadAndSaveHttpFileAsync(fileSpec, cancellationToken);
            }
        }

        private async Task DownloadAndSaveHttpFileAsync<T>(IRemoteFileSpec<T> fileSpec, CancellationToken cancellationToken)
            where T : class
        {
            using (var stream = await this.httpClient.GetAsync(fileSpec.Url, cancellationToken).ConfigureAwait(false))
            {
                var unzippedStream = stream;

                if (fileSpec.IsGZipped)
                {
                    unzippedStream = new GZipStream(stream, CompressionMode.Decompress);
                }

                using (var reader = new StreamReader(unzippedStream, Encoding.UTF8, true))
                {
                    string text = await reader.ReadToEndAsync().ConfigureAwait(false);
                    await SaveFileContentAsync(text, fileSpec, cancellationToken);
                }
            }
        }

        private async Task SaveFileContentAsync<T>(string text, IRemoteFileSpec<T> fileSpec, CancellationToken cancellationToken)
            where T : class
        {
            using (var file = File.Open(fileSpec.LocalPath, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(file))
            {


                this.logger.LogDebug($"Saving {text.Length} characters to {fileSpec.LocalPath}");

                text = text.Replace("&#x0;", string.Empty);

                await writer.WriteAsync(text).ConfigureAwait(false);

                await writer.FlushAsync().ConfigureAwait(false);
            }
        }
    }
}
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Net;
//using System.Net.Http;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Jellyfin.AniDbMetaStructure.Tests.TestHelpers
//{
//    internal class TestHttpClient : HttpClient
//    {
//        private readonly HttpClient client;

//        public TestHttpClient()
//        {
//            this.client = new HttpClient();
//        }

//        public async Task<HttpResponseMessage> GetResponse(HttpRequestOptions options)
//        {
//            this.client.DefaultRequestHeaders.Clear();

//            foreach (var optionsRequestHeader in options.RequestHeaders)
//                this.client.DefaultRequestHeaders.Add(optionsRequestHeader.Key, optionsRequestHeader.Value);

//            try
//            {
//                var response = await this.client.GetStreamAsync(options.Url);

//                return new HttpResponseMessage
//                {
//                    Content = response,
//                    StatusCode = HttpStatusCode.OK
//                };
//            }
//            catch
//            {
//                return new HttpResponseMessage
//                {
//                    Content = null,
//                    StatusCode = HttpStatusCode.NotFound
//                };
//            }
//        }

//        public Task<HttpResponseMessage> SendAsync(HttpRequestOptions options, string httpMethod)
//        {
//            throw new NotImplementedException();
//        }

//        public async Task<HttpResponseMessage> Post(HttpRequestOptions options)
//        {
//            this.client.DefaultRequestHeaders.Clear();

//            foreach (var optionsRequestHeader in options.RequestHeaders)
//                this.client.DefaultRequestHeaders.Add(optionsRequestHeader.Key, optionsRequestHeader.Value);

//            var response = await this.client.PostAsync(options.Url,
//                new StringContent(options.RequestContent.ToString(), Encoding.UTF8, options.RequestContentType));

//            var responseContent = await response.Content.ReadAsStreamAsync();

//            return new HttpResponseMessage
//            {
//                Content = responseContent,
//                StatusCode = response.StatusCode
//            };
//        }

//        public Task<string> GetTempFile(HttpRequestOptions options)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<HttpResponseMessage> GetTempFileResponse(HttpRequestOptions options)
//        {
//            throw new NotImplementedException();
//        }

//        public void Dispose()
//        {
//            throw new NotImplementedException();
//        }

//        public Task<Stream> Get(string url, SemaphoreSlim resourcePool, CancellationToken cancellationToken)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<Stream> Get(string url, CancellationToken cancellationToken)
//        {
//            throw new NotImplementedException();
//        }

//        public async Task<Stream> Get(HttpRequestOptions options)
//        {
//            this.client.DefaultRequestHeaders.Clear();

//            foreach (var optionsRequestHeader in options.RequestHeaders)
//                this.client.DefaultRequestHeaders.Add(optionsRequestHeader.Key, optionsRequestHeader.Value);

//            var response = await this.client.GetStreamAsync(options.Url);

//            return response;
//        }

//        public Task<Stream> Post(string url, Dictionary<string, string> postData, SemaphoreSlim resourcePool,
//            CancellationToken cancellationToken)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<Stream> Post(string url, Dictionary<string, string> postData, CancellationToken cancellationToken)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<Stream> Post(HttpRequestOptions options, Dictionary<string, string> postData)
//        {
//            throw new NotImplementedException();
//        }

//        public IDisposable GetConnectionContext(HttpRequestOptions options)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
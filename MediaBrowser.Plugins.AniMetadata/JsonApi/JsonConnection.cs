using Jellyfin.AniDbMetaStructure.Infrastructure;
using LanguageExt;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Jellyfin.AniDbMetaStructure.JsonApi
{
    internal class JsonConnection : IJsonConnection
    {
        private readonly HttpClient httpClient;
        private readonly ICustomJsonSerialiser jsonSerialiser;
        private readonly ILogger logger;

        public JsonConnection(ICustomJsonSerialiser jsonSerialiser, ILogger logger)
        {
            this.httpClient = new HttpClient();
            this.jsonSerialiser = jsonSerialiser;
            this.logger = logger;
        }

        public Task<Either<FailedRequest, Response<TResponseData>>> PostAsync<TResponseData>(
            IPostRequest<TResponseData> request, Option<string> oAuthAccessToken)
        {
            return PostAsync(request, oAuthAccessToken, ParseResponse<TResponseData>);
        }

        public Task<Either<FailedRequest, Response<TResponseData>>> GetAsync<TResponseData>(
            IGetRequest<TResponseData> request, Option<string> oAuthAccessToken)
        {
            return GetAsync(request, oAuthAccessToken, ParseResponse<TResponseData>);
        }

        public Task<Either<TFailedRequest, Response<TResponseData>>> PostAsync<TFailedRequest, TResponseData>(
            IPostRequest<TResponseData> request, Option<string> oAuthAccessToken,
            Func<string, ICustomJsonSerialiser, HttpResponseMessage, Either<TFailedRequest, Response<TResponseData>>>
                responseHandler)
        {
            SetToken(oAuthAccessToken);

            this.logger.LogDebug($"Posting: '{JsonConvert.SerializeObject(request.Data)}' to '{request.Url}'");

            var response = this.httpClient.PostAsJsonAsync(request.Url, request.Data);

            return response.Map(r => ApplyResponseHandler(responseHandler, r));
        }

        public Task<Either<TFailedRequest, Response<TResponseData>>> GetAsync<TFailedRequest, TResponseData>(
            IGetRequest<TResponseData> request, Option<string> oAuthAccessToken,
            Func<string, ICustomJsonSerialiser, HttpResponseMessage, Either<TFailedRequest, Response<TResponseData>>>
                responseHandler)
        {
            SetToken(oAuthAccessToken);

            this.logger.LogDebug($"Getting: '{request.Url}'");

            var response = this.httpClient.GetAsync(request.Url);

            return response.Map(r => ApplyResponseHandler(responseHandler, r));
        }

        private Either<TFailedRequest, Response<TResponseData>> ApplyResponseHandler<TFailedRequest, TResponseData>(
            Func<string, ICustomJsonSerialiser, HttpResponseMessage, Either<TFailedRequest, Response<TResponseData>>>
                responseHandler, HttpResponseMessage response)
        {
            string responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            this.logger.LogDebug(response.StatusCode != HttpStatusCode.OK
                ? $"Request failed (http {response.StatusCode}): '{responseContent}'"
                : $"Response: {responseContent}");

            return responseHandler(responseContent, this.jsonSerialiser, response);
        }

        private Either<FailedRequest, Response<TResponseData>> ParseResponse<TResponseData>(string responseContent,
            ICustomJsonSerialiser serialiser, HttpResponseMessage response)
        {
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new FailedRequest(response.StatusCode, responseContent);
            }

            return new Response<TResponseData>(serialiser.Deserialise<TResponseData>(responseContent));
        }

        private void SetToken(Option<string> token)
        {
            token.IfSome(t => this.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", t));
        }
    }
}
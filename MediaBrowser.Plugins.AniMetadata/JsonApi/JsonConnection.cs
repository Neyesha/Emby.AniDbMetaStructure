using Emby.AniDbMetaStructure.Infrastructure;
using LanguageExt;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Emby.AniDbMetaStructure.JsonApi
{
    internal class JsonConnection : IJsonConnection
    {
        private readonly HttpClient httpClient;
        private readonly ICustomJsonSerialiser jsonSerialiser;
        private readonly ILogger logger;

        public JsonConnection(HttpClient httpClient, ICustomJsonSerialiser jsonSerialiser, ILogger logger)
        {
            this.httpClient = httpClient;
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
            var requestOptions = new HttpRequestOptions
            {
                AcceptHeader = "application/json",
                Url = request.Url,
                RequestContent = new ReadOnlyMemory<char>(this.jsonSerialiser.Serialise(request.Data).ToCharArray()),
                RequestContentType = "application/json"
            };

            SetToken(requestOptions, oAuthAccessToken);

            this.logger.LogDebug($"Posting: '{requestOptions.RequestContent}' to '{requestOptions.Url}'");

            var response = this.httpClient.Post(requestOptions);

            return response.Map(r => this.ApplyResponseHandler(responseHandler, r));
        }

        public Task<Either<TFailedRequest, Response<TResponseData>>> GetAsync<TFailedRequest, TResponseData>(
            IGetRequest<TResponseData> request, Option<string> oAuthAccessToken,
            Func<string, ICustomJsonSerialiser, HttpResponseMessage, Either<TFailedRequest, Response<TResponseData>>>
                responseHandler)
        {
            var requestOptions = new HttpRequestOptions
            {
                AcceptHeader = "application/json",
                Url = request.Url
            };

            SetToken(requestOptions, oAuthAccessToken);

            this.logger.LogDebug($"Getting: '{requestOptions.Url}'");

            var response = this.httpClient.GetResponse(requestOptions);

            return response.Map(r => this.ApplyResponseHandler(responseHandler, r));
        }

        private Either<TFailedRequest, Response<TResponseData>> ApplyResponseHandler<TFailedRequest, TResponseData>(
            Func<string, ICustomJsonSerialiser, HttpResponseMessage, Either<TFailedRequest, Response<TResponseData>>>
                responseHandler, HttpResponseMessage response)
        {
            string responseContent = this.GetStreamText(response.Content);

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

        private void SetToken(HttpRequestOptions requestOptions, Option<string> token)
        {
            token.IfSome(t => requestOptions.RequestHeaders.Add("Authorization", $"Bearer {t}"));
        }

        private string GetStreamText(Stream stream)
        {
            if (stream == null)
            {
                return null;
            }

            var reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }
    }
}
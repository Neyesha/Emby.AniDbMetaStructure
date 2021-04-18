using Jellyfin.AniDbMetaStructure.Infrastructure;
using LanguageExt;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Jellyfin.AniDbMetaStructure.JsonApi
{
    internal interface IJsonConnection
    {
        Task<Either<FailedRequest, Response<TResponseData>>> PostAsync<TResponseData>(
            IPostRequest<TResponseData> request, Option<string> oAuthAccessToken);

        Task<Either<TFailedRequest, Response<TResponseData>>> PostAsync<TFailedRequest, TResponseData>(
            IPostRequest<TResponseData> request, Option<string> oAuthAccessToken,
            Func<string, ICustomJsonSerialiser, HttpResponseMessage, Either<TFailedRequest, Response<TResponseData>>>
                responseHandler);

        Task<Either<FailedRequest, Response<TResponseData>>> GetAsync<TResponseData>(
            IGetRequest<TResponseData> request, Option<string> oAuthAccessToken);

        Task<Either<TFailedRequest, Response<TResponseData>>> GetAsync<TFailedRequest, TResponseData>(
            IGetRequest<TResponseData> request, Option<string> oAuthAccessToken,
            Func<string, ICustomJsonSerialiser, HttpResponseMessage, Either<TFailedRequest, Response<TResponseData>>>
                responseHandler);
    }
}
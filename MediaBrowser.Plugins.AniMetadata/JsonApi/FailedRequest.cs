using Jellyfin.AniDbMetaStructure.Process;
using System;
using System.Net;

namespace Jellyfin.AniDbMetaStructure.JsonApi
{
    internal class FailedRequest
    {
        public FailedRequest(HttpStatusCode statusCode, string responseContent)
        {
            StatusCode = statusCode;
            ResponseContent = responseContent;
        }

        public HttpStatusCode StatusCode { get; }

        public string ResponseContent { get; }

        public static Func<FailedRequest, ProcessFailedResult> ToFailedResult(ProcessResultContext resultContext)
        {
            return r => resultContext.Failed($"Request failed with {r.StatusCode}: {r.ResponseContent}");
        }
    }
}
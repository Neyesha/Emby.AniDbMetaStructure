﻿using Emby.AniDbMetaStructure.JsonApi;

namespace Emby.AniDbMetaStructure.AniList.Requests
{
    internal abstract class AniListQueryRequest<TResponse> : PostRequest<AniListQueryResponse<TResponse>>
    {
        private const string AniListUrl = "https://graphql.anilist.co";

        protected AniListQueryRequest(string query, object variables) : base(AniListUrl,
            new
            {
                query,
                variables
            })
        {
        }
    }
}
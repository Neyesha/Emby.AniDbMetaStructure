using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.AniList.Requests
{
    internal class FailedAniListRequest<TResponseData>
    {
        public FailedAniListRequest(TResponseData data, Option<int> rateLimit, Option<int> rateLimitRemaining,
            Option<int> rateLimitResetSeconds)
        {
            Data = data;
            RateLimit = rateLimit;
            RateLimitRemaining = rateLimitRemaining;
            RateLimitResetSeconds = rateLimitResetSeconds;
        }

        public TResponseData Data { get; }

        public Option<int> RateLimit { get; }

        public Option<int> RateLimitRemaining { get; }

        public Option<int> RateLimitResetSeconds { get; }
    }
}
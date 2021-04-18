namespace Jellyfin.AniDbMetaStructure.Infrastructure
{
    public interface IRateLimiters
    {
        IRateLimiter AniDb { get; }
    }
}
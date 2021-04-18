using System.Threading;
using System.Threading.Tasks;

namespace Jellyfin.AniDbMetaStructure.Infrastructure
{
    public interface IRateLimiter
    {
        SemaphoreSlim Semaphore { get; }
        Task TickAsync();
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Jellyfin.AniDbMetaStructure.Infrastructure
{
    internal static class TaskExtensions
    {
        public static TaskAwaiter<IEnumerable<T>> GetAwaiter<T>(this IEnumerable<Task<T>> tasks)
        {
            return Task.WhenAll(tasks).ContinueWith(t => t.Result.AsEnumerable()).GetAwaiter();
        }
    }
}
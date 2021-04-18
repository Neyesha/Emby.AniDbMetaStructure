using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.Infrastructure
{
    internal static class AwaitExtensions
    {
        public static TaskAwaiter<Option<T>> GetAwaiter<T>(this Option<Task<T>> taskOption)
        {
            return taskOption.Match(
                to => to.ContinueWith(t => (Option<T>)t.Result).GetAwaiter(),
                () => Task.FromResult(Option<T>.None).GetAwaiter());
        }

        public static TaskAwaiter<Option<T>> GetAwaiter<T>(this OptionAsync<T> optionAsync)
        {
            return optionAsync.ToOption().GetAwaiter();
        }
    }
}
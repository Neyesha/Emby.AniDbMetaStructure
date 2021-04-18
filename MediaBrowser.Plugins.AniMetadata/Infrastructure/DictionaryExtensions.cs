using System.Collections.Generic;

namespace Emby.AniDbMetaStructure.Infrastructure
{
    public static class DictionaryExtensions
    {
        public static T GetOrDefault<TKey, T>(this IDictionary<TKey, T> dict, TKey key)
        {
            return dict.TryGetValue(key, out var value) ? value : default(T);
        }
    }
}
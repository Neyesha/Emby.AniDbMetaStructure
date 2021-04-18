using System;
using System.Collections.Generic;

namespace Jellyfin.AniDbMetaStructure.PropertyMapping
{
    /// <summary>
    ///     A collection of <see cref="IPropertyMapping" />s that can map data from one or more sources to a target object
    /// </summary>
    public interface IPropertyMappingCollection : IEnumerable<IPropertyMapping>
    {
        /// <summary>
        ///     Map a single source to a target
        /// </summary>
        /// <returns>The modified target</returns>
        TTarget Apply<TTarget>(object source, TTarget target, Action<string> log);

        /// <summary>
        ///     Map multiple sources to a target, taking data from only one source for each mapped property
        /// </summary>
        /// <returns>The modified target</returns>
        TTarget Apply<TTarget>(IEnumerable<object> sources, TTarget target, Action<string> log);
    }
}
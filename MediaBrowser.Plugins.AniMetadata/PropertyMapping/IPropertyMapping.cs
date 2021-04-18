using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.PropertyMapping
{
    /// <summary>
    ///     A mapping that sets a target property based on a source property
    /// </summary>
    public interface IPropertyMapping
    {
        /// <summary>
        ///     The name of the property as displayed to the user
        /// </summary>
        string FriendlyName { get; }

        /// <summary>
        ///     The name of the property on the target object that will be set
        /// </summary>
        string TargetPropertyName { get; }

        /// <summary>
        ///     The name of the source of the data being used
        /// </summary>
        string SourceName { get; }

        /// <summary>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns>True if the source and target are supported by this instance, otherwise false</returns>
        bool CanApply(object source, object target);

        /// <summary>
        ///     Applies the mapping to the target based on the source data, throws if the source is not supported
        /// </summary>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns>The modified target instance</returns>
        Option<TTarget> Apply<TTarget>(object source, TTarget target);
    }
}
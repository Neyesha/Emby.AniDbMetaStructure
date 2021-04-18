using Jellyfin.AniDbMetaStructure.Infrastructure;

namespace Jellyfin.AniDbMetaStructure.Files
{
    /// <summary>
    ///     A specification of a file which is populated by explicitly specifying the data
    /// </summary>
    /// <typeparam name="TRoot">The type of the serialised data</typeparam>
    internal interface ILocalFileSpec<out TRoot> : IFileSpec<TRoot> where TRoot : class
    {
        /// <summary>
        ///     The serialiser to user when dealing with this file
        /// </summary>
        ISerialiser Serialiser { get; }
    }
}
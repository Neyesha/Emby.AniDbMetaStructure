namespace Jellyfin.AniDbMetaStructure.Files
{
    /// <summary>
    ///     A specification of a file containing serialised data of a known type
    /// </summary>
    /// <typeparam name="TRoot">The type of the serialised data</typeparam>
    internal interface IFileSpec<out TRoot> where TRoot : class
    {
        /// <summary>
        ///     The local path to the file
        /// </summary>
        string LocalPath { get; }
    }
}
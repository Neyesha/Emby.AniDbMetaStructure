namespace Jellyfin.AniDbMetaStructure.Files
{
    /// <summary>
    ///     A specification of a remote file which can be downloaded on demand
    /// </summary>
    /// <typeparam name="TRoot">The type of the serialised data</typeparam>
    internal interface IRemoteFileSpec<out TRoot> : IFileSpec<TRoot> where TRoot : class
    {
        bool IsGZipped { get; }

        string Url { get; }
    }
}
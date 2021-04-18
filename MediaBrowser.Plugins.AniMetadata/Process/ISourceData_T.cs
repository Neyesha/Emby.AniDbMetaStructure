namespace Jellyfin.AniDbMetaStructure.Process
{
    internal interface ISourceData<out TData> : ISourceData where TData : class
    {
        new TData Data { get; }
    }
}
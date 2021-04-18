namespace Jellyfin.AniDbMetaStructure.JsonApi
{
    internal interface IRequest<TResponse>
    {
        string Url { get; }
    }
}
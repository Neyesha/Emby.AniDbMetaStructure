namespace Jellyfin.AniDbMetaStructure.JsonApi
{
    internal abstract class PostRequest<TResponse> : Request<TResponse>, IPostRequest<TResponse>
    {
        protected PostRequest(string url, object data) : base(url)
        {
            this.Data = data;
        }

        public object Data { get; }
    }
}
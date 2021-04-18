namespace Jellyfin.AniDbMetaStructure.JsonApi
{
    public class Response<TResponseData>
    {
        public Response(TResponseData data)
        {
            this.Data = data;
        }

        public TResponseData Data { get; }
    }
}
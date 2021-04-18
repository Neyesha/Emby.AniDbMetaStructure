namespace Jellyfin.AniDbMetaStructure.Infrastructure
{
    public interface ISerialiser
    {
        T Deserialise<T>(string text);

        string Serialise<T>(T obj);
    }
}
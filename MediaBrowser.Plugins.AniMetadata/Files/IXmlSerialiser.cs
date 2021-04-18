using Jellyfin.AniDbMetaStructure.Infrastructure;

namespace Jellyfin.AniDbMetaStructure.Files
{
    public interface IXmlSerialiser : ISerialiser
    {
        void SerialiseToFile<T>(string filePath, T data) where T : class;
    }
}
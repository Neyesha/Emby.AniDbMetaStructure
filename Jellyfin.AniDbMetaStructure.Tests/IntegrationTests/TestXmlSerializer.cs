using System;
using System.IO;
using MediaBrowser.Model.Serialization;

namespace Emby.AniDbMetaStructure.Tests.IntegrationTests
{
    public class TestXmlSerializer : IXmlSerializer
    {
        public object DeserializeFromStream(Type type, Stream stream)
        {
            throw new NotImplementedException();
        }

        public void SerializeToStream(object obj, Stream stream)
        {
            throw new NotImplementedException();
        }

        public void SerializeToFile(object obj, string file)
        {
            throw new NotImplementedException();
        }

        public object DeserializeFromFile(Type type, string file)
        {
            throw new NotImplementedException();
        }

        public object DeserializeFromBytes(Type type, byte[] buffer)
        {
            throw new NotImplementedException();
        }
    }
}
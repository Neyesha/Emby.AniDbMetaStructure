using System.IO;

namespace Jellyfin.AniDbMetaStructure.Infrastructure
{
    internal static class Streams
    {
        public static Stream ToStream(string value)
        {
            var stream = new MemoryStream();

            WriteToStream(stream, value);

            stream.Position = 0;

            return stream;
        }

        public static void WriteToStream(Stream stream, string value)
        {
            var writer = new StreamWriter(stream);

            writer.Write(value);
            writer.Flush();
        }

        public static string ReadAll(Stream stream)
        {
            var reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }
    }
}
using Jellyfin.AniDbMetaStructure.Files;
using System.IO;

namespace Jellyfin.AniDbMetaStructure.AniDb.SeriesData
{
    internal class SeriesFileSpec : IRemoteFileSpec<AniDbSeriesData>
    {
        private const string ClientName = "mediabrowser";
        private const string SeriesPath = "anidb\\series";
        private readonly string rootPath;

        public SeriesFileSpec(string rootPath, int aniDbSeriesId)
        {
            this.rootPath = rootPath;
            const string seriesQueryUrl =
                "http://api.anidb.net:9001/httpapi?request=anime&client={0}&clientver=1&protover=1&aid={1}";

            Url = string.Format(seriesQueryUrl, ClientName, aniDbSeriesId);
            LocalPath = GetSeriesCacheFilePath(aniDbSeriesId);
        }

        public string Url { get; }

        public string LocalPath { get; }

        public bool IsGZipped => true;

        private string GetSeriesCacheFilePath(int aniDbSeriesId)
        {
            return Path.Combine(this.rootPath, SeriesPath, aniDbSeriesId.ToString(), "series.xml");
        }
    }
}
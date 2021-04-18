using System.IO;
using Jellyfin.AniDbMetaStructure.Files;
using Jellyfin.AniDbMetaStructure.Infrastructure;
using Jellyfin.AniDbMetaStructure.TvDb.Data;

namespace Jellyfin.AniDbMetaStructure.TvDb
{
    internal class TvDbSeriesEpisodesFileSpec : ILocalFileSpec<TvDbEpisodeCollection>
    {
        private readonly ICustomJsonSerialiser jsonSerialiser;
        private readonly string rootPath;
        private readonly int tvDbSeriesId;

        public TvDbSeriesEpisodesFileSpec(ICustomJsonSerialiser jsonSerialiser, string rootPath, int tvDbSeriesId)
        {
            this.jsonSerialiser = jsonSerialiser;
            this.rootPath = rootPath;
            this.tvDbSeriesId = tvDbSeriesId;
        }

        public string LocalPath => Path.Combine(this.rootPath, $"anidb\\tvdb\\{this.tvDbSeriesId}_Episodes.json");

        public ISerialiser Serialiser => this.jsonSerialiser;
    }
}
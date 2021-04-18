using System.IO;
using Jellyfin.AniDbMetaStructure.Files;

namespace Jellyfin.AniDbMetaStructure.AniDb.Titles
{
    internal class TitlesFileSpec : IRemoteFileSpec<TitleListData>
    {
        private const string TitlesPath = "anidb\\titles";
        private readonly string rootPath;

        public TitlesFileSpec(string rootPath)
        {
            this.rootPath = rootPath;
        }

        public string Url => "https://neyesha.myqnapcloud.com:8081/animetitles.xml";

        public string LocalPath => Path.Combine(this.rootPath, TitlesPath, "titles.xml");

        public bool IsGZipped => false;
    }
}
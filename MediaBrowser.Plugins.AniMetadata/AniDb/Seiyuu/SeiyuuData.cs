using System.Xml.Serialization;

namespace Jellyfin.AniDbMetaStructure.AniDb.Seiyuu
{
    public class SeiyuuData
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlAttribute("picture")]
        public string PictureFileName { get; set; }

        [XmlText]
        public string Name { get; set; }

        [XmlIgnore]
        public string PictureUrl => $"http://img7.anidb.net/pics/anime/{PictureFileName}";
    }
}
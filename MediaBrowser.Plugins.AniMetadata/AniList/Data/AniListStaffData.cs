namespace Jellyfin.AniDbMetaStructure.AniList.Data
{
    internal class AniListStaffData
    {
        private readonly InnerStaffData node;

        public AniListStaffData(InnerStaffData node, string role)
        {
            this.node = node;
            Role = role;
        }

        public AniListPersonNameData Name => this.node.Name;

        public AniListImageUrlData Image => this.node.Image;

        public string Role { get; }

        public class InnerStaffData
        {
            public InnerStaffData(AniListPersonNameData name, AniListImageUrlData image)
            {
                Name = name;
                Image = image;
            }

            public AniListPersonNameData Name { get; }

            public AniListImageUrlData Image { get; }
        }
    }
}
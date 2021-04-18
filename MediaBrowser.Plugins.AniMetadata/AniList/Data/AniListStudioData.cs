namespace Jellyfin.AniDbMetaStructure.AniList.Data
{
    internal class AniListStudioData
    {
        private readonly StudioName node;

        public AniListStudioData(bool isMain, StudioName node)
        {
            IsMain = isMain;
            this.node = node;
        }

        public bool IsMain { get; }

        public string Name => this.node.Name;

        public class StudioName
        {
            public StudioName(string name)
            {
                Name = name;
            }

            public string Name { get; }
        }
    }
}
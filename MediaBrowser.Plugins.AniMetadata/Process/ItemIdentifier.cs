using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.Process
{
    internal class ItemIdentifier : IItemIdentifier
    {
        public ItemIdentifier(Option<int> index, Option<int> parentIndex, string name)
        {
            this.Index = index;
            this.ParentIndex = parentIndex;
            this.Name = name;
        }

        public Option<int> Index { get; }

        public Option<int> ParentIndex { get; }

        public string Name { get; }

        public override string ToString()
        {
            return
                $"Name: '{this.Name}' Index: '{this.Index.Map(i => i.ToString()).IfNone(string.Empty)}', ParentIndex: '{this.ParentIndex.Map(i => i.ToString()).IfNone(string.Empty)}'";
        }
    }
}
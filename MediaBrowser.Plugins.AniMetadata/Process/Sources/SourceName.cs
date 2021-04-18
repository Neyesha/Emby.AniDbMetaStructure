namespace Jellyfin.AniDbMetaStructure.Process.Sources
{
    internal class SourceName
    {
        public SourceName(string name)
        {
            this.Name = name;
        }

        public string Name { get; }

        public static implicit operator string(SourceName sourceName)
        {
            return sourceName.Name;
        }

        public override string ToString()
        {
            return this;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.Process
{
    /// <summary>
    ///     The data supplied by Emby for an item at the start of the process
    /// </summary>
    internal class EmbyItemData : IEmbyItemData
    {

        public EmbyItemData(IMediaItemType itemType, IItemIdentifier identifier, IDictionary<string, int> existingIds,
            string language, IEnumerable<EmbyItemId> parentIds)
        {
            this.ParentIds = parentIds;
            this.ItemType = itemType;
            this.Identifier = identifier;
            this.Language = language;
            this.ExistingIds = existingIds ?? new Dictionary<string, int>();
        }

        public IMediaItemType ItemType { get; }

        /// <summary>
        ///     The identifier provided by Emby
        /// </summary>
        public IItemIdentifier Identifier { get; }

        public IDictionary<string, int> ExistingIds { get; }

        public string Language { get; }

        public IEnumerable<EmbyItemId> ParentIds { get; }

        /// <summary>
        ///     True if this data came from the file system rather than the Emby library
        /// </summary>
        public bool IsFileData => !this.ExistingIds.Any();

        /// <summary>
        ///     Get the id that already exists in Emby for a particular source
        /// </summary>
        public Option<int> GetExistingId(string sourceName)
        {
            return !this.ExistingIds.ContainsKey(sourceName) ? Option<int>.None : this.ExistingIds[sourceName];
        }

        /// <summary>
        ///     Get the id of the parent of the specified item type for a particular source
        /// </summary>
        public Option<int> GetParentId(IMediaItemType itemType, ISource source)
        {
            var parentId = this.ParentIds.Find(id => id.ItemType == itemType && id.SourceName == source.Name);

            return parentId.Map(id => id.Id);
        }

        public override string ToString()
        {
            return $"{this.ItemType} '{this.Identifier}'";
        }
    }
}
using LanguageExt;
using System.Collections.Generic;

namespace Jellyfin.AniDbMetaStructure.Process
{
    internal interface IJellyfinItemData
    {
        IDictionary<string, int> ExistingIds { get; }

        IItemIdentifier Identifier { get; }

        bool IsFileData { get; }

        IMediaItemType ItemType { get; }

        string Language { get; }

        IEnumerable<JellyfinItemId> ParentIds { get; }

        Option<int> GetExistingId(string sourceName);

        Option<int> GetParentId(IMediaItemType itemType, ISource source);
    }
}
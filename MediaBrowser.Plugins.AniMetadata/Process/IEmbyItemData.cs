using LanguageExt;
using System.Collections.Generic;

namespace Jellyfin.AniDbMetaStructure.Process
{
    internal interface IEmbyItemData
    {
        IDictionary<string, int> ExistingIds { get; }

        IItemIdentifier Identifier { get; }

        bool IsFileData { get; }

        IMediaItemType ItemType { get; }

        string Language { get; }

        IEnumerable<EmbyItemId> ParentIds { get; }

        Option<int> GetExistingId(string sourceName);

        Option<int> GetParentId(IMediaItemType itemType, ISource source);
    }
}
using System;
using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.Process
{
    internal class IdentifierOnlySourceData : ISourceData<IdentifierOnlySourceData>
    {
        public IdentifierOnlySourceData(ISource source, Option<int> id, IItemIdentifier identifier, IMediaItemType itemType)
        {
            this.Source = source ?? throw new ArgumentNullException(nameof(source));
            this.Id = id;
            this.Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            this.ItemType = itemType;
        }

        public object Data => this;

        public Option<int> Id { get; }

        public IItemIdentifier Identifier { get; }

        public IMediaItemType ItemType { get; set; }

        public ISource Source { get; }

        IdentifierOnlySourceData ISourceData<IdentifierOnlySourceData>.Data => this;

        public Option<TData> GetData<TData>() where TData : class
        {
            return this as TData;
        }
    }
}
using System;
using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.Process
{
    internal class SourceData<TData> : ISourceData<TData> where TData : class
    {
        private readonly TData data;

        public SourceData(ISource source, Option<int> id, IItemIdentifier identifier, TData data)
        {
            this.data = data ?? throw new ArgumentNullException(nameof(data));
            this.Id = id;
            this.Source = source ?? throw new ArgumentNullException(nameof(source));
            this.Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
        }
        
        public ISource Source { get; }

        public Option<int> Id { get; }

        public IItemIdentifier Identifier { get; }

        public IMediaItemType ItemType { get; }

        public TData Data => this.data;

        object ISourceData.Data => this.Data;
        
        public Option<TRequestedData> GetData<TRequestedData>() where TRequestedData : class
        {
            return this.data as TRequestedData;
        }
    }
}
using System.Collections.Generic;

namespace Jellyfin.AniDbMetaStructure.AniList.Data
{
    internal class GraphQlEdge<T>
    {
        public GraphQlEdge(IEnumerable<T> edges)
        {
            Edges = edges;
        }

        public IEnumerable<T> Edges { get; }
    }
}
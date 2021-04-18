using System.Collections;
using Jellyfin.AniDbMetaStructure.Process;

namespace Jellyfin.AniDbMetaStructure.Tests.TestHelpers
{
    internal class MediaItemTypeTestCases : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            var cases = new IMediaItemType[] { MediaItemTypes.Series, MediaItemTypes.Season, MediaItemTypes.Episode };

            foreach (var mediaItemType in cases) yield return mediaItemType;
        }
    }
}
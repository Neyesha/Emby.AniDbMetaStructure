namespace Jellyfin.AniDbMetaStructure.Mapping
{
    public class SpecialEpisodePosition
    {
        public SpecialEpisodePosition(int specialEpisodeIndex, int followingStandardEpisodeIndex)
        {
            this.SpecialEpisodeIndex = specialEpisodeIndex;
            this.FollowingStandardEpisodeIndex = followingStandardEpisodeIndex;
        }

        public int SpecialEpisodeIndex { get; }

        public int FollowingStandardEpisodeIndex { get; }
    }
}
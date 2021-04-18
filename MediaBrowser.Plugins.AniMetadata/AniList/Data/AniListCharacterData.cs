using System.Collections.Generic;

namespace Jellyfin.AniDbMetaStructure.AniList.Data
{
    internal class AniListCharacterData
    {
        private readonly InnerCharacterData node;

        public AniListCharacterData(InnerCharacterData node, IEnumerable<VoiceActorData> voiceActors)
        {
            this.node = node;
            VoiceActors = voiceActors;
        }

        public AniListPersonNameData Name => this.node.Name;

        public AniListImageUrlData Image => this.node.Image;

        public IEnumerable<VoiceActorData> VoiceActors { get; }

        public class InnerCharacterData
        {
            public InnerCharacterData(AniListPersonNameData name, AniListImageUrlData image)
            {
                Name = name;
                Image = image;
            }

            public AniListPersonNameData Name { get; }
            public AniListImageUrlData Image { get; }
        }

        public class VoiceActorData
        {
            public VoiceActorData(string language, AniListImageUrlData image, AniListPersonNameData name)
            {
                Language = language;
                Image = image;
                Name = name;
            }

            public string Language { get; }
            public AniListImageUrlData Image { get; }
            public AniListPersonNameData Name { get; }
        }
    }
}
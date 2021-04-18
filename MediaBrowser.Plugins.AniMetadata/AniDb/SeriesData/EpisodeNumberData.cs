using System;
using System.Xml.Serialization;

namespace Jellyfin.AniDbMetaStructure.AniDb.SeriesData
{
    public class EpisodeNumberData : IAniDbEpisodeNumber
    {
        /// <summary>
        ///     1 = Normal episode,
        ///     2 = Special,
        ///     3 = Op / Ed,
        ///     4 = Trailer,
        ///     5 = Parody / Fandub
        /// </summary>
        [XmlAttribute("type")]
        public int RawType { get; set; }

        /// <summary>
        ///     Where x is an integer:
        ///     x for a normal episode,
        ///     Sx for a special episode,
        ///     Cx for an op / ed,
        ///     Px for a parody / fandub
        ///     Tx for a trailer / promo
        ///     Ox for other episodes
        /// </summary>
        [XmlText]
        public string RawNumber { get; set; }

        public int Number
        {
            get
            {
                var episodeNumberString = this.RemoveEpisodeNumberPrefix(this.RawNumber);

                if (int.TryParse(episodeNumberString, out var episodeNumber))
                {
                    return episodeNumber;
                }

                throw new FormatException($"Can't parse episode number: '{episodeNumberString}' (raw value: {this.RawNumber})");
            }
        }

        public EpisodeType Type => (EpisodeType)this.RawType;

        public int SeasonNumber => this.Type == EpisodeType.Normal ? 1 : 0;

        private string RemoveEpisodeNumberPrefix(string episodeNumber)
        {
            episodeNumber = episodeNumber ?? "-1";

            switch (this.Type)
            {
                case EpisodeType.Special:
                    return episodeNumber.TrimStart('S');
                case EpisodeType.OpEd:
                    return episodeNumber.TrimStart('C');
                case EpisodeType.ParodyOrFanDub:
                    return episodeNumber.TrimStart('P');
                case EpisodeType.Trailer:
                    return episodeNumber.TrimStart('T');
                case EpisodeType.Other:
                    return episodeNumber.TrimStart('O');
                default:
                    return episodeNumber;
            }
        }

        public override string ToString()
        {
            return this.RawNumber;
        }
    }
}
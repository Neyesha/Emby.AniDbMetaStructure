﻿using System.Threading.Tasks;
using Emby.AniDbMetaStructure.AniDb.SeriesData;
using Emby.AniDbMetaStructure.Process;
using LanguageExt;

namespace Emby.AniDbMetaStructure.SourceDataLoaders
{
    /// <summary>
    ///     Loads TvDb season data based on existing AniDb series information
    /// </summary>
    internal class TvDbSeasonFromAniDb : ISourceDataLoader
    {
        private readonly ISources sources;

        public TvDbSeasonFromAniDb(ISources sources)
        {
            this.sources = sources;
        }

        public bool CanLoadFrom(object sourceData)
        {
            return sourceData is ISourceData<AniDbSeriesData>;
        }

        public Task<Either<ProcessFailedResult, ISourceData>> LoadFrom(IMediaItem mediaItem, object sourceData)
        {
            var resultContext = new ProcessResultContext(nameof(TvDbSeasonFromAniDb),
                mediaItem.EmbyData.Identifier.Name,
                mediaItem.ItemType);

            return mediaItem.EmbyData.Identifier.Index
                .ToEither(resultContext.Failed("No season index provided by Emby"))
                .Map(this.CreateSourceData)
                .AsTask();
        }

        private ISourceData CreateSourceData(int seasonIndex)
        {
            return new IdentifierOnlySourceData(this.sources.TvDb, seasonIndex,
                new ItemIdentifier(seasonIndex, Option<int>.None, $"Season {seasonIndex}"), MediaItemTypes.Season);
        }
    }
}
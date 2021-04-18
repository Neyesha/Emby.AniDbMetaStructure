using Jellyfin.AniDbMetaStructure.Files;
using Jellyfin.AniDbMetaStructure.Mapping.Data;
using Jellyfin.AniDbMetaStructure.Process;
using LanguageExt;
using MediaBrowser.Common.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xem.Api;
using Xem.Api.Mapping;
using static LanguageExt.Prelude;

namespace Jellyfin.AniDbMetaStructure.Mapping
{
    internal class MappingList : IMappingList
    {
        private readonly IFileCache fileCache;
        private readonly Lazy<Task<IEnumerable<SeriesMapping>>> mappingListTaskLazy;
        private readonly Lazy<Task<IEnumerable<SeriesMapping>>> xemAniDbMappingListTaskLazy;
        private readonly Lazy<Task<IEnumerable<SeriesMapping>>> xemTvDbMappingListTaskLazy;
        private readonly MappingsFileSpec mappingsFileSpec;
        private readonly XemAniDbMappingsFileSpec mappingsAniDbXemFileSpec;
        private readonly XemTvDbMappingsFileSpec mappingsTvDbXemFileSpec;

        public MappingList(IApplicationPaths applicationPaths, IFileCache fileCache, IApiClient xemApiClient)
        {
            this.mappingsFileSpec = new MappingsFileSpec(applicationPaths.CachePath);
            this.mappingsAniDbXemFileSpec = new XemAniDbMappingsFileSpec(applicationPaths.CachePath, xemApiClient);
            this.mappingsTvDbXemFileSpec = new XemTvDbMappingsFileSpec(applicationPaths.CachePath, xemApiClient);
            this.fileCache = fileCache;
            this.mappingListTaskLazy =
                new Lazy<Task<IEnumerable<SeriesMapping>>>(() => CreateMappingListAsync(CancellationToken.None));
            this.xemAniDbMappingListTaskLazy =
                new Lazy<Task<IEnumerable<SeriesMapping>>>(() => CreateXemAniDbMappingListAsync(CancellationToken.None));
            this.xemTvDbMappingListTaskLazy =
                new Lazy<Task<IEnumerable<SeriesMapping>>>(() => CreateXemTvDbMappingListAsync(CancellationToken.None));
        }

        public async Task<Either<ProcessFailedResult, ISeriesMapping>> GetSeriesMappingFromAniDb(int aniDbSeriesId,
            ProcessResultContext resultContext)
        {
            var result = await this.mappingListTaskLazy.Value
                .Map(seriesMappings => seriesMappings.Where(m => m.Ids.AniDbSeriesId == aniDbSeriesId).ToList())
                .Map(matchingSeriesMappings =>
                {
                    switch (matchingSeriesMappings.Count)
                    {
                        case 0:
                            return GetFallbackSeriesMappingFromAniDb(aniDbSeriesId, resultContext).GetAwaiter().GetResult();

                        case 1:
                            return Right<ProcessFailedResult, ISeriesMapping>(matchingSeriesMappings.Single());

                        default:
                            return GetFallbackSeriesMappingFromAniDb(aniDbSeriesId, resultContext).GetAwaiter().GetResult();
                    }
                });


            return result;
        }

        private async Task<Either<ProcessFailedResult, ISeriesMapping>> GetFallbackSeriesMappingFromAniDb(int aniDbSeriesId,
            ProcessResultContext resultContext)
        {
            return await this.xemAniDbMappingListTaskLazy.Value
                .Map(seriesMappings => seriesMappings.Where(m => m.Ids.AniDbSeriesId == aniDbSeriesId).ToList())
                .Map(matchingSeriesMappings =>
                {
                    switch (matchingSeriesMappings.Count)
                    {
                        case 0:
                            return Left<ProcessFailedResult, ISeriesMapping>(
                                resultContext.Failed($"No series mapping for AniDb series Id '{aniDbSeriesId}'"));

                        case 1:
                            return Right<ProcessFailedResult, ISeriesMapping>(matchingSeriesMappings.Single());

                        default:
                            return Left<ProcessFailedResult, ISeriesMapping>(
                                resultContext.Failed(
                                    $"Multiple series mappings match AniDb series Id '{aniDbSeriesId}'"));
                    }
                });
        }

        public Task<Either<ProcessFailedResult, IEnumerable<ISeriesMapping>>> GetSeriesMappingsFromTvDb(
            int tvDbSeriesId, ProcessResultContext resultContext)
        {
            return this.mappingListTaskLazy.Value.Map(seriesMappings =>
                    seriesMappings.Where(m => m.Ids.TvDbSeriesId == tvDbSeriesId).ToList())
                .Map(matchingSeriesMappings =>
                    matchingSeriesMappings.Any()
                        ? Right<ProcessFailedResult, IEnumerable<ISeriesMapping>>(matchingSeriesMappings)
                        : GetFallbackSeriesMappingFromTvDb(tvDbSeriesId, resultContext).GetAwaiter().GetResult());
        }

        private async Task<Either<ProcessFailedResult, IEnumerable<ISeriesMapping>>> GetFallbackSeriesMappingFromTvDb(int tvDbSeriesId,
            ProcessResultContext resultContext)
        {
            return await this.xemTvDbMappingListTaskLazy.Value
                .Map(seriesMappings => seriesMappings.Where(m => m.Ids.TvDbSeriesId == tvDbSeriesId).ToList())
                .Map(matchingSeriesMappings =>
                {
                    switch (matchingSeriesMappings.Count)
                    {
                        case 0:
                            return Left<ProcessFailedResult, IEnumerable<ISeriesMapping>>(
                                resultContext.Failed($"No series mapping for TvDb series Id '{tvDbSeriesId}'"));

                        case 1:
                            return Right<ProcessFailedResult, IEnumerable<ISeriesMapping>>(matchingSeriesMappings);

                        default:
                            return Left<ProcessFailedResult, IEnumerable<ISeriesMapping>>(
                                resultContext.Failed(
                                    $"Multiple series mappings match TvDb series Id '{tvDbSeriesId}'"));
                    }
                });
        }

        private async Task<IEnumerable<SeriesMapping>> CreateMappingListAsync(CancellationToken cancellationToken)
        {
            var mappingList = await this.fileCache.GetFileContentAsync(this.mappingsFileSpec, cancellationToken);

            return mappingList.Match(l =>
                {
                    if (!IsValidData(l))
                    {
                        return new List<SeriesMapping>();
                    }

                    return l.AnimeSeriesMapping.Select(SeriesMapping.FromData).Somes();
                },
                () => new List<SeriesMapping>());
        }

        private async Task<IEnumerable<SeriesMapping>> CreateXemAniDbMappingListAsync(CancellationToken cancellationToken)
        {
            var mappingList = await this.fileCache.GetFileContentAsync(this.mappingsAniDbXemFileSpec, cancellationToken);

            return mappingList.Match(l =>
            {
                if (!IsValidData(l))
                {
                    return new List<SeriesMapping>();
                }

                return l.Select(r => SeriesMapping.FromData(r, EntityType.AniDb)).Somes();
            },
                () => new List<SeriesMapping>());
        }

        private async Task<IEnumerable<SeriesMapping>> CreateXemTvDbMappingListAsync(CancellationToken cancellationToken)
        {
            var mappingList = await this.fileCache.GetFileContentAsync(this.mappingsTvDbXemFileSpec, cancellationToken);

            return mappingList.Match(l =>
            {
                if (!IsValidData(l))
                {
                    return new List<SeriesMapping>();
                }

                return l.Select(r => SeriesMapping.FromData(r, EntityType.TvDb)).Somes();
            },
                () => new List<SeriesMapping>());
        }

        private static bool IsValidData(IDictionary<string, string[]> data)
        {
            return data?.Any() == true;
        }

        private static bool IsValidData(AnimeMappingListData data)
        {
            return data?.AnimeSeriesMapping != null;
        }
    }
}
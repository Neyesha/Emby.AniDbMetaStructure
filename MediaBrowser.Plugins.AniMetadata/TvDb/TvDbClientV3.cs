using AutoMapper;
using Jellyfin.AniDbMetaStructure.Configuration;
using Jellyfin.AniDbMetaStructure.Files;
using Jellyfin.AniDbMetaStructure.Infrastructure;
using Jellyfin.AniDbMetaStructure.JsonApi;
using Jellyfin.AniDbMetaStructure.TvDb.Data;
using Jellyfin.AniDbMetaStructure.TvDb.Data.Mappers;
using Jellyfin.AniDbMetaStructure.TvDb.Requests;
using LanguageExt;
using MediaBrowser.Common.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TvDbSharper;
using TvDbSharper.Dto;

namespace Jellyfin.AniDbMetaStructure.TvDb
{
    internal class TvDbClientV3 : ITvDbClient
    {
        private readonly IApplicationPaths applicationPaths;
        private readonly IFileCache fileCache;
        private readonly ILogger logger;
        private readonly TvDbSharper.ITvDbClient tvDbClient;
        private readonly IMapper dataMapper;
        private readonly ICustomJsonSerialiser jsonSerialiser;

        public TvDbClientV3(IJsonConnection jsonConnection, IFileCache fileCache, IApplicationPaths applicationPaths,
            ILogger logger, ICustomJsonSerialiser jsonSerialiser, PluginConfiguration configuration)
        {
            this.logger = logger;
            this.fileCache = fileCache;
            this.applicationPaths = applicationPaths;
            this.jsonSerialiser = jsonSerialiser;
            this.tvDbClient = new TvDbClient();
            this.tvDbClient.Authentication.AuthenticateAsync(configuration.TvDbApiKey).GetAwaiter().GetResult();
            this.dataMapper = CreateDataMapper();
        }

        private IMapper CreateDataMapper()
        {
            // Configure AutoMapper
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<int?, Option<long>>().ConvertUsing(new LongConverter());
                cfg.CreateMap<string, Option<DateTime>>().ConvertUsing(new DateTimeConverter());
                cfg.CreateMap<string, Option<AirDay>>().ConvertUsing(new AirDayConverter());
                cfg.CreateMap<Series, TvDbSeriesData>()
                    .ForMember(dest => dest.Runtime, opt => opt.ConvertUsing(new IntFormatter()))
                    .ForMember(dest => dest.SiteRating, opt => opt.MapFrom(src => (float)(src.SiteRating ?? 0m)));
                cfg.CreateMap<EpisodeRecord, TvDbEpisodeData>()
                    .ForMember(dest => dest.AiredEpisodeNumber, opt => opt.MapFrom(src => src.AiredEpisodeNumber ?? 0m))
                    .ForMember(dest => dest.AiredSeason, opt => opt.MapFrom(src => src.AiredSeason ?? 0m))
                    .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(src => (int)src.LastUpdated))
                    .ForMember(dest => dest.SiteRating, opt => opt.MapFrom(src => (float)(src.SiteRating ?? 0m)))
                    .ForMember(dest => dest.SiteRatingCount, opt => opt.MapFrom(src => src.SiteRatingCount ?? 0m));
            });

            return configuration.CreateMapper();
        }

        public async Task<Option<TvDbSeriesData>> GetSeriesAsync(int tvDbSeriesId)
        {
            var localSeriesData = GetLocalTvDbSeriesData(tvDbSeriesId);

            var episodes = await localSeriesData.MatchAsync(e => e,
                async () =>
                {
                    var seriesData = await RequestSeriesAsync(tvDbSeriesId);

                    seriesData.Iter(SaveTvDbSeries);

                    return seriesData;
                });

            return episodes;
        }

        public async Task<Option<TvDbSeriesData>> FindSeriesAsync(string seriesName)
        {
            string comparableName = GetComparableName(seriesName);

            var response = await this.tvDbClient.Search.SearchSeriesByNameAsync(seriesName);
            var data = ParseResponse<SeriesSearchResult[], SeriesSearchResult[]>(response);

            return data.Match(
                            r =>
                            {
                                var matchingSeries = r.Data.ToList();
                                var bestResult = matchingSeries.OrderBy(
                                        i =>
                                        {
                                            var tvdbTitles = new List<string>();
                                            tvdbTitles.Add(GetComparableName(i.SeriesName));
                                            tvdbTitles.AddRange(i.Aliases);
                                            return tvdbTitles.Contains(comparableName, StringComparer.OrdinalIgnoreCase) ? 0 : 1;
                                        })
                                    .ThenBy(i => matchingSeries.IndexOf(i))
                                    .FirstOrDefault();

                                if (bestResult != null)
                                {
                                    try
                                    {
                                        return GetSeriesAsync(bestResult.Id).GetAwaiter().GetResult();
                                    }
                                    catch (TvDbServerException)
                                    {
                                        //Do nothing
                                    }
                                }
                                return Option<TvDbSeriesData>.None;
                            },
                            fr => Option<TvDbSeriesData>.None);
        }

        /// <summary>
        /// The remove
        /// </summary>
        private const string Remove = "\"'!`?";

        /// <summary>
        /// The spacers
        /// </summary>
        private const string Spacers = "/,.:;\\(){}[]+-_=–*";  // (there are not actually two - in the they are different char codes)

        private string GetComparableName(string name)
        {
            name = name.ToLower();
            var sb = new StringBuilder();
            foreach (char c in name)
            {
                if (c >= 0x2B0 && c <= 0x0333)
                {
                    // skip char modifier and diacritics
                }
                else if (Remove.IndexOf(c) > -1)
                {
                    // skip chars we are removing
                }
                else if (Spacers.IndexOf(c) > -1)
                {
                    sb.Append(" ");
                }
                else if (c == '&')
                {
                    sb.Append(" and ");
                }
                else
                {
                    sb.Append(c);
                }
            }
            name = sb.ToString();
            name = name.Replace(", the", "");
            name = name.Replace("the ", " ");
            name = name.Replace(" the ", " ");

            string prevName;
            do
            {
                prevName = name;
                name = name.Replace("  ", " ");
            } while (name.Length != prevName.Length);

            return name.Trim();
        }

        public Task<Option<List<TvDbEpisodeData>>> GetEpisodesAsync(int tvDbSeriesId)
        {
            var localEpisodes = GetLocalTvDbEpisodeData(tvDbSeriesId).Map(e => e.ToList());

            var episodes = localEpisodes.Match(e => Task.FromResult((Option<List<TvDbEpisodeData>>)e),
                () => RequestEpisodesAsync(tvDbSeriesId));

            return episodes;
        }

        public async Task<Option<TvDbEpisodeData>> GetEpisodeAsync(int tvDbSeriesId, int seasonIndex,
            int episodeIndex)
        {
            var episodes = await GetEpisodesAsync(tvDbSeriesId);

            return episodes.Match(ec =>
                    ec.Find(e => e.AiredSeason == seasonIndex && e.AiredEpisodeNumber == episodeIndex),
                () => Option<TvDbEpisodeData>.None);
        }

        public async Task<Option<TvDbEpisodeData>> GetEpisodeAsync(int tvDbSeriesId, int absoluteEpisodeIndex)
        {
            var episodes = await GetEpisodesAsync(tvDbSeriesId);

            return episodes.Match(ec => ((IEnumerable<TvDbEpisodeData>)ec).Find(e =>
                    e.AbsoluteNumber.Match(index => index == absoluteEpisodeIndex, () => false)),
                () => Option<TvDbEpisodeData>.None);
        }

        private async Task<Option<TvDbSeriesData>> RequestSeriesAsync(int tvDbSeriesId)
        {
            var response = await this.tvDbClient.Series.GetAsync(tvDbSeriesId);

            return ParseResponse<Series, TvDbSeriesData>(response).Match(
                r => r.Data,
                fr => Option<TvDbSeriesData>.None);
        }

        private Either<FailedRequest, Response<TResponseData>> ParseResponse<TResponseType, TResponseData>(TvDbResponse<TResponseType> response)
        {
            if (response.Data == null && response.Errors?.InvalidFilters?.Any() == true || response.Errors?.InvalidLanguage?.Any() == true || response.Errors?.InvalidQueryParams?.Any() == true)
            {
                return new FailedRequest(HttpStatusCode.OK, response.ToString());
            }

            // Perform mapping
            var data = this.dataMapper.Map<TResponseType, TResponseData>(response.Data);

            return new Response<TResponseData>(data);
        }

        private Option<IEnumerable<TvDbEpisodeData>> GetLocalTvDbEpisodeData(int tvDbSeriesId)
        {
            var fileSpec = new TvDbSeriesEpisodesFileSpec(this.jsonSerialiser, this.applicationPaths.CachePath, tvDbSeriesId);

            return this.fileCache.GetFileContent(fileSpec).Select(c => c.Episodes);
        }

        private Option<TvDbSeriesData> GetLocalTvDbSeriesData(int tvDbSeriesId)
        {
            var fileSpec = new TvDbSeriesFileSpec(this.jsonSerialiser, this.applicationPaths.CachePath, tvDbSeriesId);

            return this.fileCache.GetFileContent(fileSpec);
        }

        private async Task<Option<List<TvDbEpisodeData>>> RequestEpisodesAsync(int tvDbSeriesId)
        {
            var response = await this.tvDbClient.Series.GetEpisodesAsync(tvDbSeriesId, 1);
            var data = ParseResponse<EpisodeRecord[], TvDbEpisodeData[]>(response);

            return await data.Match(async r =>
                {
                    var episodes = r.Data.ToList();

                    if (response.Links.Last > 1)
                    {
                        episodes = episodes.Concat(await RequestEpisodePagesAsync(tvDbSeriesId, 2, response.Links.Last ?? 2))
                            .ToList();
                    }

                    var episodeDetails = (await episodes.Map(e => e.Id).Map(RequestEpisodeDetailAsync)).Somes().ToList();

                    SaveTvDbEpisodes(tvDbSeriesId, episodeDetails);

                    return (Option<List<TvDbEpisodeData>>)episodeDetails.ToList();
                },
                fr => Task.FromResult(Option<List<TvDbEpisodeData>>.None));
        }

        private async Task<Option<TvDbEpisodeData>> RequestEpisodeDetailAsync(int episodeId)
        {
            var response = await this.tvDbClient.Episodes.GetAsync(episodeId);
            var data = ParseResponse<EpisodeRecord, TvDbEpisodeData>(response);

            return data.Match(r => r.Data,
                fr => null);
        }

        private void SaveTvDbEpisodes(int tvDbSeriesId, IEnumerable<TvDbEpisodeData> episodes)
        {
            var fileSpec = new TvDbSeriesEpisodesFileSpec(this.jsonSerialiser, this.applicationPaths.CachePath, tvDbSeriesId);

            this.fileCache.SaveFile(fileSpec, new TvDbEpisodeCollection(episodes));
        }

        private void SaveTvDbSeries(TvDbSeriesData tvDbSeries)
        {
            var fileSpec = new TvDbSeriesFileSpec(this.jsonSerialiser, this.applicationPaths.CachePath, tvDbSeries.Id);

            this.fileCache.SaveFile(fileSpec, tvDbSeries);
        }

        private async Task<IEnumerable<TvDbEpisodeData>> RequestEpisodePagesAsync(int tvDbSeriesId,
            int startPageIndex, int endPageIndex)
        {
            var episodeData = new List<TvDbEpisodeData>();

            for (int i = startPageIndex; i <= endPageIndex; i++)
            {
                var response = await this.tvDbClient.Series.GetEpisodesAsync(tvDbSeriesId, i);
                var data = ParseResponse<EpisodeRecord[], TvDbEpisodeData[]>(response);
                var dataList = data.RightToList().Select(d => d.Data);
                dataList.Iter(e => episodeData.AddRange(e));
            }

            return episodeData;
        }

        private async Task<Option<List<TvDbEpisodeData>>> RequestEpisodesPageAsync(int tvDbSeriesId, int pageIndex)
        {
            var request = new GetEpisodesRequest(tvDbSeriesId, pageIndex);

            var response = await this.tvDbClient.Series.GetEpisodesAsync(tvDbSeriesId, pageIndex);
            var data = ParseResponse<EpisodeRecord[], TvDbEpisodeData[]>(response);

            return data.Match(r => r.Data.ToList(),
                fr => Option<List<TvDbEpisodeData>>.None);
        }
    }
}
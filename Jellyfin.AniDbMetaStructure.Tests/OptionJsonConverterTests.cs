using System;
using System.Collections.Generic;
using Jellyfin.AniDbMetaStructure.Infrastructure;
using Jellyfin.AniDbMetaStructure.TvDb;
using Jellyfin.AniDbMetaStructure.TvDb.Data;
using Jellyfin.AniDbMetaStructure.TvDb.Requests;
using FluentAssertions;
using LanguageExt;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Jellyfin.AniDbMetaStructure.Tests
{
    [TestFixture]
    public class OptionJsonConverterTests
    {
        [SetUp]
        public void Setup()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new OptionJsonConverter() }
            };
            this.dataNull = new TvDbEpisodeSummaryData(1, "Test", Option<long>.None, 1, 2, 2, new DateTime(2007, 10, 6),
                "Overview");
            this.dataNonNull = new TvDbEpisodeSummaryData(1, "Test", 5L, 1, 2, 2, new DateTime(2007, 10, 6), "Overview");
        }

        private TvDbEpisodeSummaryData dataNull;
        private TvDbEpisodeSummaryData dataNonNull;

        [Test]
        public void AirDayEnum_CanDeserialise()
        {
            var serialised = @"{
  ""Id"": 78914,
  ""SeriesName"": ""Full Metal Panic!"",
  ""AirsDayOfWeek"": 1
}";

            JsonConvert.DeserializeObject<TvDbSeriesData>(serialised)
                .Should()
                .BeEquivalentTo(new TvDbSeriesData(78914, "Full Metal Panic!", Option<DateTime>.None, null, 0,
                    Option<AirDay>.Some(AirDay.Tuesday), null, 0, null, null, null));
        }

        [Test]
        public void ComplexMaybeSome_CanDeserialise()
        {
            var serialised = @"
{
  ""links"": {
    ""first"": 1,
    ""last"": 1,
    ""next"": null,
    ""prev"": null
  },
  ""data"": [
    {
      ""absoluteNumber"": 1,
      ""airedEpisodeNumber"": 1,
      ""airedSeason"": 1,
      ""airedSeasonID"": 29182,
      ""dvdEpisodeNumber"": null,
      ""dvdSeason"": null,
      ""episodeName"": ""Celestial Being"",
      ""firstAired"": ""2007-10-06"",
      ""id"": 340368,
      ""language"": {
        ""episodeName"": ""en"",
        ""overview"": ""en""
      },
      ""lastUpdated"": 1496255818,
      ""overview"": ""Celestial Being, a private army dedicated to eradicating war, begins demonstrating the powers of their new \""MS-GUNDAM\"" suits by interrupting the public demonstration of AEU's latest Mobile Suit, the AEU Enact and by protecting the Human Reform League's Space Elevator, \""Tenchu\"" from being attacked by terrorists when their mobile suits had attempted to launch rockets on the \""Tenchu\"", earning a news appearance from various TV news channels where Celestial Being's goals were publicly stated by Aeoria Schenberg.""
    }
  ]
}";

            JsonConvert.DeserializeObject<GetEpisodesRequest.Response>(serialised)
                .Should()
                .BeEquivalentTo(new GetEpisodesRequest.Response(
                        new[]
                        {
                            new TvDbEpisodeSummaryData(340368, "Celestial Being", 1L, 1, 1, 1496255818,
                                new DateTime(2007, 10, 6),
                                @"Celestial Being, a private army dedicated to eradicating war, begins demonstrating the powers of their new ""MS-GUNDAM"" suits by interrupting the public demonstration of AEU's latest Mobile Suit, the AEU Enact and by protecting the Human Reform League's Space Elevator, ""Tenchu"" from being attacked by terrorists when their mobile suits had attempted to launch rockets on the ""Tenchu"", earning a news appearance from various TV news channels where Celestial Being's goals were publicly stated by Aeoria Schenberg.")
                        },
                        new GetEpisodesRequest.PageLinks(1, 1, Option<int>.None, Option<int>.None)),
                    o => o.Excluding(i =>
                        i.SelectedMemberInfo.Name == "Value" &&
                        i.SelectedMemberInfo.DeclaringType == typeof(Option<int>)));
        }

        [Test]
        public void MaybeNone_CanDeserialise()
        {
            var serialised = @"{
		""Id"": 1,
		""EpisodeName"": ""Test"",
		""AbsoluteNumber"": null,
		""AiredEpisodeNumber"": 1,
		""AiredSeason"": 2,
		""LastUpdated"": 2,
        ""firstAired"": ""2007-10-06"",
        ""overview"": ""Overview""
	}";

            JsonConvert.DeserializeObject<TvDbEpisodeSummaryData>(serialised)
                .Should()
                .BeEquivalentTo(this.dataNull);
        }

        [Test]
        public void MaybeNone_CanSerialise()
        {
            JsonConvert.SerializeObject(this.dataNull)
                .Should()
                .Be(
                    "{\"Id\":1,\"EpisodeName\":\"Test\",\"AbsoluteNumber\":null,\"AiredEpisodeNumber\":1,\"AiredSeason\":2,\"LastUpdated\":2,\"FirstAired\":\"2007-10-06T00:00:00\",\"Overview\":\"Overview\"}");
        }

        [Test]
        public void MaybeSome_CanDeserialise()
        {
            var serialised = @"{
		""Id"": 1,
		""EpisodeName"": ""Test"",
		""AbsoluteNumber"": 5,
		""AiredEpisodeNumber"": 1,
		""AiredSeason"": 2,
		""LastUpdated"": 2,
        ""firstAired"": ""2007-10-06"",
        ""overview"": ""Overview""
	}";

            JsonConvert.DeserializeObject<TvDbEpisodeSummaryData>(serialised)
                .Should()
                .BeEquivalentTo(this.dataNonNull);
        }

        [Test]
        public void MaybeSome_CanSerialise()
        {
            JsonConvert.SerializeObject(this.dataNonNull)
                .Should()
                .Be(
                    "{\"Id\":1,\"EpisodeName\":\"Test\",\"AbsoluteNumber\":5,\"AiredEpisodeNumber\":1,\"AiredSeason\":2,\"LastUpdated\":2,\"FirstAired\":\"2007-10-06T00:00:00\",\"Overview\":\"Overview\"}");
        }
    }
}
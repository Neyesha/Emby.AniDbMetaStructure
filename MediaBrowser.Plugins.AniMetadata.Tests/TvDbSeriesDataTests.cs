﻿using System.Collections.Generic;
using FluentAssertions;
using LanguageExt.UnsafeValueAccess;
using MediaBrowser.Plugins.AniMetadata.TvDb;
using MediaBrowser.Plugins.AniMetadata.TvDb.Data;
using Newtonsoft.Json;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests
{
    using Infrastructure;

    [TestFixture]
    public class TvDbSeriesDataTests
    {
        [SetUp]
        public void Setup()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new OptionJsonConverter() }
            };
        }

        [Test]
        public void Deserialise_DailydAirsDayOfWeek_ReturnsSome()
        {
            var serialiser = new JsonSerialiser();

            var value = serialiser.Deserialise<TvDbSeriesData>(@"{
                airsDayOfWeek: ""Daily""
            }")
                .AirsDayOfWeek;

            value.ValueUnsafe().Should().Be(AirDay.Daily);
        }

        [Test]
        public void Deserialise_EmptyAirsDayOfWeek_SetsToNone()
        {
            var serialiser = new JsonSerialiser();

            var value = serialiser.Deserialise<TvDbSeriesData>(@"{
                airsDayOfWeek: """"
            }")
                .AirsDayOfWeek;

            value.IsNone.Should().BeTrue();
        }

        [Test]
        public void Deserialise_NullAirsDayOfWeek_SetsToNone()
        {
            var serialiser = new JsonSerialiser();

            var value = serialiser.Deserialise<TvDbSeriesData>(@"{
                airsDayOfWeek: null
            }")
                .AirsDayOfWeek;

            value.IsNone.Should().BeTrue();
        }

        [Test]
        public void Deserialise_ValidAirsDayOfWeek_ReturnsSome()
        {
            var serialiser = new JsonSerialiser();

            var value = serialiser.Deserialise<TvDbSeriesData>(@"{
                airsDayOfWeek: ""Saturday""
            }")
                .AirsDayOfWeek;

            value.ValueUnsafe().Should().Be(AirDay.Saturday);
        }

        [Test]
        public void Deserialise_ZeroAirsDayOfWeek_SetsToMonday()
        {
            var serialiser = new JsonSerialiser();

            var value = serialiser.Deserialise<TvDbSeriesData>(@"{
                airsDayOfWeek: 0
            }")
                .AirsDayOfWeek;

            value.ValueUnsafe().Should().Be(AirDay.Monday);
        }
    }
}
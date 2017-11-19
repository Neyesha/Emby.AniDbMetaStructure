﻿using System;
using System.Linq.Expressions;
using FluentAssertions;
using MediaBrowser.Plugins.AniMetadata.PropertyMapping;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests
{
    [TestFixture]
    public class PropertyMappingCollectionTests
    {
        private class AniDbSource
        {
            public string AniDbValue => "AniDb";
        }

        private class TvDbSource
        {
            public string TvDbValue => "TvDb";
        }

        private class Metadata
        {
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
            public string TargetValueA { get; set; } = "TargetValueA";

            public string TargetValueB { get; set; } = "TargetValueB";
        }

        [Test]
        public void Apply_CopiesSourceDataToTarget()
        {
            var aniDbMapping =
                PropertyMapping.Create(t => t.TargetValueA,
                    (AniDbSource s, Metadata t) => t.TargetValueA = s.AniDbValue, "AniDb");
            var tvDbMapping =
                PropertyMapping.Create(t => t.TargetValueB,
                    (TvDbSource s, Metadata t) => t.TargetValueB = s.TvDbValue, "TvDb");

            var aniDbSource = new AniDbSource();
            var metadata = new Metadata();

            var metadataMapping =
                new PropertyMappingCollection(new IPropertyMapping[] { aniDbMapping, tvDbMapping });

            metadataMapping.Apply(aniDbSource, metadata, m => { });

            metadata.TargetValueA.Should().Be("AniDb");
            metadata.TargetValueB.Should().Be("TargetValueB");
        }

        [Test]
        public void Apply_MultipleSources_CopiesSourceDataToTarget()
        {
            var aniDbMapping =
                PropertyMapping.Create(t => t.TargetValueA,
                    (AniDbSource s, Metadata t) => t.TargetValueA = s.AniDbValue, "AniDb");
            var tvDbMapping =
                PropertyMapping.Create(t => t.TargetValueB,
                    (TvDbSource s, Metadata t) => t.TargetValueB = s.TvDbValue, "TvDb");

            var aniDbSource = new AniDbSource();
            var tvDbSource = new TvDbSource();
            var metadata = new Metadata();

            var metadataMapping =
                new PropertyMappingCollection(new IPropertyMapping[] { aniDbMapping, tvDbMapping });

            metadataMapping.Apply(new object[] { aniDbSource, tvDbSource }, metadata, m => { });

            metadata.TargetValueA.Should().Be("AniDb");
            metadata.TargetValueB.Should().Be("TvDb");
        }

        [Test]
        public void Apply_MultipleSourcesSameTarget_TakesFirstSource()
        {
            var aniDbMapping =
                PropertyMapping.Create(t => t.TargetValueA,
                    (AniDbSource s, Metadata t) => t.TargetValueA = s.AniDbValue, "AniDb");
            var tvDbMapping =
                PropertyMapping.Create(t => t.TargetValueA,
                    (TvDbSource s, Metadata t) => t.TargetValueA = s.TvDbValue, "TvDb");

            var aniDbSource = new AniDbSource();
            var tvDbSource = new TvDbSource();
            var metadata = new Metadata();

            var metadataMapping =
                new PropertyMappingCollection(new IPropertyMapping[] { aniDbMapping, tvDbMapping });

            metadataMapping.Apply(new object[] { aniDbSource, tvDbSource }, metadata, m => { });

            metadata.TargetValueA.Should().Be("AniDb");
            metadata.TargetValueB.Should().Be("TargetValueB");
        }

        private static class PropertyMapping
        {
            public static PropertyMapping<TSource, TTarget, TTargetProperty> Create<TSource, TTarget, TTargetProperty>(
                Expression<Func<TTarget, TTargetProperty>> targetPropertySelector,
                Action<TSource, TTarget> map, string sourceName) where TTarget : class where TSource : class
            {
                return new PropertyMapping<TSource, TTarget, TTargetProperty>("", targetPropertySelector, map, sourceName);
            }
        }
    }
}
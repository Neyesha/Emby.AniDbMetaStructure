using Jellyfin.AniDbMetaStructure.Process;
using Jellyfin.AniDbMetaStructure.Process.Sources;
using NSubstitute;
using System;
using System.Linq;

namespace Jellyfin.AniDbMetaStructure.Tests.TestHelpers
{
    internal class TestSources : ISources
    {
        private readonly Lazy<IAniDbSource> aniDbSource = new Lazy<IAniDbSource>(() => AniDbSource);
        private readonly Lazy<IAniListSource> aniListSource = new Lazy<IAniListSource>(() => AniListSource);
        private readonly Lazy<ITvDbSource> tvDbSource = new Lazy<ITvDbSource>(() => TvDbSource);

        public static IAniDbSource AniDbSource
        {
            get
            {
                var aniDb = Substitute.For<IAniDbSource>();
                aniDb.Name.Returns(SourceNames.AniDb);

                return aniDb;
            }
        }

        public static ITvDbSource TvDbSource
        {
            get
            {
                var tvDbSource = Substitute.For<ITvDbSource>();
                tvDbSource.Name.Returns(SourceNames.TvDb);

                return tvDbSource;
            }
        }

        public static IAniListSource AniListSource
        {
            get
            {
                var aniListSource = Substitute.For<IAniListSource>();
                aniListSource.Name.Returns(SourceNames.AniList);

                return aniListSource;
            }
        }

        public IAniDbSource AniDb => this.aniDbSource.Value;

        public ITvDbSource TvDb => this.tvDbSource.Value;

        public IAniListSource AniList => this.aniListSource.Value;

        public ISource Get(string sourceName)
        {
            return new ISource[] { AniDb, TvDb, AniList }.Single(s => s.Name == sourceName);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jellyfin.AniDbMetaStructure.Process.Sources
{
    internal class Sources : ISources
    {
        private readonly Lazy<IEnumerable<ISource>> sources;
        private readonly Lazy<IAniDbSource> aniDbSource;
        private readonly Lazy<ITvDbSource> tvDbSource;
        private readonly Lazy<IAniListSource> aniListSource;

        public Sources(Func<IAniDbSource> aniDbSource, Func<ITvDbSource> tvDbSource, Func<IAniListSource> aniListSource, Func<IEnumerable<ISource>> sources)
        {
            this.aniListSource = new Lazy<IAniListSource>(aniListSource);
            this.sources = new Lazy<IEnumerable<ISource>>(sources);
            this.aniDbSource = new Lazy<IAniDbSource>(aniDbSource);
            this.tvDbSource = new Lazy<ITvDbSource>(tvDbSource);
        }

        public IAniDbSource AniDb => this.aniDbSource.Value;

        public ITvDbSource TvDb => this.tvDbSource.Value;

        public IAniListSource AniList => this.aniListSource.Value;

        public ISource Get(string sourceName)
        {
            return this.sources.Value.Single(s => s.Name == sourceName);
        }
    }
}
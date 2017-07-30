﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AnimeLists;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.Logging;

namespace MediaBrowser.Plugins.Anime.Providers.AniDB.Converter
{
    public class AnidbConverter
    {
        public Mapper Mapper { get; private set; }

        public AnidbConverter(IApplicationPaths paths, ILogManager logManager)
        {
            var data = Path.Combine(paths.CachePath, "anidb");
            Directory.CreateDirectory(data);

            var mappingPath = Path.Combine(data, "anime-list.xml");
            var downloader = new Downloader(mappingPath);
            var animelist = downloader.Download().Result;

            Mapper = new Mapper(logManager, animelist);
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.AniDbMetaStructure.Process;
using Jellyfin.AniDbMetaStructure.Process.Providers;
using Jellyfin.AniDbMetaStructure.Process.Sources;
using FluentAssertions;
using LanguageExt;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using static LanguageExt.Prelude;
using Jellyfin.AniDbMetaStructure.Infrastructure;

namespace Jellyfin.AniDbMetaStructure.Tests.Process.Providers
{
    [TestFixture]
    public class EpisodeProviderTests
    {
        [TestFixture]
        public class GetMetadata : EpisodeProviderTests
        {
            [SetUp]
            public void Setup()
            {
                this.episodeInfo = new EpisodeInfo
                {
                    Name = "EpisodeName",
                    IndexNumber = 3,
                    ParentIndexNumber = 1,
                    ProviderIds = new Dictionary<string, string>
                    {
                        { "Source", "66" }
                    }
                };
                this.mediaItemProcessorResult = Left<ProcessFailedResult, IMetadataFoundResult<Episode>>(
                    new ProcessFailedResult("FailedSource",
                        "MediaItemName", MediaItemTypes.Episode, "Failure reason"));

                this.mediaItemProcessor = Substitute.For<IMediaItemProcessor>();
                this.mediaItemProcessor.GetResultAsync(this.episodeInfo, MediaItemTypes.Episode,
                        Arg.Any<IEnumerable<JellyfinItemId>>())
                    .Returns(x => this.mediaItemProcessorResult);

                this.logger = Substitute.For<ILogger>();

                this.episodeProvider = new EpisodeProvider(this.logger, this.mediaItemProcessor);
            }

            private IMediaItemProcessor mediaItemProcessor;
            private ILogger logger;
            private EpisodeProvider episodeProvider;
            private EpisodeInfo episodeInfo;
            private Either<ProcessFailedResult, IMetadataFoundResult<Episode>> mediaItemProcessorResult;

            [Test]
            public async Task ExceptionThrown_LogsException()
            {
                var exception = new Exception("Failed");
                this.mediaItemProcessor.GetResultAsync(this.episodeInfo, MediaItemTypes.Episode, Arg.Any<IEnumerable<JellyfinItemId>>())
                    .Throws(exception);

                await this.episodeProvider.GetMetadata(this.episodeInfo, CancellationToken.None);

                this.logger.Received(1).LogError($"Failed to get data for episode '{this.episodeInfo.Name}'", exception);
            }

            [Test]
            public async Task ExceptionThrown_ReturnsNoMetadata()
            {
                this.mediaItemProcessor.GetResultAsync(this.episodeInfo, MediaItemTypes.Episode, Arg.Any<IEnumerable<JellyfinItemId>>())
                    .Throws(new Exception("Failed"));

                var result = await this.episodeProvider.GetMetadata(this.episodeInfo, CancellationToken.None);

                result.HasMetadata.Should().BeFalse();
            }

            [Test]
            public async Task FailedResult_AllowsOtherProvidersToRun()
            {
                await this.episodeProvider.GetMetadata(this.episodeInfo, CancellationToken.None);

                this.episodeInfo.Name.Should().Be("EpisodeName");
                this.episodeInfo.IndexNumber.Should().Be(3);
                this.episodeInfo.ParentIndexNumber.Should().Be(1);
                this.episodeInfo.ProviderIds.Should().ContainKey("Source");
            }

            [Test]
            public async Task FailedResult_LogsReason()
            {
                await this.episodeProvider.GetMetadata(this.episodeInfo, CancellationToken.None);

                this.logger.Received(1).LogError("Failed to get data for episode 'EpisodeName': Failure reason");
            }

            [Test]
            public async Task FailedResult_ReturnsNoMetadata()
            {
                var result = await this.episodeProvider.GetMetadata(this.episodeInfo, CancellationToken.None);

                result.HasMetadata.Should().BeFalse();
            }

            [Test]
            public async Task ProvidesParentIds()
            {
                this.episodeInfo.SeriesProviderIds = new Dictionary<string, string>
                {
                    { SourceNames.AniDb, "929" }
                };

                await this.episodeProvider.GetMetadata(this.episodeInfo, CancellationToken.None);

                await this.mediaItemProcessor.Received(1)
                    .GetResultAsync(this.episodeInfo, MediaItemTypes.Episode, Arg.Is<IEnumerable<JellyfinItemId>>(ids => ids.Count() == 1 &&
                                                                                                                     ids.Single().Id == 929 &&
                                                                                                                     ids.Single().ItemType == MediaItemTypes.Series &&
                                                                                                                     ids.Single().SourceName == SourceNames.AniDb));
            }

            [Test]
            public async Task MetadataFoundResult_LogsFoundName()
            {
                var metadataResult = new MetadataResult<Episode>
                {
                    Item = new Episode
                    {
                        Name = "MetadataName"
                    }
                };

                this.mediaItemProcessorResult = Right<ProcessFailedResult, IMetadataFoundResult<Episode>>(
                    new MetadataFoundResult<Episode>(Substitute.For<IMediaItem>(), metadataResult));

                await this.episodeProvider.GetMetadata(this.episodeInfo, CancellationToken.None);

                this.logger.Received(1).LogInformation("Found data for episode 'EpisodeName': 'MetadataName'");
            }

            [Test]
            public async Task MetadataFoundResult_PreventsOtherProvidersRunning()
            {
                var metadataResult = new MetadataResult<Episode>
                {
                    Item = new Episode
                    {
                        Name = "MetadataName"
                    }
                };

                this.mediaItemProcessorResult = Right<ProcessFailedResult, IMetadataFoundResult<Episode>>(
                    new MetadataFoundResult<Episode>(Substitute.For<IMediaItem>(), metadataResult));

                await this.episodeProvider.GetMetadata(this.episodeInfo, CancellationToken.None);

                this.episodeInfo.Name.Should().BeEmpty();
                this.episodeInfo.IndexNumber.Should().BeNull();
                this.episodeInfo.ParentIndexNumber.Should().BeNull();
                this.episodeInfo.ProviderIds.Should().BeEmpty();
            }

            [Test]
            public async Task MetadataFoundResult_ReturnsResult()
            {
                var metadataResult = new MetadataResult<Episode>
                {
                    Item = new Episode
                    {
                        Name = "MetadataName"
                    }
                };

                this.mediaItemProcessorResult = Right<ProcessFailedResult, IMetadataFoundResult<Episode>>(
                    new MetadataFoundResult<Episode>(Substitute.For<IMediaItem>(), metadataResult));

                var result = await this.episodeProvider.GetMetadata(this.episodeInfo, CancellationToken.None);

                result.Should().Be(metadataResult);
            }
        }
    }
}
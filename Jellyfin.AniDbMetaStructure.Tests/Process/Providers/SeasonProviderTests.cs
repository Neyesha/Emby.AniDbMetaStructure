using System;
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
    public class SeasonProviderTests
    {
        [TestFixture]
        public class GetMetadata : SeasonProviderTests
        {
            [SetUp]
            public void Setup()
            {
                this.seasonInfo = new SeasonInfo
                {
                    Name = "SeasonName",
                    IndexNumber = 3,
                    ParentIndexNumber = 1,
                    ProviderIds = new Dictionary<string, string>
                    {
                        { "Source", "66" }
                    }
                };
                this.mediaItemProcessorResult = Left<ProcessFailedResult, IMetadataFoundResult<Season>>(
                    new ProcessFailedResult("FailedSource",
                        "MediaItemName", MediaItemTypes.Season, "Failure reason"));

                this.mediaItemProcessor = Substitute.For<IMediaItemProcessor>();
                this.mediaItemProcessor.GetResultAsync(this.seasonInfo, MediaItemTypes.Season, Arg.Any<IEnumerable<EmbyItemId>>())
                    .Returns(x => this.mediaItemProcessorResult);

                this.logger = Substitute.For<ILogger>();

                this.seasonProvider = new SeasonProvider(this.logger, this.mediaItemProcessor);
            }

            private IMediaItemProcessor mediaItemProcessor;
            private ILogger logger;
            private SeasonProvider seasonProvider;
            private SeasonInfo seasonInfo;
            private Either<ProcessFailedResult, IMetadataFoundResult<Season>> mediaItemProcessorResult;

            [Test]
            public async Task ExceptionThrown_LogsException()
            {
                var exception = new Exception("Failed");
                this.mediaItemProcessor.GetResultAsync(this.seasonInfo, MediaItemTypes.Season, Arg.Any<IEnumerable<EmbyItemId>>())
                    .Throws(exception);

                await this.seasonProvider.GetMetadata(this.seasonInfo, CancellationToken.None);

                this.logger.Received(1).LogError("Failed to get data for season 'SeasonName'", exception);
            }

            [Test]
            public async Task ExceptionThrown_ReturnsNoMetadata()
            {
                this.mediaItemProcessor.GetResultAsync(this.seasonInfo, MediaItemTypes.Season, Arg.Any<IEnumerable<EmbyItemId>>())
                    .Throws(new Exception("Failed"));

                var result = await this.seasonProvider.GetMetadata(this.seasonInfo, CancellationToken.None);

                result.HasMetadata.Should().BeFalse();
            }

            [Test]
            public async Task FailedResult_AllowsOtherProvidersToRun()
            {
                await this.seasonProvider.GetMetadata(this.seasonInfo, CancellationToken.None);

                this.seasonInfo.Name.Should().Be("SeasonName");
                this.seasonInfo.IndexNumber.Should().Be(3);
                this.seasonInfo.ParentIndexNumber.Should().Be(1);
                this.seasonInfo.ProviderIds.Should().ContainKey("Source");
            }

            [Test]
            public async Task FailedResult_LogsReason()
            {
                await this.seasonProvider.GetMetadata(this.seasonInfo, CancellationToken.None);

                this.logger.Received(1).LogError("Failed to get data for season 'SeasonName': Failure reason");
            }

            [Test]
            public async Task FailedResult_ReturnsNoMetadata()
            {
                var result = await this.seasonProvider.GetMetadata(this.seasonInfo, CancellationToken.None);

                result.HasMetadata.Should().BeFalse();
            }

            [Test]
            public async Task ProvidesParentIds()
            {
                this.seasonInfo.SeriesProviderIds = new Dictionary<string, string>
                {
                    { SourceNames.AniDb, "929" }
                };

                await this.seasonProvider.GetMetadata(this.seasonInfo, CancellationToken.None);

                await this.mediaItemProcessor.Received(1)
                    .GetResultAsync(this.seasonInfo, MediaItemTypes.Season, Arg.Is<IEnumerable<EmbyItemId>>(ids => ids.Count() == 1 &&
                                                                                                                   ids.Single().Id == 929 &&
                                                                                                                   ids.Single().ItemType == MediaItemTypes.Series &&
                                                                                                                   ids.Single().SourceName == SourceNames.AniDb));
            }

            [Test]
            public async Task MetadataFoundResult_LogsFoundName()
            {
                var metadataResult = new MetadataResult<Season>
                {
                    Item = new Season
                    {
                        Name = "MetadataName"
                    }
                };

                this.mediaItemProcessorResult = Right<ProcessFailedResult, IMetadataFoundResult<Season>>(
                    new MetadataFoundResult<Season>(Substitute.For<IMediaItem>(), metadataResult));

                await this.seasonProvider.GetMetadata(this.seasonInfo, CancellationToken.None);

                this.logger.Received(1).LogInformation("Found data for season 'SeasonName': 'MetadataName'");
            }

            [Test]
            public async Task MetadataFoundResult_PreventsOtherProvidersRunning()
            {
                var metadataResult = new MetadataResult<Season>
                {
                    Item = new Season
                    {
                        Name = "MetadataName"
                    }
                };

                this.mediaItemProcessorResult = Right<ProcessFailedResult, IMetadataFoundResult<Season>>(
                    new MetadataFoundResult<Season>(Substitute.For<IMediaItem>(), metadataResult));

                await this.seasonProvider.GetMetadata(this.seasonInfo, CancellationToken.None);

                this.seasonInfo.Name.Should().BeEmpty();
                this.seasonInfo.IndexNumber.Should().BeNull();
                this.seasonInfo.ParentIndexNumber.Should().BeNull();
                this.seasonInfo.ProviderIds.Should().BeEmpty();
            }

            [Test]
            public async Task MetadataFoundResult_ReturnsResult()
            {
                var metadataResult = new MetadataResult<Season>
                {
                    Item = new Season
                    {
                        Name = "MetadataName"
                    }
                };

                this.mediaItemProcessorResult = Right<ProcessFailedResult, IMetadataFoundResult<Season>>(
                    new MetadataFoundResult<Season>(Substitute.For<IMediaItem>(), metadataResult));

                var result = await this.seasonProvider.GetMetadata(this.seasonInfo, CancellationToken.None);

                result.Should().Be(metadataResult);
            }
        }
    }
}
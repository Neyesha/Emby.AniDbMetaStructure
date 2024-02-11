using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.AniDbMetaStructure.Configuration;
using Jellyfin.AniDbMetaStructure.Process;
using Jellyfin.AniDbMetaStructure.Process.Providers;
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
    public class SeriesProviderTests
    {
        [TestFixture]
        public class GetMetadata : SeriesProviderTests
        {
            [SetUp]
            public void Setup()
            {
                this.seriesInfo = new SeriesInfo
                {
                    Name = "SeriesName",
                    IndexNumber = 3,
                    ParentIndexNumber = 1,
                    ProviderIds = new Dictionary<string, string>
                    {
                        { "Source", "66" }
                    }
                };
                this.mediaItemProcessorResult = Left<ProcessFailedResult, IMetadataFoundResult<Series>>(
                    new ProcessFailedResult("FailedSource",
                        "MediaItemName", MediaItemTypes.Series, "Failure reason"));

                this.mediaItemProcessor = Substitute.For<IMediaItemProcessor>();
                this.mediaItemProcessor.GetResultAsync(this.seriesInfo, MediaItemTypes.Series, Enumerable.Empty<JellyfinItemId>())
                    .Returns(x => this.mediaItemProcessorResult);

                this.logger = Substitute.For<ILogger>();

                this.pluginConfiguration = Substitute.For<IPluginConfiguration>();
                this.pluginConfiguration.ExcludedSeriesNames.Returns(Enumerable.Empty<string>());

                this.seriesProvider = new SeriesProvider(this.logger, this.mediaItemProcessor, this.pluginConfiguration);
            }

            private IMediaItemProcessor mediaItemProcessor;
            private ILogger logger;
            private IPluginConfiguration pluginConfiguration;
            private SeriesProvider seriesProvider;
            private SeriesInfo seriesInfo;
            private Either<ProcessFailedResult, IMetadataFoundResult<Series>> mediaItemProcessorResult;

            [Test]
            [TestCase("Exclude", "Exclude", true)]
            [TestCase("Exclude", "excLude", true)]
            [TestCase("Exclude", "Exclude1", false)]
            [TestCase("Exclude1", "Exclude", false)]
            public async Task JellyfinTitleInExcludeList_LogsSkip(string name, string excludedName,
                bool isExcluded)
            {
                this.seriesInfo.Name = name;

                this.pluginConfiguration.ExcludedSeriesNames.Returns(new[] { excludedName });

                await this.seriesProvider.GetMetadata(this.seriesInfo, CancellationToken.None);

                if (isExcluded)
                {
                    this.logger.Received(1).LogInformation($"Skipping series '{name}' as it is excluded");
                }
                else
                {
                    this.logger.DidNotReceive().LogInformation($"Skipping series '{name}' as it is excluded");
                }
            }

            [Test]
            [TestCase("Exclude", "Exclude", true)]
            [TestCase("Exclude", "excLude", true)]
            [TestCase("Exclude", "Exclude1", false)]
            [TestCase("Exclude1", "Exclude", false)]
            public async Task JellyfinTitleInExcludeList_ReturnsNoMetadata(string name, string excludedName,
                bool isExcluded)
            {
                var metadataResult = new MetadataResult<Series>
                {
                    Item = new Series
                    {
                        Name = "MetadataName"
                    },
                    HasMetadata = true
                };

                this.mediaItemProcessorResult = Right<ProcessFailedResult, IMetadataFoundResult<Series>>(
                    new MetadataFoundResult<Series>(Substitute.For<IMediaItem>(), metadataResult));

                this.seriesInfo.Name = name;

                this.pluginConfiguration.ExcludedSeriesNames.Returns(new[] { excludedName });

                var result = await this.seriesProvider.GetMetadata(this.seriesInfo, CancellationToken.None);

                if (isExcluded)
                {
                    result.HasMetadata.Should().BeFalse();
                }
                else
                {
                    result.HasMetadata.Should().BeTrue();
                }
            }

            [Test]
            public async Task ExceptionThrown_LogsException()
            {
                var exception = new Exception("Failed");
                this.mediaItemProcessor.GetResultAsync(this.seriesInfo, MediaItemTypes.Series, Enumerable.Empty<JellyfinItemId>())
                    .Throws(exception);

                await this.seriesProvider.GetMetadata(this.seriesInfo, CancellationToken.None);

                this.logger.Received(1).LogError("Failed to get data for series 'SeriesName'", exception);
            }

            [Test]
            public async Task ExceptionThrown_ReturnsNoMetadata()
            {
                this.mediaItemProcessor.GetResultAsync(this.seriesInfo, MediaItemTypes.Series, Enumerable.Empty<JellyfinItemId>())
                    .Throws(new Exception("Failed"));

                var result = await this.seriesProvider.GetMetadata(this.seriesInfo, CancellationToken.None);

                result.HasMetadata.Should().BeFalse();
            }

            [Test]
            public async Task FailedResult_AllowsOtherProvidersToRun()
            {
                await this.seriesProvider.GetMetadata(this.seriesInfo, CancellationToken.None);

                this.seriesInfo.Name.Should().Be("SeriesName");
                this.seriesInfo.IndexNumber.Should().Be(3);
                this.seriesInfo.ParentIndexNumber.Should().Be(1);
                this.seriesInfo.ProviderIds.Should().ContainKey("Source");
            }

            [Test]
            public async Task FailedResult_LogsReason()
            {
                await this.seriesProvider.GetMetadata(this.seriesInfo, CancellationToken.None);

                this.logger.Received(1).LogError("Failed to get data for series 'SeriesName': Failure reason");
            }

            [Test]
            public async Task FailedResult_ReturnsNoMetadata()
            {
                var result = await this.seriesProvider.GetMetadata(this.seriesInfo, CancellationToken.None);

                result.HasMetadata.Should().BeFalse();
            }

            [Test]
            public async Task GetsResult()
            {
                await this.seriesProvider.GetMetadata(this.seriesInfo, CancellationToken.None);

                await this.mediaItemProcessor.Received(1)
                    .GetResultAsync(this.seriesInfo, MediaItemTypes.Series, Enumerable.Empty<JellyfinItemId>());
            }

            [Test]
            public async Task MetadataFoundResult_LogsFoundName()
            {
                var metadataResult = new MetadataResult<Series>
                {
                    Item = new Series
                    {
                        Name = "MetadataName"
                    }
                };

                this.mediaItemProcessorResult = Right<ProcessFailedResult, IMetadataFoundResult<Series>>(
                    new MetadataFoundResult<Series>(Substitute.For<IMediaItem>(), metadataResult));

                await this.seriesProvider.GetMetadata(this.seriesInfo, CancellationToken.None);

                this.logger.Received(1).LogInformation("Found data for series 'SeriesName': 'MetadataName'");
            }

            [Test]
            public async Task MetadataFoundResult_PreventsOtherProvidersRunning()
            {
                var metadataResult = new MetadataResult<Series>
                {
                    Item = new Series
                    {
                        Name = "MetadataName"
                    }
                };

                this.mediaItemProcessorResult = Right<ProcessFailedResult, IMetadataFoundResult<Series>>(
                    new MetadataFoundResult<Series>(Substitute.For<IMediaItem>(), metadataResult));

                await this.seriesProvider.GetMetadata(this.seriesInfo, CancellationToken.None);

                this.seriesInfo.Name.Should().BeEmpty();
                this.seriesInfo.IndexNumber.Should().BeNull();
                this.seriesInfo.ParentIndexNumber.Should().BeNull();
                this.seriesInfo.ProviderIds.Should().BeEmpty();
            }

            [Test]
            public async Task MetadataFoundResult_ReturnsResult()
            {
                var metadataResult = new MetadataResult<Series>
                {
                    Item = new Series
                    {
                        Name = "MetadataName"
                    }
                };

                this.mediaItemProcessorResult = Right<ProcessFailedResult, IMetadataFoundResult<Series>>(
                    new MetadataFoundResult<Series>(Substitute.For<IMediaItem>(), metadataResult));

                var result = await this.seriesProvider.GetMetadata(this.seriesInfo, CancellationToken.None);

                result.Should().Be(metadataResult);
            }
        }
    }
}
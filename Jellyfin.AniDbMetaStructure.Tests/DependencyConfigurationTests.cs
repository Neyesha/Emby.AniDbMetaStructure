using System;
using Jellyfin.AniDbMetaStructure.EntryPoints;
using Jellyfin.AniDbMetaStructure.Infrastructure;
using Jellyfin.AniDbMetaStructure.Tests.TestHelpers;
using FluentAssertions;
using NUnit.Framework;
using SimpleInjector;

namespace Jellyfin.AniDbMetaStructure.Tests
{
    [TestFixture]
    public class DependencyConfigurationTests
    {
        [Test]
        [TestCase(typeof(EpisodeProviderEntryPoint))]
        [TestCase(typeof(ImageProvider))]
        [TestCase(typeof(PersonImageProvider))]
        [TestCase(typeof(PersonProvider))]
        [TestCase(typeof(SeasonProviderEntryPoint))]
        [TestCase(typeof(SeriesProviderEntryPoint))]
        public void CanResolveEntryPoints(Type entryPointType)
        {
            Action action = () => DependencyConfiguration.Resolve(entryPointType, new TestApplicationHost());

            TestPlugin.EnsurePluginStaticSingletonAvailable();

            action.Should().NotThrow();
        }

        [Test]
        public void ConfigurationIsValid()
        {
            DependencyConfiguration.Resolve<IRateLimiters>(new TestApplicationHost());

            TestPlugin.EnsurePluginStaticSingletonAvailable();

            DependencyConfiguration.Container.Verify(VerificationOption.VerifyAndDiagnose);
        }
    }
}
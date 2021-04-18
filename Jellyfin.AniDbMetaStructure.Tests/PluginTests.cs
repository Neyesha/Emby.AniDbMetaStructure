using System.Linq;
using Jellyfin.AniDbMetaStructure.Tests.TestHelpers;
using FluentAssertions;
using NUnit.Framework;

namespace Emby.AniDbMetaStructure.Tests
{
    [TestFixture]
    public class PluginTests
    {
        [Test]
        public void ConfigurationPageExists()
        {
            var embeddedResourcePath =
                new TestPlugin().GetPages()
                    .Single()
                    .EmbeddedResourcePath;

            var assembly = typeof(Plugin).Assembly;

            assembly.GetManifestResourceNames().Should().Contain(embeddedResourcePath);
        }
    }
}
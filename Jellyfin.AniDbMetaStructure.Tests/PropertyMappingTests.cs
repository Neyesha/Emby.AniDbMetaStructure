using Jellyfin.AniDbMetaStructure.PropertyMapping;
using FluentAssertions;
using MediaBrowser.Controller.Entities;
using NUnit.Framework;

namespace Jellyfin.AniDbMetaStructure.Tests
{
    [TestFixture]
    public class PropertyMappingTests
    {
        private class Source
        {
            public string SourceValue => "Source";
        }

        private class Target : BaseItem
        {
            // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
            public string TargetValue { get; set; } = "Target";
        }

        [Test]
        public void Apply_CopiesValueFromSourceToTarget()
        {
            var propertyMapping =
                new PropertyMapping<Source, Target, string>(string.Empty, t => t.TargetValue,
                    (s, t) => t.TargetValue = s.SourceValue,
                    string.Empty);

            var source = new Source();
            var target = new Target();

            propertyMapping.Apply(source, target);

            target.TargetValue.Should().Be("Source");
        }

        [Test]
        public void CanApply_MatchingSourceAndTargetTypes_ReturnsTrue()
        {
            var propertyMapping =
                new PropertyMapping<Source, Target, string>(string.Empty, t => t.TargetValue,
                    (s, t) => t.TargetValue = s.SourceValue,
                    string.Empty);

            propertyMapping.CanApply(new Source(), new Target()).Should().BeTrue();
        }

        [Test]
        public void CanApply_MismatchingSourceType_ReturnsFalse()
        {
            var propertyMapping =
                new PropertyMapping<Source, Target, string>(string.Empty, t => t.TargetValue,
                    (s, t) => t.TargetValue = s.SourceValue,
                    string.Empty);

            propertyMapping.CanApply(new object(), new Target());
        }

        [Test]
        public void CanApply_MismatchingTargetType_ReturnsFalse()
        {
            var propertyMapping =
                new PropertyMapping<Source, Target, string>(string.Empty, t => t.TargetValue,
                    (s, t) => t.TargetValue = s.SourceValue,
                    string.Empty);

            propertyMapping.CanApply(new Source(), new object()).Should().BeFalse();
        }

        [Test]
        public void TargetPropertyName_ReturnsSelectedTargetProperty()
        {
            var propertyMapping =
                new PropertyMapping<Source, Target, string>(string.Empty, t => t.TargetValue,
                    (s, t) => t.TargetValue = s.SourceValue,
                    string.Empty);

            propertyMapping.TargetPropertyName.Should().Be("TargetValue");
        }
    }
}
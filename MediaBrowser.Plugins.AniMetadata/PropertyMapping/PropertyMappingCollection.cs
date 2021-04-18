using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.PropertyMapping
{
    internal class PropertyMappingCollection : IPropertyMappingCollection
    {
        private readonly IEnumerable<IPropertyMapping> propertyMappings;

        public PropertyMappingCollection(IEnumerable<IPropertyMapping> propertyMappings)
        {
            this.propertyMappings = propertyMappings;
        }

        public TMetadata Apply<TMetadata>(object source, TMetadata target, Action<string> log)
        {
            this.propertyMappings.GroupBy(m => m.TargetPropertyName)
                .Select(g => g.FirstOrDefault(m => m.CanApply(source, target)))
                .Where(m => m != null)
                .Iter(m => this.ApplyMapping(m, source, target, log));

            return target;
        }

        public TMetadata Apply<TMetadata>(IEnumerable<object> sources, TMetadata target, Action<string> log)
        {
            this.propertyMappings.GroupBy(m => m.TargetPropertyName)
                .Select(g => g.Select(m => this.GetMappingApplication(m, sources, target, log)).Somes().FirstOrDefault())
                .Where(a => a != null)
                .Iter(a => a(target));

            return target;
        }

        public IEnumerator<IPropertyMapping> GetEnumerator()
        {
            return this.propertyMappings.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private Option<Action<TMetadata>> GetMappingApplication<TMetadata>(IPropertyMapping propertyMapping,
            IEnumerable<object> sources, TMetadata target, Action<string> log)
        {
            Option<object> source = sources.FirstOrDefault(s => propertyMapping.CanApply(s, target));

            return source.Map(s => (Action<TMetadata>)(t => this.ApplyMapping(propertyMapping, s, t, log)));
        }

        private void ApplyMapping<TMetadata>(IPropertyMapping mapping, object source, TMetadata target,
            Action<string> log)
        {
            log.Invoke($"Applying mapping from source {mapping.SourceName} -> {mapping.TargetPropertyName}");

            mapping.Apply(source, target);
        }
    }
}
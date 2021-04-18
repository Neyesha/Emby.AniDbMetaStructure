using System;
using System.Linq.Expressions;
using System.Reflection;
using LanguageExt;

namespace Jellyfin.AniDbMetaStructure.PropertyMapping
{
    internal class PropertyMapping<TSource, TTarget, TTargetProperty> : IPropertyMapping
        where TSource : class where TTarget : class
    {
        private readonly Action<TSource, TTarget> apply;
        private readonly Func<TSource, TTarget, bool> canApply;

        public PropertyMapping(string friendlyName, Expression<Func<TTarget, TTargetProperty>> targetPropertySelector,
            Action<TSource, TTarget> apply, string sourceName) : this(friendlyName, targetPropertySelector, apply,
            sourceName, (s, t) => true)
        {
        }

        public PropertyMapping(string friendlyName, Expression<Func<TTarget, TTargetProperty>> targetPropertySelector,
            Action<TSource, TTarget> apply, string sourceName, Func<TSource, TTarget, bool> canApply)
        {
            var targetPropertyInfo = this.GetPropertyInfo(targetPropertySelector);

            this.TargetPropertyName = targetPropertyInfo.Name;
            this.SourceName = sourceName;

            this.canApply = canApply;
            this.FriendlyName = friendlyName;
            this.apply = apply;
        }

        public string FriendlyName { get; }

        public string TargetPropertyName { get; }

        public string SourceName { get; }

        public bool CanApply(object source, object target)
        {
            return source is TSource s && target is TTarget t && this.canApply(s, t);
        }

        public Option<T> Apply<T>(object source, T target)
        {
            Option<TSource> typedSource = source as TSource;
            Option<TTarget> typedTarget = target as TTarget;

            return typedSource.Bind(s => typedTarget.Map(t =>
            {
                this.apply(s, t);
                return target;
            }));
        }

        private PropertyInfo GetPropertyInfo<T>(Expression<Func<T, TTargetProperty>> propertySelector)
        {
            Option<MemberExpression> memberExpression = propertySelector.Body as MemberExpression;

            var propertyInfo = memberExpression.Map(m => m.Member as PropertyInfo);

            return propertyInfo.Match(m => m,
                () => throw new Exception($"{nameof(propertySelector)} is not a member expression"));
        }

        public override string ToString()
        {
            return $"PropertyMapping: {this.SourceName} -> {this.TargetPropertyName}";
        }
    }
}
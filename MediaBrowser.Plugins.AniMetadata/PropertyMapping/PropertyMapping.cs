﻿using System;
using System.Linq.Expressions;
using System.Reflection;
using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.PropertyMapping
{
    internal class PropertyMapping<TSource, TTarget, TTargetProperty> : IPropertyMapping
        where TSource : class where TTarget : class
    {
        private readonly Action<TSource, TTarget> _apply;
        private readonly Func<TSource, TTarget, bool> _canApply;

        public PropertyMapping(string friendlyName, Expression<Func<TTarget, TTargetProperty>> targetPropertySelector,
            Action<TSource, TTarget> apply, string sourceName) : this(friendlyName, targetPropertySelector, apply,
            sourceName, (s, t) => true)
        {
        }

        public PropertyMapping(string friendlyName, Expression<Func<TTarget, TTargetProperty>> targetPropertySelector,
            Action<TSource, TTarget> apply, string sourceName, Func<TSource, TTarget, bool> canApply)
        {
            var targetPropertyInfo = GetPropertyInfo(targetPropertySelector);

            TargetPropertyName = targetPropertyInfo.Name;
            SourceName = sourceName;

            _canApply = canApply;
            FriendlyName = friendlyName;
            _apply = apply;
        }

        public string FriendlyName { get; }

        public string TargetPropertyName { get; }

        public string SourceName { get; }

        public bool CanApply(object source, object target)
        {
            return source is TSource s && target is TTarget t && _canApply(s, t);
        }

        public Option<T> Apply<T>(object source, T target)
        {
            Option<TSource> typedSource = source as TSource;
            Option<TTarget> typedTarget = target as TTarget;

            return typedSource.Bind(s => typedTarget.Map(t =>
            {
                _apply(s, t);
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
    }
}
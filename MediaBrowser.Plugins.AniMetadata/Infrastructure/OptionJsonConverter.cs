using System;
using System.Linq;
using System.Reflection;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using Newtonsoft.Json;

namespace Jellyfin.AniDbMetaStructure.Infrastructure
{
    internal class OptionJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var wrappedType = value.GetType().GetGenericArguments()[0];

            var hasValue = (bool)value.GetType().GetProperty("IsSome").GetValue(value);

            if (hasValue)
            {
                var methods = typeof(UnsafeValueAccessExtensions).GetMethods()
                    .Where(m => m.Name == nameof(UnsafeValueAccessExtensions.Value) &&
                        m.GetGenericArguments().Length == 1)
                    .ToList();

                var method = methods.First().MakeGenericMethod(wrappedType);

                writer.WriteValue(method.Invoke(null, new[] { value }));
            }
            else
            {
                Type nullableType;

                if (wrappedType.IsValueType)
                {
                    nullableType = typeof(Nullable<>).MakeGenericType(wrappedType);
                }
                else
                {
                    nullableType = wrappedType;
                }

                writer.WriteValue(Activator.CreateInstance(nullableType));
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var none = objectType.GetField(nameof(Option<object>.None)).GetValue(null);

            if (reader.Value == null)
            {
                return none;
            }

            var wrappedType = objectType.GetGenericArguments()[0];
            var castValue = this.GetCastValue(reader.Value, wrappedType, none);

            if (castValue == none)
            {
                return none;
            }

            return objectType.GetMethod("Some", BindingFlags.Public | BindingFlags.Static)
                .Invoke(null, new[] { castValue });
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsConstructedGenericType && objectType.GetGenericTypeDefinition() == typeof(Option<>);
        }

        private object GetCastValue(object value, Type type, object none)
        {
            if (value is string s && s == string.Empty)
            {
                return none;
            }

            if (typeof(Enum).IsAssignableFrom(type))
            {
                return Enum.Parse(type, value.ToString());
            }

            if (value is long && !type.IsAssignableFrom(typeof(long)))
            {
                return none;
            }

            return Convert.ChangeType(value, type);
        }
    }
}
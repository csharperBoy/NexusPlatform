using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

// Core/Domain/Common/Optional.cs

namespace Core.Domain.Common
{
    // اینترفیس کمکی برای تشخیص غیرژنریک
    public interface IOptional
    {
        bool IsSet { get; }
        object? Value { get; }
    }

    public readonly struct Optional<T> : IOptional
    {
        public bool IsSet { get; }
        public T? Value { get; }
        object? IOptional.Value => Value; // پیاده‌سازی اینترفیس

        public Optional(T? value)
        {
            Value = value;
            IsSet = true;
        }

        public static Optional<T> Undefined => default;

        public static implicit operator Optional<T>(T? value) => new(value);

        public T? GetValueOrDefault(T? defaultValue = default) => IsSet ? Value : defaultValue;
    }

    public class OptionalJsonConverter<T> : JsonConverter<Optional<T>>
    {
        public override bool HandleNull => true;

        public override Optional<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return new Optional<T>(default);
            }

            var value = JsonSerializer.Deserialize<T>(ref reader, options);
            return new Optional<T>(value);
        }

        public override void Write(Utf8JsonWriter writer, Optional<T> value, JsonSerializerOptions options)
        {
            if (value.IsSet)
            {
                JsonSerializer.Serialize(writer, value.Value, options);
            }
            else
            {
                // در صورتی که به صورت مستقیم سریالایز شود
                writer.WriteNullValue();
            }
        }
    }

    public class OptionalJsonConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeToConvert.IsGenericType)
                return false;

            return typeToConvert.GetGenericTypeDefinition() == typeof(Optional<>);
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            Type valueType = typeToConvert.GetGenericArguments()[0];
            Type converterType = typeof(OptionalJsonConverter<>).MakeGenericType(valueType);

            return (JsonConverter)Activator.CreateInstance(converterType)!;
        }
    }
}
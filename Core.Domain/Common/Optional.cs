using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Core.Domain.Common
{
    public readonly struct Optional<T>
    {
        public bool IsSet { get; }
        public T? Value { get; }

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
        // این خاصیت حیاتی است تا System.Text.Json مقدار null صریح در JSON را به متد Read بفرستد
        public override bool HandleNull => true;

        public override Optional<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                // کلید در JSON وجود داشته اما مقدار آن null بوده است (IsSet = true, Value = null)
                return new Optional<T>(default);
            }

            // کلید وجود داشته و مقدار دارد
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
                // اگر تنظیم شده که فیلدهای غیر ست شده هم سریالایز شوند، می‌توانید null بنویسید
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

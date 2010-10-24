using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Sharpnote.Json
{
    /// <summary>
    /// Custom class for converting date formats with Simplenote
    /// </summary>
    public class DateTimeEpochConverter: DateTimeConverterBase
    {
        private static readonly DateTimeOffset _epoch = new DateTimeOffset(new DateTime(1970, 1, 1));

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var isNullable = (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Nullable<>));
            var returnType = isNullable ? Nullable.GetUnderlyingType(objectType) : objectType;
            CheckType(returnType);

            if (reader.TokenType == JsonToken.Null)
            {
                if (isNullable) return null;
                //otherwise error
                throw new FormatException("Cannot parse null value for non nullable DateTimeOffset");
            }
            //otherwise parse the value
            var value = reader.Value.ToString();
            if (string.IsNullOrEmpty(value)) return null;
            double seconds;
            if (double.TryParse(value, out seconds))
                return _epoch.AddSeconds(seconds);
            //otherwise error
            throw new FormatException("Cannot parse value: " + value +" as seconds for DateTimeOffset");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            CheckType(value.GetType());

            var time = (DateTimeOffset)value;
            var span = new TimeSpan(time.Ticks - _epoch.Ticks);

            writer.WriteValue(span.TotalSeconds.ToString());
        }

        private void CheckType(Type type)
        {
            if (type != typeof(DateTimeOffset))
                throw new NotImplementedException("Converter only implemented for DateTimeOffset or Nullable<DateTimeOffset>");
        }
    }
}

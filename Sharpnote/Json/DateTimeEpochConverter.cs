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
                return SecondsToDate(seconds);

            //otherwise error
            throw new FormatException("Cannot parse value: " + value +" as seconds for DateTimeOffset");
        }
 
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            CheckType(value.GetType());

            var time = (DateTimeOffset)value;
            writer.WriteValue(DateToSeconds(time));
        }

        private void CheckType(Type type)
        {
            if (type != typeof(DateTimeOffset))
                throw new NotImplementedException("Converter only implemented for DateTimeOffset or Nullable<DateTimeOffset>");
        }

        /// <summary>
        /// Returns the number of seconds since 01/01/1970
        /// </summary>
        /// <param name="dto">The date to be parsed</param>
        /// <returns></returns>
        public static string DateToSeconds(DateTimeOffset dto)
        {
            var span = new TimeSpan(dto.Ticks - _epoch.Ticks);
            return span.TotalSeconds.ToString();
        }

        /// <summary>
        /// Gets a date from the given number of seconds
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static DateTimeOffset SecondsToDate(double seconds)
        {
            return _epoch.AddSeconds(seconds);
        }
    }
}

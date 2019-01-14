using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace DocDbRepo.Sample
{
    internal class EpochConverter
        : DateTimeConverterBase
    {
        private static readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value != null)
            {
                writer.WriteRawValue($"{(long)((DateTime)value - _epoch).TotalSeconds}");
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.Value == null
                ? null
                : (object)_epoch.AddSeconds((long)reader.Value);
        }
    }
}
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace CosmosDbRepository.Sample
{
    internal enum EpochUnits
    {
        Seconds,
        Milliseconds
    }

    internal class EpochConverter
        : DateTimeConverterBase
    {
        private static readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private readonly EpochUnits _units;

        public EpochConverter()
            : this(EpochUnits.Seconds)
        {
        }

        public EpochConverter(EpochUnits units)
        {
            _units = units;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value != null)
            {
                switch (_units)
                {
                    case EpochUnits.Seconds:
                        writer.WriteRawValue($"{(long)((DateTime)value - _epoch).TotalSeconds}");
                        break;

                    case EpochUnits.Milliseconds:
                        writer.WriteRawValue($"{(long)((DateTime)value - _epoch).TotalMilliseconds}");
                        break;

                    default:
                        throw new ArgumentException($"Unsupported value of {_units}", nameof(_units));
                }
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            object result;

            if (reader.Value != null)
            {
                switch (_units)
                {
                    case EpochUnits.Seconds:
                        result = _epoch.AddSeconds((long)reader.Value);
                        break;

                    case EpochUnits.Milliseconds:
                        result = _epoch.AddMilliseconds((long)reader.Value);
                        break;

                    default:
                        throw new ArgumentException($"Unsupported value of {_units}", nameof(_units));
                }
            }
            else
            {
                result = null;
            }

            return result;
        }
    }
}
using System;
using Newtonsoft.Json;

namespace CosmosDbRepository.Types
{
    public class DocumentIdJsonConverter
        : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DocumentId);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return new DocumentId();

            if (reader.TokenType != JsonToken.String)
                throw new JsonSerializationException($"Unexpected token parsing date. Expected String, got {reader.TokenType}.");

            var documentId = new DocumentId();
            documentId = reader.Value.ToString();

            return documentId;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (!(value is DocumentId documentId))
                throw new JsonSerializationException($"Unexpected value when converting id. Expected DocumentId, got {value?.GetType().Name ?? "Null"}.");

            writer.WriteValue(documentId.Id);
        }
    }
}
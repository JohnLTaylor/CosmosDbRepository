using Newtonsoft.Json;
using System;

namespace CosmosDbRepositoryTest
{
    public class ComplexTestData<T> : TestData<T>
    {
        [JsonProperty("tag")]
        public string Tag { get; set; }

        [JsonProperty("xRefId")]
        public string XRefId { get; set; }

        [JsonProperty("date")]
        public DateTimeOffset Date { get; set; }

        [JsonProperty("childItems")]
        public ChildTestData[] ChildItems { get; set; }
    }

    public class ChildTestData
    {
        [JsonProperty("booleanValue")]
        public bool BooleanValue { get; set; }

        [JsonProperty("grandchildItems")]
        public GrandchildTestData[] GrandchildItems { get; set; }
    }

    public class GrandchildTestData
    {
        [JsonProperty("dataType")]
        public string DataType { get; set; }

        [JsonProperty("dataCategory")]
        public string DataCategory { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("numericValue")]
        public float NumericValue { get; set; }
    }
}

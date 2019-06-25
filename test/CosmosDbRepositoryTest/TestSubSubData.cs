using Newtonsoft.Json;
using System;

namespace CosmosDbRepositoryTest
{
    public class TestSubSubData
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
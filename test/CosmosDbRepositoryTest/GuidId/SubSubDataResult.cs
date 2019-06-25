using Newtonsoft.Json;
using System;

namespace CosmosDbRepositoryTest.GuidId
{
    class SubSubDataResult
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        [JsonProperty("data")]
        public string Data { get; set; }
        [JsonProperty("rank")]
        public int Rank { get; set; }
        [JsonProperty("fid")]
        public Guid FId { get; set; }
        [JsonProperty("value")]
        public string Value { get; set; }
    }
}

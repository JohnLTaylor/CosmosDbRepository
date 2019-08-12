using Newtonsoft.Json;

namespace CosmosDbRepositorySubstituteTest
{
    public class TestData<T>
    {
        [JsonProperty("id")]
        public T Id { get; set; }

        [JsonProperty("data")]
        public string Data { get; set; }

        [JsonProperty("rank")]
        public int Rank { get; set; }

        [JsonProperty("_etag")]
        public string ETag { get; set; }

        [JsonProperty("_ts")]
        public long UpdateEpoch { get; set; }
    }
}
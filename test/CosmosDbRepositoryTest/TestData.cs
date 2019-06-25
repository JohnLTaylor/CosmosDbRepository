using Newtonsoft.Json;

namespace CosmosDbRepositoryTest
{
    public class TestData<T>
    {
        [JsonProperty("id")]
        public T Id { get; set; }

        [JsonProperty("data")]
        public string Data { get; set; }

        [JsonProperty("subdata")]
        public TestSubData[] Subdata { get; set; }

        [JsonProperty("rank")]
        public int Rank { get; set; }

        [JsonProperty("_etag")]
        public string ETag { get; set; }

        [JsonProperty("_ts")]
        public long UpdateEpoch { get; set; }
    }
}
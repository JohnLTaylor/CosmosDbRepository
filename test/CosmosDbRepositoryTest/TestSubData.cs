using Newtonsoft.Json;

namespace CosmosDbRepositoryTest
{
    public class TestSubData
    {
        [JsonProperty("subsubdata")]
        public TestSubSubData[] SubSubData { get; set; }
    }
}
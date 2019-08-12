using System;

namespace CosmosDbRepositorySubstituteTest
{
    public class TestConfig
    {
        public string CollectionName { get; set; }

        internal TestConfig Clone()
        {
            return new TestConfig
            {
                CollectionName = CollectionName
            };
        }
    }
}
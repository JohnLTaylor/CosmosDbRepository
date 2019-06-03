using System;

namespace CosmosDbRepositoryTest
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
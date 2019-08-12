namespace CosmosDbRepositorySubstituteTest
{
    public class EnvironmentConfig
    {
        public bool DeleteDatabaseOnClose { get; set; }
        public bool RandomizeDbName { get; set; }
        public bool RandomizeCollectionName { get; set; }
        public bool DeleteCollectionsOnClose { get; set; }
    }
}
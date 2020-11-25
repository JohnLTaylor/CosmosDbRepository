namespace CosmosDbRepository
{
    public interface ICosmosDbQueryStatsCollector
    {
        void Collect(ICosmosDbQueryStats cosmosDbQueryStats);
    }
}

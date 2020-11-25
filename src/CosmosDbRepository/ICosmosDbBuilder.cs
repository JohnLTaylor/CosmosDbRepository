using Microsoft.Azure.Documents;
using Microsoft.Extensions.Logging;
using System;

namespace CosmosDbRepository
{
    public interface ICosmosDbBuilder
    {
        ICosmosDbBuilder NoCreate();
        ICosmosDbBuilder WithId(string name);
        ICosmosDbBuilder WithDefaultThroughput(int? defaultThroughput);
        ICosmosDbBuilder WithPerformanceLogging(ILogger logger, double ruTriggerLevel);
        ICosmosDbBuilder WithQueryStats(ICosmosDbQueryStatsCollector collector);
        ICosmosDbBuilder AddCollection<T>(string id = null, Action<ICosmosDbRepositoryBuilder<T>> func = null);
        ICosmosDb Build(IDocumentClient client);
    }
}

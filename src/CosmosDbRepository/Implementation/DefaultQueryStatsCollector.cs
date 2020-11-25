using Microsoft.Extensions.Logging;

namespace CosmosDbRepository.Implementation
{
    internal class DefaultQueryStatsCollector
        : ICosmosDbQueryStatsCollector
    {
        private ILogger _logger;
        private double _ruTriggerLevel;

        public DefaultQueryStatsCollector(ILogger logger, double ruTriggerLevel)
        {
            _logger = logger;
            _ruTriggerLevel = ruTriggerLevel;
        }

        public void Collect(ICosmosDbQueryStats cosmosDbQueryStats)
        {
            if (cosmosDbQueryStats.RequestCharge >= _ruTriggerLevel)
            {
                _logger.LogInformation(cosmosDbQueryStats.ToString());
            }
        }
    }
}

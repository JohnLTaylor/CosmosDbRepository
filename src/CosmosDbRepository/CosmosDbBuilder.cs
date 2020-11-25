using CosmosDbRepository.Implementation;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CosmosDbRepository
{
    public class CosmosDbBuilder
        : ICosmosDbBuilder
    {
        private List<ICosmosDbRepositoryBuilder> _collectionBuilders = new List<ICosmosDbRepositoryBuilder>();
        private int? _defaultThroughput;
        private bool _createOnMissing = true;
        private ICosmosDbQueryStatsCollector _statsCollector;

        public string Id { get; private set; }

        public ICosmosDbBuilder NoCreate()
        {
            _createOnMissing = false;
            return this;
        }

        public ICosmosDbBuilder WithId(string Id)
        {
            if (this.Id != null) throw new InvalidOperationException("Id already set");
            if (string.IsNullOrWhiteSpace(Id))
            {
                throw new ArgumentException("Invalid database id", nameof(Id));
            }

            this.Id = Id;
            return this;
        }

        public ICosmosDbBuilder WithDefaultThroughput(int? defaultThroughput)
        {
            _defaultThroughput = defaultThroughput;
            return this;
        }

        public ICosmosDbBuilder WithPerformanceLogging(ILogger logger, double ruTriggerLevel)
        {
            return WithQueryStats(new DefaultQueryStatsCollector(logger, ruTriggerLevel));
        }

        public ICosmosDbBuilder WithQueryStats(ICosmosDbQueryStatsCollector collector)
        {
            _statsCollector = collector;
            return this;
        }

        public ICosmosDbBuilder AddCollection<T>(string id = null, Action<ICosmosDbRepositoryBuilder<T>> func = null)
        {
            id = GetCollectionName<T>(id);

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Invalid collection id", nameof(id));
            }

            var builder = new CosmosDbRepositoryBuilder<T>()
                .WithId(id);

            _collectionBuilders.Add(builder);

            func?.Invoke(builder);

            return this;
        }

        public ICosmosDb Build(IDocumentClient client)
        {
            if (string.IsNullOrWhiteSpace(Id)) throw new InvalidOperationException("Id not specified");

            var documentDb = new CosmosDb(client, Id, _defaultThroughput, _collectionBuilders, _createOnMissing, _statsCollector);

            return documentDb;
        }

        private string GetCollectionName<T>(string name)
        {
            if (name != null)
                return name;

            var attrib = typeof(T).GetCustomAttributes(false).OfType<CosmosDbRepositoryNameAttribute>().SingleOrDefault();

            return attrib?.Name ?? typeof(T).Name;
        }
    }
}

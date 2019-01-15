using CosmosDbRepository.Implementation;
using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CosmosDbRepository
{
    public class CosmosDbBuilder
        : ICosmosDbBuilder
    {
        private Dictionary<string, ICosmosDbRepositoryBuilder> _collectionBuilders = new Dictionary<string, ICosmosDbRepositoryBuilder>(StringComparer.OrdinalIgnoreCase);

        public string Id { get; private set; }

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

        public ICosmosDbBuilder AddCollection<T>(string id = null, Action<ICosmosDbRepositoryBuilder> func = null)
        {
            id = GetCollectionName<T>(id);

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Invalid collection id", nameof(id));
            }

            var builder = new CosmosDbRepositoryBuilder<T>()
                .WithId(id);

            _collectionBuilders.Add(id, builder);

            func?.Invoke(builder);

            return this;
        }

        public ICosmosDb Build(IDocumentClient client)
        {
            if (string.IsNullOrWhiteSpace(Id)) throw new InvalidOperationException("Id not specified");

            var documentDb = new CosmosDb(client, Id, _collectionBuilders.Values);

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace CosmosDbRepository.Implementation
{
    internal class CosmosDb
        : ICosmosDb
    {
        private readonly IDocumentClient _client;
        private readonly AsyncLazy<Database> _database;
        private readonly string _id;
        private readonly List<ICosmosDbRepository> _repositories;

        Task<string> ICosmosDb.SelfLinkAsync => SelfLinkAsync();

        public CosmosDb(IDocumentClient client, string databaseId, IEnumerable<ICosmosDbRepositoryBuilder> repositories)
        {
            if (string.IsNullOrWhiteSpace(databaseId))
            {
                throw new ArgumentException("Invalid name", nameof(databaseId));
            }

            _client = client ?? throw new ArgumentNullException(nameof(client));
            _id = databaseId;

            _database = new AsyncLazy<Database>(() => GetOrCreateDatabaseAsync());
            _repositories = repositories.Select(cb => cb.Build(_client, this)).ToList();
        }

        public async Task<string> SelfLinkAsync() => (await _database).SelfLink;

        public ICosmosDbRepository<T> Repository<T>(string name)
        {
            return (ICosmosDbRepository<T>)_repositories.First(r => r.Id == name);
        }

        public ICosmosDbRepository<T> Repository<T>()
        {
            return (ICosmosDbRepository<T>)_repositories.First(r => r.Type == typeof(T));
        }

        private async Task<Database> GetOrCreateDatabaseAsync()
        {
            var database = _client.CreateDatabaseQuery().Where(db => db.Id == _id).AsEnumerable().FirstOrDefault();

            return database != null
                ? database
                : await _client.CreateDatabaseAsync(new Database { Id = _id });
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
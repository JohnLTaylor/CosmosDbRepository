using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace CosmosDbRepository.Implementation
{
    internal class CosmosDb
        : ICosmosDb
    {
        private readonly IDocumentClient _client;
        private readonly AsyncLazy<Database> _database;
        private readonly string _id;
        private readonly int? _defaultThroughput;
        private readonly List<ICosmosDbRepository> _repositories;

        Task<string> ICosmosDb.SelfLinkAsync => SelfLinkAsync();

        public CosmosDb(IDocumentClient client, string databaseId, int? defaultThroughput, IEnumerable<ICosmosDbRepositoryBuilder> repositories, bool createOnMissing)
        {
            if (string.IsNullOrWhiteSpace(databaseId))
            {
                throw new ArgumentException("Invalid name", nameof(databaseId));
            }

            _client = client ?? throw new ArgumentNullException(nameof(client));
            _id = databaseId;
            _defaultThroughput = defaultThroughput;

            _database = new AsyncLazy<Database>(() => GetOrCreateDatabaseAsync(createOnMissing));
            _repositories = repositories.Select(cb => cb.Build(_client, this, _defaultThroughput)).ToList();
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

        public async Task<bool> DeleteAsync(RequestOptions options = null)
        {
            var response = await _client.DeleteDatabaseAsync(await SelfLinkAsync(), options);
            return response.StatusCode == HttpStatusCode.NoContent;
        }

        public async Task Init()
        {
            await _database;

            foreach (var repo in _repositories)
            {
                await repo.Init();
            }
        }

        private async Task<Database> GetOrCreateDatabaseAsync(bool createOnMissing)
        {
            var database = _client.CreateDatabaseQuery().Where(db => db.Id == _id).AsEnumerable().FirstOrDefault();

            return database != null
                ? database
                : createOnMissing
                ? await _client.CreateDatabaseAsync(new Database { Id = _id })
                : throw new InvalidOperationException($"Database { _id } does not exist");
        }
    }
}
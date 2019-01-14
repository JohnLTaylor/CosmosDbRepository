using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace DocDbRepo.Implementation
{
    internal class DocumentDb
        : IDocumentDb
    {
        private readonly IDocumentClient _client;
        private readonly AsyncLazy<Database> _database;
        private readonly string _id;
        private readonly Dictionary<string, IDbCollection> _collections;

        Task<string> IDocumentDb.SelfLinkAsync => SelfLinkAsync();

        public DocumentDb(IDocumentClient client, string databaseId, IEnumerable<IDbCollectionBuilder> collections)
        {
            if (string.IsNullOrWhiteSpace(databaseId))
            {
                throw new ArgumentException("Invalid name", nameof(databaseId));
            }

            _client = client ?? throw new ArgumentNullException(nameof(client));
            _id = databaseId;

            _database = new AsyncLazy<Database>(() => GetOrCreateDatabaseAsync());
            _collections = collections.Select(cb => cb.Build(_client, this)).ToDictionary(c => c.Id);
        }

        public async Task<string> SelfLinkAsync() => (await _database).SelfLink;

        public IDbCollection<T> Repository<T>(string name = null)
        {
            return (IDbCollection<T>)_collections[GetCollectionName<T>(name)];
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

            var attrib = typeof(T).GetCustomAttributes(false).OfType<DocumentCollectionNameAttribute>().SingleOrDefault();

            return attrib?.Name ?? typeof(T).Name;
        }
    }
}
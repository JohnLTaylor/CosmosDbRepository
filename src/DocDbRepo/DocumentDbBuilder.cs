using DocDbRepo.Implementation;
using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DocDbRepo
{
    public class DocumentDbBuilder
        : IDocumentDbBuilder
    {
        private Dictionary<string, IDbCollectionBuilder> _collectionBuilders = new Dictionary<string, IDbCollectionBuilder>(StringComparer.OrdinalIgnoreCase);

        public string Id { get; private set; }

        public IDocumentDbBuilder WithId(string Id)
        {
            if (this.Id != null) throw new InvalidOperationException("Id already set");
            if (string.IsNullOrWhiteSpace(Id))
            {
                throw new ArgumentException("Invalid database id", nameof(Id));
            }

            this.Id = Id;
            return this;
        }

        public IDocumentDbBuilder AddCollection<T>(string id = null, Action<IDbCollectionBuilder> func = null)
        {
            id = GetCollectionName<T>(id);

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Invalid collection id", nameof(id));
            }

            var builder = new DbCollectionBuilder<T>()
                .WithId(id);

            _collectionBuilders.Add(id, builder);

            func?.Invoke(builder);

            return this;
        }

        public IDocumentDb Build(IDocumentClient client)
        {
            if (string.IsNullOrWhiteSpace(Id)) throw new InvalidOperationException("Id not specified");

            var documentDb = new DocumentDb(client, Id, _collectionBuilders.Values);

            return documentDb;
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

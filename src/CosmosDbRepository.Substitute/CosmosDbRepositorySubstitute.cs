using CosmosDbRepository.Types;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace CosmosDbRepository.Substitute
{
    public class CosmosDbRepositorySubstitute<T>
        : CosmosDbRepositorySubstituteBase<T>
    {
        private readonly List<EntityStorage> _entities = new List<EntityStorage>();

        protected override T AddEntityStorageItem(T entity, RequestOptions requestOptions)
        {
            var item = new EntityStorage(entity);

            if (DocumentId.IsNullOrEmpty(item.Id))
                item.Id = Guid.NewGuid().ToString();

            lock (_entities)
            {
                if (_entities.Any(cfg => cfg.Id == item.Id))
                    throw CreateDbException(HttpStatusCode.Conflict, "Duplicate id");

                item.ETag = $"\"{Guid.NewGuid()}\"";
                item.TS = DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond;
                _entities.Add(item);
            }

            return DeepClone(item.Entity);
        }

        protected override T UpsertEntityStorageItem(T entity, RequestOptions requestOptions)
        {
            var item = new EntityStorage(entity);

            lock (_entities)
            {
                var index = _entities.FindIndex(d => d.Id == item.Id);

                if (index >= 0)
                {
                    if (CheckETag(entity, _entities[index], out var exception))
                        throw exception;

                    _entities.RemoveAt(index);
                }

                item.ETag = $"\"{Guid.NewGuid()}\"";
                item.TS = DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond;
                _entities.Add(item);
            }

            return DeepClone(item.Entity);
        }


        protected override T ReplaceEntityStorageItem(T entity, RequestOptions requestOptions)
        {
            var item = new EntityStorage(entity);

            lock (_entities)
            {
                var index = _entities.FindIndex(d => d.Id == item.Id);

                if (index < 0)
                {
                    throw CreateDbException(HttpStatusCode.NotFound, "Not Found");
                }

                if (CheckETag(entity, _entities[index], out var exception))
                    throw exception;

                _entities.RemoveAt(index);

                item.ETag = $"\"{Guid.NewGuid()}\"";
                item.TS = DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond;
                _entities.Add(item);
            }

            return DeepClone(item.Entity);
        }


        protected override T GetEntityStorageItem(DocumentId itemId, RequestOptions requestOptions)
        {
            lock (_entities)
            {
                EntityStorage item = _entities.FirstOrDefault(i => i.Id == itemId);
                return item == default(EntityStorage) ? default(T) : DeepClone(item.Entity);
            }
        }

        protected override T[] GetEntityStorageItems(FeedOptions feedOptions)
        {
            lock (_entities)
            {
                return _entities.Select(i => DeepClone(i.Entity)).ToArray();
            }
        }

        protected override bool DeleteEntityStorageItem(DocumentId id, RequestOptions requestOptions)
        {
            lock (_entities)
            {
                return _entities.RemoveAll(cfg => cfg.Id == id) > 0;
            }
        }

        protected override bool DeleteEntityStorageItem(T entity, RequestOptions requestOptions)
        {
            var item = new EntityStorage(entity);

            lock (_entities)
            {
                var index = _entities.FindIndex(d => d.Id == item.Id);

                if (index < 0)
                {
                    throw CreateDbException(HttpStatusCode.NotFound);
                }

                if (CheckETag(item.Entity, _entities[index], out var exception))
                    throw exception;

                _entities.RemoveAt(index);
                return true;
            }
        }
    }
}

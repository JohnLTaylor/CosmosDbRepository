using CosmosDbRepository.Types;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace CosmosDbRepository.Substitute
{
    public class CosmosDbRepositoryPartitionedSubstitute<T>
    : CosmosDbRepositorySubstituteBase<T>
    {
        private readonly Dictionary<string, List<EntityStorage>> _entities = new Dictionary<string, List<EntityStorage>>();
        private readonly Func<T, object> _partionkeySelector;

        public CosmosDbRepositoryPartitionedSubstitute(Func<T, object> partionkeySelector)
        {
            _partionkeySelector = partionkeySelector;
        }

        protected override T AddEntityStorageItem(T entity, RequestOptions requestOptions)
        {
            var partitionKey = GetPartionKey(entity, requestOptions);
            var item = new EntityStorage(entity);

            if (DocumentId.IsNullOrEmpty(item.Id))
                item.Id = Guid.NewGuid().ToString();

            lock (_entities)
            {                
                if (!_entities.TryGetValue(partitionKey, out var entities))
                {
                    entities = new List<EntityStorage>();
                    _entities.Add(partitionKey, entities);
                }

                if (entities.Any(cfg => cfg.Id == item.Id))
                    throw CreateDbException(HttpStatusCode.Conflict, "Duplicate id");

                item.ETag = $"\"{Guid.NewGuid()}\"";
                item.TS = DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond;
                entities.Add(item);
            }

            return DeepClone(item.Entity);
        }

        protected override T UpsertEntityStorageItem(T entity, RequestOptions requestOptions)
        {
            var partitionKey = GetPartionKey(entity, requestOptions);
            var item = new EntityStorage(entity);

            lock (_entities)
            {
                if (!_entities.TryGetValue(partitionKey, out var entities))
                {
                    entities = new List<EntityStorage>();
                    _entities.Add(partitionKey, entities);
                }

                var index = entities.FindIndex(d => d.Id == item.Id);

                if (index >= 0)
                {
                    if (CheckETag(entity, entities[index], out var exception))
                        throw exception;

                    entities.RemoveAt(index);
                }

                item.ETag = $"\"{Guid.NewGuid()}\"";
                item.TS = DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond;
                entities.Add(item);
            }

            return DeepClone(item.Entity);
        }


        protected override T ReplaceEntityStorageItem(T entity, RequestOptions requestOptions)
        {
            var partitionKey = GetPartionKey(entity, requestOptions);
            var item = new EntityStorage(entity);

            lock (_entities)
            {
                if (!_entities.TryGetValue(partitionKey, out var entities))
                {
                    throw CreateDbException(HttpStatusCode.NotFound, "Not Found");
                }

                var index = entities.FindIndex(d => d.Id == item.Id);

                if (index < 0)
                {
                    throw CreateDbException(HttpStatusCode.NotFound, "Not Found");
                }

                if (CheckETag(entity, entities[index], out var exception))
                    throw exception;

                entities.RemoveAt(index);

                item.ETag = $"\"{Guid.NewGuid()}\"";
                item.TS = DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond;
                entities.Add(item);
            }

            return DeepClone(item.Entity);
        }


        protected override T GetEntityStorageItem(DocumentId itemId, RequestOptions requestOptions)
        {
            var partitionKey = CheckPartionKey(requestOptions);

            lock (_entities)
            {
                if (!_entities.TryGetValue(partitionKey, out var entities))
                {
                    return default(T);
                }

                EntityStorage item = entities.FirstOrDefault(i => i.Id == itemId);
                return item == default(EntityStorage) ? default(T) : DeepClone(item.Entity);
            }
        }

        protected override T[] GetEntityStorageItems(FeedOptions feedOptions)
        {
            var partitionKey = CheckPartionKey(feedOptions);

            lock (_entities)
            {
                if (feedOptions.EnableCrossPartitionQuery)
                {
                    return _entities.Values.SelectMany(l => l.Select(i => DeepClone(i.Entity))).ToArray();
                }

                if (!_entities.TryGetValue(partitionKey, out var entities))
                {
                    throw CreateDbException(HttpStatusCode.NotFound, "Not Found");
                }

                return entities.Select(i => DeepClone(i.Entity)).ToArray();
            }
        }

        protected override bool DeleteEntityStorageItem(DocumentId id, RequestOptions requestOptions)
        {
            var partitionKey = CheckPartionKey(requestOptions);
            
            lock (_entities)
            {
                if (!_entities.TryGetValue(partitionKey, out var entities))
                {
                    throw CreateDbException(HttpStatusCode.NotFound, "Not Found");
                }

                var index = entities.FindIndex(d => d.Id == id);

                if (index < 0)
                {
                    return false;
                }
                
                entities.RemoveAt(index);

                if (entities.Count == 0)
                {
                    _entities.Remove(partitionKey);
                }

                return true;
            }
        }

        protected override bool DeleteEntityStorageItem(T entity, RequestOptions requestOptions)
        {
            var partitionKey = GetPartionKey(entity, requestOptions);
            var item = new EntityStorage(entity);

            lock (_entities)
            {
                if (!_entities.TryGetValue(partitionKey, out var entities))
                {
                    throw CreateDbException(HttpStatusCode.NotFound, "Not Found");
                }

                var index = entities.FindIndex(d => d.Id == item.Id);

                if (index < 0)
                {
                    return false;
                }

                if (CheckETag(item.Entity, entities[index], out var exception))
                    throw exception;

                entities.RemoveAt(index);

                if (entities.Count == 0)
                {
                    _entities.Remove(partitionKey);
                }

                return true;
            }
        }

        private string GetPartionKey(T entity, RequestOptions requestOptions)
        {
            if (requestOptions?.PartitionKey == null)
            {
                if (_partionkeySelector == null)
                {
                    throw new InvalidOperationException("PartitionkeySelector must be specified");
                }

                return _partionkeySelector(entity).ToString();
            }

            return requestOptions.PartitionKey.ToString();
        }

        private string CheckPartionKey(RequestOptions requestOptions)
        {
            if (requestOptions?.PartitionKey == null)
            {
                throw new InvalidOperationException("PartitionKey must be specified");
            }

            return requestOptions.PartitionKey.ToString();
        }

        private string CheckPartionKey(FeedOptions feedOptions)
        {
            if (feedOptions?.EnableCrossPartitionQuery != true && feedOptions?.PartitionKey == null)
            {
                throw new InvalidOperationException("PartitionKey must be specified");
            }

            return feedOptions?.PartitionKey?.ToString();
        }
    }
}

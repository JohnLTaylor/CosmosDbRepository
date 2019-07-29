using CosmosDbRepository.Types;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace CosmosDbRepository.Substitute
{
    public class CosmosDbRepositorySubstitute<TEntity>
        : ICosmosDbRepository<TEntity>
    {
        private readonly List<EntityStorage> _entities = new List<EntityStorage>();
        private static readonly Type _dbExceptionType = typeof(DocumentClientException);

        public string Id => throw new NotImplementedException();

        public Type Type => throw new NotImplementedException();

        public Task<string> AltLink => throw new NotImplementedException();

        public Task<TEntity> AddAsync(TEntity entity, RequestOptions requestOptions = null)
        {
            var item = new EntityStorage(entity);

            if (string.IsNullOrEmpty(item.Id))
                item.Id = Guid.NewGuid().ToString();

            lock (_entities)
            {
                if (_entities.Any(cfg => cfg.Id == item.Id))
                    throw CreateDbException(HttpStatusCode.Conflict, "Duplicate id");

                item.ETag = $"\"{Guid.NewGuid()}\"";

                _entities.Add(item);
            }

            return Task.FromResult(DeepClone(item.Entity));
        }

        public Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate = null, Func<IQueryable<TEntity>, IQueryable<TEntity>> clauses = null, FeedOptions feedOptions = null)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(RequestOptions requestOptions = null)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteDocumentAsync(DocumentId itemId, RequestOptions requestOptions = null)
        {
            bool result;

            lock (_entities)
            {
                result = _entities.RemoveAll(cfg => cfg.Id == itemId) > 0;
            }

            return Task.FromResult(result);
        }

        public Task<bool> DeleteDocumentAsync(TEntity entity, RequestOptions requestOptions = null)
        {
            var item = new EntityStorage(entity);
            return DeleteDocumentAsync(item.Id, requestOptions);
        }

        public async Task<IList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate = null, Func<IQueryable<TEntity>, IQueryable<TEntity>> clauses = null, FeedOptions feedOptions = null)
        {
            var result = await FindAsync(feedOptions?.MaxItemCount ?? 0, null, predicate, clauses, feedOptions);
            return result.Items;
        }

        public Task<CosmosDbRepositoryPagedResults<TEntity>> FindAsync(int pageSize, string continuationToken, Expression<Func<TEntity, bool>> predicate = null, Func<IQueryable<TEntity>, IQueryable<TEntity>> clauses = null, FeedOptions feedOptions = null)
        {
            IEnumerable<TEntity> entities;

            if (string.IsNullOrEmpty(continuationToken))
            {
                lock (_entities)
                {
                    entities = _entities.Select(i => i.Entity).ToArray();
                }
            }
            else
            {
                entities = JsonConvert.DeserializeObject<TEntity[]>(continuationToken);
            }

            if (predicate != default)
                entities = entities.Where(predicate.Compile());

            if (clauses != default)
                entities = clauses.Invoke(entities.AsQueryable());

            var result = new CosmosDbRepositoryPagedResults<TEntity>()
            {
                Items = entities.Select(DeepClone).ToList()
            };

            if (pageSize > 0 && pageSize < result.Items.Count)
            {
                result.ContinuationToken = JsonConvert.SerializeObject(result.Items.Skip(pageSize));
                result.Items = result.Items.Take(pageSize).ToList();
            }

            return Task.FromResult(result);
        }

        public Task<TEntity> FindFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate = null, Func<IQueryable<TEntity>, IQueryable<TEntity>> clauses = null, FeedOptions feedOptions = null)
        {
            throw new NotImplementedException();
        }

        public Task<TEntity> GetAsync(TEntity entity, RequestOptions requestOptions = null)
        {
            var item = new EntityStorage(entity);
            return GetAsync(item.Id, requestOptions);
        }

        public Task<TEntity> GetAsync(DocumentId itemId, RequestOptions requestOptions = null)
        {
            TEntity item;

            lock (_entities)
            {
                item = _entities.FirstOrDefault(cfg => cfg.Id == itemId).Entity;
            }

            return Task.FromResult(DeepClone(item));
        }

        public Task Init()
        {
            throw new NotImplementedException();
        }

        public Task<TEntity> ReplaceAsync(TEntity entity, RequestOptions requestOptions = null)
        {
            throw new NotImplementedException();
        }

        public async Task<IList<U>> SelectAsync<U>(Expression<Func<TEntity, U>> selector, Func<IQueryable<U>, IQueryable<U>> selectClauses = null, FeedOptions feedOptions = null)
        {
            var result = await SelectAsync(feedOptions?.MaxItemCount ?? 0, default, selector, selectClauses, feedOptions);
            return result.Items;
        }

        public Task<CosmosDbRepositoryPagedResults<U>> SelectAsync<U>(int pageSize, string continuationToken, Expression<Func<TEntity, U>> selector, Func<IQueryable<U>, IQueryable<U>> selectClauses = null, FeedOptions feedOptions = null)
        {
            IEnumerable<TEntity> entities;

            if (string.IsNullOrEmpty(continuationToken))
            {
                lock (_entities)
                {
                    entities = _entities.Select(i => i.Entity).ToArray();
                }
            }
            else
            {
                entities = JsonConvert.DeserializeObject<TEntity[]>(continuationToken);
            }

            var items = entities.Select(selector.Compile());

            if (selectClauses != default)
                items = selectClauses.Invoke(items.AsQueryable());

            var result = new CosmosDbRepositoryPagedResults<U>()
            {
                Items = items.ToList()
            };

            if (pageSize > 0 && pageSize < result.Items.Count)
            {
                result.ContinuationToken = JsonConvert.SerializeObject(result.Items.Skip(pageSize));
                result.Items = result.Items.Take(pageSize).ToList();
            }

            return Task.FromResult(result);
        }

        public async Task<IList<U>> SelectAsync<U, V>(Expression<Func<V, U>> selector, Func<IQueryable<TEntity>, IQueryable<V>> whereClauses, Func<IQueryable<U>, IQueryable<U>> selectClauses = null, FeedOptions feedOptions = null)
        {
            var result = await SelectAsync(feedOptions?.MaxItemCount ?? 0, default, selector, whereClauses, selectClauses, feedOptions);
            return result.Items;
        }

        public Task<CosmosDbRepositoryPagedResults<U>> SelectAsync<U, V>(int pageSize, string continuationToken, Expression<Func<V, U>> selector, Func<IQueryable<TEntity>, IQueryable<V>> whereClauses, Func<IQueryable<U>, IQueryable<U>> selectClauses = null, FeedOptions feedOptions = null)
        {
            IEnumerable<TEntity> entities;

            if (string.IsNullOrEmpty(continuationToken))
            {
                lock (_entities)
                {
                    entities = _entities.Select(i => i.Entity).ToArray();
                }
            }
            else
            {
                entities = JsonConvert.DeserializeObject<TEntity[]>(continuationToken);
            }

            var items = whereClauses.Invoke(entities.AsQueryable()).Select(selector.Compile());

            if (selectClauses != default)
                items = selectClauses.Invoke(items.AsQueryable());

            var result = new CosmosDbRepositoryPagedResults<U>()
            {
                Items = items.ToList()
            };

            if (pageSize > 0 && pageSize < result.Items.Count)
            {
                result.ContinuationToken = JsonConvert.SerializeObject(result.Items.Skip(pageSize));
                result.Items = result.Items.Take(pageSize).ToList();
            }

            return Task.FromResult(result);
        }

        public async Task<IList<U>> SelectManyAsync<U>(Expression<Func<TEntity, IEnumerable<U>>> selector, Func<IQueryable<TEntity>, IQueryable<TEntity>> whereClauses = null, Func<IQueryable<U>, IQueryable<U>> selectClauses = null, FeedOptions feedOptions = null)
        {
            var result = await SelectManyAsync(feedOptions?.MaxItemCount ?? 0, default, selector, whereClauses, selectClauses, feedOptions);
            return result.Items;
        }

        public Task<CosmosDbRepositoryPagedResults<U>> SelectManyAsync<U>(int pageSize, string continuationToken, Expression<Func<TEntity, IEnumerable<U>>> selector, Func<IQueryable<TEntity>, IQueryable<TEntity>> whereClauses = null, Func<IQueryable<U>, IQueryable<U>> selectClauses = null, FeedOptions feedOptions = null)
        {
            IEnumerable<TEntity> entities;

            if (string.IsNullOrEmpty(continuationToken))
            {
                lock (_entities)
                {
                    entities = _entities.Select(i => i.Entity).ToArray();
                }
            }
            else
            {
                entities = JsonConvert.DeserializeObject<TEntity[]>(continuationToken);
            }

            if (whereClauses != default)
                entities = whereClauses.Invoke(entities.AsQueryable());

            var items = entities.SelectMany(selector.Compile());

            if (selectClauses != default)
                items = selectClauses.Invoke(items.AsQueryable());

            var result = new CosmosDbRepositoryPagedResults<U>()
            {
                Items = items.ToList()
            };

            if (pageSize > 0 && pageSize < result.Items.Count)
            {
                result.ContinuationToken = JsonConvert.SerializeObject(result.Items.Skip(pageSize));
                result.Items = result.Items.Take(pageSize).ToList();
            }

            return Task.FromResult(result);
        }

        public IStoredProcedure<TResult> StoredProcedure<TResult>(string id)
        {
            throw new NotImplementedException();
        }

        public IStoredProcedure<TParam, TResult> StoredProcedure<TParam, TResult>(string id)
        {
            throw new NotImplementedException();
        }

        public IStoredProcedure<TParam1, TParam2, TResult> StoredProcedure<TParam1, TParam2, TResult>(string id)
        {
            throw new NotImplementedException();
        }

        public IStoredProcedure<TParam1, TParam2, TParam3, TResult> StoredProcedure<TParam1, TParam2, TParam3, TResult>(string id)
        {
            throw new NotImplementedException();
        }

        public IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TResult> StoredProcedure<TParam1, TParam2, TParam3, TParam4, TResult>(string id)
        {
            throw new NotImplementedException();
        }

        public IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TResult> StoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TResult>(string id)
        {
            throw new NotImplementedException();
        }

        public IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TResult> StoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TResult>(string id)
        {
            throw new NotImplementedException();
        }

        public IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TResult> StoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TResult>(string id)
        {
            throw new NotImplementedException();
        }

        public IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TResult> StoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TResult>(string id)
        {
            throw new NotImplementedException();
        }

        public IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TResult> StoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TResult>(string id)
        {
            throw new NotImplementedException();
        }

        public IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TResult> StoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TResult>(string id)
        {
            throw new NotImplementedException();
        }

        public IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TResult> StoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TResult>(string id)
        {
            throw new NotImplementedException();
        }

        public IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TResult> StoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TResult>(string id)
        {
            throw new NotImplementedException();
        }

        public IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TResult> StoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TResult>(string id)
        {
            throw new NotImplementedException();
        }

        public IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TResult> StoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TResult>(string id)
        {
            throw new NotImplementedException();
        }

        public IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TResult> StoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TResult>(string id)
        {
            throw new NotImplementedException();
        }

        public IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TParam16, TResult> StoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TParam16, TResult>(string id)
        {
            throw new NotImplementedException();
        }

        public Task<TEntity> UpsertAsync(TEntity entity, RequestOptions requestOptions = null)
        {
            var item = new EntityStorage(entity);

            if (string.IsNullOrEmpty(item.Id))
                item.Id = Guid.NewGuid().ToString();

            lock (_entities)
            {

                item.ETag = $"\"{Guid.NewGuid()}\"";

                _entities.RemoveAll(cfg => cfg.Id == item.Id);
                _entities.Add(item);
            }

            return Task.FromResult(DeepClone(item.Entity));
        }

        private static TEntity DeepClone(TEntity src)
        {
            return (src == default)
                ? default
                : JsonConvert.DeserializeObject<TEntity>(JsonConvert.SerializeObject(src));
        }

        private class EntityStorage
        {
            private static readonly Action<TEntity, string> SetETag;
            private static readonly Func<TEntity, string> GetETag;
            private static readonly Func<TEntity, string> GetId;
            private static readonly Action<TEntity, string> SetId;

            public readonly TEntity Entity;

            public string Id
            {
                get => GetId(Entity);
                set => SetId(Entity, value);
            }

            public string ETag
            {
                get => GetETag(Entity);
                set => SetETag(Entity, value);
            }

            static EntityStorage()
            {
                (string name, PropertyInfo info) GetPropertyJsonName(PropertyInfo pi)
                {
                    var jsonProperty = pi.GetCustomAttribute<JsonPropertyAttribute>();
                    return (jsonProperty?.PropertyName ?? pi.Name, pi);
                }

                var properties = typeof(TEntity).GetProperties().Select(GetPropertyJsonName).ToDictionary(o => o.name, o => o.info);

                var idProperty = properties["id"];
                GetId = BuildIdGet(idProperty);
                SetId = BuildIdSet(idProperty);

                properties.TryGetValue("_etag", out var eTagProperty);

                GetETag = BuildIdGet(eTagProperty);
                SetETag = BuildIdSet(eTagProperty);
            }

            public EntityStorage(TEntity entity)
            {
                Entity = DeepClone(entity);
            }

            private static Func<TEntity, string> BuildIdGet(PropertyInfo idProperty)
            {
                var source = Expression.Parameter(typeof(TEntity), "source");
                Expression IdProperty = Expression.Property(source, idProperty);

                if (idProperty.PropertyType != typeof(string))
                {
                    IdProperty = Expression.Call(IdProperty, "ToString", new Type[0]);
                }

                return Expression.Lambda<Func<TEntity, string>>(IdProperty, source).Compile();
            }

            private static Action<TEntity, string> BuildIdSet(PropertyInfo idProperty)
            {
                if (!idProperty.CanWrite)
                {
                    return (_, __) => throw new InvalidOperationException("The id property is not assignable");
                }

                var source = Expression.Parameter(typeof(TEntity), "source");
                var value = Expression.Parameter(typeof(string), "value");


                Expression IdProperty = Expression.Property(source, idProperty);

                var body = idProperty.PropertyType != typeof(string)
                    ? Expression.Assign(IdProperty, Expression.Call(idProperty.PropertyType.GetMethod("Parse", new[] { typeof(string) }), value))
                    : Expression.Assign(IdProperty, value);

                return Expression.Lambda<Action<TEntity, string>>(body, source, value).Compile();
            }
        }

        private static DocumentClientException CreateDbException(HttpStatusCode statusCode, string message = null)
        {
            var ex = (DocumentClientException)FormatterServices.GetUninitializedObject(_dbExceptionType);
            _dbExceptionType.GetProperty("StatusCode").SetValue(ex, statusCode);
            _dbExceptionType.GetField("_message", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(ex, message);

            return ex;
        }
    }
}

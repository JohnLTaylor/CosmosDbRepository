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
    public class CosmosDbRepositorySubstitute<T>
        : ICosmosDbRepository<T>
    {
        private readonly List<EntityStorage> _entities = new List<EntityStorage>();
        private readonly List<Func<T, DocumentClientException>> _addExceptionConditions = new List<Func<T, DocumentClientException>>();
        private readonly List<Func<object, DocumentClientException>> _getExceptionConditions = new List<Func<object, DocumentClientException>>();
        private readonly List<Func<object, DocumentClientException>> _deleteExceptionConditions = new List<Func<object, DocumentClientException>>();
        private readonly List<Func<DocumentClientException>> _findExceptionConditions = new List<Func<DocumentClientException>>();
        private readonly List<Func<DocumentClientException>> _findFirstOrDefaultExceptionConditions = new List<Func<DocumentClientException>>();
        private readonly List<Func<T, DocumentClientException>> _replaceExceptionConditions = new List<Func<T, DocumentClientException>>();
        private readonly List<Func<DocumentClientException>> _selectExceptionConditions = new List<Func<DocumentClientException>>();
        private readonly List<Func<DocumentClientException>> _selectManyExceptionConditions = new List<Func<DocumentClientException>>();
        private readonly List<Func<T, DocumentClientException>> _upsertExceptionConditions = new List<Func<T, DocumentClientException>>();
        private readonly List<Func<DocumentClientException>> _countExceptionConditions = new List<Func<DocumentClientException>>();
        private readonly Dictionary<string, Func<object[], object>> _storedProcedureCallback = new Dictionary<string, Func<object[], object>>();
        private static readonly Type _dbExceptionType = typeof(DocumentClientException);

        public string Id => throw new NotImplementedException();

        public Type Type => typeof(T);

        public Task<string> AltLink => throw new NotImplementedException();

        public Task<T> AddAsync(T entity, RequestOptions requestOptions = null)
        {
            var failure = _addExceptionConditions.Select(func => func(entity)).FirstOrDefault();

            if (failure != default)
            {
                return Task.FromException<T>(failure);
            }

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

            return Task.FromResult(DeepClone(item.Entity));
        }

        public Task<int> CountAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IQueryable<T>> clauses = null, FeedOptions feedOptions = null)
        {
            var failure = _countExceptionConditions.Select(func => func()).FirstOrDefault();

            if (failure != default)
            {
                return Task.FromException<int>(failure);
            }

            IEnumerable<T> entities;

            lock (_entities)
            {
                entities = _entities.Select(i => i.Entity).ToArray();
            }

            if (predicate != default)
                entities = entities.Where(predicate.Compile());

            if (clauses != default)
                entities = clauses.Invoke(entities.AsQueryable());

            return Task.FromResult(entities.Count());
        }

        public Task<bool> DeleteAsync(RequestOptions requestOptions = null)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteDocumentAsync(DocumentId itemId, RequestOptions requestOptions = null)
        {
            var failure = _deleteExceptionConditions.Select(func => func(itemId)).FirstOrDefault();

            if (failure != default)
            {
                return Task.FromException<bool>(failure);
            }

            bool result;

            lock (_entities)
            {
                result = _entities.RemoveAll(cfg => cfg.Id == itemId) > 0;
            }

            if (result == default)
                return Task.FromException<bool>(CreateDbException(HttpStatusCode.NotFound));

            return Task.FromResult(result);
        }

        public Task<bool> DeleteDocumentAsync(T entity, RequestOptions requestOptions = null)
        {
            var item = new EntityStorage(entity);

            var failure = _deleteExceptionConditions.Select(func => func(entity)).FirstOrDefault() ??
                          _deleteExceptionConditions.Select(func => func(item.Id)).FirstOrDefault();

            if (failure != default)
            {
                return Task.FromException<bool>(failure);
            }

            lock (_entities)
            {
                var index = _entities.FindIndex(d => d.Id == item.Id);

                if (index < 0)
                {
                    return Task.FromException<bool>(CreateDbException(HttpStatusCode.NotFound));
                }

                if (CheckETag(entity, _entities[index], out var exception))
                    return Task.FromException<bool>(exception);

                _entities.RemoveAt(index);
                return Task.FromResult(true);
            }
        }

        public async Task<IList<T>> FindAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IQueryable<T>> clauses = null, FeedOptions feedOptions = null)
        {
            var result = await FindAsync(feedOptions?.MaxItemCount ?? 0, null, predicate, clauses, feedOptions);
            return result.Items;
        }

        public Task<CosmosDbRepositoryPagedResults<T>> FindAsync(int pageSize, string continuationToken, Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IQueryable<T>> clauses = null, FeedOptions feedOptions = null)
        {
            var failure = _findExceptionConditions.Select(func => func()).FirstOrDefault();

            if (failure != default)
            {
                return Task.FromException<CosmosDbRepositoryPagedResults<T>>(failure);
            }

            IEnumerable<T> entities;

            if (string.IsNullOrEmpty(continuationToken))
            {
                lock (_entities)
                {
                    entities = _entities.Select(i => i.Entity).ToArray();
                }
            }
            else
            {
                entities = JsonConvert.DeserializeObject<T[]>(continuationToken);
            }

            if (predicate != default)
                entities = entities.Where(predicate.Compile());

            if (clauses != default)
                entities = clauses.Invoke(entities.AsQueryable());

            var result = new CosmosDbRepositoryPagedResults<T>()
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

        public Task<T> FindFirstOrDefaultAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IQueryable<T>> clauses = null, FeedOptions feedOptions = null)
        {
            var failure = _findFirstOrDefaultExceptionConditions.Select(func => func()).FirstOrDefault();

            if (failure != default)
            {
                return Task.FromException<T>(failure);
            }

            IEnumerable<T> entities;

            lock (_entities)
            {
                entities = _entities.Select(i => i.Entity).ToArray();
            }

            if (predicate != default)
                entities = entities.Where(predicate.Compile());

            if (clauses != default)
                entities = clauses.Invoke(entities.AsQueryable());

            return Task.FromResult(entities.FirstOrDefault());
        }

        public Task<T> GetAsync(T entity, RequestOptions requestOptions = null)
        {
            var failure = _getExceptionConditions.Select(func => func(entity)).FirstOrDefault();

            if (failure != default)
            {
                return Task.FromException<T>(failure);
            }

            var item = new EntityStorage(entity);
            return GetAsync(item.Id, requestOptions);
        }

        public Task<T> GetAsync(DocumentId itemId, RequestOptions requestOptions = null)
        {
            var failure = _getExceptionConditions.Select(func => func(itemId)).FirstOrDefault();

            if (failure != default)
            {
                return Task.FromException<T>(failure);
            }

            EntityStorage item;

            lock (_entities)
            {
                item = _entities.FirstOrDefault(cfg => cfg.Id == itemId);
            }

            return Task.FromResult(item == default ? default : DeepClone(item.Entity));
        }

        public Task Init()
        {
            throw new NotImplementedException();
        }

        public Task<T> ReplaceAsync(T entity, RequestOptions requestOptions = null)
        {
            var failure = _replaceExceptionConditions.Select(func => func(entity)).FirstOrDefault();

            if (failure != default)
            {
                return Task.FromException<T>(failure);
            }

            var item = new EntityStorage(entity);

            lock (_entities)
            {
                var index = _entities.FindIndex(d => d.Id == item.Id);

                if (index < 0)
                {
                    throw CreateDbException(HttpStatusCode.NotFound, "Not Found");
                }

                if (CheckETag(entity, _entities[index], out var exception))
                    return Task.FromException<T>(exception);

                _entities.RemoveAt(index);

                item.ETag = $"\"{Guid.NewGuid()}\"";
                item.TS = DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond;
                _entities.Add(item);
            }

            return Task.FromResult(DeepClone(item.Entity));
        }

        public async Task<IList<U>> SelectAsync<U>(Expression<Func<T, U>> selector, Func<IQueryable<U>, IQueryable<U>> selectClauses = null, FeedOptions feedOptions = null)
        {
            var result = await SelectAsync(feedOptions?.MaxItemCount ?? 0, default, selector, selectClauses, feedOptions);
            return result.Items;
        }

        public Task<CosmosDbRepositoryPagedResults<U>> SelectAsync<U>(int pageSize, string continuationToken, Expression<Func<T, U>> selector, Func<IQueryable<U>, IQueryable<U>> selectClauses = null, FeedOptions feedOptions = null)
        {
            var failure = _selectExceptionConditions.Select(func => func()).FirstOrDefault();

            if (failure != default)
            {
                return Task.FromException<CosmosDbRepositoryPagedResults<U>>(failure);
            }

            IEnumerable<T> entities;

            if (string.IsNullOrEmpty(continuationToken))
            {
                lock (_entities)
                {
                    entities = _entities.Select(i => i.Entity).ToArray();
                }
            }
            else
            {
                entities = JsonConvert.DeserializeObject<T[]>(continuationToken);
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

        public async Task<IList<U>> SelectAsync<U, V>(Expression<Func<V, U>> selector, Func<IQueryable<T>, IQueryable<V>> whereClauses, Func<IQueryable<U>, IQueryable<U>> selectClauses = null, FeedOptions feedOptions = null)
        {
            var result = await SelectAsync(feedOptions?.MaxItemCount ?? 0, default, selector, whereClauses, selectClauses, feedOptions);
            return result.Items;
        }

        public Task<CosmosDbRepositoryPagedResults<U>> SelectAsync<U, V>(int pageSize, string continuationToken, Expression<Func<V, U>> selector, Func<IQueryable<T>, IQueryable<V>> whereClauses, Func<IQueryable<U>, IQueryable<U>> selectClauses = null, FeedOptions feedOptions = null)
        {
            var failure = _selectExceptionConditions.Select(func => func()).FirstOrDefault();

            if (failure != default)
            {
                return Task.FromException<CosmosDbRepositoryPagedResults<U>>(failure);
            }

            IEnumerable<T> entities;

            if (string.IsNullOrEmpty(continuationToken))
            {
                lock (_entities)
                {
                    entities = _entities.Select(i => i.Entity).ToArray();
                }
            }
            else
            {
                entities = JsonConvert.DeserializeObject<T[]>(continuationToken);
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

        public async Task<IList<U>> SelectManyAsync<U>(Expression<Func<T, IEnumerable<U>>> selector, Func<IQueryable<T>, IQueryable<T>> whereClauses = null, Func<IQueryable<U>, IQueryable<U>> selectClauses = null, FeedOptions feedOptions = null)
        {
            var result = await SelectManyAsync(feedOptions?.MaxItemCount ?? 0, default, selector, whereClauses, selectClauses, feedOptions);
            return result.Items;
        }

        public Task<CosmosDbRepositoryPagedResults<U>> SelectManyAsync<U>(int pageSize, string continuationToken, Expression<Func<T, IEnumerable<U>>> selector, Func<IQueryable<T>, IQueryable<T>> whereClauses = null, Func<IQueryable<U>, IQueryable<U>> selectClauses = null, FeedOptions feedOptions = null)
        {
            var failure = _selectManyExceptionConditions.Select(func => func()).FirstOrDefault();

            if (failure != default)
            {
                return Task.FromException<CosmosDbRepositoryPagedResults<U>>(failure);
            }

            IEnumerable<T> entities;

            if (string.IsNullOrEmpty(continuationToken))
            {
                lock (_entities)
                {
                    entities = _entities.Select(i => i.Entity).ToArray();
                }
            }
            else
            {
                entities = JsonConvert.DeserializeObject<T[]>(continuationToken);
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

        public Task<T> UpsertAsync(T entity, RequestOptions requestOptions = null)
        {
            var failure = _upsertExceptionConditions.Select(func => func(entity)).FirstOrDefault();

            if (failure != default)
            {
                return Task.FromException<T>(failure);
            }

            var item = new EntityStorage(entity);

            if (DocumentId.IsNullOrEmpty(item.Id))
                item.Id = Guid.NewGuid().ToString();

            lock (_entities)
            {
                var index = _entities.FindIndex(d => d.Id == item.Id);

                if (index >= 0)
                {
                    if (CheckETag(entity, _entities[index], out var exception))
                        return Task.FromException<T>(exception);

                    _entities.RemoveAt(index);
                }

                item.ETag = $"\"{Guid.NewGuid()}\"";
                item.TS = DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond;
                _entities.Add(item);
            }

            return Task.FromResult(DeepClone(item.Entity));
        }

        public IStoredProcedure<TResult> StoredProcedure<TResult>(string id)
        {
            if (!_storedProcedureCallback.TryGetValue(id, out var func))
            {
                throw new ArgumentException();
            }

            return new StoredProcedureSubstitute<TResult>(func);
        }

        public IStoredProcedure<TParam, TResult> StoredProcedure<TParam, TResult>(string id)
        {
            if (!_storedProcedureCallback.TryGetValue(id, out var func))
            {
                throw new ArgumentException();
            }

            return new StoredProcedureSubstitute<TParam, TResult>(func);
        }

        public IStoredProcedure<TParam1, TParam2, TResult> StoredProcedure<TParam1, TParam2, TResult>(string id)
        {
            if (!_storedProcedureCallback.TryGetValue(id, out var func))
            {
                throw new ArgumentException();
            }

            return new StoredProcedureSubstitute<TParam1, TParam2, TResult>(func);
        }

        public IStoredProcedure<TParam1, TParam2, TParam3, TResult> StoredProcedure<TParam1, TParam2, TParam3, TResult>(string id)
        {
            if (!_storedProcedureCallback.TryGetValue(id, out var func))
            {
                throw new ArgumentException();
            }

            return new StoredProcedureSubstitute<TParam1, TParam2, TParam3, TResult>(func);
        }

        public IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TResult> StoredProcedure<TParam1, TParam2, TParam3, TParam4, TResult>(string id)
        {
            if (!_storedProcedureCallback.TryGetValue(id, out var func))
            {
                throw new ArgumentException();
            }

            return new StoredProcedureSubstitute<TParam1, TParam2, TParam3, TParam4, TResult>(func);
        }

        public IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TResult> StoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TResult>(string id)
        {
            if (!_storedProcedureCallback.TryGetValue(id, out var func))
            {
                throw new ArgumentException();
            }

            return new StoredProcedureSubstitute<TParam1, TParam2, TParam3, TParam4, TParam5, TResult>(func);
        }

        public IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TResult> StoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TResult>(string id)
        {
            if (!_storedProcedureCallback.TryGetValue(id, out var func))
            {
                throw new ArgumentException();
            }

            return new StoredProcedureSubstitute<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TResult>(func);
        }

        public IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TResult> StoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TResult>(string id)
        {
            if (!_storedProcedureCallback.TryGetValue(id, out var func))
            {
                throw new ArgumentException();
            }

            return new StoredProcedureSubstitute<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TResult>(func);
        }

        public IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TResult> StoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TResult>(string id)
        {
            if (!_storedProcedureCallback.TryGetValue(id, out var func))
            {
                throw new ArgumentException();
            }

            return new StoredProcedureSubstitute<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TResult>(func);
        }

        public IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TResult> StoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TResult>(string id)
        {
            if (!_storedProcedureCallback.TryGetValue(id, out var func))
            {
                throw new ArgumentException();
            }

            return new StoredProcedureSubstitute<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TResult>(func);
        }

        public IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TResult> StoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TResult>(string id)
        {
            if (!_storedProcedureCallback.TryGetValue(id, out var func))
            {
                throw new ArgumentException();
            }

            return new StoredProcedureSubstitute<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TResult>(func);
        }

        public IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TResult> StoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TResult>(string id)
        {
            if (!_storedProcedureCallback.TryGetValue(id, out var func))
            {
                throw new ArgumentException();
            }

            return new StoredProcedureSubstitute<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TResult>(func);
        }

        public IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TResult> StoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TResult>(string id)
        {
            if (!_storedProcedureCallback.TryGetValue(id, out var func))
            {
                throw new ArgumentException();
            }

            return new StoredProcedureSubstitute<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TResult>(func);
        }

        public IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TResult> StoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TResult>(string id)
        {
            if (!_storedProcedureCallback.TryGetValue(id, out var func))
            {
                throw new ArgumentException();
            }

            return new StoredProcedureSubstitute<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TResult>(func);
        }

        public IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TResult> StoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TResult>(string id)
        {
            if (!_storedProcedureCallback.TryGetValue(id, out var func))
            {
                throw new ArgumentException();
            }

            return new StoredProcedureSubstitute<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TResult>(func);
        }

        public IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TResult> StoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TResult>(string id)
        {
            if (!_storedProcedureCallback.TryGetValue(id, out var func))
            {
                throw new ArgumentException();
            }

            return new StoredProcedureSubstitute<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TResult>(func);
        }

        public IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TParam16, TResult> StoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TParam16, TResult>(string id)
        {
            if (!_storedProcedureCallback.TryGetValue(id, out var func))
            {
                throw new ArgumentException();
            }

            return new StoredProcedureSubstitute<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TParam16, TResult>(func);
        }

        internal void GenerateExceptionOnGetWhen(Predicate<DocumentId> predicate,
                                                 HttpStatusCode statusCode,
                                                 string message = default)
        {
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            _getExceptionConditions.Add(id => id is DocumentId && predicate((DocumentId)id) ? CreateDbException(statusCode, message) : default);
        }

        internal void GenerateExceptionOnGetWhen(Predicate<T> predicate,
                                                 HttpStatusCode statusCode,
                                                 string message = default)
        {
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            _getExceptionConditions.Add(entity => entity is T && predicate((T)entity) ? CreateDbException(statusCode, message) : default);
        }

        internal void ClearGenerateExceptionOnGet()
        {
            _getExceptionConditions.Clear();
        }

        internal void GenerateExceptionOnAddWhen(Predicate<T> predicate,
                                                 HttpStatusCode statusCode,
                                                 string message = default)
        {
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            _addExceptionConditions.Add(entity => predicate(entity) ? CreateDbException(statusCode, message) : default);
        }

        internal void ClearGenerateExceptionOnAdd()
        {
            _addExceptionConditions.Clear();
        }

        internal void GenerateExceptionOnDeleteWhen(Predicate<DocumentId> predicate,
                                                    HttpStatusCode statusCode,
                                                    string message = default)
        {
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            _deleteExceptionConditions.Add(id => id is DocumentId && predicate((DocumentId)id) ? CreateDbException(statusCode, message) : default);
        }

        internal void GenerateExceptionOnDeleteWhen(Predicate<T> predicate,
                                                    HttpStatusCode statusCode,
                                                    string message = default)
        {
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            _deleteExceptionConditions.Add(entity => entity is T && predicate((T)entity) ? CreateDbException(statusCode, message) : default);
        }

        internal void ClearGenerateExceptionOnDelete()
        {
            _deleteExceptionConditions.Clear();
        }

        internal void GenerateExceptionOnFindWhen(Func<bool> predicate,
                                                  HttpStatusCode statusCode,
                                                  string message = default)
        {
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            _findExceptionConditions.Add(() => predicate() ? CreateDbException(statusCode, message) : default);
        }

        internal void ClearGenerateExceptionOnFind()
        {
            _findExceptionConditions.Clear();
        }

        internal void GenerateExceptionOnFindFirstOrDefaultWhen(Func<bool> predicate,
                                                                HttpStatusCode statusCode,
                                                                string message = default)
        {
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            _findFirstOrDefaultExceptionConditions.Add(() => predicate() ? CreateDbException(statusCode, message) : default);
        }

        internal void ClearGenerateExceptionOnFindFirstOrDefault()
        {
            _findFirstOrDefaultExceptionConditions.Clear();
        }

        internal void GenerateExceptionOnReplaceWhen(Predicate<T> predicate,
                                                     HttpStatusCode statusCode,
                                                     string message = default)
        {
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            _replaceExceptionConditions.Add(entity => predicate(entity) ? CreateDbException(statusCode, message) : default);
        }

        internal void ClearGenerateExceptionOnReplace()
        {
            _replaceExceptionConditions.Clear();
        }

        internal void GenerateExceptionOnSelectWhen(Func<bool> predicate,
                                                    HttpStatusCode statusCode,
                                                    string message = default)
        {
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            _selectExceptionConditions.Add(() => predicate() ? CreateDbException(statusCode, message) : default);
        }

        internal void ClearGenerateExceptionOnSelect()
        {
            _selectExceptionConditions.Clear();
        }

        internal void GenerateExceptionOnSelectManyWhen(Func<bool> predicate,
                                                        HttpStatusCode statusCode,
                                                        string message = default)
        {
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            _selectManyExceptionConditions.Add(() => predicate() ? CreateDbException(statusCode, message) : default);
        }

        internal void ClearGenerateExceptionOnSelectMany()
        {
            _selectManyExceptionConditions.Clear();
        }

        internal void GenerateExceptionOnUpsertWhen(Predicate<T> predicate,
                                                     HttpStatusCode statusCode,
                                                     string message = default)
        {
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            _upsertExceptionConditions.Add(entity => predicate(entity) ? CreateDbException(statusCode, message) : default);
        }

        internal void ClearGenerateExceptionOnUpsert()
        {
            _upsertExceptionConditions.Clear();
        }

        internal void GenerateExceptionOnCountWhen(Func<bool> predicate,
                                                   HttpStatusCode statusCode,
                                                   string message = default)
        {
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            _countExceptionConditions.Add(() => predicate() ? CreateDbException(statusCode, message) : default);
        }

        internal void ClearGenerateExceptionOnCount()
        {
            _countExceptionConditions.Clear();
        }

        internal void SetStoredProcedureHandler(string id, Func<object[], object> func)
        {
            _storedProcedureCallback[id] = func;
        }

        private static T DeepClone(T src)
        {
            return (src == default)
                ? default
                : JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(src));
        }

        private class EntityStorage
        {
            private static readonly Action<T, string> SetETag;
            private static readonly Func<T, string> GetETag;
            private static readonly Func<T, DocumentId> GetId;
            private static readonly Action<T, DocumentId> SetId;
            private static readonly Func<T, long> GetTS;
            private static readonly Action<T, long> SetTS;

            public readonly T Entity;

            public DocumentId Id
            {
                get => GetId(Entity);
                set => SetId(Entity, value);
            }

            public string ETag
            {
                get => GetETag(Entity);
                set => SetETag(Entity, value);
            }

            public long TS
            {
                get => GetTS(Entity);
                set => SetTS(Entity, value);
            }

            static EntityStorage()
            {
                (string name, PropertyInfo info) GetPropertyJsonName(PropertyInfo pi)
                {
                    var jsonProperty = pi.GetCustomAttribute<JsonPropertyAttribute>();
                    return (jsonProperty?.PropertyName ?? pi.Name, pi);
                }

                var properties = typeof(T).GetProperties().Select(GetPropertyJsonName).ToDictionary(o => o.name, o => o.info);

                var idProperty = properties["id"];
                GetId = BuildIdGet(idProperty, true);
                SetId = BuildIdSet(idProperty, true);

                properties.TryGetValue("_etag", out var eTagProperty);

                GetETag = BuildETagGet(eTagProperty, false);
                SetETag = BuildETagSet(eTagProperty, false);

                properties.TryGetValue("_ts", out var tsProperty);

                GetTS = BuildTSGet(tsProperty);
                SetTS = BuildTSSet(tsProperty);
            }

            public EntityStorage(T entity)
            {
                Entity = DeepClone(entity);
            }

            private static Func<T, DocumentId> BuildIdGet(PropertyInfo idProperty, bool required)
            {
                if (idProperty == default)
                {
                    if (required)
                        throw new InvalidOperationException("Missing field");

                    return _ => default;
                }

                var source = Expression.Parameter(typeof(T), "source");
                Expression IdProperty = Expression.Property(source, idProperty);

                IdProperty = Expression.Convert(IdProperty, typeof(DocumentId));
                return Expression.Lambda<Func<T, DocumentId>>(IdProperty, source).Compile();
            }

            private static Action<T, DocumentId> BuildIdSet(PropertyInfo idProperty, bool required)
            {
                if (idProperty == default)
                {
                    if (required)
                        throw new InvalidOperationException("Missing field");

                    return (_, __) => { };
                }

                if (!idProperty.CanWrite)
                {
                    return (_, __) => throw new InvalidOperationException("The id property is not assignable");
                }

                var source = Expression.Parameter(typeof(T), "source");
                var value = Expression.Parameter(typeof(DocumentId), "value");

                Expression IdProperty = Expression.Property(source, idProperty);
                var body = Expression.Assign(IdProperty, Expression.Convert(value, idProperty.PropertyType));
                return Expression.Lambda<Action<T, DocumentId>>(body, source, value).Compile();
            }

            private static Func<T, string> BuildETagGet(PropertyInfo idProperty, bool required)
            {
                if (idProperty == default)
                {
                    if (required)
                        throw new InvalidOperationException("Missing field");

                    return _ => default;
                }

                var source = Expression.Parameter(typeof(T), "source");
                Expression IdProperty = Expression.Property(source, idProperty);

                if (idProperty.PropertyType != typeof(string))
                {
                    IdProperty = Expression.Call(IdProperty, "ToString", new Type[0]);
                }

                return Expression.Lambda<Func<T, string>>(IdProperty, source).Compile();
            }

            private static Action<T, string> BuildETagSet(PropertyInfo idProperty, bool required)
            {
                if (idProperty == default)
                {
                    if (required)
                        throw new InvalidOperationException("Missing field");

                    return (_, __) => { };
                }

                if (!idProperty.CanWrite)
                {
                    return (_, __) => throw new InvalidOperationException("The id property is not assignable");
                }

                var source = Expression.Parameter(typeof(T), "source");
                var value = Expression.Parameter(typeof(string), "value");


                Expression IdProperty = Expression.Property(source, idProperty);

                var body = idProperty.PropertyType != typeof(string)
                    ? Expression.Assign(IdProperty, Expression.Call(idProperty.PropertyType.GetMethod("Parse", new[] { typeof(string) }), value))
                    : Expression.Assign(IdProperty, value);

                return Expression.Lambda<Action<T, string>>(body, source, value).Compile();

            }

            private static Func<T, long> BuildTSGet(PropertyInfo idProperty)
            {
                if (idProperty == default)
                {
                    return _ => 0;
                }

                if (idProperty.PropertyType != typeof(long))
                {
                    throw new InvalidOperationException("_ts is not type long");
                }

                var source = Expression.Parameter(typeof(T), "source");
                Expression IdProperty = Expression.Property(source, idProperty);

                return Expression.Lambda<Func<T, long>>(IdProperty, source).Compile();
            }

            private static Action<T, long> BuildTSSet(PropertyInfo idProperty)
            {
                if (idProperty == default)
                {
                    return (_, __) => { };
                }

                if (!idProperty.CanWrite)
                {
                    return (_, __) => throw new InvalidOperationException("The id property is not assignable");
                }

                var source = Expression.Parameter(typeof(T), "source");
                var value = Expression.Parameter(typeof(long), "value");


                Expression IdProperty = Expression.Property(source, idProperty);

                var body = Expression.Assign(IdProperty, value);

                return Expression.Lambda<Action<T, long>>(body, source, value).Compile();
            }
        }

        private static DocumentClientException CreateDbException(HttpStatusCode statusCode, string message = default)
        {
            var ex = (DocumentClientException)FormatterServices.GetUninitializedObject(_dbExceptionType);
            _dbExceptionType.GetProperty("StatusCode").SetValue(ex, statusCode);
            _dbExceptionType.GetField("_message", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(ex, message);

            return ex;
        }

        private bool CheckETag(T item, EntityStorage entity, out DocumentClientException exception)
        {
            if (new EntityStorage(item).ETag != entity.ETag)
            {
                exception = CreateDbException(HttpStatusCode.PreconditionFailed, "ETag mismatch");
                return true;
            }

            exception = default;
            return false;
        }
    }
}

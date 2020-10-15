using CosmosDbRepository.Types;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace CosmosDbRepository.Implementation
{
    internal class CosmosDbRepository
    {
        internal static readonly Type[] IndirectlySupportedIndexTypes =
        {
            typeof(Guid)
        };
    }

    internal class CosmosDbRepository<T>
        : CosmosDbRepository
        , ICosmosDbRepository<T>
    {
        private readonly IDocumentClient _client;
        private readonly ICosmosDb _documentDb;
        private readonly IndexingPolicy _indexingPolicy;
        private readonly Func<T, object> _partionkeySelector;
        private readonly PartitionKeyDefinition _partitionkeyDefinition;
        private readonly FeedOptions _defaultFeedOptions;
        private readonly bool _hasPartionKey;
        private readonly int? _throughput;
        private readonly bool _createOnMissing;
        private readonly List<StoredProcedure> _storedProcedures;
        private AsyncLazy<DocumentCollection> _collection;
        private static readonly ConcurrentDictionary<Type, Func<object, (string id, string eTag)>> _idETagHelper = new ConcurrentDictionary<Type, Func<object, (string id, string eTag)>>();
        private readonly Func<Document, T> _deserializer;
        private string _polymorphicField;
        private Dictionary<string, Func<Document, T>> _polymorphicDeserializer;

        public string Id { get; }
        public Type Type => typeof(T);
        public Task<string> AltLink => GetAltLink();

        public CosmosDbRepository(IDocumentClient client,
                                  ICosmosDb documentDb,
                                  string id,
                                  IndexingPolicy indexingPolicy,
                                  Func<T, object> partionkeySelector,
                                  PartitionKeyDefinition partitionkeyDefinition,
                                  int? throughput,
                                  IEnumerable<StoredProcedure> storedProcedures,
                                  bool createOnMissing,
                                  string polymorphicField,
                                  (string Value, Type Type)[] polymorphicTypes)
        {
            _createOnMissing = createOnMissing;
            _documentDb = documentDb;
            _client = client;
            Id = id;
            _indexingPolicy = indexingPolicy;


            if (partionkeySelector != null)
            {
                var pks = partionkeySelector;

                partionkeySelector = (T t) =>
                {
                    object result = pks(t);

                    return IndirectlySupportedIndexTypes.Contains(result?.GetType()) == true
                        ? result.ToString()
                        : result;
                };
            }

            _partionkeySelector = partionkeySelector;
            _partitionkeyDefinition = partitionkeyDefinition;
            _throughput = throughput;
            _storedProcedures = new List<StoredProcedure>(storedProcedures);
            _hasPartionKey = partitionkeyDefinition?.Paths?.Any() == true;

            _defaultFeedOptions = new FeedOptions
            {
                EnableScanInQuery = true,
                EnableCrossPartitionQuery = !_hasPartionKey
            };

            _polymorphicField = polymorphicField;
            _polymorphicDeserializer = polymorphicTypes?.ToDictionary(vt => vt.Value, vt => MakeCustomDeserializer(vt.Type));

            _deserializer = _polymorphicField != default
                          ? (Func<Document, T>)CustomDeserializer
                          : CustomDeserializer<T>;

            _collection = new AsyncLazy<DocumentCollection>(() => GetOrCreateCollectionAsync(createOnMissing));
        }

        public async Task<T> AddAsync(T entity, RequestOptions requestOptions = null)
        {
            requestOptions = GetPartionKey(entity, requestOptions);

            var addedDoc = await _client.CreateDocumentAsync((await _collection).SelfLink, entity, requestOptions);
            return _deserializer(addedDoc.Resource);
        }

        public async Task<T> ReplaceAsync(T entity, RequestOptions requestOptions = null)
        {
            requestOptions = GetPartionKey(entity, requestOptions);

            (string id, string eTag) = GetIdAndETag(entity);

            if (eTag != null)
            {
                requestOptions = requestOptions?.ShallowCopy() ?? new RequestOptions();
                requestOptions.AccessCondition = new AccessCondition { Type = AccessConditionType.IfMatch, Condition = eTag };
            }

            var documentLink = $"{(await _collection).AltLink}/docs/{Uri.EscapeUriString(id)}";

            var response = await _client.ReplaceDocumentAsync(documentLink, entity, requestOptions);

            return (response.StatusCode == HttpStatusCode.NotModified)
                ? entity
                : _deserializer(response.Resource);
        }

        public async Task<T> UpsertAsync(T entity, RequestOptions requestOptions = null)
        {
            requestOptions = GetPartionKey(entity, requestOptions);

            (string id, string eTag) = GetIdAndETag(entity);

            if (eTag != null)
            {
                requestOptions = requestOptions?.ShallowCopy() ?? new RequestOptions();
                requestOptions.AccessCondition = new AccessCondition { Type = AccessConditionType.IfMatch, Condition = eTag };
            }

            var response = await _client.UpsertDocumentAsync((await _collection).SelfLink, entity, requestOptions);

            return _deserializer(response.Resource);
        }

        public async Task<IList<T>> FindAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IQueryable<T>> clauses = null, FeedOptions feedOptions = null)
        {
            CheckPartionKey(feedOptions);

            var query =
                _client.CreateDocumentQuery<T>((await _collection).SelfLink, feedOptions ?? _defaultFeedOptions)
                .ConditionalWhere(predicate)
                .ConditionalApplyClauses(clauses)
                .AsDocumentQuery();

            var results = new List<T>();

            while (query.HasMoreResults)
            {
                var response = await query.ExecuteNextAsync<Document>().ConfigureAwait(true);
                results.AddRange(response.Select(doc => _deserializer(doc)));
            }

            return results;
        }

        public async Task<CosmosDbRepositoryPagedResults<T>> FindAsync(int pageSize, string continuationToken, Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IQueryable<T>> clauses = null, FeedOptions feedOptions = null)
        {
            feedOptions = (feedOptions ?? _defaultFeedOptions).ShallowCopy();

            feedOptions.RequestContinuation = continuationToken;
            feedOptions.MaxItemCount = pageSize == 0 ? 10000 : pageSize;

            CheckPartionKey(feedOptions);

            var query =
                _client.CreateDocumentQuery<T>((await _collection).SelfLink, feedOptions)
                .ConditionalWhere(predicate)
                .ConditionalApplyClauses(clauses)
                .AsDocumentQuery();

            var result = new CosmosDbRepositoryPagedResults<T>();

            while (query.HasMoreResults)
            {
                var response = await query.ExecuteNextAsync<Document>().ConfigureAwait(true);
                result.Items.AddRange(response.Select(doc => _deserializer(doc)));

                if (pageSize > 0 && result.Items.Count >= pageSize)
                {
                    result.ContinuationToken = response.ResponseContinuation;
                    break;
                }
            }

            return result;
        }

        public async Task<IList<U>> SelectAsync<U>(string queryString, FeedOptions feedOptions = null)
        {
            CheckPartionKey(feedOptions);

            var query =
                _client.CreateDocumentQuery<U>((await _collection).SelfLink, queryString, feedOptions ?? _defaultFeedOptions)
                .AsDocumentQuery();

            var results = new List<U>();

            while (query.HasMoreResults)
            {
                var response = await query.ExecuteNextAsync<U>().ConfigureAwait(true);
                results.AddRange(response);
            }

            return results;
        }

        public async Task<IList<T>> SelectAsync(string queryString, FeedOptions feedOptions = null)
        {
            CheckPartionKey(feedOptions);

            var query =
                _client.CreateDocumentQuery<T>((await _collection).SelfLink, queryString, feedOptions ?? _defaultFeedOptions)
                .AsDocumentQuery();

            var results = new List<T>();

            while (query.HasMoreResults)
            {
                var response = await query.ExecuteNextAsync<Document>().ConfigureAwait(true);
                results.AddRange(response.Select(doc => _deserializer(doc)));
            }

            return results;
        }

        public async Task<CosmosDbRepositoryPagedResults<U>> SelectAsync<U>(int pageSize, string continuationToken, string queryString, FeedOptions feedOptions = null)
        {
            feedOptions = (feedOptions ?? _defaultFeedOptions).ShallowCopy();

            feedOptions.RequestContinuation = continuationToken;
            feedOptions.MaxItemCount = pageSize == 0 ? 10000 : pageSize;

            CheckPartionKey(feedOptions);

            var query =
                _client.CreateDocumentQuery<T>((await _collection).SelfLink, queryString, feedOptions)
                .AsDocumentQuery();

            var result = new CosmosDbRepositoryPagedResults<U>();

            while (query.HasMoreResults)
            {
                var response = await query.ExecuteNextAsync<U>().ConfigureAwait(true);
                result.Items.AddRange(response);

                if (pageSize > 0 && result.Items.Count >= pageSize)
                {
                    result.ContinuationToken = response.ResponseContinuation;
                    break;
                }
            }

            return result;
        }

        public async Task<CosmosDbRepositoryPagedResults<T>> SelectAsync(int pageSize, string continuationToken, string queryString, FeedOptions feedOptions = null)
        {
            feedOptions = (feedOptions ?? _defaultFeedOptions).ShallowCopy();

            feedOptions.RequestContinuation = continuationToken;
            feedOptions.MaxItemCount = pageSize == 0 ? 10000 : pageSize;

            CheckPartionKey(feedOptions);

            var query =
                _client.CreateDocumentQuery<T>((await _collection).SelfLink, queryString, feedOptions)
                .AsDocumentQuery();

            var result = new CosmosDbRepositoryPagedResults<T>();

            while (query.HasMoreResults)
            {
                var response = await query.ExecuteNextAsync<Document>().ConfigureAwait(true);
                result.Items.AddRange(response.Select(doc => _deserializer(doc)));

                if (pageSize > 0 && result.Items.Count >= pageSize)
                {
                    result.ContinuationToken = response.ResponseContinuation;
                    break;
                }
            }

            return result;
        }

        public async Task<IList<U>> SelectAsync<U>(Expression<Func<T, U>> selector, Func<IQueryable<U>, IQueryable<U>> selectClauses = null, FeedOptions feedOptions = null)
        {
            CheckPartionKey(feedOptions);

            var query =
                _client.CreateDocumentQuery<T>((await _collection).SelfLink, feedOptions ?? _defaultFeedOptions)
                .Select(selector)
                .ConditionalApplyClauses(selectClauses)
                .AsDocumentQuery();

            var results = new List<U>();

            while (query.HasMoreResults)
            {
                var response = await query.ExecuteNextAsync<U>().ConfigureAwait(true);
                results.AddRange(response);
            }

            return results;
        }

        public async Task<CosmosDbRepositoryPagedResults<U>> SelectAsync<U>(int pageSize, string continuationToken, Expression<Func<T, U>> selector, Func<IQueryable<U>, IQueryable<U>> selectClauses = null, FeedOptions feedOptions = null)
        {
            feedOptions = (feedOptions ?? _defaultFeedOptions).ShallowCopy();

            feedOptions.RequestContinuation = continuationToken;
            feedOptions.MaxItemCount = pageSize == 0 ? 10000 : pageSize;

            CheckPartionKey(feedOptions);

            var query =
                _client.CreateDocumentQuery<T>((await _collection).SelfLink, feedOptions)
                .Select(selector)
                .ConditionalApplyClauses(selectClauses)
                .AsDocumentQuery();

            var result = new CosmosDbRepositoryPagedResults<U>();

            while (query.HasMoreResults)
            {
                var response = await query.ExecuteNextAsync<U>().ConfigureAwait(true);
                result.Items.AddRange(response);

                if (pageSize > 0 && result.Items.Count >= pageSize)
                {
                    result.ContinuationToken = response.ResponseContinuation;
                    break;
                }
            }

            return result;
        }

        public async Task<IList<U>> SelectAsync<U, V>(Expression<Func<V, U>> selector, Func<IQueryable<T>, IQueryable<V>> whereClauses, Func<IQueryable<U>, IQueryable<U>> selectClauses = null, FeedOptions feedOptions = null)
        {
            CheckPartionKey(feedOptions);

            var query =
                _client.CreateDocumentQuery<T>((await _collection).SelfLink, feedOptions ?? _defaultFeedOptions)
                .ApplyClauses(whereClauses)
                .Select(selector)
                .ConditionalApplyClauses(selectClauses)
                .AsDocumentQuery();

            var results = new List<U>();

            while (query.HasMoreResults)
            {
                var response = await query.ExecuteNextAsync<U>().ConfigureAwait(true);
                results.AddRange(response);
            }

            return results;
        }

        public async Task<CosmosDbRepositoryPagedResults<U>> SelectAsync<U, V>(int pageSize, string continuationToken, Expression<Func<V, U>> selector, Func<IQueryable<T>, IQueryable<V>> whereClauses, Func<IQueryable<U>, IQueryable<U>> selectClauses = null, FeedOptions feedOptions = null)
        {
            feedOptions = (feedOptions ?? _defaultFeedOptions).ShallowCopy();

            feedOptions.RequestContinuation = continuationToken;
            feedOptions.MaxItemCount = pageSize == 0 ? 10000 : pageSize;

            CheckPartionKey(feedOptions);

            var query =
                _client.CreateDocumentQuery<T>((await _collection).SelfLink, feedOptions)
                .ApplyClauses(whereClauses)
                .Select(selector)
                .ConditionalApplyClauses(selectClauses)
                .AsDocumentQuery();

            var result = new CosmosDbRepositoryPagedResults<U>();

            while (query.HasMoreResults)
            {
                var response = await query.ExecuteNextAsync<U>().ConfigureAwait(true);
                result.Items.AddRange(response);

                if (pageSize > 0 && result.Items.Count >= pageSize)
                {
                    result.ContinuationToken = response.ResponseContinuation;
                    break;
                }
            }

            return result;
        }

        public async Task<IList<TResult>> SelectManyAsync<TResult>(Expression<Func<T, IEnumerable<TResult>>> selector, Func<IQueryable<T>, IQueryable<T>> whereClauses = null, Func<IQueryable<TResult>, IQueryable<TResult>> selectClauses = null, FeedOptions feedOptions = null)
        {
            feedOptions = feedOptions ?? _defaultFeedOptions;

            CheckPartionKey(feedOptions);

            var query =
                _client.CreateDocumentQuery<T>((await _collection).SelfLink, feedOptions)
                .ConditionalApplyClauses(whereClauses)
                .SelectMany(selector)
                .ConditionalApplyClauses(selectClauses)
                .AsDocumentQuery();

            var results = new List<TResult>();

            while (query.HasMoreResults)
            {
                var response = await query.ExecuteNextAsync<TResult>().ConfigureAwait(true);
                results.AddRange(response);
            }

            return results;
        }


        public async Task<CosmosDbRepositoryPagedResults<TResult>> SelectManyAsync<TResult>(int pageSize, string continuationToken, Expression<Func<T, IEnumerable<TResult>>> selector, Func<IQueryable<T>, IQueryable<T>> whereClauses = null, Func<IQueryable<TResult>, IQueryable<TResult>> selectClauses = null, FeedOptions feedOptions = null)
        {
            feedOptions = (feedOptions ?? _defaultFeedOptions).ShallowCopy();
            feedOptions.RequestContinuation = continuationToken;
            feedOptions.MaxItemCount = pageSize == 0 ? 10000 : pageSize;

            CheckPartionKey(feedOptions);

            var query =
                _client.CreateDocumentQuery<T>((await _collection).SelfLink, feedOptions)
                .ConditionalApplyClauses(whereClauses)
                .SelectMany(selector)
                .ConditionalApplyClauses(selectClauses)
                .AsDocumentQuery();

            var result = new CosmosDbRepositoryPagedResults<TResult>();

            while (query.HasMoreResults)
            {
                var response = await query.ExecuteNextAsync<TResult>().ConfigureAwait(true);

                result.Items.AddRange(response);

                if (pageSize > 0 && result.Items.Count >= pageSize)
                {
                    result.ContinuationToken = response.ResponseContinuation;
                    break;
                }
            }

            return result;
        }


        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IQueryable<T>> clauses = null, FeedOptions feedOptions = null)
        {
            CheckPartionKey(feedOptions);

            var query = _client.CreateDocumentQuery<T>((await _collection).SelfLink, feedOptions ?? _defaultFeedOptions)
                .ConditionalWhere(predicate)
                .ConditionalApplyClauses(clauses)
                .Select(r => 1)
                .AsDocumentQuery();

            var result = 0;

            while (query.HasMoreResults)
            {
                var response = await query.ExecuteNextAsync<int>().ConfigureAwait(true);
                result += response.Count();
            }

            return result;
        }

        public async Task<T> FindFirstOrDefaultAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IQueryable<T>> clauses = null, FeedOptions feedOptions = null)
        {
            //feedOptions = (feedOptions ?? _defaultFeedOptions).ShallowCopy();
            //feedOptions.MaxItemCount = 1;

            CheckPartionKey(feedOptions);

            var query =
                _client.CreateDocumentQuery<T>((await _collection).SelfLink, feedOptions)
                .ConditionalWhere(predicate)
                .ConditionalApplyClauses(clauses)
                .AsDocumentQuery();

            T result = default(T);

            if (query.HasMoreResults)
            {
                var response = await query.ExecuteNextAsync<Document>().ConfigureAwait(true);

                while (response.Count == 0 && !string.IsNullOrEmpty(response.ResponseContinuation))
                {
                    response = await query.ExecuteNextAsync<Document>();
                }

                var doc = response.FirstOrDefault();
                result = doc != default
                    ? _deserializer(doc)
                    : default;
            }

            return result;
        }

        public async Task<T> GetAsync(T entity, RequestOptions requestOptions = null)
        {
            (string id, string eTag) = GetIdAndETag(entity);

            if (eTag != null)
            {
                requestOptions = requestOptions ?? new RequestOptions();
                requestOptions.AccessCondition = new AccessCondition { Type = AccessConditionType.IfNoneMatch, Condition = eTag };
            }

            var documentLink = $"{(await _collection).AltLink}/docs/{Uri.EscapeUriString(id)}";
            T result;

            requestOptions = GetPartionKey(entity, requestOptions);

            try
            {
                var response = await _client.ReadDocumentAsync<T>(documentLink, requestOptions);

                result = (response.StatusCode == HttpStatusCode.NotModified)
                    ? entity
                    : response.Document;
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode != HttpStatusCode.NotFound)
                    throw;

                result = default(T);
            }

            return result;
        }


        public async Task<T> GetAsync(DocumentId itemId, RequestOptions requestOptions = null)
        {
            CheckPartionKey(requestOptions);

            var documentLink = $"{(await _collection).AltLink}/docs/{Uri.EscapeUriString(itemId.Id)}";
            T result;

            try
            {
                var response = await _client.ReadDocumentAsync<T>(documentLink, requestOptions);
                result = response.Document;
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode != HttpStatusCode.NotFound)
                    throw;

                result = default(T);
            }

            return result;
        }

        public async Task<bool> DeleteDocumentAsync(DocumentId itemId, RequestOptions requestOptions = null)
        {
            var documentLink = $"{(await _collection).AltLink}/docs/{Uri.EscapeUriString(itemId.Id)}";
            CheckPartionKey(requestOptions);

            var response = await _client.DeleteDocumentAsync(documentLink, requestOptions);
            return response.StatusCode == HttpStatusCode.NoContent;

        }

        public async Task<bool> DeleteDocumentAsync(T entity, RequestOptions requestOptions = null)
        {
            (string id, string eTag) = GetIdAndETag(entity);

            if (eTag != null)
            {
                requestOptions = (requestOptions ?? new RequestOptions()).ShallowCopy();
                requestOptions.AccessCondition = new AccessCondition { Type = AccessConditionType.IfMatch, Condition = eTag };
            }

            var documentLink = $"{(await _collection).AltLink}/docs/{Uri.EscapeUriString(id)}";

            requestOptions = GetPartionKey(entity, requestOptions);

            var response = await _client.DeleteDocumentAsync(documentLink, requestOptions);
            return response.StatusCode == HttpStatusCode.NoContent;
        }

        public async Task<bool> DeleteAsync(RequestOptions requestOptions = null)
        {
            var response = await _client.DeleteDocumentCollectionAsync((await _collection).SelfLink, requestOptions);
            _collection = new AsyncLazy<DocumentCollection>(async () => await GetOrCreateCollectionAsync(_createOnMissing));

            return response.StatusCode == HttpStatusCode.NoContent;
        }

        public Task Init() => _collection.Value;

        public IStoredProcedure<TResult> StoredProcedure<TResult>(string id) => new StoreProcedureImpl<TResult>(_client, this, id);
        public IStoredProcedure<TParam,TResult> StoredProcedure<TParam,TResult>(string id) => new StoreProcedureImpl<TParam,TResult>(_client, this, id);
        public IStoredProcedure<TParam1, TParam2, TResult> StoredProcedure<TParam1, TParam2, TResult>(string id) => new StoreProcedureImpl<TParam1, TParam2, TResult>(_client, this, id);
        public IStoredProcedure<TParam1, TParam2, TParam3, TResult> StoredProcedure<TParam1, TParam2, TParam3,TResult>(string id) => new StoreProcedureImpl<TParam1, TParam2, TParam3, TResult>(_client, this, id);
        public IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TResult> StoredProcedure<TParam1, TParam2, TParam3, TParam4, TResult>(string id) => new StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TResult>(_client, this, id);
        public IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TResult> StoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TResult>(string id) => new StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TResult>(_client, this, id);
        public IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TResult> StoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TResult>(string id) => new StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TResult>(_client, this, id);
        public IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TResult> StoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TResult>(string id) => new StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TResult>(_client, this, id);
        public IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TResult> StoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TResult>(string id) => new StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TResult>(_client, this, id);
        public IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TResult> StoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TResult>(string id) => new StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TResult>(_client, this, id);
        public IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TResult> StoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TResult>(string id) => new StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TResult>(_client, this, id);
        public IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TResult> StoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TResult>(string id) => new StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TResult>(_client, this, id);
        public IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TResult> StoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TResult>(string id) => new StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TResult>(_client, this, id);
        public IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TResult> StoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TResult>(string id) => new StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TResult>(_client, this, id);
        public IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TResult> StoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TResult>(string id) => new StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TResult>(_client, this, id);
        public IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TResult> StoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TResult>(string id) => new StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TResult>(_client, this, id);
        public IStoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TParam16, TResult> StoredProcedure<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TParam16, TResult>(string id) => new StoreProcedureImpl<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8, TParam9, TParam10, TParam11, TParam12, TParam13, TParam14, TParam15, TParam16, TResult>(_client, this, id);


        private async Task<DocumentCollection> GetOrCreateCollectionAsync(bool createOnMissing)
        {
            var documentCollection = new DocumentCollection { Id = Id, IndexingPolicy = _indexingPolicy };

            if (_partitionkeyDefinition != null)
            {
                documentCollection.PartitionKey = _partitionkeyDefinition;
            }

            var resourceResponse =
                createOnMissing
                ? await _client.CreateDocumentCollectionIfNotExistsAsync(
                    await _documentDb.SelfLinkAsync,
                    documentCollection,
                    new RequestOptions { OfferThroughput = _throughput })
                : await _client.ReadDocumentCollectionAsync($"{await _documentDb.AltLinkAsync}/colls/{Uri.EscapeUriString(Id)}");

            if (_storedProcedures.Any())
            {
                var sps = (await _client.ReadStoredProcedureFeedAsync(resourceResponse.Resource.StoredProceduresLink)).ToArray();

                foreach(var sp in _storedProcedures.Where(sp => sps.FirstOrDefault(p => p.Id == sp.Id)?.Body != sp.Body))
                {
                    var updatedSp  = sps.FirstOrDefault(p => p.Id == sp.Id);

                    if (updatedSp == null)
                    {
                        await _client.CreateStoredProcedureAsync(resourceResponse.Resource.AltLink, sp);
                    }
                    else
                    {
                        updatedSp.Body = sp.Body;
                        await _client.ReplaceStoredProcedureAsync(updatedSp);
                    }                 
                }
            }

            return resourceResponse;
        }

        private (string id, string eTag) GetIdAndETag(T entity)
        {
            Func<object, (string id, string propertyInfo)> Factory(Type type)
            {
                (string name, PropertyInfo info) GetPropertyJsonName(PropertyInfo pi)
                {
                    var jsonProperty = pi.GetCustomAttribute<JsonPropertyAttribute>();
                    return (jsonProperty?.PropertyName ?? pi.Name, pi);
                }

                var properties = type.GetProperties().Select(GetPropertyJsonName).ToDictionary(o => o.name, o => o.info);

                var idProperty = properties["id"];
                properties.TryGetValue("_etag", out var eTagProperty);

                var source = Expression.Parameter(typeof(object), "src");
                var typedSource = Expression.Variable(type, "source");

                Expression idValue = Expression.Property(typedSource, idProperty);

                if (idProperty.PropertyType != typeof(string))
                {
                    idValue = Expression.Call(idValue, "ToString", new Type[0]);
                }

                Expression eTagValue = eTagProperty == null
                    ? Expression.Constant(null, typeof(string))
                    : (Expression)Expression.Property(typedSource, eTagProperty);

                if (eTagProperty != default && eTagProperty.PropertyType != typeof(string))
                {
                    eTagValue = Expression.Call(eTagValue, "ToString", new Type[0]);
                }

                var newTuple = Expression.New(typeof((string, string)).GetConstructor(new[] { typeof(string), typeof(string) }), idValue, eTagValue);
                var body = Expression.Block(new[] { typedSource }, Expression.Assign(typedSource, Expression.Convert(source, type)), newTuple);

                return Expression.Lambda<Func<object, (string, string)>>(body, source).Compile();
            }

            return _idETagHelper.GetOrAdd(entity.GetType(), Factory)(entity);
        }

        private async Task<String> GetAltLink()
        {
            return (await _collection).AltLink;
        }

        private RequestOptions GetPartionKey(T entity, RequestOptions requestOptions)
        {
            if (_hasPartionKey && requestOptions?.PartitionKey == null)
            {
                if (_partionkeySelector == null)
                {
                    throw new InvalidOperationException("PartitionkeySelector must be specified");
                }

                requestOptions = requestOptions.ShallowCopy() ?? new RequestOptions();
                requestOptions.PartitionKey = new PartitionKey(_partionkeySelector(entity));
            }

            return requestOptions;
        }

        private void CheckPartionKey(RequestOptions requestOptions)
        {
            if (_hasPartionKey && requestOptions?.PartitionKey == null)
            {
                throw new InvalidOperationException("PartitionKey must be specified");
            }
        }

        private void CheckPartionKey(FeedOptions feedOptions)
        {
            if (_hasPartionKey && feedOptions?.EnableCrossPartitionQuery != true && feedOptions?.PartitionKey == null)
            {
                throw new InvalidOperationException("PartitionKey must be specified");
            }
        }

        private static Func<Document, T> MakeCustomDeserializer(Type type)
        {
            var deserializer = typeof(CosmosDbRepository<T>)
                .GetMethod(nameof(CustomDeserializer), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(type);

            // The func takes an int
            var arg = Expression.Parameter(typeof(Document));

            // Represent the call out to "AnotherFunc"
            var call = Expression.Call(null, deserializer, arg);

            // Build the action to just make the call to "AnotherFunc"
            var action = Expression.Lambda<Func<Document, T>>(call, arg);

            // Compile the chain and send it out
            return action.Compile();
        }

        private T CustomDeserializer(Document document)
        {
            var selector = document.GetPropertyValue<string>(_polymorphicField);
            return _polymorphicDeserializer[selector](document);
        }

        private static T CustomDeserializer<TModel>(Document document)
            where TModel : T
        {
            return JsonConvert.DeserializeObject<TModel>(document.ToString());
        }
    }
}
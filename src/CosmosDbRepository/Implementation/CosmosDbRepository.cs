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
    internal class CosmosDbRepository<T>
        : ICosmosDbRepository<T>
    {
        private readonly IDocumentClient _client;
        private readonly ICosmosDb _documentDb;
        private readonly IndexingPolicy _indexingPolicy;
        private readonly FeedOptions _defaultFeedOptions;
        private readonly int? _throughput;
        private readonly List<StoredProcedure> _storedProcedures;
        private AsyncLazy<DocumentCollection> _collection;
        private static readonly ConcurrentDictionary<Type, Func<object, (string id, string eTag)>> _idETagHelper = new ConcurrentDictionary<Type, Func<object, (string id, string eTag)>>();

        public string Id { get; }
        public Type Type => typeof(T);
        public Task<string> AltLink => GetAltLink();

        public CosmosDbRepository(IDocumentClient client,
                                  ICosmosDb documentDb,
                                  string id,
                                  IndexingPolicy indexingPolicy,
                                  int? throughput,
                                  IEnumerable<StoredProcedure> storedProcedures)
        {
            _documentDb = documentDb;
            _client = client;
            Id = id;
            _indexingPolicy = indexingPolicy;
            _throughput = throughput;
            _storedProcedures = new List<StoredProcedure>(storedProcedures);

            _defaultFeedOptions = new FeedOptions
            {
                EnableScanInQuery = true,
                EnableCrossPartitionQuery = true
            };

            _collection = new AsyncLazy<DocumentCollection>(() => GetOrCreateCollectionAsync());
        }

        public async Task<T> AddAsync(T entity, RequestOptions requestOptions = null)
        {
            var addedDoc = await _client.CreateDocumentAsync((await _collection).SelfLink, entity, requestOptions);
            return JsonConvert.DeserializeObject<T>(addedDoc.Resource.ToString());
        }

        public async Task<T> ReplaceAsync(T entity, RequestOptions requestOptions = null)
        {
            (string id, string eTag) = GetIdAndETag(entity);

            if (eTag != null)
            {
                requestOptions = requestOptions ?? new RequestOptions();
                requestOptions.AccessCondition = new AccessCondition { Type = AccessConditionType.IfMatch, Condition = eTag };
            }

            var documentLink = $"{(await _collection).AltLink}/docs/{Uri.EscapeUriString(id)}";

            var response = await _client.ReplaceDocumentAsync(documentLink, entity, requestOptions);

            return (response.StatusCode == HttpStatusCode.NotModified)
                ? entity
                : JsonConvert.DeserializeObject<T>(response.Resource.ToString());
        }

        public async Task<T> UpsertAsync(T entity, RequestOptions requestOptions = null)
        {
            (string id, string eTag) = GetIdAndETag(entity);

            if (eTag != null)
            {
                requestOptions = requestOptions ?? new RequestOptions();
                requestOptions.AccessCondition = new AccessCondition { Type = AccessConditionType.IfMatch, Condition = eTag };
            }

            var response = await _client.UpsertDocumentAsync((await _collection).SelfLink, entity, requestOptions);

            return JsonConvert.DeserializeObject<T>(response.Resource.ToString());
        }

        public async Task<IList<TOut>> FindAsync<TOut>(string sql, FeedOptions feedOptions = null)
        {
            var query = _client
                .CreateDocumentQuery<TOut>((await _collection).SelfLink, sql, feedOptions ?? _defaultFeedOptions)
                .AsDocumentQuery();

            var results = new List<TOut>();
            while (query.HasMoreResults)
            {
                var response = await query.ExecuteNextAsync<TOut>().ConfigureAwait(true);
                results.AddRange(response);
            }

            return results;
        }

        public async Task<CosmosDbRepositoryPagedResults<TOut>> FindAsync<TOut>(int pageSize, string continuationToken, string sql, FeedOptions feedOptions = null)
        {
            feedOptions = (feedOptions ?? _defaultFeedOptions).ShallowCopy();

            feedOptions.RequestContinuation = continuationToken;
            feedOptions.MaxItemCount = pageSize == 0 ? 10000 : pageSize;

            var query = _client
                .CreateDocumentQuery<TOut>((await _collection).SelfLink, sql, feedOptions)
                .AsDocumentQuery();

            var results = new CosmosDbRepositoryPagedResults<TOut>();

            while (query.HasMoreResults)
            {
                var response = await query.ExecuteNextAsync<TOut>().ConfigureAwait(true);
                results.Items.AddRange(response);

                if (pageSize > 0 && results.Items.Count >= pageSize)
                {
                    results.ContinuationToken = response.ResponseContinuation;
                    break;
                }
            }

            return results;
        }

        public async Task<IList<T>> FindAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IQueryable<T>> clauses = null, FeedOptions feedOptions = null)
        {
            var query =
                _client.CreateDocumentQuery<T>((await _collection).SelfLink, feedOptions ?? _defaultFeedOptions)
                .ConditionalWhere(predicate)
                .ApplyClauses(clauses)
                .AsDocumentQuery();

            var results = new List<T>();

            while (query.HasMoreResults)
            {
                var response = await query.ExecuteNextAsync<T>().ConfigureAwait(true);
                results.AddRange(response);
            }

            return results;
        }

        public async Task<CosmosDbRepositoryPagedResults<T>> FindAsync(int pageSize, string continuationToken, Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IQueryable<T>> clauses = null, FeedOptions feedOptions = null)
        {
            feedOptions = (feedOptions ?? _defaultFeedOptions).ShallowCopy();

            feedOptions.RequestContinuation = continuationToken;
            feedOptions.MaxItemCount = pageSize == 0 ? 10000 : pageSize;

            var query =
                _client.CreateDocumentQuery<T>((await _collection).SelfLink, feedOptions)
                .ConditionalWhere(predicate)
                .ApplyClauses(clauses)
                .AsDocumentQuery();

            var result = new CosmosDbRepositoryPagedResults<T>();

            while (query.HasMoreResults)
            {
                var response = await query.ExecuteNextAsync<T>().ConfigureAwait(true);
                result.Items.AddRange(response);

                if (pageSize > 0 && result.Items.Count >= pageSize)
                {
                    result.ContinuationToken = response.ResponseContinuation;
                    break;
                }
            }

            return result;
        }

        public async Task<IList<U>> SelectAsync<U>(Expression<Func<T, U>> selector, Func<IQueryable<T>, IQueryable<T>> whereClauses = null, Func<IQueryable<U>, IQueryable<U>> selectClauses = null, FeedOptions feedOptions = null)
        {
            var query =
                _client.CreateDocumentQuery<T>((await _collection).SelfLink, feedOptions ?? _defaultFeedOptions)
                .ApplyClauses(whereClauses)
                .Select(selector)
                .ApplyClauses(selectClauses)
                .AsDocumentQuery();

            var results = new List<U>();

            while (query.HasMoreResults)
            {
                var response = await query.ExecuteNextAsync<U>().ConfigureAwait(true);
                results.AddRange(response);
            }

            return results;
        }

        public async Task<CosmosDbRepositoryPagedResults<U>> SelectAsync<U>(int pageSize, string continuationToken, Expression<Func<T, U>> selector, Func<IQueryable<T>, IQueryable<T>> whereClauses = null, Func<IQueryable<U>, IQueryable<U>> selectClauses = null, FeedOptions feedOptions = null)
        {
            feedOptions = (feedOptions ?? _defaultFeedOptions).ShallowCopy();

            feedOptions.RequestContinuation = continuationToken;
            feedOptions.MaxItemCount = pageSize == 0 ? 10000 : pageSize;

            var query =
                _client.CreateDocumentQuery<T>((await _collection).SelfLink, feedOptions)
                .ApplyClauses(whereClauses)
                .Select(selector)
                .ApplyClauses(selectClauses)
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

        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IQueryable<T>> clauses = null, FeedOptions feedOptions = null)
        {
            return await _client.CreateDocumentQuery<T>((await _collection).SelfLink, feedOptions ?? _defaultFeedOptions)
                .ConditionalWhere(predicate)
                .ApplyClauses(clauses)
                .CountAsync();
        }

        public async Task<T> FindFirstOrDefaultAsync(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IQueryable<T>> clauses = null, FeedOptions feedOptions = null)
        {
            feedOptions = (feedOptions ?? _defaultFeedOptions).ShallowCopy();
            feedOptions.MaxItemCount = 1;

            var query =
                _client.CreateDocumentQuery<T>((await _collection).SelfLink, feedOptions)
                .ConditionalWhere(predicate)
                .ApplyClauses(clauses)
                .AsDocumentQuery();

            T result = default(T);

            if (query.HasMoreResults)
            {
                var response = await query.ExecuteNextAsync<T>().ConfigureAwait(true);
                result = response.FirstOrDefault();
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

            var response = await _client.DeleteDocumentAsync(documentLink, requestOptions);
            return response.StatusCode == HttpStatusCode.NoContent;
        }

        public async Task<bool> DeleteAsync(RequestOptions requestOptions = null)
        {
            var response = await _client.DeleteDocumentCollectionAsync((await _collection).SelfLink, requestOptions);
            _collection = new AsyncLazy<DocumentCollection>(async () => await GetOrCreateCollectionAsync());

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


        private async Task<DocumentCollection> GetOrCreateCollectionAsync()
        {
            var resourceResponse = await _client.CreateDocumentCollectionIfNotExistsAsync(
                await _documentDb.SelfLinkAsync,
                new DocumentCollection { Id = Id, IndexingPolicy = _indexingPolicy },
                new RequestOptions { OfferThroughput = _throughput });

            if (_storedProcedures.Any())
            {
                var sps = (await _client.ReadStoredProcedureFeedAsync(resourceResponse.Resource.StoredProceduresLink)).ToArray();

                foreach(var sp in _storedProcedures.Where(sp => sps.FirstOrDefault(p => p.Id == sp.Id)?.Body != sp.Body))
                {
                    await _client.UpsertStoredProcedureAsync(resourceResponse.Resource.AltLink, sp);
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

                if (eTagProperty.PropertyType != typeof(string))
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
    }
}
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
        private AsyncLazy<DocumentCollection> _collection;
        private static readonly ConcurrentDictionary<Type, Func<object, (string id, string eTag)>> _idETagHelper = new ConcurrentDictionary<Type, Func<object, (string id, string eTag)>>();

        public string Id { get; }
        public Type Type => typeof(T);

        public CosmosDbRepository(IDocumentClient client, ICosmosDb documentDb, string id, IndexingPolicy indexingPolicy)
        {
            _documentDb = documentDb;
            _client = client;
            Id = id;
            _indexingPolicy = indexingPolicy;

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
                requestOptions = requestOptions?.ShallowCopy() ?? new RequestOptions();
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
                requestOptions = requestOptions?.ShallowCopy() ?? new RequestOptions();
                requestOptions.AccessCondition = new AccessCondition { Type = AccessConditionType.IfMatch, Condition = eTag };
            }

            var response = await _client.UpsertDocumentAsync((await _collection).SelfLink, entity, requestOptions);

            return JsonConvert.DeserializeObject<T>(response.Resource.ToString());
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate = null, FeedOptions feedOptions = null)
        {
            var query =
                _client.CreateDocumentQuery<T>((await _collection).SelfLink, feedOptions ?? _defaultFeedOptions)
                .ConditionalWhere(() => predicate != null, predicate)
                .Skip(1)
                .Take(100)
                .AsDocumentQuery();

            var result = new List<T>();

            while (query.HasMoreResults)
            {
                var response = await query.ExecuteNextAsync<T>().ConfigureAwait(true);
                result.AddRange(response);
            }

            return result;
        }

        public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate = null, FeedOptions feedOptions = null)
        {
            feedOptions = (feedOptions ?? _defaultFeedOptions).ShallowCopy();
            feedOptions.MaxItemCount = 1;

            var query =
                _client.CreateDocumentQuery<T>((await _collection).SelfLink, feedOptions)
                .ConditionalWhere(() => predicate != null, predicate)
                .AsDocumentQuery();

            T result = default(T);

            while (query.HasMoreResults)
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
                requestOptions = (requestOptions?.ShallowCopy() ?? new RequestOptions());
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

        public async Task<bool> RemoveAsync(DocumentId itemId, RequestOptions requestOptions = null)
        {
            var documentLink = $"{(await _collection).AltLink}/docs/{Uri.EscapeUriString(itemId.Id)}";

            var response = await _client.DeleteDocumentAsync(documentLink, requestOptions);
            return response.StatusCode == HttpStatusCode.NoContent;

        }

        public async Task<bool> RemoveAsync(T entity, RequestOptions requestOptions = null)
        {
            (string id, string eTag) = GetIdAndETag(entity);

            if (eTag != null)
            {
                requestOptions = (requestOptions?.ShallowCopy() ?? new RequestOptions());
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

        private async Task<DocumentCollection> GetOrCreateCollectionAsync()
        {
            return await _client.CreateDocumentCollectionIfNotExistsAsync(await _documentDb.SelfLinkAsync, new DocumentCollection { Id = Id, IndexingPolicy = _indexingPolicy });
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
    }
}
using CosmosDbRepository.Implementation;
using CosmosDbRepository.Types;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CosmosDbRepository
{
    public static class ICosmosDbRepositoryExtention
    {
        public static Task<T> AddAsync<T,U>(this ICosmosDbRepository<T> repo, U partitionKey, T entity, RequestOptions requestOptions = null)
        {
            requestOptions = SetPartitionKey(partitionKey, requestOptions);
            return repo.AddAsync(entity, requestOptions);
        }

        public static Task<T> ReplaceAsync<T, U>(this ICosmosDbRepository<T> repo, U partitionKey, T entity, RequestOptions requestOptions = null)
        {
            requestOptions = SetPartitionKey(partitionKey, requestOptions);
            return repo.ReplaceAsync(entity, requestOptions);
        }

        public static Task<T> UpsertAsync<T, U>(this ICosmosDbRepository<T> repo, U partitionKey, T entity, RequestOptions requestOptions = null)
        {
            requestOptions = SetPartitionKey(partitionKey, requestOptions);
            return repo.ReplaceAsync(entity, requestOptions);
        }

        public static Task<IList<T>> FindAsync<T, U>(this ICosmosDbRepository<T> repo, U partitionKey, Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IQueryable<T>> clauses = null, FeedOptions feedOptions = null)
        {
            feedOptions = SetPartitionKey(partitionKey, feedOptions);
            return repo.FindAsync(predicate, clauses, feedOptions);
        }

        public static Task<CosmosDbRepositoryPagedResults<T>> FindAsync<T, U>(this ICosmosDbRepository<T> repo, U partitionKey, int pageSize, string continuationToken, Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IQueryable<T>> clauses = null, FeedOptions feedOptions = null)
        {
            feedOptions = SetPartitionKey(partitionKey, feedOptions);
            return repo.FindAsync(pageSize, continuationToken, predicate, clauses, feedOptions);
        }

        public static Task<IList<U>> SelectAsync<T, U, V>(this ICosmosDbRepository<T> repo, V partitionKey, Expression<Func<T, U>> selector, Func<IQueryable<U>, IQueryable<U>> selectClauses = null, FeedOptions feedOptions = null)
        {
            feedOptions = SetPartitionKey(partitionKey, feedOptions);
            return repo.SelectAsync(selector, selectClauses, feedOptions);
        }

        public static Task<CosmosDbRepositoryPagedResults<U>> SelectAsync<T, U, V>(this ICosmosDbRepository<T> repo, V partitionKey, int pageSize, string continuationToken, Expression<Func<T, U>> selector, Func<IQueryable<U>, IQueryable<U>> selectClauses = null, FeedOptions feedOptions = null)
        {
            feedOptions = SetPartitionKey(partitionKey, feedOptions);
            return repo.SelectAsync(pageSize, continuationToken, selector, selectClauses, feedOptions);
        }

        public static Task<IList<U>> SelectAsync<T, U, V, W>(this ICosmosDbRepository<T> repo, W partitionKey, Expression<Func<V, U>> selector, Func<IQueryable<T>, IQueryable<V>> whereClauses = null, Func<IQueryable<U>, IQueryable<U>> selectClauses = null, FeedOptions feedOptions = null)
        {
            feedOptions = SetPartitionKey(partitionKey, feedOptions);
            return repo.SelectAsync(selector, whereClauses, selectClauses, feedOptions);
        }

        public static Task<CosmosDbRepositoryPagedResults<U>> SelectAsync<T, U, V, W>(this ICosmosDbRepository<T> repo, W partitionKey, int pageSize, string continuationToken, Expression<Func<V, U>> selector, Func<IQueryable<T>, IQueryable<V>> whereClauses, Func<IQueryable<U>, IQueryable<U>> selectClauses = null, FeedOptions feedOptions = null)
        {
            feedOptions = SetPartitionKey(partitionKey, feedOptions);
            return repo.SelectAsync(pageSize, continuationToken, selector, whereClauses, selectClauses, feedOptions);
        }
        public static Task<IList<U>> SelectManyAsync<T, U, V>(this ICosmosDbRepository<T> repo, V partitionKey, Expression<Func<T, IEnumerable<U>>> selector, Func<IQueryable<T>, IQueryable<T>> whereClauses = null, Func<IQueryable<U>, IQueryable<U>> selectClauses = null, FeedOptions feedOptions = null)
        {
            feedOptions = SetPartitionKey(partitionKey, feedOptions);
            return repo.SelectManyAsync(selector, whereClauses, selectClauses, feedOptions);
        }

        public static Task<CosmosDbRepositoryPagedResults<U>> SelectManyAsync<T, U, V>(this ICosmosDbRepository<T> repo, V partitionKey, int pageSize, string continuationToken, Expression<Func<T, IEnumerable<U>>> selector, Func<IQueryable<T>, IQueryable<T>> whereClauses = null, Func<IQueryable<U>, IQueryable<U>> selectClauses = null, FeedOptions feedOptions = null)
        {
            feedOptions = SetPartitionKey(partitionKey, feedOptions);
            return repo.SelectManyAsync(pageSize, continuationToken, selector, whereClauses, selectClauses, feedOptions);
        }

        public static Task<int> CountAsync<T, U>(this ICosmosDbRepository<T> repo, U partitionKey, Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IQueryable<T>> clauses = null, FeedOptions feedOptions = null)
        {
            feedOptions = SetPartitionKey(partitionKey, feedOptions);
            return repo.CountAsync(predicate, clauses, feedOptions);
        }

        public static Task<T> FindFirstOrDefaultAsync<T, U>(this ICosmosDbRepository<T> repo, U partitionKey, Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IQueryable<T>> clauses = null, FeedOptions feedOptions = null)
        {
            feedOptions = SetPartitionKey(partitionKey, feedOptions);
            return repo.FindFirstOrDefaultAsync(predicate, clauses, feedOptions);
        }

        public static Task<T> GetAsync<T, U>(this ICosmosDbRepository<T> repo, U partitionKey, T entity, RequestOptions requestOptions = null)
        {
            requestOptions = SetPartitionKey(partitionKey, requestOptions);
            return repo.GetAsync(entity, requestOptions);
        }

        public static Task<T> GetAsync<T, U>(this ICosmosDbRepository<T> repo, U partitionKey, DocumentId itemId, RequestOptions requestOptions = null)
        {
            requestOptions = SetPartitionKey(partitionKey, requestOptions);
            return repo.GetAsync(itemId, requestOptions);
        }

        public static Task<bool> DeleteDocumentAsync<T, U>(this ICosmosDbRepository<T> repo, U partitionKey, T entity, RequestOptions requestOptions = null)
        {
            requestOptions = SetPartitionKey(partitionKey, requestOptions);
            return repo.DeleteDocumentAsync(entity, requestOptions);
        }

        public static Task<bool> DeleteDocumentAsync<T, U>(this ICosmosDbRepository<T> repo, U partitionKey, DocumentId itemId, RequestOptions requestOptions = null)
        {
            requestOptions = SetPartitionKey(partitionKey, requestOptions);
            return repo.DeleteDocumentAsync(itemId, requestOptions);
        }

        private static RequestOptions SetPartitionKey<U>(U partitionKey, RequestOptions requestOptions)
        {
            requestOptions = requestOptions.ShallowCopy() ?? new RequestOptions();

            object pk = CosmosDbRepository.Implementation.CosmosDbRepository.IndirectlySupportedIndexTypes.Contains(partitionKey.GetType())
                ? (object)partitionKey.ToString()
                : partitionKey;

            requestOptions.PartitionKey = new PartitionKey(pk);
            return requestOptions;
        }

        private static FeedOptions SetPartitionKey<U>(U partitionKey, FeedOptions feedOptions)
        {
            feedOptions = feedOptions.ShallowCopy() ?? new FeedOptions();

            object pk = CosmosDbRepository.Implementation.CosmosDbRepository.IndirectlySupportedIndexTypes.Contains(partitionKey.GetType())
                ? (object)partitionKey.ToString()
                : partitionKey;

            feedOptions.PartitionKey = new PartitionKey(pk);
            return feedOptions;
        }
    }
}
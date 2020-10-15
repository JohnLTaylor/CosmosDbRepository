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
        public static Task<T> AddAsync<T, U>(this ICosmosDbRepository<T> repo, U partitionKey, T entity, RequestOptions requestOptions = null)
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

        public static Task<IList<U>> SelectAsync<T, U, V>(this ICosmosDbRepository<T> repo, V partitionKey, string queryString, FeedOptions feedOptions = null)
        {
            feedOptions = SetPartitionKey(partitionKey, feedOptions);
            return repo.SelectAsync<U>(queryString, feedOptions);
        }

        public static Task<CosmosDbRepositoryPagedResults<U>> SelectAsync<T, U, V>(this ICosmosDbRepository<T> repo, V partitionKey, int pageSize, string continuationToken, string queryString, FeedOptions feedOptions = null)
        {
            feedOptions = SetPartitionKey(partitionKey, feedOptions);
            return repo.SelectAsync<U>(pageSize, continuationToken, queryString, feedOptions);
        }

        public static Task<IList<T>> SelectAsync<T, V>(this ICosmosDbRepository<T> repo, V partitionKey, string queryString, FeedOptions feedOptions = null)
        {
            feedOptions = SetPartitionKey(partitionKey, feedOptions);
            return repo.SelectAsync<T>(queryString, feedOptions);
        }

        public static Task<CosmosDbRepositoryPagedResults<T>> SelectAsync<T, V>(this ICosmosDbRepository<T> repo, V partitionKey, int pageSize, string continuationToken, string queryString, FeedOptions feedOptions = null)
        {
            feedOptions = SetPartitionKey(partitionKey, feedOptions);
            return repo.SelectAsync<T>(pageSize, continuationToken, queryString, feedOptions);
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

        public static Task<IList<T>> CrossPartitionFindAsync<T>(this ICosmosDbRepository<T> repo, Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IQueryable<T>> clauses = null, FeedOptions feedOptions = null)
        {
            feedOptions = SetCrossPartition(feedOptions);
            return repo.FindAsync(predicate, clauses, feedOptions);
        }

        public static Task<CosmosDbRepositoryPagedResults<T>> CrossPartitionFindAsync<T>(this ICosmosDbRepository<T> repo, int pageSize, string continuationToken, Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IQueryable<T>> clauses = null, FeedOptions feedOptions = null)
        {
            feedOptions = SetCrossPartition(feedOptions);
            return repo.FindAsync(pageSize, continuationToken, predicate, clauses, feedOptions);
        }

        public static Task<IList<U>> CrossPartitionSelectAsync<T, U>(this ICosmosDbRepository<T> repo, string queryString, FeedOptions feedOptions = null)
        {
            feedOptions = SetCrossPartition(feedOptions);
            return repo.SelectAsync<U>(queryString, feedOptions);
        }

        public static Task<CosmosDbRepositoryPagedResults<U>> CrossPartitionSelectAsync<T, U>(this ICosmosDbRepository<T> repo, int pageSize, string continuationToken, string queryString, FeedOptions feedOptions = null)
        {
            feedOptions = SetCrossPartition(feedOptions);
            return repo.SelectAsync<U>(pageSize, continuationToken, queryString, feedOptions);
        }

        public static Task<IList<T>> CrossPartitionSelectAsync<T>(this ICosmosDbRepository<T> repo, string queryString, FeedOptions feedOptions = null)
        {
            feedOptions = SetCrossPartition(feedOptions);
            return repo.SelectAsync(queryString, feedOptions);
        }

        public static Task<CosmosDbRepositoryPagedResults<T>> CrossPartitionSelectAsync<T>(this ICosmosDbRepository<T> repo, int pageSize, string continuationToken, string queryString, FeedOptions feedOptions = null)
        {
            feedOptions = SetCrossPartition(feedOptions);
            return repo.SelectAsync<T>(pageSize, continuationToken, queryString, feedOptions);
        }

        public static Task<IList<U>> CrossPartitionSelectAsync<T, U>(this ICosmosDbRepository<T> repo, Expression<Func<T, U>> selector, Func<IQueryable<U>, IQueryable<U>> selectClauses = null, FeedOptions feedOptions = null)
        {
            feedOptions = SetCrossPartition(feedOptions);
            return repo.SelectAsync(selector, selectClauses, feedOptions);
        }

        public static Task<CosmosDbRepositoryPagedResults<U>> CrossPartitionSelectAsync<T, U>(this ICosmosDbRepository<T> repo, int pageSize, string continuationToken, Expression<Func<T, U>> selector, Func<IQueryable<U>, IQueryable<U>> selectClauses = null, FeedOptions feedOptions = null)
        {
            feedOptions = SetCrossPartition(feedOptions);
            return repo.SelectAsync(pageSize, continuationToken, selector, selectClauses, feedOptions);
        }

        public static Task<IList<U>> CrossPartitionSelectAsync<T, U, V>(this ICosmosDbRepository<T> repo, Expression<Func<V, U>> selector, Func<IQueryable<T>, IQueryable<V>> whereClauses = null, Func<IQueryable<U>, IQueryable<U>> selectClauses = null, FeedOptions feedOptions = null)
        {
            feedOptions = SetCrossPartition(feedOptions);
            return repo.SelectAsync(selector, whereClauses, selectClauses, feedOptions);
        }

        public static Task<CosmosDbRepositoryPagedResults<U>> CrossPartitionSelectAsync<T, U, V>(this ICosmosDbRepository<T> repo, int pageSize, string continuationToken, Expression<Func<V, U>> selector, Func<IQueryable<T>, IQueryable<V>> whereClauses, Func<IQueryable<U>, IQueryable<U>> selectClauses = null, FeedOptions feedOptions = null)
        {
            feedOptions = SetCrossPartition(feedOptions);
            return repo.SelectAsync(pageSize, continuationToken, selector, whereClauses, selectClauses, feedOptions);
        }
        public static Task<IList<U>> CrossPartitionSelectManyAsync<T, U>(this ICosmosDbRepository<T> repo, Expression<Func<T, IEnumerable<U>>> selector, Func<IQueryable<T>, IQueryable<T>> whereClauses = null, Func<IQueryable<U>, IQueryable<U>> selectClauses = null, FeedOptions feedOptions = null)
        {
            feedOptions = SetCrossPartition(feedOptions);
            return repo.SelectManyAsync(selector, whereClauses, selectClauses, feedOptions);
        }

        public static Task<CosmosDbRepositoryPagedResults<U>> CrossPartitionSelectManyAsync<T, U>(this ICosmosDbRepository<T> repo, int pageSize, string continuationToken, Expression<Func<T, IEnumerable<U>>> selector, Func<IQueryable<T>, IQueryable<T>> whereClauses = null, Func<IQueryable<U>, IQueryable<U>> selectClauses = null, FeedOptions feedOptions = null)
        {
            feedOptions = SetCrossPartition(feedOptions);
            return repo.SelectManyAsync(pageSize, continuationToken, selector, whereClauses, selectClauses, feedOptions);
        }

        public static Task<int> CrossPartitionCountAsync<T>(this ICosmosDbRepository<T> repo, Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IQueryable<T>> clauses = null, FeedOptions feedOptions = null)
        {
            feedOptions = SetCrossPartition(feedOptions);
            return repo.CountAsync(predicate, clauses, feedOptions);
        }

        public static Task<T> CrossPartitionFindFirstOrDefaultAsync<T>(this ICosmosDbRepository<T> repo, Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IQueryable<T>> clauses = null, FeedOptions feedOptions = null)
        {
            feedOptions = SetCrossPartition(feedOptions);
            return repo.FindFirstOrDefaultAsync(predicate, clauses, feedOptions);
        }

        public static Task<TResult> ExecuteAsync<U, T1, TResult>(this IStoredProcedure<T1, TResult> storedProcedure, U partitionKey, T1 t1, RequestOptions requestOptions = null)
        {
            requestOptions = SetPartitionKey(partitionKey, requestOptions);
            return storedProcedure.ExecuteAsync(t1, requestOptions);
        }

        public static Task<TResult> ExecuteAsync<U, T1, T2, TResult>(this IStoredProcedure<T1, T2, TResult> storedProcedure, U partitionKey, T1 t1, T2 t2, RequestOptions requestOptions = null)
        {
            requestOptions = SetPartitionKey(partitionKey, requestOptions);
            return storedProcedure.ExecuteAsync(t1, t2, requestOptions);
        }

        public static Task<TResult> ExecuteAsync<U, T1, T2, T3, TResult>(this IStoredProcedure<T1, T2, T3, TResult> storedProcedure, U partitionKey, T1 t1, T2 t2, T3 t3, RequestOptions requestOptions = null)
        {
            requestOptions = SetPartitionKey(partitionKey, requestOptions);
            return storedProcedure.ExecuteAsync(t1, t2, t3, requestOptions);
        }

        public static Task<TResult> ExecuteAsync<U, T1, T2, T3, T4, TResult>(this IStoredProcedure<T1, T2, T3, T4, TResult> storedProcedure, U partitionKey, T1 t1, T2 t2, T3 t3, T4 t4, RequestOptions requestOptions = null)
        {
            requestOptions = SetPartitionKey(partitionKey, requestOptions);
            return storedProcedure.ExecuteAsync(t1, t2, t3, t4, requestOptions);
        }

        public static Task<TResult> ExecuteAsync<U, T1, T2, T3, T4, T5, TResult>(this IStoredProcedure<T1, T2, T3, T4, T5, TResult> storedProcedure, U partitionKey, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, RequestOptions requestOptions = null)
        {
            requestOptions = SetPartitionKey(partitionKey, requestOptions);
            return storedProcedure.ExecuteAsync(t1, t2, t3, t4, t5, requestOptions);
        }

        public static Task<TResult> ExecuteAsync<U, T1, T2, T3, T4, T5, T6, TResult>(this IStoredProcedure<T1, T2, T3, T4, T5, T6, TResult> storedProcedure, U partitionKey, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, RequestOptions requestOptions = null)
        {
            requestOptions = SetPartitionKey(partitionKey, requestOptions);
            return storedProcedure.ExecuteAsync(t1, t2, t3, t4, t5, t6, requestOptions);
        }

        public static Task<TResult> ExecuteAsync<U, T1, T2, T3, T4, T5, T6, T7, TResult>(this IStoredProcedure<T1, T2, T3, T4, T5, T6, T7, TResult> storedProcedure, U partitionKey, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, RequestOptions requestOptions = null)
        {
            requestOptions = SetPartitionKey(partitionKey, requestOptions);
            return storedProcedure.ExecuteAsync(t1, t2, t3, t4, t5, t6, t7, requestOptions);
        }

        public static Task<TResult> ExecuteAsync<U, T1, T2, T3, T4, T5, T6, T7, T8, TResult>(this IStoredProcedure<T1, T2, T3, T4, T5, T6, T7, T8, TResult> storedProcedure, U partitionKey, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, RequestOptions requestOptions = null)
        {
            requestOptions = SetPartitionKey(partitionKey, requestOptions);
            return storedProcedure.ExecuteAsync(t1, t2, t3, t4, t5, t6, t7, t8, requestOptions);
        }

        public static Task<TResult> ExecuteAsync<U, T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(this IStoredProcedure<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> storedProcedure, U partitionKey, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, RequestOptions requestOptions = null)
        {
            requestOptions = SetPartitionKey(partitionKey, requestOptions);
            return storedProcedure.ExecuteAsync(t1, t2, t3, t4, t5, t6, t7, t8, t9, requestOptions);
        }

        public static Task<TResult> ExecuteAsync<U, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(this IStoredProcedure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> storedProcedure, U partitionKey, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, RequestOptions requestOptions = null)
        {
            requestOptions = SetPartitionKey(partitionKey, requestOptions);
            return storedProcedure.ExecuteAsync(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, requestOptions);
        }

        public static Task<TResult> ExecuteAsync<U, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(this IStoredProcedure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> storedProcedure, U partitionKey, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, RequestOptions requestOptions = null)
        {
            requestOptions = SetPartitionKey(partitionKey, requestOptions);
            return storedProcedure.ExecuteAsync(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, requestOptions);
        }

        public static Task<TResult> ExecuteAsync<U, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(this IStoredProcedure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> storedProcedure, U partitionKey, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, RequestOptions requestOptions = null)
        {
            requestOptions = SetPartitionKey(partitionKey, requestOptions);
            return storedProcedure.ExecuteAsync(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, requestOptions);
        }

        public static Task<TResult> ExecuteAsync<U, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(this IStoredProcedure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> storedProcedure, U partitionKey, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, T13 t13, RequestOptions requestOptions = null)
        {
            requestOptions = SetPartitionKey(partitionKey, requestOptions);
            return storedProcedure.ExecuteAsync(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, requestOptions);
        }

        public static Task<TResult> ExecuteAsync<U, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(this IStoredProcedure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> storedProcedure, U partitionKey, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, T13 t13, T14 t14, RequestOptions requestOptions = null)
        {
            requestOptions = SetPartitionKey(partitionKey, requestOptions);
            return storedProcedure.ExecuteAsync(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, requestOptions);
        }

        public static Task<TResult> ExecuteAsync<U, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(this IStoredProcedure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> storedProcedure, U partitionKey, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, T13 t13, T14 t14, T15 t15, RequestOptions requestOptions = null)
        {
            requestOptions = SetPartitionKey(partitionKey, requestOptions);
            return storedProcedure.ExecuteAsync(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15, requestOptions);
        }

        public static Task<TResult> ExecuteAsync<U, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>(this IStoredProcedure<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> storedProcedure, U partitionKey, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, T13 t13, T14 t14, T15 t15, T16 t16, RequestOptions requestOptions = null)
        {
            requestOptions = SetPartitionKey(partitionKey, requestOptions);
            return storedProcedure.ExecuteAsync(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15, t16, requestOptions);
        }

        private static RequestOptions SetPartitionKey<U>(U partitionKey, RequestOptions requestOptions)
        {
            requestOptions = requestOptions.ShallowCopy() ?? new RequestOptions();

            object pk = Implementation.CosmosDbRepository.IndirectlySupportedIndexTypes.Contains(partitionKey.GetType())
                ? (object)partitionKey.ToString()
                : partitionKey;

            requestOptions.PartitionKey = new PartitionKey(pk);
            return requestOptions;
        }

        private static FeedOptions SetPartitionKey<U>(U partitionKey, FeedOptions feedOptions)
        {
            feedOptions = feedOptions.ShallowCopy() ?? new FeedOptions();

            object pk = Implementation.CosmosDbRepository.IndirectlySupportedIndexTypes.Contains(partitionKey.GetType())
                ? (object)partitionKey.ToString()
                : partitionKey;

            feedOptions.PartitionKey = new PartitionKey(pk);
            return feedOptions;
        }

        private static FeedOptions SetCrossPartition(FeedOptions feedOptions)
        {
            feedOptions = feedOptions.ShallowCopy() ?? new FeedOptions();
            feedOptions.EnableCrossPartitionQuery = true;
            return feedOptions;
        }
    }
}
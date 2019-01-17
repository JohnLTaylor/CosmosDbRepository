using CosmosDbRepository.Types;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CosmosDbRepository
{
    public interface ICosmosDbRepository
    {
        string Id { get; }
        Type Type { get; }
    }

    public interface ICosmosDbRepository<T>
        : ICosmosDbRepository
    {
        Task<T> AddAsync(T entity, RequestOptions requestOptions = null);
        Task<T> GetAsync(T entity, RequestOptions requestOptions = null);
        Task<T> GetAsync(DocumentId itemId, RequestOptions requestOptions = null);
        Task<IList<T>> FindAsync(Expression<Func<T, bool>> predicate = null, FeedOptions feedOptions = null);
        Task<CosmosDbRepositoryPagedResults<T>> FindAsync(int pageSize, string continuationToken, Expression<Func<T, bool>> predicate = null, FeedOptions feedOptions = null);
        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate = null, FeedOptions feedOptions = null);
        Task<T> UpsertAsync(T entity, RequestOptions requestOptions = null);
        Task<T> ReplaceAsync(T entity, RequestOptions requestOptions = null);
        Task<bool> RemoveAsync(DocumentId itemId, RequestOptions requestOptions = null);
        Task<bool> RemoveAsync(T entity, RequestOptions requestOptions = null);
        Task<bool> DeleteAsync(RequestOptions requestOptions = null);
    }
}
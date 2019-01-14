using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DocumentDBRepo
{
    public interface IDbCollection
    {
        string Id { get; }
    }

    public interface IDbCollection<T>
        : IDbCollection
    {
        Task<T> AddAsync(T entity, RequestOptions requestOptions = null);
        Task<T> GetAsync(T entity, RequestOptions requestOptions = null);
        Task<T> GetAsync(DocumentId itemId, RequestOptions requestOptions = null);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate = null, FeedOptions feedOptions = null);
        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate = null, FeedOptions feedOptions = null);
        Task<T> UpsertAsync(T entity, RequestOptions requestOptions = null);
        Task<T> ReplaceAsync(T entity, RequestOptions requestOptions = null);
        Task<bool> RemoveAsync(DocumentId itemId, RequestOptions requestOptions = null);
        Task<bool> RemoveAsync(T entity, RequestOptions requestOptions = null);
        Task<bool> DeleteAsync(RequestOptions requestOptions = null);
    }
}
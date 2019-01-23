using System;
using System.Linq;
using System.Linq.Expressions;

namespace CosmosDbRepository.Implementation
{
    internal static class WhereExtension
    {
        public static IQueryable<TSource> ConditionalWhere<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            return (predicate != null)
                ? source.Where(predicate)
                : source;
        }

        public static IQueryable<TSource> ApplyClauses<TSource>(this IQueryable<TSource> source, Func<IQueryable<TSource>, IQueryable<TSource>> clauses)
        {
            return (clauses != null)
                ? clauses.Invoke(source)
                : source;
        }

    }
}

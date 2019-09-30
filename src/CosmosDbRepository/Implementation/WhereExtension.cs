using System;
using System.Linq;
using System.Linq.Expressions;

namespace CosmosDbRepository.Implementation
{
    internal static class WhereExtension
    {
        public static IQueryable<TSource> ConditionalWhere<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return (predicate != default(Expression<Func<TSource, bool>>))
                ? source.Where(predicate)
                : source;
        }

        public static IQueryable<TSource> ConditionalApplyClauses<TSource>(this IQueryable<TSource> source, Func<IQueryable<TSource>, IQueryable<TSource>> clauses)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return (clauses != default)
                ? clauses.Invoke(source)
                : source;
        }

        public static IQueryable<TResult> ApplyClauses<TSource, TResult>(this IQueryable<TSource> source, Func<IQueryable<TSource>, IQueryable<TResult>> clauses)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (clauses is null)
            {
                throw new ArgumentNullException(nameof(clauses));
            }

            return clauses.Invoke(source);
        }

    }
}

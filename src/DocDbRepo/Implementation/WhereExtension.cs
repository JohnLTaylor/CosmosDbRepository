using System;
using System.Linq;
using System.Linq.Expressions;

namespace DocDbRepo.Implementation
{
    internal static class WhereExtension
    {
        public static IQueryable<TSource> ConditionalWhere<TSource>(this IQueryable<TSource> source, Func<bool> condition, Expression<Func<TSource, bool>> predicate)
        {
            return (condition?.Invoke() ?? throw new ArgumentNullException(nameof(condition)))
                ? source.Where(predicate ?? throw new ArgumentNullException(nameof(predicate)))
                : source;
        }
    }
}

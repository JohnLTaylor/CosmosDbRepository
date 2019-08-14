using CosmosDbRepository.Types;
using System;
using System.Net;

namespace CosmosDbRepository.Substitute
{
    public static class CosmosDbRepositorySubstituteExtensions
    {
        public static void GenerateExceptionOnGetWhen<T>(this ICosmosDbRepository<T> self,
                                                         Predicate<DocumentId> predicate,
                                                         HttpStatusCode statusCode,
                                                         string message = default)
        {
            if (self is null) throw new ArgumentNullException(nameof(self));
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            if (!(self is CosmosDbRepositorySubstitute<T> substitute))
                throw new ArgumentException($"self is not a CosmosDbRepositorySubstitute<{typeof(T).Name}>", nameof(self));

            substitute.GenerateExceptionOnGetWhen(predicate, statusCode, message);
        }

        public static void ClearGenerateExceptionOnGet<T>(this ICosmosDbRepository<T> self)
        {
            if (self is null) throw new ArgumentNullException(nameof(self));

            if (!(self is CosmosDbRepositorySubstitute<T> substitute))
                throw new ArgumentException($"self is not a CosmosDbRepositorySubstitute<{typeof(T).Name}>", nameof(self));

            substitute.ClearGenerateExceptionOnGet();
        }

        public static void GenerateExceptionOnAddWhen<T>(this ICosmosDbRepository<T> self,
                                                         Predicate<T> predicate,
                                                         HttpStatusCode statusCode,
                                                         string message = default)
        {
            if (self is null) throw new ArgumentNullException(nameof(self));
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            if (!(self is CosmosDbRepositorySubstitute<T> substitute))
                throw new ArgumentException($"self is not a CosmosDbRepositorySubstitute<{typeof(T).Name}>", nameof(self));

            substitute.GenerateExceptionOnAddWhen(predicate, statusCode, message);
        }

        public static void ClearGenerateExceptionOnAdd<T>(this ICosmosDbRepository<T> self)
        {
            if (self is null) throw new ArgumentNullException(nameof(self));

            if (!(self is CosmosDbRepositorySubstitute<T> substitute))
                throw new ArgumentException($"self is not a CosmosDbRepositorySubstitute<{typeof(T).Name}>", nameof(self));

            substitute.ClearGenerateExceptionOnAdd();
        }

        public static void GenerateExceptionOnCountWhen<T>(this ICosmosDbRepository<T> self,
                                                         Func<bool> predicate,
                                                         HttpStatusCode statusCode,
                                                         string message = default)
        {
            if (self is null) throw new ArgumentNullException(nameof(self));
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            if (!(self is CosmosDbRepositorySubstitute<T> substitute))
                throw new ArgumentException($"self is not a CosmosDbRepositorySubstitute<{typeof(T).Name}>", nameof(self));

            substitute.GenerateExceptionOnCountWhen(predicate, statusCode, message);
        }

        public static void ClearGenerateExceptionOnCount<T>(this ICosmosDbRepository<T> self)
        {
            if (self is null) throw new ArgumentNullException(nameof(self));

            if (!(self is CosmosDbRepositorySubstitute<T> substitute))
                throw new ArgumentException($"self is not a CosmosDbRepositorySubstitute<{typeof(T).Name}>", nameof(self));

            substitute.ClearGenerateExceptionOnCount();
        }
    }
}

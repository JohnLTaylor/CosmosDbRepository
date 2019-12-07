using CosmosDbRepository.Types;
using System;
using System.Net;

namespace CosmosDbRepository.Substitute
{
    public static class CosmosDbRepositorySubstituteBaseExtensions
    {
        public static void GenerateExceptionOnGetWhen<T>(this ICosmosDbRepository<T> self,
                                                         Predicate<DocumentId> predicate,
                                                         HttpStatusCode statusCode,
                                                         string message = default)
        {
            if (self is null) throw new ArgumentNullException(nameof(self));
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            if (!(self is CosmosDbRepositorySubstitute<T> substitute))
                throw new ArgumentException($"self is not a CosmosDbRepositorySubstituteBase<{typeof(T).Name}>", nameof(self));

            substitute.GenerateExceptionOnGetWhen(predicate, statusCode, message);
        }

        public static void GenerateExceptionOnGetWhen<T>(this ICosmosDbRepository<T> self,
                                                         Predicate<T> predicate,
                                                         HttpStatusCode statusCode,
                                                         string message = default)
        {
            if (self is null) throw new ArgumentNullException(nameof(self));
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            if (!(self is CosmosDbRepositorySubstitute<T> substitute))
                throw new ArgumentException($"self is not a CosmosDbRepositorySubstituteBase<{typeof(T).Name}>", nameof(self));

            substitute.GenerateExceptionOnGetWhen(predicate, statusCode, message);
        }

        public static void ClearGenerateExceptionOnGet<T>(this ICosmosDbRepository<T> self)
        {
            if (self is null) throw new ArgumentNullException(nameof(self));

            if (!(self is CosmosDbRepositorySubstitute<T> substitute))
                throw new ArgumentException($"self is not a CosmosDbRepositorySubstituteBase<{typeof(T).Name}>", nameof(self));

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
                throw new ArgumentException($"self is not a CosmosDbRepositorySubstituteBase<{typeof(T).Name}>", nameof(self));

            substitute.GenerateExceptionOnAddWhen(predicate, statusCode, message);
        }

        public static void ClearGenerateExceptionOnAdd<T>(this ICosmosDbRepository<T> self)
        {
            if (self is null) throw new ArgumentNullException(nameof(self));

            if (!(self is CosmosDbRepositorySubstitute<T> substitute))
                throw new ArgumentException($"self is not a CosmosDbRepositorySubstituteBase<{typeof(T).Name}>", nameof(self));

            substitute.ClearGenerateExceptionOnAdd();
        }

        public static void GenerateExceptionOnDeleteWhen<T>(this ICosmosDbRepository<T> self,
                                                            Predicate<DocumentId> predicate,
                                                            HttpStatusCode statusCode,
                                                            string message = default)
        {
            if (self is null) throw new ArgumentNullException(nameof(self));
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            if (!(self is CosmosDbRepositorySubstitute<T> substitute))
                throw new ArgumentException($"self is not a CosmosDbRepositorySubstituteBase<{typeof(T).Name}>", nameof(self));

            substitute.GenerateExceptionOnDeleteWhen(predicate, statusCode, message);
        }

        public static void GenerateExceptionOnDeleteWhen<T>(this ICosmosDbRepository<T> self,
                                                            Predicate<T> predicate,
                                                            HttpStatusCode statusCode,
                                                            string message = default)
        {
            if (self is null) throw new ArgumentNullException(nameof(self));
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            if (!(self is CosmosDbRepositorySubstitute<T> substitute))
                throw new ArgumentException($"self is not a CosmosDbRepositorySubstituteBase<{typeof(T).Name}>", nameof(self));

            substitute.GenerateExceptionOnDeleteWhen(predicate, statusCode, message);
        }

        public static void ClearGenerateExceptionOnDelete<T>(this ICosmosDbRepository<T> self)
        {
            if (self is null) throw new ArgumentNullException(nameof(self));

            if (!(self is CosmosDbRepositorySubstitute<T> substitute))
                throw new ArgumentException($"self is not a CosmosDbRepositorySubstituteBase<{typeof(T).Name}>", nameof(self));

            substitute.ClearGenerateExceptionOnDelete();
        }

        public static void GenerateExceptionOnFindWhen<T>(this ICosmosDbRepository<T> self,
                                                          Func<bool> predicate,
                                                          HttpStatusCode statusCode,
                                                          string message = default)
        {
            if (self is null) throw new ArgumentNullException(nameof(self));
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            if (!(self is CosmosDbRepositorySubstitute<T> substitute))
                throw new ArgumentException($"self is not a CosmosDbRepositorySubstituteBase<{typeof(T).Name}>", nameof(self));

            substitute.GenerateExceptionOnFindWhen(predicate, statusCode, message);
        }

        public static void ClearGenerateExceptionOnFind<T>(this ICosmosDbRepository<T> self)
        {
            if (self is null) throw new ArgumentNullException(nameof(self));

            if (!(self is CosmosDbRepositorySubstitute<T> substitute))
                throw new ArgumentException($"self is not a CosmosDbRepositorySubstituteBase<{typeof(T).Name}>", nameof(self));

            substitute.ClearGenerateExceptionOnFind();
        }

        public static void GenerateExceptionOnFindFirstOrDefaultWhen<T>(this ICosmosDbRepository<T> self,
                                                                        Func<bool> predicate,
                                                                        HttpStatusCode statusCode,
                                                                        string message = default)
        {
            if (self is null) throw new ArgumentNullException(nameof(self));
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            if (!(self is CosmosDbRepositorySubstitute<T> substitute))
                throw new ArgumentException($"self is not a CosmosDbRepositorySubstituteBase<{typeof(T).Name}>", nameof(self));

            substitute.GenerateExceptionOnFindFirstOrDefaultWhen(predicate, statusCode, message);
        }

        public static void ClearGenerateExceptionOnFindFirstOrDefault<T>(this ICosmosDbRepository<T> self)
        {
            if (self is null) throw new ArgumentNullException(nameof(self));

            if (!(self is CosmosDbRepositorySubstitute<T> substitute))
                throw new ArgumentException($"self is not a CosmosDbRepositorySubstituteBase<{typeof(T).Name}>", nameof(self));

            substitute.ClearGenerateExceptionOnFindFirstOrDefault();
        }

        public static void GenerateExceptionOnReplaceWhen<T>(this ICosmosDbRepository<T> self,
                                                             Predicate<T> predicate,
                                                             HttpStatusCode statusCode,
                                                             string message = default)
        {
            if (self is null) throw new ArgumentNullException(nameof(self));
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            if (!(self is CosmosDbRepositorySubstitute<T> substitute))
                throw new ArgumentException($"self is not a CosmosDbRepositorySubstituteBase<{typeof(T).Name}>", nameof(self));

            substitute.GenerateExceptionOnReplaceWhen(predicate, statusCode, message);
        }

        public static void ClearGenerateExceptionOnReplace<T>(this ICosmosDbRepository<T> self)
        {
            if (self is null) throw new ArgumentNullException(nameof(self));

            if (!(self is CosmosDbRepositorySubstitute<T> substitute))
                throw new ArgumentException($"self is not a CosmosDbRepositorySubstituteBase<{typeof(T).Name}>", nameof(self));

            substitute.ClearGenerateExceptionOnReplace();
        }

        public static void GenerateExceptionOnSelectWhen<T>(this ICosmosDbRepository<T> self,
                                                            Func<bool> predicate,
                                                            HttpStatusCode statusCode,
                                                            string message = default)
        {
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));
            if (self is null) throw new ArgumentNullException(nameof(self));

            if (!(self is CosmosDbRepositorySubstitute<T> substitute))
                throw new ArgumentException($"self is not a CosmosDbRepositorySubstituteBase<{typeof(T).Name}>", nameof(self));

            substitute.GenerateExceptionOnSelectWhen(predicate, statusCode, message);
        }

        public static void ClearGenerateExceptionOnSelect<T>(this ICosmosDbRepository<T> self)
        {
            if (self is null) throw new ArgumentNullException(nameof(self));

            if (!(self is CosmosDbRepositorySubstitute<T> substitute))
                throw new ArgumentException($"self is not a CosmosDbRepositorySubstituteBase<{typeof(T).Name}>", nameof(self));

            substitute.ClearGenerateExceptionOnSelect();
        }

        public static void GenerateExceptionOnSelectManyWhen<T>(this ICosmosDbRepository<T> self,
                                                             Func<bool> predicate,
                                                             HttpStatusCode statusCode,
                                                             string message = default)
        {
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));
            if (self is null) throw new ArgumentNullException(nameof(self));

            if (!(self is CosmosDbRepositorySubstitute<T> substitute))
                throw new ArgumentException($"self is not a CosmosDbRepositorySubstituteBase<{typeof(T).Name}>", nameof(self));

            substitute.GenerateExceptionOnSelectManyWhen(predicate, statusCode, message);
        }

        public static void ClearGenerateExceptionOnSelectMany<T>(this ICosmosDbRepository<T> self)
        {
            if (self is null) throw new ArgumentNullException(nameof(self));

            if (!(self is CosmosDbRepositorySubstitute<T> substitute))
                throw new ArgumentException($"self is not a CosmosDbRepositorySubstituteBase<{typeof(T).Name}>", nameof(self));

            substitute.ClearGenerateExceptionOnSelectMany();
        }

        public static void GenerateExceptionOnUpsertWhen<T>(this ICosmosDbRepository<T> self,
                                                             Predicate<T> predicate,
                                                             HttpStatusCode statusCode,
                                                             string message = default)
        {
            if (self is null) throw new ArgumentNullException(nameof(self));
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            if (!(self is CosmosDbRepositorySubstitute<T> substitute))
                throw new ArgumentException($"self is not a CosmosDbRepositorySubstituteBase<{typeof(T).Name}>", nameof(self));

            substitute.GenerateExceptionOnUpsertWhen(predicate, statusCode, message);
        }

        public static void ClearGenerateExceptionOnUpsert<T>(this ICosmosDbRepository<T> self)
        {
            if (self is null) throw new ArgumentNullException(nameof(self));

            if (!(self is CosmosDbRepositorySubstitute<T> substitute))
                throw new ArgumentException($"self is not a CosmosDbRepositorySubstituteBase<{typeof(T).Name}>", nameof(self));

            substitute.ClearGenerateExceptionOnUpsert();
        }

        public static void GenerateExceptionOnCountWhen<T>(this ICosmosDbRepository<T> self,
                                                           Func<bool> predicate,
                                                           HttpStatusCode statusCode,
                                                           string message = default)
        {
            if (self is null) throw new ArgumentNullException(nameof(self));
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));

            if (!(self is CosmosDbRepositorySubstitute<T> substitute))
                throw new ArgumentException($"self is not a CosmosDbRepositorySubstituteBase<{typeof(T).Name}>", nameof(self));

            substitute.GenerateExceptionOnCountWhen(predicate, statusCode, message);
        }

        public static void ClearGenerateExceptionOnCount<T>(this ICosmosDbRepository<T> self)
        {
            if (self is null) throw new ArgumentNullException(nameof(self));

            if (!(self is CosmosDbRepositorySubstitute<T> substitute))
                throw new ArgumentException($"self is not a CosmosDbRepositorySubstituteBase<{typeof(T).Name}>", nameof(self));

            substitute.ClearGenerateExceptionOnCount();
        }

        public static void SetStoredProcedureHandler<T>(this ICosmosDbRepository<T> self, string id, Func<object[], object> func)
        {
            if (self is null) throw new ArgumentNullException(nameof(self));

            if (!(self is CosmosDbRepositorySubstitute<T> substitute))
                throw new ArgumentException($"self is not a CosmosDbRepositorySubstituteBase<{typeof(T).Name}>", nameof(self));

            substitute.SetStoredProcedureHandler(id, func);
        }
    }
}

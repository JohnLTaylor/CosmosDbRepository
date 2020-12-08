using Microsoft.Azure.Documents;
using System;
using System.Linq.Expressions;

namespace CosmosDbRepository
{
    public interface ICosmosDbRepositoryBuilder
    {
        ICosmosDbRepository Build(IDocumentClient client, ICosmosDb documentDb, int? defaultThroughput, bool createOnMissing, ICosmosDbQueryStatsCollector statsCollector);
    }

    public interface ICosmosDbRepositoryBuilder<T>
        : ICosmosDbRepositoryBuilder
    {
        ICosmosDbRepositoryBuilder<T> NoCreate();
        ICosmosDbRepositoryBuilder<T> WithId(string name);
        ICosmosDbRepositoryBuilder<T> WithThroughput(int? defaultThroughput);
        ICosmosDbRepositoryBuilder<T> IncludeIndexPath(string path, params Index[] indexes);
        ICosmosDbRepositoryBuilder<T> IncludePartitionkeyPath(string path);
        ICosmosDbRepositoryBuilder<T> IncludePartitionkeySelector(Func<T, object> partitionkeySelector);
        ICosmosDbRepositoryBuilder<T> ExcludeIndexPath(params string[] paths);
        ICosmosDbRepositoryBuilder<T> StoredProcedure(string id, string body);
        ICosmosDbRepositoryBuilder<T> EnablePolymorphism<TMember>(Expression<Func<T, TMember>> typeSelectMember, params (TMember Value, Type Type)[] valueTypes);
    }
}

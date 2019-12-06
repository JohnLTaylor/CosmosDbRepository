using CosmosDbRepository;
using System;

namespace CosmosDbRepositorySubstituteTest
{
    public interface IPartitionedTestingContext<T>
        : IDisposable
    {
        ICosmosDbRepository<T> Repo { get; }
    }
}
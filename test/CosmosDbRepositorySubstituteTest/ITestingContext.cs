using CosmosDbRepository;
using System;

namespace CosmosDbRepositorySubstituteTest
{
    public interface ITestingContext<T>
        : IDisposable
    {
        ICosmosDbRepository<T> Repo { get; }
    }
}
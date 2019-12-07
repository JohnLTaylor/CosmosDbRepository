using CosmosDbRepository;
using CosmosDbRepository.Substitute;
using System;

namespace CosmosDbRepositorySubstituteTest
{
    public class TestingSubstituteContext<T>
        : ITestingContext<T>
    {
        public ICosmosDbRepository<T> Repo { get; private set; }

        public TestingSubstituteContext(Func<T, object> partionkeySelector, bool? partitioned)
        {
            Repo = new CosmosDbRepositorySubstitute<T>(partionkeySelector, partitioned);
        }

        public void Dispose()
        {
        }
    }
}
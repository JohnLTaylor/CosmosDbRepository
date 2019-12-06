using CosmosDbRepository;
using CosmosDbRepository.Substitute;
using System;

namespace CosmosDbRepositorySubstituteTest
{
    public class TestingPartitionedSubstituteContext<T>
        : IPartitionedTestingContext<T>
    {
        public ICosmosDbRepository<T> Repo { get; private set; }

        public TestingPartitionedSubstituteContext(Func<T, object> partionkeySelector)
        {
            Repo = new CosmosDbRepositoryPartitionedSubstitute<T>(partionkeySelector);
        }

        public void Dispose()
        {
        }
    }
}
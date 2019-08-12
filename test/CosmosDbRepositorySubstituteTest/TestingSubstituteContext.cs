using CosmosDbRepository;
using CosmosDbRepository.Substitute;

namespace CosmosDbRepositorySubstituteTest
{
    public class TestingSubstituteContext<T>
        : ITestingContext<T>
    {
        public ICosmosDbRepository<T> Repo { get; private set; }

        public TestingSubstituteContext()
        {
            Repo = new CosmosDbRepositorySubstitute<T>();
        }

        public void Dispose()
        {
        }
    }
}
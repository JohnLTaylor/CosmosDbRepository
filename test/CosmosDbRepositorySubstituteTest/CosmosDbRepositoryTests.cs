using CosmosDbRepository;
using System;
using System.Threading.Tasks;

namespace CosmosDbRepositorySubstituteTest
{
    public class CosmosDbRepositoryTests<T>
    {
        protected Task<TestData<Guid>> GetTestData(
            TestingContext<TestData<Guid>> context,
            string uniqueData,
            int rank = 0,
            Action<TestData<Guid>> setupAction = null)
        {
            var data = new TestData<Guid>
            {
                Id = Guid.NewGuid(),
                Data = uniqueData,
                Rank = rank
            };

            setupAction?.Invoke(data);

            return context.Repo.AddAsync(data);
        }

        protected ITestingContext<T> CreateContext(IServiceProvider services, Action<ICosmosDbBuilder> builderCallback = null, Action<ICosmosDbRepositoryBuilder> repoBuilderCallback = null)
        {
            return new TestingContext<T>(services, builderCallback, repoBuilderCallback);
        }

        protected ITestingContext<T> CreateSubstituteContext()
        {
            return new TestingSubstituteContext<T>();
        }
    }
}
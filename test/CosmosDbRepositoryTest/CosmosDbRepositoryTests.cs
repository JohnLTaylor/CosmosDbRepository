using CosmosDbRepository;
using System;
using System.Threading.Tasks;

namespace CosmosDbRepositoryTest
{
    public class CosmosDbRepositoryTests<T>
    {
        protected Task<TestData<Guid>> GetTestData(TestingContext<TestData<Guid>> context, string uniqueData, int rank = 0)
        {
            var data = new TestData<Guid>
            {
                Id = Guid.NewGuid(),
                Data = uniqueData,
                Rank = rank
            };

            return context.Repo.AddAsync(data);
        }

        protected TestingContext<T> CreateContext(Action<ICosmosDbBuilder> builderCallback = null, Action<ICosmosDbRepositoryBuilder> repoBuilderCallback = null)
        {
            return new TestingContext<T>(builderCallback, repoBuilderCallback);
        }
    }
}
using CosmosDbRepository;
using System;
using System.Threading.Tasks;

namespace CosmosDbRepositorySubstituteTest
{
    public class CosmosDbRepositoryPartitionedTests<T>
    {
        protected Task<TestData<Guid>> GetTestData(
            PartitionedTestingContext<TestData<Guid>> context,
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

        protected IPartitionedTestingContext<T> CreateContext(Action<ICosmosDbBuilder> builderCallback = null, Action<ICosmosDbRepositoryBuilder<T>> repoBuilderCallback = null)
        {
            return new PartitionedTestingContext<T>(builderCallback, repoBuilderCallback);
        }

        protected IPartitionedTestingContext<T> CreateSubstituteContext(Func<T, object> partionkeySelector)
        {
            return new TestingPartitionedSubstituteContext<T>(partionkeySelector);
        }
    }
}
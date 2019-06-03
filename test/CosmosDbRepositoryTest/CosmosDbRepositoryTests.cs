using CosmosDbRepository;
using System;

namespace CosmosDbRepositoryTest
{
    public class CosmosDbRepositoryTests<T>
    {

        protected TestingContext<T> CreateContext(Action<ICosmosDbBuilder> builderCallback = null, Action<ICosmosDbRepositoryBuilder> repoBuilderCallback = null)
        {
            return new TestingContext<T>(builderCallback, repoBuilderCallback);
        }
    }
}
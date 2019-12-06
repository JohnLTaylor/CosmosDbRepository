using CosmosDbRepository;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace CosmosDbRepositorySubstituteTest
{
    public class PartitionedTestingContext<T>
        : IPartitionedTestingContext<T>
    {
        public readonly DocumentClient DbClient;
        public readonly ICosmosDb CosmosDb;
        public readonly CosmosDbConfig DbConfig;
        public readonly TestConfig TestConfig;
        public readonly EnvironmentConfig EnvConfig;
        public ICosmosDbRepository<T> Repo { get; private set; }

        private bool _disposed;

        public PartitionedTestingContext(Action<ICosmosDbBuilder> builderCallback, Action<ICosmosDbRepositoryBuilder<T>> repoBuilderCallback)
        {
            var services = TestFramework.Services;
            DbConfig = services.GetRequiredService<IOptions<CosmosDbConfig>>().Value;
            TestConfig = services.GetRequiredService<IOptions<TestConfig>>().Value.Clone();
            EnvConfig = services.GetRequiredService<IOptions<EnvironmentConfig>>().Value;

            if (EnvConfig.RandomizeCollectionName)
            {
                TestConfig.CollectionName = $"{TestConfig.CollectionName}{Guid.NewGuid()}";
            }

            DbClient = new DocumentClient(new Uri(DbConfig.DbEndPoint), DbConfig.DbKey);
            var builder = new CosmosDbBuilder()
                .WithId(DbConfig.DbName)
                .WithDefaultThroughput(400)
                .AddCollection<T>(TestConfig.CollectionName, repoBuilderCallback);

            builderCallback?.Invoke(builder);

            CosmosDb = builder.Build(DbClient);

            Repo = CosmosDb.Repository<T>();
        }

        public void Dispose()
        {
            if (!_disposed && EnvConfig.DeleteCollectionsOnClose)
            {
                Repo.DeleteAsync();
                DbClient.Dispose();
                _disposed = true;
            }
        }
    }
}